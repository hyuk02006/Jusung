#include <stdio.h>
#pragma warning (disable:4996)
#define DATA_FILE_NAME "c:\\temp\\AddrBook_List.dat"

typedef struct _USERDATA
{
	char strName[32];	  // �̸�
	int nAge;			  // ����
	int nGender;		  // ����
	struct USERDATA* pNext;
} USERDATA;

//�迭�� ó��
#define		MAX_BOOKCOUNT	10
int		g_nCount = 0;
USERDATA	g_AddrBook[MAX_BOOKCOUNT];

//���� ��� ��� ���� �� ����
USERDATA g_Head = { 0, };


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
	//1. �޸� Ȯ��
	USERDATA* pNewUser = NULL;
	pNewUser = (USERDATA*)malloc(sizeof(USERDATA));
	memset(pNewUser, 0, sizeof(USERDATA));

	//2. ����� ���� �Է�
	printf("�̸��� �Է��ϼ���:");
	scanf_s("%s", pNewUser->strName, 32);
	printf("���̸� �Է��ϼ���:");
	scanf("%d", &pNewUser->nAge);
	printf("������ �Է��ϼ���<����:0,����:1:>");
	scanf_s("%d", &pNewUser->nGender);
	pNewUser->pNext = NULL;

	//3. ��ũ ����
	pNewUser->pNext = g_Head.pNext;
	g_Head.pNext = pNewUser;
	
}

void Search()
{
	USERDATA* ud = NULL;

	char name[20];
	printf("�˻��� �̸��� �Է����ּ���:");
	scanf("%s", &name);
	ud = g_Head.pNext;

	while (ud!=NULL)    
	{
		if (strcmp(ud->strName, name) == 0)
		{
			printf("������� ������ Ȯ���Ͽ����ϴ�. \n �̸� : %s\t���� : %d\t���� : %s\n ",
				ud->strName,
				ud->nAge,
				ud->nGender == 0 ? "����" : "����");
			getch();

			return;     

		}

		else if (ud->pNext == NULL)   
		{
			printf("ã�� �̸��� �����ϴ�.\n");
			getch();

			return;      
		}
		ud = ud->pNext;      

	}


}
void PrintAll()
{
	USERDATA* pTmp = g_Head.pNext;

	while (pTmp != 0)
	{
		printf("[%p] �̸�:%s ����:%d ����:%s\n",
			pTmp,
			pTmp->strName,
			pTmp->nAge,
			pTmp->nGender == 0 ? "����" : "����");
		
		
		//���� ���� �̵�
		pTmp = pTmp->pNext;
	}
	getch();


	
}

void Delete()
{
	USERDATA* pTemp = NULL; 
	USERDATA* pPrev = NULL;
		
	char name[20];
	printf("������ �̸��� �Է��ϼ���:");
	scanf_s("%s",&name,20);

	pTemp = g_Head.pNext;
	pPrev = g_Head.pNext;
	while (pTemp != NULL)
	{
		if (strcmp(pTemp->strName, name) == 0)//hit
		{
			//ud = ud;
			//pre->pNext = pTemp->pNext;
			//pTemp = pre;

			pPrev->pNext = pTemp->pNext;
			free(pTemp);

			printf("������ �Ϸ�Ǿ����ϴ�\n");
			getch();
			return;

		}
		
		pPrev = pTemp;
		pTemp = pTemp->pNext;
	
	}


}

int LoadData(char* szFileName)
{
	USERDATA* ud =g_Head.pNext;
	USERDATA* last;
	//g_AddrBook <-- szFileName
	//--------------------------
	//1.��Ʈ��(����) ����(wb)
	FILE* stream = NULL;
	fopen_s(&stream, szFileName, "rb");

	//2.��Ʈ��(����)�� ����
	fread(ud, sizeof(USERDATA), 1, stream);

	for (int i = 0; i < g_nCount; i++)
	{
		ud = (USERDATA*)malloc(sizeof(USERDATA));
		if (fread(ud, sizeof(USERDATA), 1, stream) != NULL)
			ud = ud->pNext;
	}
	//3.��Ʈ��(����) ����
	fclose(stream);

	return 0;
}


int SaveData(char* szFileName)
{

	USERDATA* ud = g_Head.pNext;

	//g_AddrBook --> szFileName
	//--------------------------
	//1.��Ʈ��(����) ����(wb)
	FILE* stream = NULL;
	fopen_s(&stream, szFileName, "wb");

	fwrite(ud, sizeof(USERDATA), 1, stream);

	//2.��Ʈ��(����)�� ����
	while (ud != NULL)
	{
		ud = (USERDATA*)malloc(sizeof(USERDATA));
		if (fwrite(ud, sizeof(USERDATA), 1, stream) != NULL)
		ud = ud->pNext;

	}

	//3.��Ʈ��(����) ����
	fclose(stream);
	return 0;
}

void ReleseList()
{

}
void main()
{
	int nMenu = 0;
	//LoadData(DATA_FILE_NAME);//���Ͽ��� �о����

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
	//ReleseList();
}
