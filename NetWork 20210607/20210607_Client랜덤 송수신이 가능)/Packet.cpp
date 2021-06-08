//packet.cpp
#pragma pack(1) //구조체의 크기를 1byte 씩 쌓아올림
#include <stdio.h>
#include <string.h>
#include "packet.h"

NEWMEMBER pack_Newmember(const char* name, const char* id, const char* pw, int age)
{
	NEWMEMBER packet;
	packet.flag = PACK_NEWMEMBER;
	strcpy_s(packet.name, sizeof(packet.name), name);
	strcpy_s(packet.id, sizeof(packet.id), id);
	strcpy_s(packet.pw, sizeof(packet.pw), pw);
	packet.age = age;

	return packet;
}

LOGIN pack_Login(const char* id, const char* pw)
{
	LOGIN packet = { 0, };
	packet.flag = PACK_LOGIN;
	strcpy_s(packet.id, sizeof(packet.id), id);
	strcpy_s(packet.pw, sizeof(packet.pw), pw);

	return packet;
}

LOGIN pack_Logout(const char* id)
{
	LOGIN packet = { 0, };
	packet.flag = PACK_LOGOUT;
	strcpy_s(packet.id, sizeof(packet.id), id);

	return packet;
}
