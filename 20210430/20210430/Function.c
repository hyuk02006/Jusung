#include<stdio.h>

int Add(int, int);
int Sub(int, int);

int z; //��������


void count()
{
	static int ct = 0;
	ct++;
	printf("�Ҹ� Ƚ��: %d\n", ct);
	
}
int facto(int x)
{	
	int sum = 1;
	for (int i = 1; i <=x; i++)
	{
		sum = sum *i;
	}

	printf("%d!= %d\n", x,sum);
	return sum;

}
void self_service()
{
	printf("self_service\n");
	//self_service();
}
int facto2(int n)
{
	if (n <= 1)//1!
	{
		printf("facto2: %d ����\n", n);
		return 1;
	}
	else
	{
		printf("facto2: %d * �Լ�ȣ��%d!\n",n,n-1 );
		return n * facto2(n - 1);
	}
}
void swap(int x, int y)//call by value
{

	printf("\tswap ȣ���� :%d,%d\n", x, y);

	static int temp;
	temp = x;
	x = y;
	y = temp;

	printf("\tswap ȣ���� :%d,%d\n", x, y);
	
}
void swap2(int* x, int* y)//call by address
{

	printf("\tswap ȣ���� :%d,%d\n", *x, *y);

	int temp;
	temp = *x;
	*x = *y;
	*y = temp;

	printf("\tswap ȣ���� :%d,%d\n", *x, *y);

}
//call by reference c++����!
/*
void swap3(int& x, int& y)//call by reference c++����!
{

	printf("\tswap ȣ���� :%d,%d\n", x, y);

	int temp;
	temp = x;
	x =y;
	y = temp;

	printf("\tswap ȣ���� :%d,%d\n", x, y);

}
*/
int main()
{
	int aaa = 10;
	int bbb = 20;

	printf("swap ȣ���� :%d,%d\n", aaa, bbb);
	swap(aaa, bbb);
	swap2(&aaa, &bbb);

	printf("swap ȣ���� :%d,%d\n", aaa, bbb);

	return 0;
	int (*p1)(int, int) =NULL;
	p1 = Add;
	int result1 =p1(10, 60);
	printf("p1�ǰ��: %d\n", result1);

	p1 = Sub;
	int result2 = p1(10, 60);
	printf("p1�ǰ��: %d\n", result2);

	p1 = facto2;
	int result3 = p1(10, 60);
	printf("p1�ǰ��: %d\n",result3);


	p1 = self_service;
	//p1();// error..


	void(*p2)(void) = NULL;
	p2 = self_service;
	p2();


	/*
	int y = facto(5);
	printf("facto���:%d\n", y);

	self_service();
	int k=facto2(5);
	printf("facto2���:%d\n", k);

	int a = 10;
	int b = 20;
	int c = a + b;

	int result = Add(2, 3);
	int result1 = Sub(b, a);

	printf("%d %d\n", result, result1);
	*/
	return 0;
}


int Add(int x, int y)
{
	int sum = x + y;
	return sum;

}

int Sub(int x, int y)
{
	int sum = x - y;
	return sum;
}
