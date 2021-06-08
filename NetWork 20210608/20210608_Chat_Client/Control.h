//control.h

#pragma once
#include "CMyClient.h"


class CMy20210608ChatClientDlg;	//Ŭ���� ����!(��������)

class Control
{
	//�̱��� ���� ���� ---------------------------------------------
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
	//------------------------------------------------------------
	CMyClient client; 
	CMy20210608ChatClientDlg* pForm = NULL;		//<============================

public:
	void RecvData(const char* msg, int size);
	void ParentForm(CMy20210608ChatClientDlg* pDlg);

	//Form -> Control ȣ���ϴ� �޼���
public:
	void InsertMember(const char* id, const char* pw, const char* name);
	void LoginMember(const char* id, const char* pw);
	void LogoutMember(const char* id);

};

