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

public:
	int SendData(SOCKET sock, const char* msg, int size);
	void SendAllData(SOCKET sock, const char* msg, int size);
	
private:
	int RecvData(SOCKET s, char* buf, int len, int flags);
	int Recvn(SOCKET s, char* buf, int len, int flags);

private:
	void Run();
	void GetAddress(SOCKET sock, char* ip, int* port);	

private:
	static DWORD __stdcall WorkThread(LPVOID valud);
};

