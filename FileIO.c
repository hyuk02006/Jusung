#include <stdio.h>
#pragma warning (disable:4996)
int main(void)
{
	char ch = 0;
	int age;
	char name[20];

	printf("���� �Է�:");

	scanf("%d", &age);
	//fflush(stdin);//�Է� ���۸� ����.
	getchar(); //���� ���� ����
	printf("�̸� �Է�:");
	gets(name);

	//scanf("%s", &name);

	printf("%d,%s\n", age, name);

	//1.Ű���� -> �����
	/*
	while (ch !=EOF) //ctrl+z (-1)
	{
		ch = getchar();//�б�
		//putchar(ch);//����
	}
	*/
	//2.Ű���� -> ����
	/*
	FILE* stream;
	fopen_s(&stream, "C:\\temp\\data.txt","wt");

	while (ch != EOF) //ctrl+z (-1)
	{
		//ch = getchar();//�б�
		ch = fgetc(stdin);

		putchar(ch);//����� ����
		fputc(ch, stream);//���� ����
	}
	fclose(stream);
	*/

	//3.���� -> ����
	/*
	FILE* stream1;
	FILE* stream2;
	int input = 0;

	fopen_s(&stream1, "C:\\temp\\data.txt", "r");
	fopen_s(&stream2, "C:\\temp\\data2.txt", "w");

	while (input != EOF) //ctrl+z (-1)
	{
		input = fgetc(stream1); //1�� ���Ͽ��� �б�
		fputc(input, stream2);  //2�� ���Ͽ� ����
		fputc(input, stdout);	//����Ϳ� ����
	}

	fclose(stream1);
	fclose(stream2);
	*/

	//4.Ű����(���ڿ�->����)
	/*
	FILE* stream;
	char buffer[50];
	fopen_s(&stream, "C:\\temp\\data3.txt", "wt");


	fgets(buffer,sizeof(buffer),stdin);//Ű����� ����
	fputs(buffer, stream);

	fclose(stream);
	*/

	//5.Ű����(�����Է�) ->����
	/*
	FILE* stream;
	char name[20];
	int kor, eng, math, total;
	double avg;
	int answer = 0;
	fopen_s(&stream, "C:\\temp\\data4.txt", "w");
	
	while (1)
	{
	
		printf("�̸��Է�:");
		//scanf_s("%s", &name, 20);
		getchar(); //���ʿ��� enther����
		gets(name);
		
		//puts(name);
		printf("\n");
		printf("��������,��������,�������� �Է�:");
		fscanf_s(stdin, "%d %d %d", &kor, &eng, &math);
		
		total = kor + eng + math;
		avg = total / 3;
		fprintf(stdout, "�̸�:%s ����:%d ����:%d ����:%d ����:%d ���:%f\n", name, kor, eng, math, total, avg);
	
		fprintf(stream, "�̸�:%s ����:%d ����:%d ����:%d ����:%d ���:%f\n", name, kor, eng, math, total, avg);

		puts("����Ͻðڽ��ϱ�?(1:yes,0:no)");
		scanf_s("%d", &answer);
		if (answer == 0)
			break;

		printf("\n");
	}
	fclose(stream);
	*/


	return 0;

}








