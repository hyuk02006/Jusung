//control.h
#pragma once
#include "CMyClient.h"

class Control
{

	//��Ŭ�� ���� ����-----------------------------------------------------------------------
private:
	Control();
	static Control* instance;
public:
	static Control* getInstance()
	{
		if (instance == NULL)
		{
			instance = new Control();
		}
		return instance;
	}
	//--------------------------------------------------------------------------------------

	CMyClient client;
public:
	void RecvData(const char* msg, int size);

public:
	void InsertMember();
	void Login();
	void Logout();
};

