//control.h

#pragma once
#include "CMyClient.h"


class CMFCApplication1Dlg;	//클래스 선언!(전방참조)

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
	CMFCApplication1Dlg* pForm = NULL;		//<============================

public:
	void RecvData(const char* msg, int size);
	void ParentForm(CMFCApplication1Dlg* pDlg);

	//Form -> Control 호출하는 메서드
public:
	void Draw(CPoint p);

};

