//cfilerecv.cpp
#include "CFileRecv.h"
#include <stdio.h>

#define _CRT_SECURE_NO_WARNINGS
#define _WINSOCK_DEPRECATED_NO_WARNINGS

#define FILE_BUFSIZE 4096

CFileRecv::CFileRecv(SOCKET _sock)
{
	sock = _sock;
}

//외부 접근하는 기능 함수
bool CFileRecv::FileRecv()
{
	CloseHandle(CreateThread(0, 0, RecvThread, this, 0, 0));
	return true;
}
DWORD WINAPI CFileRecv::RecvThread(LPVOID p)
{
	CFileRecv* pThis = (CFileRecv *)p;
	try
	{
		char filename[100];
		ZeroMemory(filename, 100);
		pThis->FileNameRecv(filename);
		int totalbytes = pThis->FileSizeRecv();
		pThis->FileDataRecv(filename, totalbytes);
		closesocket(pThis->sock);

		return 1;
	}
	catch (const char* msg)
	{
		printf("[에러]%s\n", msg);
		return 0;
	}
}

void CFileRecv::FileNameRecv(char* filename)
{

	int retval = recvn(sock, filename, 100, 0);
	if (retval == SOCKET_ERROR) 
		throw "파일 이름  수신 오류";

	printf("-> 받을 파일 이름: %s\n", filename);

}

int CFileRecv::FileSizeRecv()
{
	int totalbytes;
	int retval = recvn(sock, (char*)&totalbytes, sizeof(totalbytes), 0);
	if (retval == SOCKET_ERROR)
	{
		throw "파일 크기 수신 오류";

		printf("-> 받을 파일 크기: %d\n", totalbytes);
		return totalbytes;
	}

}

void CFileRecv::FileDataRecv(const char* filename,int totalbytes)
{
	FILE* fp = fopen(filename, "wb");
	if (fp == NULL)
		throw " 파일 입축력 오류";
	

	//파일 데이터 받기
	char buf[FILE_BUFSIZE];
	int numtotal = 0;
	while (true) 
	{
		int retval = recvn(sock, buf, FILE_BUFSIZE, 0);
		if (retval == SOCKET_ERROR)
			throw "파일 데이터 수신오류";

		else if (retval == 0)
			break;
		else 
		{
			fwrite(buf, 1, retval, fp);
			if (ferror(fp))
				throw" 파일 입출력 수신 오류";
			numtotal += retval;
		}
	}
	fclose(fp);

	// 수신 결과 출력
	if (numtotal == totalbytes)
		throw "파일 수신 실패";
}

int CFileRecv :: recvn(SOCKET s, char* buf, int len, int flags)
{
	int received;
	char* ptr = buf;

	int left = len;

	while (left > 0) {
		received = recv(s, ptr, left, flags);
		if (received == SOCKET_ERROR)
			return SOCKET_ERROR;
		else if (received == 0)
			break;
		left -= received;
		ptr += received;
	}

	return (len - left);

}
