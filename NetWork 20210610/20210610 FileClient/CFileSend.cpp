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

//외부 접근하는 기능 함수
bool CFileSend::FileSend(const char* filename)
{
	try
	{
		FILE* fp = fopen(filename, "rb");
		if (fp == NULL)
			throw "파일 생성 오류";

		FileNameSend(filename);
		int totalbytes = FileSizeSend(fp);
		FileDataSend(fp,totalbytes);
		return true;
	}
	catch (const char* msg)
	{
		printf("[에러] %s\n", msg);
		return false; 
	}

}


void CFileSend::FileNameSend(const char* filename)
{
	int retval = send(sock, filename, 100, 0);
	if (retval == SOCKET_ERROR)
		throw "파일 이름 전송 오류";
}


int CFileSend::FileSizeSend(FILE* fp)
{
	fseek(fp, 0, SEEK_END);//파일포인터를 파일의 끝으로 이동!
	int totalbytes = ftell(fp);//해당 위치의 값 반환

	//파일 크기 보내기(4byte)
	int retval = send(sock, (char*)&totalbytes, sizeof(totalbytes), 0);
	if (retval == SOCKET_ERROR)
		throw "파일 크기 전송 오류";

	return 0;
}
void CFileSend::FileDataSend(FILE* fp,int totalbytes)
{
	char buf[FILE_BUFSIZE];	//4096byte	
		int numread;
	int numtotal = 0;

	// 파일 데이터 보내기
	rewind(fp);	

	while (1) {
		numread = fread(buf, 1, FILE_BUFSIZE, fp);
		if (numread > 0) {
			int retval = send(sock, buf, numread, 0);
			if (retval == SOCKET_ERROR)
				throw "파일 데이터 전송 오류";

			numtotal += numread;
		}
		else if (numread == 0 && numtotal == totalbytes) {
			printf("파일 전송 완료!: %d 바이트\n", numtotal);			break;
		}
		else 
			throw "파일 read 오류";
	}
}

