#include <stdio.h>
#pragma warning (disable:4996)
#define DATA_FILE_NAME "c:\\temp\\AddrBook_List.dat"

typedef struct _USERDATA
{
	char strName[32];	  // 이름
	int nAge;			  // 나이
	int nGender;		  // 성별
	struct USERDATA* pNext;
} USERDATA;

//배열로 처리
#define		MAX_BOOKCOUNT	10
int		g_nCount = 0;
USERDATA	g_AddrBook[MAX_BOOKCOUNT];

//더미 헤드 노드 선언 및 정의
USERDATA g_Head = { 0, };


int PrintUI()
{
	int nInput = 0;

	system("cls");
	printf("===================================================\n");
	printf("전화번호부(Array)\n");
	printf("---------------------------------------------------\n");
	printf("[1] 추가\t [2] 검색\t [3] 전체보기\t [4] 삭제\t [0] 종료\n");
	printf("===================================================\n");

	// 사용자가 선택한 메뉴의 값을 반환한다.
	scanf_s("%d", &nInput);
	getchar();//버퍼에 남은 엔터 제거용
	return nInput;
}

void Add()
{
	//1. 메모리 확보
	USERDATA* pNewUser = NULL;
	pNewUser = (USERDATA*)malloc(sizeof(USERDATA));
	memset(pNewUser, 0, sizeof(USERDATA));

	//2. 사용자 정보 입력
	printf("이름을 입력하세요:");
	scanf_s("%s", pNewUser->strName, 32);
	printf("나이를 입력하세요:");
	scanf("%d", &pNewUser->nAge);
	printf("성별을 입력하세요<남성:0,여성:1:>");
	scanf_s("%d", &pNewUser->nGender);
	pNewUser->pNext = NULL;

	//3. 링크 조정
	pNewUser->pNext = g_Head.pNext;
	g_Head.pNext = pNewUser;
	
}

void Search()
{
	USERDATA* ud = NULL;

	char name[20];
	printf("검색할 이름을 입력해주세요:");
	scanf("%s", &name);
	ud = g_Head.pNext;

	while (ud!=NULL)    
	{
		if (strcmp(ud->strName, name) == 0)
		{
			printf("사용자의 정보를 확인하였습니다. \n 이름 : %s\t나이 : %d\t성별 : %s\n ",
				ud->strName,
				ud->nAge,
				ud->nGender == 0 ? "남성" : "여성");
			getch();

			return;     

		}

		else if (ud->pNext == NULL)   
		{
			printf("찾는 이름이 없습니다.\n");
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
		printf("[%p] 이름:%s 나이:%d 성별:%s\n",
			pTmp,
			pTmp->strName,
			pTmp->nAge,
			pTmp->nGender == 0 ? "남성" : "여성");
		
		
		//다음 노드로 이동
		pTmp = pTmp->pNext;
	}
	getch();


	
}

void Delete()
{
	USERDATA* pTemp = NULL; 
	USERDATA* pPrev = NULL;
		
	char name[20];
	printf("삭제할 이름을 입력하세요:");
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

			printf("삭제가 완료되었습니다\n");
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
	//1.스트림(파일) 개방(wb)
	FILE* stream = NULL;
	fopen_s(&stream, szFileName, "rb");

	//2.스트림(파일)에 저장
	fread(ud, sizeof(USERDATA), 1, stream);

	for (int i = 0; i < g_nCount; i++)
	{
		ud = (USERDATA*)malloc(sizeof(USERDATA));
		if (fread(ud, sizeof(USERDATA), 1, stream) != NULL)
			ud = ud->pNext;
	}
	//3.스트림(파일) 폭파
	fclose(stream);

	return 0;
}


int SaveData(char* szFileName)
{

	USERDATA* ud = g_Head.pNext;

	//g_AddrBook --> szFileName
	//--------------------------
	//1.스트림(파일) 개방(wb)
	FILE* stream = NULL;
	fopen_s(&stream, szFileName, "wb");

	fwrite(ud, sizeof(USERDATA), 1, stream);

	//2.스트림(파일)에 저장
	while (ud != NULL)
	{
		ud = (USERDATA*)malloc(sizeof(USERDATA));
		if (fwrite(ud, sizeof(USERDATA), 1, stream) != NULL)
		ud = ud->pNext;

	}

	//3.스트림(파일) 폭파
	fclose(stream);
	return 0;
}

void ReleseList()
{

}
void main()
{
	int nMenu = 0;
	//LoadData(DATA_FILE_NAME);//파일에서 읽어오기

	// 메인 이벤트 반복문
	while ((nMenu = PrintUI()) != 0) {
		switch (nMenu) {
		case 1:		  // Add : 이름, 나이, 성별을 입력받아 친구를 추가하는 함수
			Add();
			break;

		case 2:		  // Search : 이름을 입력받아 검색하고, 검색된 결과를 보여주는 함수
			Search();
			break;

		case 3:		 // Print all : 전화부에 들어있는 모든 데이터를 화면에 출력하는 함수
			PrintAll();
			break;

		case 4:
			Delete();
			break;
		}
	}
	SaveData(DATA_FILE_NAME);//파일에 저장하기
	//ReleseList();
}
