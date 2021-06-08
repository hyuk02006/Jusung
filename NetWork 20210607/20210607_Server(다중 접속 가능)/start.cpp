//start.cpp

#include <stdio.h>
#include "CMyNet.h"

int main()
{
	try
	{
		CMyNet net;
		net.CreateSocket(9000);
	}
	catch (const char* msg)
	{
		printf("¿¡·¯ : %s\n", msg);
	}

	return 0;
}
