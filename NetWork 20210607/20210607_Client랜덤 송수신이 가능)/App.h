//app.h
#pragma once
#include "Control.h"

//�帧
class App
{
public:
	void Init();	//Logo ȣ��
	void Run();		//Menu ȣ��
	void Exit();	//ending ȣ��

private:
	void Logo();
	char MenuPrint();
	void Ending();
};

