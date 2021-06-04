//cmynet.h
#pragma once
#include <WinSock2.h>	//�����
#pragma comment(lib,"ws2_32.lib") //dll import ����
#include <vector>
using namespace std;

class CMyNet
{
private:
	SOCKET listen_socket;
	vector<SOCKET> clients; //SOCKET�� �����ϴ� �迭, ũ��� ���������� �����ȴ�.

	//������ & �Ҹ���
public:
	CMyNet();
	~CMyNet();

	//�޼���
public:
	void CreateSocket(int port);

private:
	void Run();
	void GetAddress(SOCKET sock,char* ip,int* port );
	void RecvMessage(SOCKET sock);

private:
	static DWORD __stdcall WorkThread(LPVOID value);
};

