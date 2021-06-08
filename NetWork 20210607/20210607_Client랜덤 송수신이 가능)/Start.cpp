//Start.cpp
#include <stdio.h>
#include "App.h"

int main()
{

	App app;
	app.Init();
	app.Run();
	app.Exit();

	/*
	CMyClient client;
	client.CreateSocket("192.168.0.93", 9000);

	while (true)
	{
		//사용자에게 문자열을 입력!
		char buf[256] = "\0";
		printf(">> ");		gets_s(buf, sizeof(buf));
		if (strlen(buf) == 0)
			break;

		client.SendData(buf, strlen(buf));
	}
	*/

	return 0;
}