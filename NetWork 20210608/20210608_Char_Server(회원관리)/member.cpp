//member.cpp

#include<stdio.h>
#include"member.h"

Member:: Member(const char* _name, const char* _id, const char* _pw)
{
	islogin = false;
	strcpy_s(name, sizeof(name), _name);
	strcpy_s(id, sizeof(id), _id);
	strcpy_s(pw, sizeof(pw), _pw);
}