//cmyclient.cpp

#include <stdio.h>
#include <ws2tcpip.h>	//inet_pton
#include "CMyClient.h"
#include "Control.h"
CMyClient::CMyClient() : sock(0)
{
	//1. 라이브러리 초기화(Winsock 2.2버전)
	WSADATA wsa;
	if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
	{
		printf("윈도우 소켓 초기화 실패\n");
		exit(-1);
	}
}

CMyClient::~CMyClient()
{
	//2. 라이브러리 해제
	WSACleanup();
}

void CMyClient::CreateSocket(const char* ip, int port)
{
	sock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (sock == INVALID_SOCKET)
		throw "소켓 생성 오류";

	SOCKADDR_IN addr;
	memset(&addr, 0, sizeof(addr));
	addr.sin_family = AF_INET;
	addr.sin_port = htons(port);
	unsigned int numberaddr;
	inet_pton(AF_INET, ip, &numberaddr);
	addr.sin_addr.s_addr = numberaddr;
	int retval = connect(sock, (SOCKADDR*)&addr, sizeof(addr));
	if (retval == SOCKET_ERROR)
		throw "서버 연결 실패";

	//수신 Thread생성하는 위치
	CloseHandle(CreateThread(0, 0, RecvThread, (LPVOID)this, 0, 0));
}

void CMyClient::SendData(const char* msg, int length)
{
	int retval1 = send(sock, msg, length, 0);
	printf("[송신] %dbyte\n", retval1);	
}

DWORD __stdcall CMyClient::RecvThread(LPVOID value)
{
	CMyClient* pClient = (CMyClient*)value;
	SOCKET sock = (SOCKET)pClient->sock;
	while (true)
	{
		//수신
		char buf[48];
		int retval = pClient->Recvn(sock, buf, sizeof(buf), 0);
		if (retval == -1 || retval == 0)
		{
			printf("소켓 오류 or 상대방이 종료함\n");
			closesocket(sock);
			return 0;
		}
		Control::getInstance()->RecvData(buf, retval);
	}
	return 0;
}

int CMyClient::Recvn(SOCKET s, char* buf, int len, int flags)
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
