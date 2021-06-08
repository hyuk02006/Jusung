//control.h
/*
* ����������(Gof) : � ������ �ذ��ϱ� ���� Ŭ������ ��� ������ ���ΰ�?
*                  �𵨿� ���� �̾߱�.
* ��) �̱�������
*     �ϳ��� ��ü�� ���������� Ŭ���� ����.
*/
#pragma once
#include <vector>
using namespace std;
#include "packet.h"
#include "CMyNet.h"
#include "member.h"

class Control
{
	//�̱��� ���� ���� ---------------------------------------------
private:
	Control() {}
	static Control* instance;
public:
	static Control* getInsatnce() 
	{
		if (instance == NULL)
		{
			instance = new Control();			
		}
		return instance;  
	}
	//------------------------------------------------------------
private:
	CMyNet net;
	vector<Member> personlist;

public:
	void Run();
	void RecvData(int sock, const char* msg, int size);

	//����Լ�...
private:
	bool NewMember(PACKETNEWMEMBER* pMem);
	bool LoginMember(PACKLOGIN* pLogin);
	bool LogoutMember(PACKLOGIN* pLogin);

	void PrintMemberData();
};

