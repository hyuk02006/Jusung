//control.cpp
#include "pch.h"
#include <stdio.h>
#include "20210608_Chat_ClientDlg.h"					//<==============================
#include "CChatDlg.h"
#include "Control.h"
#include "packet.h"
#include "ChatControl.h"

Control* Control::instance = NULL;	

Control::Control()
{
	client.CreateSocket("192.168.0.93", 9000);
}

void Control::ParentForm(CMy20210608ChatClientDlg* pDlg)  //<============================
{
	pForm = pDlg;
}

void Control::RecvData(const char* msg, int size)
{
	printf(">> [수신데이터] %dbyte\n", size);
	int* p = (int*)msg;
	
	if (*p == ACK_NEWPMEMBER_S)
	{
		MessageBox(0, "알림", ">> 회원가입 성공", MB_OK);
	}
	else if (*p == ACK_NEWPMEMBER_F)
	{
		MessageBox(0, "알림", ">> 회원가입 실패", MB_OK);
	}
	else if (*p == ACK_LOGIN_S)
	{
		PACKLOGIN* pLogin = (PACKLOGIN*)msg;

		//모달로 채팅화면 생성
		pForm->ShowWindow(SW_HIDE);
		CChatDlg dlg;
		dlg.SendData(pLogin->name);
		dlg.DoModal();

		pForm->ShowWindow(SW_SHOW);
		pForm->ShowWindow(SW_NORMAL);
		LogoutMember(pLogin->id);
	}
	else if (*p == ACK_LOGIN_F)
	{
		MessageBox(0, "알림", ">> 로그인 실패", MB_OK);
	}
	else if (*p == ACK_LOGOUT_S)
	{
		MessageBox(0, "알림", ">> 로그아웃 성공", MB_OK);
	}
	else if (*p == ACK_LOGOUT_F)
	{
		MessageBox(0, "알림", ">> 로그아웃 실패", MB_OK);
	}
	else if (*p == ACK_SHORTMESSAGE)
	{
		ChatControl::getInstance()->RecvData(msg, size);
	}
	else if (*p == ACK_MEMOMESSAGE)
	{
		ChatControl::getInstance()->RecvData(msg, size);
	}
}

void Control::InsertMember(const char* id, const char* pw, const char* name)
{
	//서버 전송(1. 패킷 생성, 2. 전송)
	PACKETNEWMEMBER pack = PACKETNEWMEMBER::CreatePacket(id, pw, name);
	client.SendData((const char*)&pack, sizeof(pack));
}


void Control::LoginMember(const char* id, const char* pw)
{
	//서버 전송(1. 패킷 생성, 2. 전송)
	PACKLOGIN pack = PACKLOGIN::CreatePacket(id, pw);
	client.SendData((const char*)&pack, sizeof(pack));
}


void Control::LogoutMember(const char* id)
{
	//서버 전송(1. 패킷 생성, 2. 전송)
	PACKLOGOUT pack = PACKLOGOUT::CreatePacket(id);
	client.SendData((const char*)&pack, sizeof(pack));
}
