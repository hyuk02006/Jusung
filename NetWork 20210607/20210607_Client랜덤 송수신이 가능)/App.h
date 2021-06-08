//app.h
#pragma once
#include "Control.h"

//흐름
class App
{
public:
	void Init();	//Logo 호출
	void Run();		//Menu 호출
	void Exit();	//ending 호출

private:
	void Logo();
	char MenuPrint();
	void Ending();
};

