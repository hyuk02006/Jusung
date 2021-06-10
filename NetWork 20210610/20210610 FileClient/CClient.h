//CClient.h
#pragma once
#define _WINSOCK_DEPRECATED_NO_WARNINGS

#include <winsock2.h>		
#include <stdlib.h>		
#include <stdio.h>

class CClient
{
private:
	SOCKET sock;

public:
	CClient();
	~CClient();
public:

	SOCKET getSock() const;
public:

	bool CreateSocket(const char* ip, int port);
	void CloseSocket();
};

