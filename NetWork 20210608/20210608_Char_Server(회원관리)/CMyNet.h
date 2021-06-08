//cmynet.h

#pragma once

#include <WinSock2.h>					//�����
#pragma comment(lib, "ws2_32.lib")		//dll import����
#include <vector>
using namespace std;

class CMyNet
{
private:
	SOCKET listen_socket;
	vector<SOCKET> clients;  //SOCKET�� �����ϴ� �迭!, ũ��� ���������� �����ȴ�.

	//������ & �Ҹ���
public:
	CMyNet();
	~CMyNet();	

	//�޼���
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

