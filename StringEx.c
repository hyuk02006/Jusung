#include <stdio.h>

int main(void)
{
	//int arr[3] = { 1,2,3 };
	//char latter[3] = { 'A','B','C' };

	//char latter2[5] = "ABC"; //'\n'
	//char string[] = "I LOVE YOU!";

	////���ڿ� ���� ���
	//int count = 0;
	//int i=0;
	//while (string[i++] > '\n')
	//	count++;

	//printf("���ڿ�����: %d\n", count);

	//printf("strlen ����: %d\n", strlen(string));

	//count = sizeof(string);
	//printf("sizeof ����: %d\n", count);

	//char temp;
	//int count1=0;

	//for (i = 0; i<string[i] ; i++)
	//	count1++;
	//printf("���ڿ�����: %d\n", count1);

	//for (i = 0; i < count1 / 2; i++)
	//{
	//	temp = string[i];
	//	string[i] = string[count1 - i - 1];
	//	string[count1 - i - 1] = temp;
	//}
	//printf("���ڿ�������: %s\n", string);

	/////////////////////////////////////////////////
	/*
	//2.�� ���ڿ��� ���ļ� ���	
	char str1[100] = "I love you.";
	char str2[100] = "Do you love me.";

//	strcat(str1, str2);		//���ڿ� ���� �ڵ�
	strncat(str1, str2,10);
	printf("1.%s\n", str1);
	printf("2.%s\n", str2);
	*/


	//����� �ڵ�
	/*
	int len1 = strlen(str1);
	int len2 = strlen(str2);

	for (int i = 0; i < len2; i++)
	{
		str1[len1 + i] = str2[i];
	}
	printf("%s\n", str1);
	printf("%s\n", str2);
	*/

	/*
	char temp;
	int i, j;
	int count = 0;
	int count1 = 0;

	for (i = 0; i < str1[i]; i++)
		count++;
		for (j = 0; i < str2[j]; j++)
			count1++;

	for (i = 0; i < count; i++)
		str1[count + i] = str2[i];
		

	printf("3.%s\n", str1);
	printf("4.%s\n", str2);
	*/

	////////////////////////////////
	
	//���ڵ�(��������)
	/*
	//3.���� ����
	char str1[100] = "I love you.";
	char str2[100] = { 0, };
	int len1 = strlen(str1);

	//�� �ڵ�
	//for (int i = 0; i < len1; i++)
	//{
	//	if (str1[i] == ' ')
	//	{
	//		str1[i] = str1[i+1];
	//		//printf("%s\n", str1);
	//	
	//	}
	//	for (int j = 0; j < len1; j++)
	//	{
	//		if (str1[j] == ' ')
	//		{
	//			str1[j] = str1[j + 1];
	//			str1[j + 1] = ' ';
	//			
	//		}
	//	}
	//
	//}


	int j = 0;
	for (int i = 0; i < len1; i++)
	{
		if (str1[i] != ' ')
		{
			str1[j] = str1[i];
			j++;
		}
	}
	//str1[j] = '\n';
	str1[j] = '\0';
	*/

	

	//����� �ڵ�
	/*
	for (int i = 0; i < len1; i++) 
	{
		char t = str1[i];
		if (t != ' ')
		{
			str2[j] = t;
			j++;
		}

	}
	str2[j] = '\0'; // ������ ���� ������� 
	printf("%s\n", str1); //Iloveyou.
	printf("%s\n", str2); //Iloveyou.
	*/


	//���ڿ� ������
	/*
	char string[] = "I love you!";
	char string2[20] = { 0,};
	int i;
	int len1 = strlen(string);


	for ( i = 0; i < len1 ; i++)
	{
	
		 string2[i]= string[len1 - i-1] ;
		
	}
	
	string[i] = '\0'; // ������ ���� ������� (���Ṯ��)
	printf("%s\n", string2);
	*/


	//��ҹ��� ��ȯ
	char str[] = "I am a Boy";
	int len1 = strlen(str);
	
	
	for (int i = 0; i < len1; i++)
	{
		if (str[i]>='A' &&str[i] <='Z')
		{
			str[i] = str[i]+32;
		}
		else if(str[i] >= 'a' && str[i] <= 'z')
			str[i] = str[i]-32;
	
	}

	printf("%s\n", str);
	

	return 0;
}