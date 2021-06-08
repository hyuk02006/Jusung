//cmynet.h

#pragma once

#include <WinSock2.h>					//선언부
#pragma comment(lib, "ws2_32.lib")		//dll import정보
#include <vector>
using namespace std;

class CMyNet
{
private:
	SOCKET listen_socket;
	vector<SOCKET> clients;  //SOCKET을 저장하는 배열!, 크기는 가변적으로 관리된다.

	//생성자 & 소멸자
public:
	CMyNet();
	~CMyNet();	

	//메서드
public:
	void CreateSocket(int port);

private:
	void Run();
	void GetAddress(SOCKET sock, char* ip, int* port);
	int Recvn(SOCKET s, char* buf, int len, int flags);

private:
	static DWORD __stdcall WorkThread(LPVOID valud);
};

