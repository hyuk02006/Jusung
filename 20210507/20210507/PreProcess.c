#include <stdio.h>

#define MYADD(a,b) (a+b)	//��ũ�� �Լ�
//#define MYMUL(a,b) (a*b)	//��ũ�� �Լ�
#define MYMUL(a,b) ((a)*(b))	//��ũ�� �Լ�

#define MYMAKESTR(a) #a	//#:���ڿ��� �������
#define MYPASTR(a,b) a##b	//##:�ѵ���� �������

#define AAA

#ifndef AAA
	#define MYMESSEGE "I am a boy"

#else
	#define MYMESSEGE "You are a girl"

#endif
int myAdd(int x,int y)		//���Լ�
{
	int result;
	result = x + y;
	return result;
}

int main()
{

	printf("1.%s\n",__FILE__);
	printf("4.%d\n", __LINE__);
	printf("2.%s\n",__DATE__);
	printf("3.%s\n",__TIME__);



	puts(MYMESSEGE);
	int nData = 10;

#ifdef ABC
	printf("%d\n", MYPASTR(11, 22));
	printf("%d\n", MYPASTR(nDa, ta));
#else
	printf("%d\n", nData);
	printf("%s\n", MYMAKESTR(nData));
	printf("%s\n", MYMAKESTR(11));
	printf("%s\n", MYMAKESTR(3.14));
#endif

	printf("��ũ�� �Լ�:%d\n", MYMUL(3, 5));
	printf("��ũ�� �Լ�:%d\n", MYMUL(3+2, 5));


	printf("��ũ�� �Լ�:%d\n", MYADD(3, 5));
	printf("�� �Լ� :%d\n", myAdd(3, 5));

	printf("��ũ�� �Լ�:%f\n", MYADD(5.8, 5));
	printf("�� �Լ� :%d\n", myAdd(5.8, 5));


	return 0;
}