//cmynet.cpp

#include <stdio.h>
#include <ws2tcpip.h>	//inet_pton
#include "CMyNet.h"
#include "Control.h"

#define DATA_MAX		1024

//구조체 정의(맴버 : CMyNet*,  SOCKET)
//CreateThread할 때 구조체 변수의 주소를 전달!
CMyNet* g_mynet = NULL;

CMyNet::CMyNet() : listen_socket(0)
{
	//1. 라이브러리 초기화(Winsock 2.2버전)
	WSADATA wsa;
	if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
	{
		printf("윈도우 소켓 초기화 실패\n");
		exit(-1);
	}
	g_mynet = this;
}

CMyNet::~CMyNet()
{
	//2. 라이브러리 해제
	WSACleanup();
}

void CMyNet::CreateSocket(int port)
{
	listen_socket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (listen_socket == INVALID_SOCKET)
		throw "소켓 생성 오류";

	SOCKADDR_IN addr;
	memset(&addr, 0, sizeof(addr));  //API ZeroMemory(&addr, sizeof(addr));
	addr.sin_family = AF_INET;
	addr.sin_port = htons(port);
	addr.sin_addr.s_addr = htonl(INADDR_ANY); // long형을 network byte order로.
	int retval = bind(listen_socket, (SOCKADDR*)&addr, sizeof(addr));
	if (retval == SOCKET_ERROR)
		throw "bind 오류";

	retval = listen(listen_socket, SOMAXCONN);
	if (retval == SOCKET_ERROR)
		throw "listen 오류";

	Run();
}

void CMyNet::Run()
{
	SOCKET clientsocket;
	SOCKADDR_IN clientaddr;
	int addrlen = sizeof(clientaddr);  //<=======================초기화
	
	printf("클라이언트 연결 대기:192.168.0.93:9000\n");

	while (true)
	{		
		clientsocket = accept(listen_socket, (SOCKADDR*)&clientaddr, &addrlen);
		if (clientsocket == INVALID_SOCKET)
		{
			printf("accept 오류\n");
			continue;
		}

		char ip[20];
		int port;
		GetAddress(clientsocket, ip, &port);
		printf("[클라이언트 접속] %s:%d\n", ip, port);

		//vector에 저장 <------------------------------------------------
		clients.push_back(clientsocket); // 0번째 인덱스부터 차곡차곡 저장
										 // 배열의 크기가 자동으로 증가!

		//통신 스레드 생성(접속한 소켓 전달)
		CloseHandle(CreateThread(0, 0, WorkThread, (LPVOID)clientsocket, 0, 0));
		
	}
}

DWORD __stdcall CMyNet::WorkThread(LPVOID value)
{
	SOCKET sock = (SOCKET)value;	

	while (true)
	{
		//수신
		char buf[DATA_MAX];
		int retval = g_mynet->RecvData(sock, buf, sizeof(buf), 0);
		if (retval == -1 || retval == 0)
		{
			printf("소켓 오류 or 상대방이 종료함\n");
			break;
		}
				
		Control::getInsatnce()->RecvData(sock, buf, retval);	

		//송신부 제거!!!!
	}

	//vector 삭제 알고리즘
	for (int i = 0; i < (int)g_mynet->clients.size(); i++) //<====
	{
		SOCKET s = g_mynet->clients[i];
		if (s == sock)
		{
			//erase는 삭제할 배열의 주소(위치)를 요구한다.
			//- clients.begin() : 배열의 시작주소를 반환
			g_mynet->clients.erase( g_mynet->clients.begin() + i);
			closesocket(sock);
			return 0;
		}
	}
	return 0;
}

//연결된 통신 소켓을 이용해서 주소를 획득
//getpeername(상대방), getsockname(자신)
void CMyNet::GetAddress(SOCKET sock, char* ip, int* port)
{
	SOCKADDR_IN addr;
	int addrlenth= sizeof(addr);
	getpeername(sock, (SOCKADDR*)&addr, &addrlenth);
	
	//inet_ntoa : 정수주소 -> 문자열
	//#include <ws2tcpip.h>	//inet_pton
	inet_ntop(AF_INET, &(addr.sin_addr.s_addr), ip, INET_ADDRSTRLEN);
	*port = ntohs(addr.sin_port);
}


int  CMyNet::Recvn(SOCKET s, char* buf, int len, int flags)
{
	int received;
	char* ptr = buf;
	int left = len;
	while (left > 0)
	{
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

int CMyNet::RecvData(SOCKET s, char* buf, int len, int flags)
{
	//1) 4byte의 헤더
	int size;
	int retval = g_mynet->Recvn(s, (char*)&size, sizeof(int), 0);

	//2) 가변데이터 획득
	retval = g_mynet->Recvn(s, buf, size, 0);
	return retval;
}

int CMyNet::SendData(SOCKET sock, const char* msg, int size)
{
	//개인에게 전달!
	//1) size 전송
	int retval = send(sock, (char*)&size, sizeof(int), 0);

	//2) 가변데이터 전송
	retval = send(sock, msg, size, 0);
	printf("[1대 1 송신] %dbyte\n", retval);
	return retval;
}

void CMyNet::SendAllData(SOCKET sock, const char* msg, int size)
{
	//전체 전송(1대 다 통신)
	int retval = 0;
	for (int i = 0; i < (int)g_mynet->clients.size(); i++) //<====
	{
		SOCKET s = g_mynet->clients[i];	 //연산자 재정의[]
		retval = SendData(s, msg, size);
	}		
	printf("[1대 다 송신] %dbyte\n", retval);
}