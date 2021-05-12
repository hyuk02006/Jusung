#include <stdio.h>
#include <windows.h>
#define MAX 6

//자판기 프로그램
typedef struct vending {
	const char* szLabel;
	const int nPrice;
}vd;


int main()
{

	vd vd[MAX] =
	{
		{"콜라",1000},
		{"오렌지쥬스",1500},
		{"생수",700},
		{"사이다",2000},
		{"환타",1800},
		{"커피",2500},
	};

	int nMenu = -1; //초기값
	//char szLabel[MAX][12] = { "콜라","오렌지쥬스","생수","사이다","환타","커피" };

	//const char *szLabel[MAX] = { "콜라","오렌지쥬스","생수","사이다","환타","커피" };
	//const int nPrice[MAX] = { 1000,1500,700,2000,1800,2500 };//가격표
	int nMoney;//입력된 금액
	int nChange;//거스름돈


	printf("szLabel의 사이즈: %d \n\n", sizeof(vd->szLabel));



	//while (nMenu != 0)
	//while(1)//무한루프
	for (;;)//무한루프
	{
		////////////////////////////////////
		//1.메뉴선택

		printf("======음료수 자판기========\n");

		for (int i = 0; i < MAX; i++)
			//printf("%d.%s %d원\n", i + 1, szLabel[i], nPrice[i]);	//배열 반복문
			printf("%d.%s %d원\n", i + 1, vd[i].szLabel, vd[i].nPrice);	//배열 반복문

		printf("0.종료\n");
		printf("===========================\n");
		printf("원하는 음료버튼을 선택하세요 :");
		scanf_s("%d", &nMenu); //입력받기

		if (nMenu == 0)
		{
			printf("Bye\n\n");
			break;
		}
		////////////////////////////////////////
		//2.금액 처리
		printf("=================\n");
		printf("금액을 입력하세요: \n");
		scanf_s("%d", &nMoney);//입력받기

		if (nMoney >= vd[nMenu-1].nPrice)
		{
			nChange = nMoney - vd[nMenu-1].nPrice;
			printf("잔돈은 %d원 입니다.\n", nChange);
		}
		else
		{
			printf("금액이 %d원 부족합니다.\n", vd[nMenu - 1].nPrice - nMoney);
			printf("금액이 %d원 반환.\n", nMoney);
			continue;


		}

		////////////////////////////////////////
		//3.음료수 출력
		if (nMenu >= 1 && nMenu <= MAX)
			printf("%s를 받으세요\n", vd[nMenu - 1].szLabel);
		else
			printf("잘못 선택하셨습니다.\n\n");


	}
	//switch로 할수있는 일은 if로 다 할 수 있음.

	return 0;
}