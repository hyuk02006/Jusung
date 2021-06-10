//cfilesend.h
#pragma once
#include <WinSock2.h>
#include <stdio.h>
#include <stdlib.h>	
#pragma comment(lib,"Ws2_32.lib")

class CFileSend
{
private:
	SOCKET sock;

public:
	CFileSend(SOCKET _sock);

public:
	bool FileSend(const char* filename);

private:
	void FileNameSend(const char* filename);
	int FileSizeSend(FILE* fp);
	void FileDataSend(FILE* fp, int totalbytes);

};

