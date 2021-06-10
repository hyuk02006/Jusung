//cserver.h

#pragma once
#include <winsock2.h>		
#include <stdlib.h>		
#include <stdio.h>

class CServer
{
private:
	SOCKET listen_sock; //措扁家南
	SOCKET sock;//烹脚家南
	bool isConnect = FALSE; //立加咯何

public:
	CServer();
	~CServer();

public:
	SOCKET getSock() const;
	bool getIsConeect() const { return isConnect; }

public:
	bool CreateSocket( int port);
	void CloseSocket();
private:
	static DWORD WINAPI ListenThread(LPVOID p);
};

