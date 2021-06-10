//control.h

#pragma once
#include "CMyClient.h"


class CMFCApplication1Dlg;	//Ŭ���� ����!(��������)

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
	CMFCApplication1Dlg* pForm = NULL;		//<============================

public:
	void RecvData(const char* msg, int size);
	void ParentForm(CMFCApplication1Dlg* pDlg);

	//Form -> Control ȣ���ϴ� �޼���
public:
	void Draw(CPoint p);

};

