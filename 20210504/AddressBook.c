#include <stdio.h>
#include<string.h>
#pragma warning (disable:4996)
#define DATA_FILE_NAME "c:\\temp\\addr.dat"

typedef struct _USERDATA
{
	char strName[32];	  // �̸�
	int nAge;			  // ����
	int nGender;		  // ����
} USERDATA;

//�迭�� ó��
#define		MAX_BOOKCOUNT	10
int		g_nCount = 0;
USERDATA	g_AddrBook[MAX_BOOKCOUNT];



int PrintUI()
{
	int nInput = 0;

	system("cls");
	printf("===================================================\n");
	printf("��ȭ��ȣ��(Array)\n");
	printf("---------------------------------------------------\n");
	printf("[1] �߰�\t [2] �˻�\t [3] ��ü����\t [4] ����\t [0] ����\n");
	printf("===================================================\n");

	// ����ڰ� ������ �޴��� ���� ��ȯ�Ѵ�.
	scanf_s("%d", &nInput);
	getchar();//���ۿ� ���� ���� ���ſ�
	return nInput;
}

void Add()
{
	if (g_nCount > MAX_BOOKCOUNT)
	{
		printf("���� ���� ����\n");
		getch();
		return;
	}

	USERDATA ud;

	printf("�̸��� �Է��ϼ���:");
	gets(&ud.strName);

	printf("���̸� �Է��ϼ���:");
	scanf_s("%d", &ud.nAge);

	printf("������ �Է��ϼ���<����:0,����:1:>");
	scanf_s("%d", &ud.nGender);
	
	
	strcpy(g_AddrBook[g_nCount].strName, ud.strName);
	g_AddrBook[g_nCount].nGender = ud.nGender;
	g_AddrBook[g_nCount].nAge = ud.nAge;


	g_nCount++;
}

void Search()
{
	int temp = 0;
	char name[20];
	printf("�˻��� �̸��� �Է����ּ���:");
	gets(&name);//1���� �Է�->���ڿ�

	for (int i = 0; i < g_nCount; i++)
	{
		temp = strcmp(name, g_AddrBook[i].strName);//0, 1, -1
		if (temp == 0)//strcmp, strcpy
		{
			printf("������� ������ Ȯ���Ͽ����ϴ�. \n �̸� : %s\t���� : %d\t���� : %s\n ",
				g_AddrBook[i].strName,
				g_AddrBook[i].nAge,
				g_AddrBook[i].nGender == 0 ? "����" : "����");
			break;
		}
	}
	if (temp != 0)
		printf("����� �̸��� �����ϴ�\n");

	getch();

}
void PrintAll()
{
	if (g_nCount == 0)
	{
		printf("����� �����ϴ�.\n");
		getch();

		return;
	}

	for (int i = 0; i < g_nCount; i++)
	{
		printf("�̸�: %s\t ����:%d  ����:%s\n",
			g_AddrBook[i].strName,
			g_AddrBook[i].nAge,
			g_AddrBook[i].nGender == 0 ? "����" : "����");
	}
	getch();
}

void Delete()
{
	char name[15];
	int temp = 0;
	int index = 0;

	if (g_nCount == 0)
	{
		printf("����� �����ϴ�.\n");
		getch();
		return;
	}

	printf("������ �̸��� �Է��ϼ���:");
	gets(name);

	for (int i = 0; i < g_nCount; i++)
	{
		temp = strcmp(g_AddrBook[i].strName, name);
		if (temp == 0)
		{
			index = i;
			break;
		}
	}

	for (int j = index; j < MAX_BOOKCOUNT; j++)
	{
		g_AddrBook[j] = g_AddrBook[j + 1];
	}

	g_nCount--;
	if (temp != 0)
		printf("����� �̸��� �����ϴ�\n");
	else 
		printf("�����Ǿ����ϴ�.\n");
	getch();
	

}

int LoadData(char* szFileName)
{
	//g_AddrBook <-- szFileName
	//--------------------------
	//1.��Ʈ��(����) ����(wb)
	FILE* stream = NULL;
	fopen_s(&stream, szFileName, "rb");
	
	//2.��Ʈ��(����)�� ����
	fread(&g_nCount , sizeof(int), 1, stream); //�������

	for (int i = 0; i < g_nCount; i++)
	{
		fread(g_AddrBook+i, sizeof(USERDATA), 1, stream);
	}

	//3.��Ʈ��(����) ����
	fclose(stream);
	return 0;
}


int SaveData(char* szFileName)
{
	//g_AddrBook --> szFileName
	//--------------------------
	//1.��Ʈ��(����) ����(wb)
	FILE* stream = NULL;
	fopen_s(&stream, szFileName, "wb");

	fwrite(&g_nCount , sizeof(int), 1, stream);//��� ����

	//2.��Ʈ��(����)�� ����
	for (int i = 0; i < g_nCount; i++)
	{
		fwrite(g_AddrBook+i, sizeof(USERDATA), 1, stream);//������ ����
	}

	//3.��Ʈ��(����) ����
	fclose(stream);
	return 0;
}

void main()
{
	int nMenu = 0;
	LoadData(DATA_FILE_NAME);//���Ͽ��� �о����

	// ���� �̺�Ʈ �ݺ���
	while ((nMenu = PrintUI()) != 0) {
		switch (nMenu) {
		case 1:		  // Add : �̸�, ����, ������ �Է¹޾� ģ���� �߰��ϴ� �Լ�
			Add();
			break;

		case 2:		  // Search : �̸��� �Է¹޾� �˻��ϰ�, �˻��� ����� �����ִ� �Լ�
			Search();
			break;

		case 3:		 // Print all : ��ȭ�ο� ����ִ� ��� �����͸� ȭ�鿡 ����ϴ� �Լ�
			PrintAll();
			break;

		case 4:
			Delete();
			break;
		}
	}
	SaveData(DATA_FILE_NAME);//���Ͽ� �����ϱ�
}
