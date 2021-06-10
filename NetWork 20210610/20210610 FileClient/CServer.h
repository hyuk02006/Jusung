//cserver.h

#pragma once
#include <winsock2.h>		
#include <stdlib.h>		
#include <stdio.h>

class CServer
{
private:
	SOCKET listen_sock; //������
	SOCKET sock;//��ż���
	bool isConnect = FALSE; //���ӿ���

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

