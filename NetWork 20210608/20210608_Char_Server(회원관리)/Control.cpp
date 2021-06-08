//Control.cpp

#include <stdio.h>
#include "packet.h"
#include "Control.h"
#include "member.h"
Control* Control::instance = NULL;

void Control::Run()
{
	try
	{
		net.CreateSocket(9000);
	}
	catch (const char* msg)
	{
		printf("���� : %s\n", msg);
	}
}

void Control::RecvData(int sock, const char* msg, int size)
{
	printf(">> [���ŵ�����] %dbyte\n", size);
	int* p = (int*)msg;
	if (*p == PACK_NEWMEMBER)
	{
		printf("ȸ������\n");
		PACKETNEWMEMBER* member = (PACKETNEWMEMBER*)msg;
		if (NewMember(member) == true)
		{
			member->flag = ACK_NEWPMEMBER_S;
		}
		else
		{
			member->flag = ACK_NEWPMEMBER_F;
		}
		net.SendData(sock, msg, size);	//<=======����!!!!!!!!!!
	}
	else if (*p == PACK_LOGIN)
	{
		printf("�α���\n");
		PACKLOGIN* login = (PACKLOGIN*)msg;
		if (LoginMember(login) == true)
		{
			login->flag = ACK_LOGIN_S;
		}
		else
		{
			login->flag = ACK_LOGIN_F;
		}
		net.SendData(sock, msg, size);	//<=======����!!!!!!!!!!
	}
	else if (*p == PACK_LOGOUT)
	{
		printf("�α׾ƿ�\n");
		PACKLOGOUT* logout = (PACKLOGOUT*)msg;
		if (LogoutMember(logout) == true)
		{
			logout->flag = ACK_LOGOUT_S;
		}
		else
		{
			logout->flag = ACK_LOGOUT_F;
		}
		net.SendData(sock, msg, size);	//<=======����!!!!!!!!!!
	}
}

bool Control::NewMember(PACKETNEWMEMBER* pMem)
{
	Member member(pMem->name, pMem->id, pMem->pw);

	personlist.push_back(member);
	PrintMemberData();
	return true;
}

bool Control::LoginMember(PACKLOGIN* pLogin)
{
	for (int i = 0; i < (int)personlist.size(); i++)
	{
		Member mem = personlist[i];
		if (strcmp(mem.getID(), pLogin->id) == 0 &&
			strcmp(mem.getPW(), pLogin->pw) == 0 && mem.getIsLogin() == false)
		{
			//plogin�� ������ ���� ä���
			strcpy_s(pLogin->name, sizeof(pLogin->name), mem.getID());

			//vetor�� �α��� ���°� ����
			personlist[i].setIsLogin(true);
			PrintMemberData();		//<=======================����Ȯ��
			return true;
		}
	}
	return false;
}
bool Control::LogoutMember(PACKLOGIN* pLogout)
{
	for (int i = 0; i < (int)personlist.size(); i++)
	{
		Member mem = personlist[i];
		if (strcmp(mem.getID(), pLogout->id) == 0 && mem.getIsLogin() ==true)
		{
			//vetor�� �α��� ���°� ����
			personlist[i].setIsLogin(false);
			PrintMemberData();		//<=======================����Ȯ��
			return true;
		}
	}
	return false;
}



void Control::PrintMemberData()
{
	system("cls");

	printf("ȸ���� : %d\n", personlist.size());
	for (int i = 0; i < (int)personlist.size(); i++)
	{
		Member member = personlist[i];
		printf("[%d] %s\t%s\t%s\t%c\n",
			i, member.getName(), member.getID(), member.getPW(), member.getIsLogin() ? '0' : 'X');
	}
}