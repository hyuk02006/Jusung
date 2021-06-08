//control.h
/*
* 디자인패턴(Gof) : 어떤 문제를 해결하기 위해 클래스를 어떻게 설계할 것인가?
*                  모델에 관한 이야기.
* 예) 싱글톤패턴
*     하나의 객체만 생성가능한 클래스 구조.
*/
#pragma once
#include <vector>
using namespace std;
#include "packet.h"
#include "CMyNet.h"


class Control
{
	//싱글톤 패턴 적용 ---------------------------------------------
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

