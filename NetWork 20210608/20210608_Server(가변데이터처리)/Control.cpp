//Control.cpp

#include <stdio.h>
#include "packet.h"
#include "Control.h"

Control* Control::instance = NULL;

void Control::Run()
{
	try
	{
		net.CreateSocket(9000);
	}
	catch (const char* msg)
	{
		printf("에러 : %s\n", msg);
	}
}

void Control::RecvData(int sock, const char* msg, int size)
{
	printf(">> [수신데이터] %dbyte\n", size);
	int* p = (int*)msg;
	if (*p == PACK_PERSON)
	{
		printf("회원가입\n");
		PACKETPERSON* person = (PACKETPERSON*)msg;
		if (NewMember(person) == true)
		{
			person->flag = ACK_PERSON_S;
		}
		else
		{
			person->flag = ACK_PERSON_F;
		}
		net.SendData(sock, msg, size);	//<=======전송!!!!!!!!!!
	}
	else if (*p == PACK_LISTALL)
	{
		printf("회원리스트요청\n");
		PACKLISTALL* persons = (PACKLISTALL*)msg;
		ListAll(persons);
		persons->flag = ACK_LISTALL;
		net.SendData(sock, msg, size);	//<=======전송!!!!!!!!!!
	}
}

bool Control::NewMember(PACKETPERSON* pMem)
{
	Person person;
	strcpy_s(person.name, sizeof(person.name), pMem->per[0].name);	
	person.age = pMem->per[0].age;
	person.gender = pMem->per[0].gender;

	personlist.push_back(person);
	PrintMemberData();
	return true;
}

void Control::ListAll(PACKLISTALL* persons)
{
	Person person;
	persons->count = personlist.size();
	for (int i = 0; i < (int)personlist.size(); i++)
	{
		Person person = personlist[i];
		persons->per[i] = person;
	}
}

void Control::PrintMemberData()
{
	system("cls");

	printf("회원수 : %d\n", personlist.size());
	for (int i = 0; i < (int)personlist.size(); i++)
	{
		Person person = personlist[i];
		printf("[%d] %s\t%d\t%s\n",
			i, person.name, person.age, (person.gender?"남":"여"));
	}
}