#include <stdio.h>
#pragma warning (disable:4996)

struct point
{
	int x;
	int y;

};

typedef struct rect
{
	int x;
	int y;
	int w;
	int h;

}MYRECT;

typedef int AGE;	//typedef는 구조체에만 쓰이는것이 아님..

typedef struct point MYPOINT;

#pragma pack(push,1)	//1byte 단위로 패킹
struct USERDATA {
	char ch;
	int age;

}; //5B ->8B

struct MYDATA {
	char ch;
	int age;
	double hight;


}; //13B ->16B
#pragma pack(pop)

struct SCORE 
{

	int kor;
	int eng;
	int math;

	int sum;
	double avg;

};
struct student 
{

	int no;
	char name[20];
	struct SCORE s;

};
typedef struct student STU;

typedef struct employee 
{

	int no;
	char name[20];


}EMP;

typedef struct worker
{

	int no;
	char name[20];

	struct worker* pLink;


}WOK;

union shape 
{
	int x;
	int y;
	
};

union differ
{
	char a;
	int b;
	double c;
};
int main(void)
{

	//열거형 -----------------------------------------------------
	
	enum season{SPRING,SUMMER,FALL,WINTER};
	enum week { SUN,MON,TUE,WED,THU,FRI,SAT };

	int mybirth = SPRING;
	int today = MON;

	printf("내 생일은 %d, %d\n", mybirth, today);


	//공용체 -----------------------------------------------------
	{

		union shape test ;
		test.x = 10;
		printf("union: %d ,%d\n", test.x, test.y);
		test.y = 20;
		printf("union: %d ,%d\n", test.x, test.y);
		printf("union사이즈: %d \n",sizeof(union shape));	//4
		printf("union사이즈: %d \n", sizeof(test));			//4

		union differ test2;
		printf("union사이즈: %d \n", sizeof(union differ));	//8
		printf("union사이즈: %d \n", sizeof(test2));		//8



		return 0;
	}



	//구조체의 자기참조 포인터-------------------------------------
	{
		WOK a = { 1,"홍길동",NULL };
		WOK b = { 2,"김수한무",NULL };
		WOK c = { 3,"강호동",NULL };
		WOK d = { 4,"유재석",NULL };
		WOK e = { 5,"하하",NULL };
		
		a.pLink = &b;
		b.pLink = &c;
		c.pLink = &d;
		d.pLink = &e;

		WOK* pHead =&a; //초기값
		
		while(pHead!=NULL)
		{
			printf("[%p] %d.%s [%p]\n",
				pHead, pHead->no, pHead->name, pHead->pLink);
			pHead = pHead->pLink;
		}
		return 0;
	}


	//구조체의 포인터----------------------------------------------
	{
		STU song = { 10,"Songkihyuk",{80,100,100,} };
		STU* pStu = NULL;
		pStu = &song;

		pStu->s.sum = pStu->s.kor + pStu->s.eng + pStu->s.math;		//포인터 -> 
		pStu->s.avg = pStu->s.sum / 3;

		printf("StudentID:%d Name:%s \nStudent Grade\n\tKor:%d,Eng: %d Math: %d\nTotal: %d,Avg: %.f\n",
			pStu->no, pStu->name, pStu->s.kor, pStu->s.eng, pStu->s.math, pStu->s.sum, pStu->s.avg);	

	}
	return 0;

	//중첩 구조체--------------------------------------------------

	struct student song = { 10,"Songkihyuk",{100,100,100} };
	song.s.sum = song.s.kor + song.s.eng + song.s.math;			//배열 .
	song.s.avg = song.s.sum / 3;
	printf("StudentID:%d Name:%s \nStudent Grade\n\tKor:%d,Eng: %d Math: %d\nTotal: %d,Avg: %.f\n",
		song.no, song.name, song.s.kor, song.s.eng, song.s.math, song.s.sum, song.s.avg);



	return 0;

	//구조체의 배열------------------------------------------------
	struct SCORE kim = { 90,90,100 };
	struct SCORE myClass[5] =
	{
		{90,90,100 },
		{50,80,100 },
		{30,20,80 },
		{90,90,40 },
		{90,60,55 },

	};
	//for (int i = 0; i < sizeof(myClass) / sizeof(struct SCORE); i++)
	//	scanf_s("%d %d %d", &myClass[i].kor, &myClass[i].eng, &myClass[i].math);

	for (int i = 0; i < 5; i++) {
		myClass[i].sum = myClass[i].kor + myClass[i].eng + myClass[i].math;
		myClass[i].avg = myClass[i].sum / 3;
		printf("myClass의 %d 성적\n총점:%d,평균:%.2f\n", i + 1,
			myClass[i].sum, myClass[i].avg);
	}



	//for (int i = 0; i < 5; i++)
	//	printf("kim의 성적\n국어:%d,영어:%d수학:%d\n총점:%d,평균:%f\n",

	kim.sum = kim.kor + kim.eng + kim.math;
	kim.avg = kim.sum / 3;
	printf("kim의 성적\n국어:%d,영어:%d수학:%d\n총점:%d,평균:%f\n",
		kim.kor, kim.eng, kim.math, kim.sum, kim.avg);




	AGE kim1 = 45;
	AGE lee = 33;

	int a = 100;
	a = 10;

	struct point pt1 = { 10,20 };
	struct point pt2;
	MYPOINT pt3;

	pt2.x = 100;
	pt2.y = 200;

	printf("pt1 : %d ,%d\n", pt1.x, pt1.y);
	printf("pt2 :%d ,%d\n", pt2.x, pt2.y);

	struct rect rect1 = { 10,20,30,40 };
	printf("rect1 :%d ,%d, %d, %d\n", rect1.x, rect1.y, rect1.h, rect1.w);

	printf("MYPOINT의 사이즈:%d\n", sizeof(MYPOINT));		//8b
	printf("MYRECT의 사이즈:%d\n", sizeof(MYRECT));		//16b
	printf("MYDATA의 사이즈:%d\n", sizeof(struct MYDATA));		//16b win32 8byte 단위로 맞춰줌(8byte 패킹)  but #pragma pack(push,1) #prama pack(pop) -> 1byte로 패킹
	printf("USERDATA의 사이즈:%d\n", sizeof(struct USERDATA));		//8b win32 8byte 단위로 맞춰줌(8byte 패킹)


	return 0;

}