//#:��ó����(Pre-Processor) 
// ó���� = �����Ϸ�
#include <stdio.h>
//������� �ȿ��� �ѹ��� ���Խ�Ű�� ��
#pragma once 

//������������(define)
#define MAX 30
//#define MY_ARRAY

//�����Ϸ� ���������� ����� ������ ���� �츱�� (ifdef)
#ifdef MY_ARRAY //////////////////////////////////////

int main(void)
{
	printf("Hello, World! \n");
	return 0;
}

#else ///////////////////////////////////////////////


int stdio_main(void)
{
	printf("Hello, World!\n\n");
	printf("Hello, World!\n\n");

	int a = 255;

	printf("10����:%d\n", a);
	printf("8����:%#o\n", a);
	printf("16����:%#x\n\n", a);

	double b = 89.567;
	printf("%f\n", b);
	printf("%F\n", b);
	printf("%e\n", b); //8.9567 * 10
	printf("%E\n\n", b);

	printf("a�� �ּ� : %p\n", &a);
	printf("main�� �ּ� : %p\n", &stdio_main);
	printf("printf�� �ּ� : %p\n\n", &printf);

	
	printf("enter a char: ");
	char ch = getchar();

	printf("���: %c,%d\n",ch,ch);
	printf("�빮�ڷκ�ȯ: %c,%d\n", ch-32, ch-32);





	return 0;
}

#endif /////////////////////////////////////////////