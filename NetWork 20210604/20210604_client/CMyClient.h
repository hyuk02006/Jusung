//CMyClient.h
#pragma once
#include <WinSock2.h>	//�����
#pragma comment(lib,"ws2_32.lib") //dll import ����
class CMyClient
{
private:
	SOCKET sock;

	//������ & �Ҹ���
public:
	CMyClient();
	~CMyClient();

	//�޼���
public:
	void CreateSocket(const char* ip, int port);
	void RecvMessage(SOCKET sock);
	void SendData(const char* msg, int length);

};

