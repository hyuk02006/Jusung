#include <stdio.h>

#define PI 3.14	//��� �޸𸮰� ����
int main()
{
	//const
	/*
	double pi = 3.14;
	pi = 4.14;

	int aaa = 1;
	int bbb = 2;
	//const  int* p = NULL;	//*p�� ����
	//int* const p = NULL;	//p�� ����
	const int* const p = NULL;	//�Ѵ� ����

	p = &aaa;
	printf("1.p�ǰ� : %d\n", *p);	//1

	aaa = 100;
	printf("2.p�ǰ� : %d\n", *p);	//100
	
	*p = 1000; //����
	printf("3.p�ǰ� : %d\n", *p);	//1000

	p = &bbb;
	printf("4.p�ǰ� : %d\n", *p);	//100


	return 0;
	*/

	int a[3]; // --> a[2]== *a(a+2)
	int arr[2][3] = { 10,20,30,40,50,60 }; //->arr[1][2],*(*(arr+1)+2),*(arr[1])+2)
	printf("arr�� ������:%d\n", sizeof(arr)); //24B

	int* ptr0;
	printf("ptr0�� ������:%d\n", sizeof(ptr0)); //4B


	ptr0 = arr;
	printf("1.��:%d ,�ּ�:%d\n", *ptr0, ptr0); //10 
	ptr0++;
	printf("2.��:%d ,�ּ�:%d\n", *ptr0, ptr0);//20 ,4B����
	ptr0++;
	printf("3.��:%d ,�ּ�:%d\n", *ptr0, ptr0); //30,4B����
	ptr0++;
	printf("4.��:%d ,�ּ�:%d\n", *ptr0, ptr0);//40, 4B����

	int* ptr1[3]; //������ �迭
	printf("ptr1�� ������:%d\n", sizeof(ptr1)); //12B
	ptr1[1] = arr;
	printf("11.��:%d ,�ּ�:%d\n", **(ptr1 + 1), *(ptr1 + 1)); // 10
	printf("12.��:%d ,�ּ�:%d\n", *(ptr1[1] + 1), ptr1[1]); // 10



	int(*ptr2)[3];
	printf("ptr2�� ������:%d\n", sizeof(ptr2));	//4B
	ptr2 = arr;
	printf("11.��:%d ,�ּ�:%d\n", **ptr2, *ptr2); // 10
	ptr2 = ptr2 + 1;
	printf("12.��:%d ,�ּ�:%d\n", **ptr2, *ptr2); // 40, 12B ����

	ptr2 = arr;
	printf("%d\n", ptr2[0]);			//�ּ�
	printf("%d\n", ptr2[1]);			//�ּ�
	printf("%d\n", ptr2[0][0]);			//��(10)
	printf("%d\n", ptr2[1][0]);			//��(40)


	return 0;

}