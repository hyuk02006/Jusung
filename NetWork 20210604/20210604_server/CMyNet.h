//cmynet.h
#pragma once
#include <WinSock2.h>	//�����
#pragma comment(lib,"ws2_32.lib") //dll import ����

class CMyNet
{
private:
	SOCKET listen_socket;

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

	
};

