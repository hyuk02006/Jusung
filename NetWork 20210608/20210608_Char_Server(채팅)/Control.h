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


public:
	void Run();
	void RecvData(int sock, const char* msg, int size);


};

