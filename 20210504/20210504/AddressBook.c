#include <stdio.h>
#include<string.h>
#pragma warning (disable:4996)
#define DATA_FILE_NAME "c:\\temp\\addr.dat"

typedef struct _USERDATA
{
	char strName[32];	  // 이름
	int nAge;			  // 나이
	int nGender;		  // 성별
} USERDATA;

//배열로 처리
#define		MAX_BOOKCOUNT	10
int		g_nCount = 0;
USERDATA	g_AddrBook[MAX_BOOKCOUNT];



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
	if (g_nCount > MAX_BOOKCOUNT)
	{
		printf("저장 공간 부족\n");
		getch();
		return;
	}

	USERDATA ud;

	printf("이름을 입력하세요:");
	gets(&ud.strName);

	printf("나이를 입력하세요:");
	scanf_s("%d", &ud.nAge);

	printf("성별을 입력하세요<남성:0,여성:1:>");
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
	printf("검색할 이름을 입력해주세요:");
	gets(&name);//1글자 입력->문자열

	for (int i = 0; i < g_nCount; i++)
	{
		temp = strcmp(name, g_AddrBook[i].strName);//0, 1, -1
		if (temp == 0)//strcmp, strcpy
		{
			printf("사용자의 정보를 확인하였습니다. \n 이름 : %s\t나이 : %d\t성별 : %s\n ",
				g_AddrBook[i].strName,
				g_AddrBook[i].nAge,
				g_AddrBook[i].nGender == 0 ? "남성" : "여성");
			break;
		}
	}
	if (temp != 0)
		printf("사용자 이름이 없습니다\n");

	getch();

}
void PrintAll()
{
	if (g_nCount == 0)
	{
		printf("목록이 없습니다.\n");
		getch();

		return;
	}

	for (int i = 0; i < g_nCount; i++)
	{
		printf("이름: %s\t 나이:%d  성별:%s\n",
			g_AddrBook[i].strName,
			g_AddrBook[i].nAge,
			g_AddrBook[i].nGender == 0 ? "남성" : "여성");
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
		printf("목록이 없습니다.\n");
		getch();
		return;
	}

	printf("삭제할 이름을 입력하세요:");
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
		printf("사용자 이름이 없습니다\n");
	else 
		printf("삭제되었습니다.\n");
	getch();
	

}

int LoadData(char* szFileName)
{
	//g_AddrBook <-- szFileName
	//--------------------------
	//1.스트림(파일) 개방(wb)
	FILE* stream = NULL;
	fopen_s(&stream, szFileName, "rb");
	
	//2.스트림(파일)에 저장
	fread(&g_nCount , sizeof(int), 1, stream); //헤더저장

	for (int i = 0; i < g_nCount; i++)
	{
		fread(g_AddrBook+i, sizeof(USERDATA), 1, stream);
	}

	//3.스트림(파일) 폭파
	fclose(stream);
	return 0;
}


int SaveData(char* szFileName)
{
	//g_AddrBook --> szFileName
	//--------------------------
	//1.스트림(파일) 개방(wb)
	FILE* stream = NULL;
	fopen_s(&stream, szFileName, "wb");

	fwrite(&g_nCount , sizeof(int), 1, stream);//헤더 저장

	//2.스트림(파일)에 저장
	for (int i = 0; i < g_nCount; i++)
	{
		fwrite(g_AddrBook+i, sizeof(USERDATA), 1, stream);//데이터 저장
	}

	//3.스트림(파일) 폭파
	fclose(stream);
	return 0;
}

void main()
{
	int nMenu = 0;
	LoadData(DATA_FILE_NAME);//파일에서 읽어오기

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
}
