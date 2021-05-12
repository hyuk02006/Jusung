#include<stdio.h>
#pragma warning (disable:4996)

int main(void)
{
	int t, r1;

	char a[20];

	scanf("%d", &t);

	for (int i = 1; i <= t; i++)
	{
		scanf("%d", &r1);
		scanf("%s", &a);

		for (int j = 0; j <strlen(a); j++)
		{
			for (int k = 0; k < r1; k++) 
			{
				if (a[j] != '\n')
					printf("%c", a[j]);
			}
		}
		printf("\n");
	}


	return 0;

}