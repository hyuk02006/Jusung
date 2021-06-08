//control.cpp
#include "Control.h"
#include <stdio.h>
#include "Packet.h"

Control* Control::instance = NULL;

Control::Control()
{
	client.CreateSocket("192.168.0.95", 9000);
}

void Control::RecvData(const char* msg, int size)
{
	printf(">> [수신데이터] %dbyte\n", size);
	int* p = (int*)msg;

	if (*p == ACK_NEWMEMBER_S)
	{
		printf(">>회원가입 성공\n");
	}
	else if (*p == ACK_NEWMEMBER_F)
	{
		printf(">>회원가입 실패\n");
	}
	else if (*p == ACK_LOGIN_S)
	{
		printf(">>로그인 성공\n");
	}
	else if (*p == ACK_LOGIN_F)
	{
		printf(">>로그인 실패\n");
	}
	else if (*p == ACK_LOGOUT_S)
	{
		printf(">>로그아웃 성공\n");
	}
	else if (*p == ACK_LOGOUT_F)
	{
		printf(">>로그아웃 실패\n");
	}
}

void Control::InsertMember()
{
	char name[20], id[10], pw[10];
	int age;
	printf("이름 : ");		gets_s(name, sizeof(name));
	printf("아이디 : ");		gets_s(id, sizeof(id));
	printf("패스워드 : ");	gets_s(pw, sizeof(pw));
	printf("나이 : ");		scanf_s("%d", &age);
	char dummy =getchar();

	//서버 전송 (1. 패킷 생성 , 2. 전송)
	NEWMEMBER pack =pack_Newmember(name, id, pw, age);
	client.SendData((const char*)&pack, sizeof(pack));

}

void Control::Login()
{
	
	char id[10], pw[10];
	printf("아이디 :");		gets_s(id, sizeof(id));
	printf("패스워드 :");	gets_s(pw, sizeof(pw));
	
	//서버 전송
	LOGIN pack = pack_Login(id, pw);
	client.SendData((const char*)&pack, sizeof(pack));
}

void Control::Logout()
{
	char id[10];
	printf("아이디: "); gets_s(id, sizeof(id));

	//서버 전송
	LOGIN pack = pack_Logout(id);
	client.SendData((const char*)&pack, sizeof(pack));
}
