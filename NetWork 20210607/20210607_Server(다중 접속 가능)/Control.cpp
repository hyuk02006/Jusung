//control.cpp
#include <stdio.h>
#include "Control.h"
#include"Packet.h"

Control* Control::instance = NULL;

void Control::RecvData(const char* msg,int size)
{
	printf(">> [수신데이터] %dbyte\n", size);
	int* p = (int*)msg;

	if (*p == PACK_NEWMEMBER)
	{
		printf("회원가입\n");
		if (NewMember((NEWMEMBER*)msg) == true)
		{
			NEWMEMBER* pmem = (NEWMEMBER*)msg;
			pmem->flag = ACK_NEWMEMBER_S;
		}
		else
		{
			NEWMEMBER* pmem = (NEWMEMBER*)msg;
			pmem->flag = ACK_NEWMEMBER_F;
		}
	}
	if (*p == PACK_LOGIN)
	{
		printf("로그인\n");
		
		if (Login((LOGIN*)msg) == true)
		{
			LOGIN* pLogin = (LOGIN*)msg;
			pLogin->flag = ACK_LOGIN_S;

		}
		else
		{
			LOGIN* pLogin = (LOGIN*)msg;
			pLogin->flag = ACK_LOGIN_F;

		}
	}
}


bool Control::NewMember(NEWMEMBER* pMem)
{
	NEWMEMBER mem;
	mem.flag = 0;	//login여부의 값으로 활용
	strcpy_s(mem.name, sizeof(mem.name), pMem->name);
	strcpy_s(mem.id, sizeof(mem.id), pMem->id);
	strcpy_s(mem.pw, sizeof(mem.pw), pMem->pw);
	mem.age = pMem->age;

	memberlist.push_back(mem);
	return true;
}

bool Control::Login(LOGIN* pLogin)
{
	for (int i = 0; i < (int)memberlist.size(); i++)
	{
		NEWMEMBER member = memberlist[i];
		if (strcmp(member.id, pLogin->id) == 0 && strcmp(member.pw, pLogin->id) == 0)
		{
			//내부 정보 수정
			memberlist[i].flag = 1;
			return true;
		}
	}
	return false;
}