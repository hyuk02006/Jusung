//Control.cpp

#include <stdio.h>
#include "packet.h"
#include "Control.h"

Control* Control::instance = NULL;

void Control::Run()
{
	try
	{
		net.CreateSocket(8000);
	}
	catch (const char* msg)
	{
		printf("에러 : %s\n", msg);
	}
}

void Control::RecvData(int sock, const char* msg, int size)
{
	printf(">> [수신데이터] %dbyte\n", size);
	int* p = (int*)msg;
	if (*p == PACK_SHORTMESSAGE)
	{
		PACKETSHORTMESSAGE* pmsg = (PACKETSHORTMESSAGE*)msg;
		printf(">>[%s] %s\n", pmsg->name, pmsg->msg);
		pmsg->flag = ACK_SHORTMESSAGE;
		net.SendAllData(sock, msg, size);   //<====== 전송!!
	}
	else if (*p == PACK_MEMOMESSAGE)
	{
		PAKETMEMO* pmsg = (PAKETMEMO*)msg;
		printf(">>[%s] %s\n", pmsg->name, pmsg->msg);
		pmsg->flag = ACK_MEMOMESSAGE;
		net.SendAllData(sock, msg, size);   //<====== 전송!!
	}
}

