//cmynet.h
#pragma once
#include <WinSock2.h>	//선언부
#pragma comment(lib,"ws2_32.lib") //dll import 정보

class CMyNet
{
private:
	SOCKET listen_socket;

	//생성자 & 소멸자
public:
	CMyNet();
	~CMyNet();

	//메서드
public:
	void CreateSocket(int port);

private:
	void Run();
	void GetAddress(SOCKET sock,char* ip,int* port );
	void RecvMessage(SOCKET sock);

	
};

