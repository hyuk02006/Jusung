//start.cpp

#include <stdio.h>
#include "Control.h"

int main()
{
	Control* con = Control::getInsatnce();
	con->Run();

	return 0;
}
