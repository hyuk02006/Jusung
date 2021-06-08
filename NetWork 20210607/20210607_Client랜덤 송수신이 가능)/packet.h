//packet.h
/*
* App �������� ����
* 1) ��Ŷ ���� define
* 2) ��Ŷ�� ���� ����ü ���� (ù��° �ɹ��� flag����)
*/
#pragma once

#define PACK_NEWMEMBER 1
#define PACK_LOGIN 2
#define PACK_LOGOUT 3


#define ACK_NEWMEMBER_S 10
#define ACK_NEWMEMBER_F 11
#define ACK_LOGIN_S		12
#define ACK_LOGIN_F		13
#define ACK_LOGOUT_S		14
#define ACK_LOGOUT_F		15

typedef struct tagNewMember
{
	int flag;
	char name[20];
	char id[10];
	char pw[10];
	int age;
}NEWMEMBER;

typedef struct tagLogin
{
	int flag;
	char name[20];
	char id[10];
	char pw[10];
	int age;

}LOGIN;

NEWMEMBER pack_Newmember(const char* name, const char* id, const char* pw, int age);
LOGIN pack_Login(const char* id, const char* pw);
LOGIN pack_Logout(const char* id);

