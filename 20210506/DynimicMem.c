#include <stdio.h>
#pragma warning (disable:4996)

int main()
{
	//동적 메모리 + 자기참조 구조체 :링크드 리스트
	{
		typedef struct worker
		{

			int no;
			char name[20];

			struct worker* pNext;//자기참조 포인터
			//struct worker* pLinkPrev;	
			//struct worker* pLinkNext;

		}WOK;

		WOK* pHead = NULL;
		WOK* pNewUser = NULL;

		int count = 0;
		int answer = 1;
		while (answer == 1)
		{
			//1.새로운 사원 1명을 위한 동적 메모리 할당
			pNewUser = (WOK*)malloc(sizeof(WOK));//동적메모리 할당

			//2.사원 1명 정보 입력
			printf("사원의 이름 입력:");
			scanf_s("%s", pNewUser->name, 20);
			pNewUser->no = count + 1;
			pNewUser->pNext = NULL;

			//3.링크 조정
			pNewUser->pNext = pHead;
			pHead = pNewUser;
			count++;
			printf("계속입력(0:no/1:yes):");
			scanf_s("%d%*c", &answer);
			
		}
		//결과 출력
		WOK* pTmp = pHead;
		while (pTmp != NULL)
		{
			printf("[%p] %d.%s [%p]\n",
				pTmp, pTmp->no, pTmp->name, pTmp->pNext);
			//다음 노드로 이동
			pTmp = pTmp->pNext;
		
		}

		return 0;
	}
	{
		char pDrink[3][12] = { 0, };

		///////////////////
		//입력받기
		char temp[100];
		for (int i = 0; i < 3; i++)
		{
			printf("%d번 음료이름:", i + 1);
			scanf_s("%s", pDrink[i],12);


		}
		////////////////////
		//출력하기

		printf("음료이름 출력\n");
		for (int i = 0; i < 3; i++)
		{
			printf("\t%d번 음료:%s \n", i + 1, pDrink[i]);
		
		}
		return 0;
	}

	{

		printf("=====음료관리 =======\n");
		int nkind = 0;	//음료수 종류 갯수
		printf("음료의 종류는 총 몇개인가요?");
		scanf_s("%d%*c", &nkind);
		int nMaxLen = 0;//음료이름의 최대 길이
		printf("음료이름의 최대길이는 얼마가요?");
		scanf_s("%d%*c", &nMaxLen);

		char** pDrink = NULL;

		pDrink = (char**)malloc(sizeof(char*) * nkind);

		printf("pDrink의 사이즈=%d,메모리=%d\n",
			sizeof(pDrink), _msize(pDrink));

		///////////////////
		//입력받기
		char temp[100];
		for (int i = 0; i < nkind; i++)
		{

			printf("\t%d번 음료이름:", i + 1);
			scanf_s("%s", &temp, 100);

			//*(pDrink + i) = (char*)malloc(sizeof(char) * (nMaxLen+ 1));
			int len = strlen(temp);
			*(pDrink + i) = (char*)malloc(sizeof(char) * (len+1));

			//strcpy(pDrink[i], temp);	//문자열 함수
			memcpy(pDrink[i], temp, len + 1);
		}
		////////////////////
		//출력하기

		printf("음료이름 출력\n");
		for (int i = 0; i < nkind; i++)
		{
			printf("\t%d번 음료:%s(%d) \n", i + 1, pDrink[i], _msize(pDrink[i]));
			free(pDrink[i]);

		}
		return 0;

	}
	//동적메모리
	char* p1 = (char*)malloc(sizeof(char) * 2);	//강제 형변환이 중요!
	p1[0] = 'a';
	p1[1] = 'b';
	printf("%c,%c\n", p1[0], p1[1]);
	printf("사이즈: %d\n", sizeof(p1)); //4 -->포인터이기 떄문
	printf("힙사이즈: %d\n", _msize(p1)); //_msize ->힙안에 사이즈이기 때문
	free(p1);


	int b = 8;
	int* p2 = (int*)malloc(b);//8byte ->힙영역에 잡힘
	p2[0] = 10;
	p2[1] = 20;
	printf("%d,%d\n", p2[0], p2[1]);
	printf("사이즈: %d\n", sizeof(p2)); //4 -->포인터이기 떄문
	printf("힙사이즈: %d\n", _msize(p2)); //_msize ->힙안에 사이즈이기 때문

	free(p2);

	return 0;

}