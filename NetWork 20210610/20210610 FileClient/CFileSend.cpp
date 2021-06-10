//cfile.cpp
#define _CRT_SECURE_NO_WARNINGS
#define _WINSOCK_DEPRECATED_NO_WARNINGS
#include "CFileSend.h"
#include <stdio.h>

#define FILE_BUFSIZE 4096

CFileSend::CFileSend(SOCKET _sock)
{
	sock = _sock;
}

//�ܺ� �����ϴ� ��� �Լ�
bool CFileSend::FileSend(const char* filename)
{
	try
	{
		FILE* fp = fopen(filename, "rb");
		if (fp == NULL)
			throw "���� ���� ����";

		FileNameSend(filename);
		int totalbytes = FileSizeSend(fp);
		FileDataSend(fp,totalbytes);
		return true;
	}
	catch (const char* msg)
	{
		printf("[����] %s\n", msg);
		return false; 
	}

}


void CFileSend::FileNameSend(const char* filename)
{
	int retval = send(sock, filename, 100, 0);
	if (retval == SOCKET_ERROR)
		throw "���� �̸� ���� ����";
}


int CFileSend::FileSizeSend(FILE* fp)
{
	fseek(fp, 0, SEEK_END);//���������͸� ������ ������ �̵�!
	int totalbytes = ftell(fp);//�ش� ��ġ�� �� ��ȯ

	//���� ũ�� ������(4byte)
	int retval = send(sock, (char*)&totalbytes, sizeof(totalbytes), 0);
	if (retval == SOCKET_ERROR)
		throw "���� ũ�� ���� ����";

	return 0;
}
void CFileSend::FileDataSend(FILE* fp,int totalbytes)
{
	char buf[FILE_BUFSIZE];	//4096byte	
		int numread;
	int numtotal = 0;

	// ���� ������ ������
	rewind(fp);	

	while (1) {
		numread = fread(buf, 1, FILE_BUFSIZE, fp);
		if (numread > 0) {
			int retval = send(sock, buf, numread, 0);
			if (retval == SOCKET_ERROR)
				throw "���� ������ ���� ����";

			numtotal += numread;
		}
		else if (numread == 0 && numtotal == totalbytes) {
			printf("���� ���� �Ϸ�!: %d ����Ʈ\n", numtotal);			break;
		}
		else 
			throw "���� read ����";
	}
}

