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
	printf(">> [���ŵ�����] %dbyte\n", size);
	int* p = (int*)msg;
	
	if (*p == ACK_NEWPMEMBER_S)
	{
		MessageBox(0, "�˸�", ">> ȸ������ ����", MB_OK);
	}
	else if (*p == ACK_NEWPMEMBER_F)
	{
		MessageBox(0, "�˸�", ">> ȸ������ ����", MB_OK);
	}
	else if (*p == ACK_LOGIN_S)
	{
		PACKLOGIN* pLogin = (PACKLOGIN*)msg;

		//��޷� ä��ȭ�� ����
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
		MessageBox(0, "�˸�", ">> �α��� ����", MB_OK);
	}
	else if (*p == ACK_LOGOUT_S)
	{
		MessageBox(0, "�˸�", ">> �α׾ƿ� ����", MB_OK);
	}
	else if (*p == ACK_LOGOUT_F)
	{
		MessageBox(0, "�˸�", ">> �α׾ƿ� ����", MB_OK);
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
	//���� ����(1. ��Ŷ ����, 2. ����)
	PACKETNEWMEMBER pack = PACKETNEWMEMBER::CreatePacket(id, pw, name);
	client.SendData((const char*)&pack, sizeof(pack));
}


void Control::LoginMember(const char* id, const char* pw)
{
	//���� ����(1. ��Ŷ ����, 2. ����)
	PACKLOGIN pack = PACKLOGIN::CreatePacket(id, pw);
	client.SendData((const char*)&pack, sizeof(pack));
}


void Control::LogoutMember(const char* id)
{
	//���� ����(1. ��Ŷ ����, 2. ����)
	PACKLOGOUT pack = PACKLOGOUT::CreatePacket(id);
	client.SendData((const char*)&pack, sizeof(pack));
}
