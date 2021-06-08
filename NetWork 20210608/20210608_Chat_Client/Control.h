//control.h

#pragma once
#include "CMyClient.h"


class CMy20210608ChatClientDlg;	//클래스 선언!(전방참조)

class Control
{
	//싱글톤 패턴 적용 ---------------------------------------------
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

	//Form -> Control 호출하는 메서드
public:
	void InsertMember(const char* id, const char* pw, const char* name);
	void LoginMember(const char* id, const char* pw);
	void LogoutMember(const char* id);

};

