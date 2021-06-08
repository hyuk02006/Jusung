//cmyclient.h
#pragma once

#include <WinSock2.h>					//선언부
#pragma comment(lib, "ws2_32.lib")		//dll import정보

class CMyClient
{
private:
	SOCKET sock;

	//생성자 & 소멸자
public:
	CMyClient();
	~CMyClient();

	//메서드
public:
	void CreateSocket(const char* ip, int port);
	void SendData(const char* msg, int length);

private:
	int RecvData(SOCKET s, char* buf, int len, int flags);
	int  Recvn(SOCKET s, char* buf, int len, int flags);

private:
	static DWORD __stdcall RecvThread(LPVOID valud);
};

