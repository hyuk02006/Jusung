//control.cpp
#include "pch.h"
#include <stdio.h>
#include "MFCApplication1Dlg.h"					//<==============================
#include "Control.h"
#include "packet.h"


Control* Control::instance = NULL;

Control::Control()
{
	client.CreateSocket("192.168.0.93", 9000);
}

void Control::ParentForm(CMFCApplication1Dlg* pDlg)  //<============================
{
	pForm = pDlg;
}

void Control::RecvData(const char* msg, int size)
{
	printf(">> [수신데이터] %dbyte\n", size);
	int* p = (int*)msg;
	if (*p == ACK_POINT)
	{
		PACKETPOINT* pDraw = (PACKETPOINT*)msg; //자신의 형태로 변환 패킷!
		pForm->DrawPaint(pDraw->x, pDraw->y, pDraw->width, pDraw->r, pDraw->g, pDraw->b);

	}
}


void Control::Draw(CPoint p)
{
	//서버 전송(1. 패킷 생성, 2. 전송)
	PACKETPOINT pack = PACKETPOINT::CreatePacket(p.x, p.y);
	client.SendData((const char*)&pack, sizeof(pack));

}
