//#:전처리기(Pre-Processor) 
// 처리기 = 컴파일러
#include <stdio.h>
//헤더파일 안에서 한번만 포함시키는 것
#pragma once 

//정의정의정의(define)
#define MAX 30
//#define MY_ARRAY

//컴파일러 들어오기전에 블록을 나눠서 누굴 살릴지 (ifdef)
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

	printf("10진수:%d\n", a);
	printf("8진수:%#o\n", a);
	printf("16진수:%#x\n\n", a);

	double b = 89.567;
	printf("%f\n", b);
	printf("%F\n", b);
	printf("%e\n", b); //8.9567 * 10
	printf("%E\n\n", b);

	printf("a의 주소 : %p\n", &a);
	printf("main의 주소 : %p\n", &stdio_main);
	printf("printf의 주소 : %p\n\n", &printf);

	
	printf("enter a char: ");
	char ch = getchar();

	printf("결과: %c,%d\n",ch,ch);
	printf("대문자로변환: %c,%d\n", ch-32, ch-32);





	return 0;
}

#endif /////////////////////////////////////////////