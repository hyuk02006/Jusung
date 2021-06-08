//chatcontrol.h
#pragma once
#include "CMyClient.h"


class CChatDlg;	//Ŭ���� ����!(��������)

class ChatControl
{
		//�̱��� ���� ���� ---------------------------------------------
private:
	ChatControl();
	static ChatControl* instance;
public:
	static ChatControl* getInstance()
	{
		if (instance == NULL)
		{
			instance = new ChatControl();
		}
		return instance;
	}
	//------------------------------------------------------------
	CMyClient client; 
	CChatDlg* pForm = NULL;		//<============================

public:
	void RecvData(const char* msg, int size);
	void ParentForm(CChatDlg* pDlg);

	//Form -> Control ȣ���ϴ� �޼���
public:
	void ShortMesaage(const char* name, const char* msg);
	void MemoMessage(const char* name, const char* msg);

};

