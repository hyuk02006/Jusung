//cfilerecv.h
#pragma once
#define _WINSOCK_DEPRECATED_NO_WARNINGS
#define _CRT_SECURE_NO_WARNINGS
#include <WinSock2.h>
#include <stdio.h>
#pragma comment(lib,"Ws2_32.lib")


class CFileRecv
{
private:
	SOCKET sock;

public:
	CFileRecv(SOCKET _sock);

public:
	bool FileRecv();

private:
	static DWORD WINAPI RecvThread(LPVOID p);
	void FileNameRecv(char* filename);
	int FileSizeRecv();
	void FileDataRecv(const char* filename,int totalbytes);

private:
	int recvn(SOCKET s, char* buf, int len, int flags);
};

