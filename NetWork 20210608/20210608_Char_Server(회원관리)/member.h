#pragma once

//������ ������ ������ Ÿ��!
#include <string.h>

class Member
{
	bool islogin;
	char name[20];
	char id[10];
	char pw[10];

public:
	Member(const char* _name, const char* _id, const char* _pw);

	//get Method(�������Լ�), ��������� ���� �Ұ�
public : 
	bool getIsLogin() const { return islogin; }
	const char* getName() const { return name; }
	const char* getID() const { return id; }
	const char* getPW() const { return pw; }

	void setIsLogin(bool b) { islogin = b; }

};
