//cserver.h
#include "CServer.h"
#define BUFSIZE 4096

CServer::CServer():sock(0)
{
	WSADATA wsa;
	if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
		exit(-1);
}
CServer::~CServer()
{
	WSACleanup();
}

SOCKET CServer::getSock() const
{
	return sock;
}

bool CServer::CreateSocket(int port)
{
	// socket()
	listen_sock = socket(AF_INET, SOCK_STREAM, 0);
	if (listen_sock == INVALID_SOCKET)
		return false;

	// bind()
	SOCKADDR_IN serveraddr;
	ZeroMemory(&serveraddr, sizeof(serveraddr));
	serveraddr.sin_family = AF_INET;
	serveraddr.sin_port = htons(9000);
	serveraddr.sin_addr.s_addr = htonl(INADDR_ANY);
	int retval = bind(listen_sock, (SOCKADDR*)&serveraddr, sizeof(serveraddr));
	if (retval == SOCKET_ERROR)
		return false;

	// listen()
	retval = listen(listen_sock, SOMAXCONN);
	if (retval == SOCKET_ERROR) 
		return false;

	CloseHandle(CreateThread(0,0,ListenThread,this,0,0));

	return true;
}


DWORD WINAPI CServer::ListenThread(LPVOID p)
{
	HANDLE hEvent;
	hEvent = CreateEvent(0, FALSE, FALSE, "e"); //ÀÚµ¿ , nonsignal

	CServer* pserver = (CServer*)p;
	SOCKADDR_IN clientaddr;
	int addrlen =sizeof(clientaddr);
	pserver->sock = accept(pserver->listen_sock, (SOCKADDR*)&clientaddr,&addrlen);
	if (pserver->sock == INVALID_SOCKET)
		exit(-1);

	SetEvent(hEvent);
	pserver->isConnect = TRUE;
	return -1;
}

void CServer::CloseSocket()
{
	closesocket(sock);
	closesocket(listen_sock);
}
