//FileSender.cpp
// [ File Sender ]
#define _WINSOCK_DEPRECATED_NO_WARNINGS
#define _CRT_SECURE_NO_WARNINGS

#include <winsock2.h>	
#pragma comment(lib,"Ws2_32.lib")
#include <stdlib.h>		
#include <stdio.h>

#define BUFSIZE 4096
#define FILE_NAME "sample.pdf"
// ���� �Լ� ���� ��� �� ����
void err_quit(const char* msg);
// ���� �Լ� ���� ���
void err_display(const char* msg);

int main(int argc, char* argv[])
{
	int retval;

	// ���� �ʱ�ȭ
	WSADATA wsa;
	if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
		return -1;

	// socket()
	SOCKET sock = socket(AF_INET, SOCK_STREAM, 0);
	if (sock == INVALID_SOCKET) err_quit("socket()");

	// connect()
	SOCKADDR_IN serveraddr;
	ZeroMemory(&serveraddr, sizeof(serveraddr));
	serveraddr.sin_family = AF_INET;
	serveraddr.sin_port = htons(9000);

	serveraddr.sin_addr.s_addr = inet_addr("127.0.0.1");
	retval = connect(sock, (SOCKADDR*)&serveraddr, sizeof(serveraddr));
	if (retval == SOCKET_ERROR) err_quit("connect()");


	//------------------���� IO---------------------------------

	// ���� ����
	FILE* fp = fopen(FILE_NAME, "rb");
	if (fp == NULL) { perror("���� ����� ����");		return -1; }

	// 1. ���� �̸� ������(100byte)
	char filename[100];
	ZeroMemory(filename, 100);
	sprintf_s(filename, FILE_NAME);
	retval = send(sock, filename, 100, 0);
	if (retval == SOCKET_ERROR) err_quit("send()");


	// 2. ���� ũ�� ���
	fseek(fp, 0, SEEK_END);//���������͸� ������ ������ �̵�!
	int totalbytes = ftell(fp);//�ش� ��ġ�� �� ��ȯ

	//3.���� ũ�� ������(4byte)
	retval = send(sock, (char*)&totalbytes, sizeof(totalbytes), 0);
	if (retval == SOCKET_ERROR) err_quit("send()");



	// 4. ���� ������ ���ۿ� ����� ����
	char buf[BUFSIZE];	//4096byte	
	int numread;
	int numtotal = 0;

	// ���� ������ ������
	rewind(fp); // ���� �����͸� ���� ������ �̵� //1*BUFSIZE
	//1.�ó����� : buf�� ũ�� 40696, ����ũ�� : 4096���� ���� �� �ִ�.
	//			  �ѹ��� Read�� ������ ��ü ������ ȹ��.. �̶� ��ȯ���� ���� ������ ũ��!
	//2.�ó����� : buf�� ũ�� 40696, ����ũ�� : 4096���� Ŭ �� �ִ�.
	//			  �������� Read�� ������ ��ü ������ ȹ��.. ��) 4096 + 4096 + 4096 + 10

	while (1) {
		numread = fread(buf, 1, BUFSIZE, fp);
		if (numread > 0) {
			retval = send(sock, buf, numread, 0);
			if (retval == SOCKET_ERROR) {
				err_display("send()");				break;
			}
			numtotal += numread;
		}
		else if (numread == 0 && numtotal == totalbytes) {
			printf("���� ���� �Ϸ�!: %d ����Ʈ\n", numtotal);			break;
		}
		else {
			perror("���� ����� ����");			break;
		}
	}
	fclose(fp);
	// closesocket()
	closesocket(sock);
	// ���� ����	
	WSACleanup();
	return 0;
}



// ���� �Լ� ���� ��� �� ����
void err_quit(const char* msg)
{
	LPVOID lpMsgBuf;
	FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER |
		FORMAT_MESSAGE_FROM_SYSTEM,
		NULL, WSAGetLastError(),
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR)&lpMsgBuf, 0, NULL);
	MessageBox(NULL, (LPCTSTR)lpMsgBuf, msg, MB_ICONERROR);
	LocalFree(lpMsgBuf);
	exit(-1);
}

// ���� �Լ� ���� ���
void err_display(const char* msg)
{
	LPVOID lpMsgBuf;
	FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER |
		FORMAT_MESSAGE_FROM_SYSTEM,
		NULL, WSAGetLastError(),
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR)&lpMsgBuf, 0, NULL);
	printf("[%s] %s", msg, (LPCTSTR)lpMsgBuf);
	LocalFree(lpMsgBuf);
}



