#include <stdio.h>
#pragma warning (disable:4996)

int main()
{
	//���� �޸� + �ڱ����� ����ü :��ũ�� ����Ʈ
	{
		typedef struct worker
		{

			int no;
			char name[20];

			struct worker* pNext;//�ڱ����� ������
			//struct worker* pLinkPrev;	
			//struct worker* pLinkNext;

		}WOK;

		WOK* pHead = NULL;
		WOK* pNewUser = NULL;

		int count = 0;
		int answer = 1;
		while (answer == 1)
		{
			//1.���ο� ��� 1���� ���� ���� �޸� �Ҵ�
			pNewUser = (WOK*)malloc(sizeof(WOK));//�����޸� �Ҵ�

			//2.��� 1�� ���� �Է�
			printf("����� �̸� �Է�:");
			scanf_s("%s", pNewUser->name, 20);
			pNewUser->no = count + 1;
			pNewUser->pNext = NULL;

			//3.��ũ ����
			pNewUser->pNext = pHead;
			pHead = pNewUser;
			count++;
			printf("����Է�(0:no/1:yes):");
			scanf_s("%d%*c", &answer);
			
		}
		//��� ���
		WOK* pTmp = pHead;
		while (pTmp != NULL)
		{
			printf("[%p] %d.%s [%p]\n",
				pTmp, pTmp->no, pTmp->name, pTmp->pNext);
			//���� ���� �̵�
			pTmp = pTmp->pNext;
		
		}

		return 0;
	}
	{
		char pDrink[3][12] = { 0, };

		///////////////////
		//�Է¹ޱ�
		char temp[100];
		for (int i = 0; i < 3; i++)
		{
			printf("%d�� �����̸�:", i + 1);
			scanf_s("%s", pDrink[i],12);


		}
		////////////////////
		//����ϱ�

		printf("�����̸� ���\n");
		for (int i = 0; i < 3; i++)
		{
			printf("\t%d�� ����:%s \n", i + 1, pDrink[i]);
		
		}
		return 0;
	}

	{

		printf("=====������� =======\n");
		int nkind = 0;	//����� ���� ����
		printf("������ ������ �� ��ΰ���?");
		scanf_s("%d%*c", &nkind);
		int nMaxLen = 0;//�����̸��� �ִ� ����
		printf("�����̸��� �ִ���̴� �󸶰���?");
		scanf_s("%d%*c", &nMaxLen);

		char** pDrink = NULL;

		pDrink = (char**)malloc(sizeof(char*) * nkind);

		printf("pDrink�� ������=%d,�޸�=%d\n",
			sizeof(pDrink), _msize(pDrink));

		///////////////////
		//�Է¹ޱ�
		char temp[100];
		for (int i = 0; i < nkind; i++)
		{

			printf("\t%d�� �����̸�:", i + 1);
			scanf_s("%s", &temp, 100);

			//*(pDrink + i) = (char*)malloc(sizeof(char) * (nMaxLen+ 1));
			int len = strlen(temp);
			*(pDrink + i) = (char*)malloc(sizeof(char) * (len+1));

			//strcpy(pDrink[i], temp);	//���ڿ� �Լ�
			memcpy(pDrink[i], temp, len + 1);
		}
		////////////////////
		//����ϱ�

		printf("�����̸� ���\n");
		for (int i = 0; i < nkind; i++)
		{
			printf("\t%d�� ����:%s(%d) \n", i + 1, pDrink[i], _msize(pDrink[i]));
			free(pDrink[i]);

		}
		return 0;

	}
	//�����޸�
	char* p1 = (char*)malloc(sizeof(char) * 2);	//���� ����ȯ�� �߿�!
	p1[0] = 'a';
	p1[1] = 'b';
	printf("%c,%c\n", p1[0], p1[1]);
	printf("������: %d\n", sizeof(p1)); //4 -->�������̱� ����
	printf("��������: %d\n", _msize(p1)); //_msize ->���ȿ� �������̱� ����
	free(p1);


	int b = 8;
	int* p2 = (int*)malloc(b);//8byte ->�������� ����
	p2[0] = 10;
	p2[1] = 20;
	printf("%d,%d\n", p2[0], p2[1]);
	printf("������: %d\n", sizeof(p2)); //4 -->�������̱� ����
	printf("��������: %d\n", _msize(p2)); //_msize ->���ȿ� �������̱� ����

	free(p2);

	return 0;

}