//CMyClient.cpp
#include "CMyClient.h"
#include <stdio.h>
#include <ws2tcpip.h> //inet_ntop()

CMyClient::CMyClient() : sock(0)
{
	//1. ���̺귯�� �ʱ�ȭ(Winsock 2.2����)
	WSADATA wsa;
	if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
	{
		printf("������ ���� �ʱ�ȭ ���� \n");
		exit(-1);
	}
}

CMyClient::~CMyClient()
{
	//2. ���̺귯�� ����
	WSACleanup();
}

void CMyClient::CreateSocket(const char* ip,int port)
{
	sock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (sock == INVALID_SOCKET)
		throw "���� ���� ����";

	SOCKADDR_IN addr;
	memset(&addr, 0, sizeof(addr)); //API ZeroMemory(&addr,sizeof(addr));
	addr.sin_family = AF_INET;
	addr.sin_port = htons(port);
	unsigned int numberaddr;
	inet_pton(AF_INET, ip, &numberaddr);
	addr.sin_addr.s_addr = numberaddr;
	int retval = connect(sock, (SOCKADDR*)&addr, sizeof(addr));
	if (retval == SOCKET_ERROR)
		throw "���� ���� ����";

	//RecvMessage(sock);

	//���� Thread�����ϴ� ��ġ
	CloseHandle(CreateThread(NULL, 0, RecvThread, (LPVOID*)sock, 0, 0));
}

void CMyClient::RecvMessage(SOCKET sock)
{
	// ����
	char buf[256];
	strcpy_s(buf, "������ ��");

	int retval = send(sock, buf, strlen(buf), 0);
	printf("[�۽�]%dbyte\n", retval);

	int retval1 = recv(sock, buf, sizeof(buf), 0);
	buf[retval1] = '\0';
	printf(">> [���ŵ�����] %s\n", buf);

}

void CMyClient::SendData(const char* msg, int length)
{
	int retval1 = send(sock, msg, length, 0);
	printf("[�۽�]%dbyte\n", retval1);
}

DWORD WINAPI CMyClient ::RecvThread(LPVOID p)
{
	SOCKET sock = (SOCKET)p;
	//����
	while (true)
	{
		char buf[256];
		int retval = recv(sock, buf, sizeof(buf), 0);
		if (retval == -1 || retval == 0)
		{
			printf("���� ���� or ������ ������\n");
			closesocket(sock);
			break;
		}
		buf[retval] = '\0'; //������ �ѱ涧 strlen() ��ŭ�� �Ѱ�ٰ� ����
		printf(">>[���ŵ�����] %s\n", buf);
	}

	return 0;
}