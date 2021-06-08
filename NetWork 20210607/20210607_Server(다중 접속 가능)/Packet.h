//packet.h
#pragma once
/*
* App �������� ����
* 1) ��Ŷ ���� define
* 2) ��Ŷ�� ���� ����ü ���� (ù��° �ɹ��� flag����)
*/
#pragma once

#define PACK_NEWMEMBER 1
#define PACK_LOGIN 2

#define ACK_NEWMEMBER_S 10
#define ACK_NEWMEMBER_F 11
#define ACK_LOGIN_S		12
#define ACK_LOGIN_F		13

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
