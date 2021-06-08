//chatcontrol.h

#include "pch.h"
#include "ChatControl.h"
#include <stdio.h>
#include "CChatDlg.h"
#include "packet.h"


ChatControl* ChatControl::instance = NULL;

ChatControl::ChatControl()
{
	client.CreateSocket("192.168.0.93", 8000);
}

void ChatControl::ParentForm(CChatDlg* pDlg)  //<============================
{
	pForm = pDlg;
}

void ChatControl::RecvData(const char* msg, int size)
{
	printf(">> [수신데이터] %dbyte\n", size);
	int* p = (int*)msg;

	if (*p == ACK_SHORTMESSAGE)
	{
		PACKETSHORTMESSAGE * smsg = (PACKETSHORTMESSAGE*)msg;
		pForm->Shortmessage(smsg);
	}
	else if (*p == ACK_MEMOMESSAGE)
	{
		PAKETMEMO* smsg = (PAKETMEMO*)msg;
		pForm->MemoMessage(smsg);
	}

}

void ChatControl::ShortMesaage(const char* name, const char* msg)
{
	//서버 전송(1. 패킷 생성, 2. 전송)
	PACKETSHORTMESSAGE pack = PACKETSHORTMESSAGE::CreatePacket(name, msg);
	client.SendData((const char*)&pack, sizeof(pack));
}

void ChatControl::MemoMessage(const char* name, const char* msg)
{
	PACKETSHORTMESSAGE pack = PACKETSHORTMESSAGE::CreatePacket(name, msg,false);
	client.SendData((const char*)&pack, sizeof(pack));
}
