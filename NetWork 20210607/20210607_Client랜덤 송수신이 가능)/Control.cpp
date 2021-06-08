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
	printf(">> [���ŵ�����] %dbyte\n", size);
	int* p = (int*)msg;

	if (*p == ACK_NEWMEMBER_S)
	{
		printf(">>ȸ������ ����\n");
	}
	else if (*p == ACK_NEWMEMBER_F)
	{
		printf(">>ȸ������ ����\n");
	}
	else if (*p == ACK_LOGIN_S)
	{
		printf(">>�α��� ����\n");
	}
	else if (*p == ACK_LOGIN_F)
	{
		printf(">>�α��� ����\n");
	}
	else if (*p == ACK_LOGOUT_S)
	{
		printf(">>�α׾ƿ� ����\n");
	}
	else if (*p == ACK_LOGOUT_F)
	{
		printf(">>�α׾ƿ� ����\n");
	}
}

void Control::InsertMember()
{
	char name[20], id[10], pw[10];
	int age;
	printf("�̸� : ");		gets_s(name, sizeof(name));
	printf("���̵� : ");		gets_s(id, sizeof(id));
	printf("�н����� : ");	gets_s(pw, sizeof(pw));
	printf("���� : ");		scanf_s("%d", &age);
	char dummy =getchar();

	//���� ���� (1. ��Ŷ ���� , 2. ����)
	NEWMEMBER pack =pack_Newmember(name, id, pw, age);
	client.SendData((const char*)&pack, sizeof(pack));

}

void Control::Login()
{
	
	char id[10], pw[10];
	printf("���̵� :");		gets_s(id, sizeof(id));
	printf("�н����� :");	gets_s(pw, sizeof(pw));
	
	//���� ����
	LOGIN pack = pack_Login(id, pw);
	client.SendData((const char*)&pack, sizeof(pack));
}

void Control::Logout()
{
	char id[10];
	printf("���̵�: "); gets_s(id, sizeof(id));

	//���� ����
	LOGIN pack = pack_Logout(id);
	client.SendData((const char*)&pack, sizeof(pack));
}
