//Start.cpp

#include <stdio.h>
#include "CMyNet.h"
#include <WinSock2.h>	//�����
#pragma comment(lib,"ws2_32.lib") //dll import ����
#include <ws2tcpip.h> //inet_ntop()
//#pragma warning (disable:4996)

int main()
{
	try
	{
	CMyNet net;
	net.CreateSocket(9000);
	}
	catch (const char* msg)
	{
		printf("���� %s\n", msg);
	}
	return 0;

}

