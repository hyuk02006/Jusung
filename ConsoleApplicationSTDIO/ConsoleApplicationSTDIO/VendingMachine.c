#include <stdio.h>
#include <windows.h>
#define MAX 6

//���Ǳ� ���α׷�
typedef struct vending {
	const char* szLabel;
	const int nPrice;
}vd;


int main()
{

	vd vd[MAX] =
	{
		{"�ݶ�",1000},
		{"�������꽺",1500},
		{"����",700},
		{"���̴�",2000},
		{"ȯŸ",1800},
		{"Ŀ��",2500},
	};

	int nMenu = -1; //�ʱⰪ
	//char szLabel[MAX][12] = { "�ݶ�","�������꽺","����","���̴�","ȯŸ","Ŀ��" };

	//const char *szLabel[MAX] = { "�ݶ�","�������꽺","����","���̴�","ȯŸ","Ŀ��" };
	//const int nPrice[MAX] = { 1000,1500,700,2000,1800,2500 };//����ǥ
	int nMoney;//�Էµ� �ݾ�
	int nChange;//�Ž�����


	printf("szLabel�� ������: %d \n\n", sizeof(vd->szLabel));



	//while (nMenu != 0)
	//while(1)//���ѷ���
	for (;;)//���ѷ���
	{
		////////////////////////////////////
		//1.�޴�����

		printf("======����� ���Ǳ�========\n");

		for (int i = 0; i < MAX; i++)
			//printf("%d.%s %d��\n", i + 1, szLabel[i], nPrice[i]);	//�迭 �ݺ���
			printf("%d.%s %d��\n", i + 1, vd[i].szLabel, vd[i].nPrice);	//�迭 �ݺ���

		printf("0.����\n");
		printf("===========================\n");
		printf("���ϴ� �����ư�� �����ϼ��� :");
		scanf_s("%d", &nMenu); //�Է¹ޱ�

		if (nMenu == 0)
		{
			printf("Bye\n\n");
			break;
		}
		////////////////////////////////////////
		//2.�ݾ� ó��
		printf("=================\n");
		printf("�ݾ��� �Է��ϼ���: \n");
		scanf_s("%d", &nMoney);//�Է¹ޱ�

		if (nMoney >= vd[nMenu-1].nPrice)
		{
			nChange = nMoney - vd[nMenu-1].nPrice;
			printf("�ܵ��� %d�� �Դϴ�.\n", nChange);
		}
		else
		{
			printf("�ݾ��� %d�� �����մϴ�.\n", vd[nMenu - 1].nPrice - nMoney);
			printf("�ݾ��� %d�� ��ȯ.\n", nMoney);
			continue;


		}

		////////////////////////////////////////
		//3.����� ���
		if (nMenu >= 1 && nMenu <= MAX)
			printf("%s�� ��������\n", vd[nMenu - 1].szLabel);
		else
			printf("�߸� �����ϼ̽��ϴ�.\n\n");


	}
	//switch�� �Ҽ��ִ� ���� if�� �� �� �� ����.

	return 0;
}