//Start.cpp
#include <stdio.h>
#include "CMyClient.h"
int main()
{
	CMyClient Client;
	Client.CreateSocket("192.168.0.93", 9000);

	while (true)
	{
		//����ڿ��� ���ڿ��� �Է�!
		char buf[256] ="\0";
		printf(">>");	gets_s(buf, sizeof(buf));
		if (strlen(buf) == 0)
			break;

		Client.SendData(buf, strlen(buf));
	}
	return 0;
}