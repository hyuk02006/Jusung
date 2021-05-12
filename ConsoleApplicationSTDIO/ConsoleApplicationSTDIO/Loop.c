#include <stdio.h>


int main(void)
{
	/*int a;

	int sum = 0;
	int sum1 = 0;
	int sum2 = 0;
	int sum3 = 0;
	int sum4 = 0;


	printf("숫자를 입력하세요: ");
	scanf_s("%d", &a);

	for (int i = 0; i <= a; i++)
	{
		sum += i;

	}

	for (int i = 0; i <= a; i++)
	{
		if (i % 2 == 0)
			sum1 += i;

	}


	for (int i = 0; i <= a ; i=i+2)
	{

		sum3 += i;

	}

	int i=0;
	while (i <= a)
	{

		sum4 += i;
		i = i + 2;
	}

	printf("결과는:%d\n", sum);
	printf("짝수의 합:%d\n", sum1);

	printf("짝수의 합:%d\n", sum3);
	printf("짝수의 합:%d\n", sum4);*/

	int a;

	printf("정수를 입력하세요:");
	scanf_s("%d", &a);
	for (int i = 1; i <= a; i++)
	{
		for (int j = 1; j <= i; j++)
		{
			printf("%d ", j);
		}
		printf("\n");
	}
	printf("\n");


	for (int i = 0; i < a; i++)
	{

		for (int j = i; j <= i - 1; j++)
		{
			printf(" ");
		}
		for (int k = 0; k <= (i * 2); k++)
		{
			printf("*");
		}
		printf("\n");
	}
	printf("\n");


	for (int i = 0; i < a; i++)
	{
		for (int j = 1; j < a - i; j++)
		{
			printf(" ");
		}
		for (int k = 0; k < i + 1; k++)
			printf("*");
		printf("\n");
	}

	printf("\n");





	for (int i = 0; i < a; i++)
	{
		for (int j = 0; j < a - i; j++)
		{
			printf("*");
		}

		printf("\n");
	}
	printf("\n");


	for (int i = 0; i < a; i++)
	{
		for (int j = 1; j < i - 1; j++)
		{
			printf("*");


		}
		for (int k = 0; k < a - 1; k++)
			printf(" ");
		printf("\n");
	}

	printf("\n");

	
	for (int i = 0; i < a; i++)
	{
		for (int j = i; j < 5; j++)
		{
			printf(" ");
		}
		for (int k = 0; k <= (i * 2); k++)
		{
			printf("*");
		}
		printf("\n");
	}
	for (int  i = a; i > 0; i--)
	{
		for (int  j = i; j <= a; j++)
		{
			printf(" ");
		}
		for (int  k = 0; k <= ((i - 1) * 2); k++)
		{
			printf("*");
		}
		printf("\n");
	}
	return 0;




	for (int i = 0; i <a; i++)
	{
		for (int j = 1; j <a-i; j++)
		{
			printf(" ");
		}
		for (int k = 0; k < i + 1; k++)
			printf("*");
		printf("\n");
	}

	printf("\n");





	for (int i = 0; i < a; i++)
	{
		for (int j = 0; j < a - i; j++)
		{
			printf("*");
		}
	
		printf("\n");
	}
	printf("\n");


	for (int i = 0; i < a; i++)
	{
		for (int j = 1; j <  i-1; j++)
		{
			printf("*");
		
		
		}
		for (int k = 0; k < a - 1; k++)
			printf(" ");
		printf("\n");
	}

	printf("\n");





	return 0;
}
