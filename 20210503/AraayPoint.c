#include <stdio.h>

#define PI 3.14	//상수 메모리가 잡힘
int main()
{
	//const
	/*
	double pi = 3.14;
	pi = 4.14;

	int aaa = 1;
	int bbb = 2;
	//const  int* p = NULL;	//*p가 에러
	//int* const p = NULL;	//p가 에러
	const int* const p = NULL;	//둘다 에러

	p = &aaa;
	printf("1.p의값 : %d\n", *p);	//1

	aaa = 100;
	printf("2.p의값 : %d\n", *p);	//100
	
	*p = 1000; //에러
	printf("3.p의값 : %d\n", *p);	//1000

	p = &bbb;
	printf("4.p의값 : %d\n", *p);	//100


	return 0;
	*/

	int a[3]; // --> a[2]== *a(a+2)
	int arr[2][3] = { 10,20,30,40,50,60 }; //->arr[1][2],*(*(arr+1)+2),*(arr[1])+2)
	printf("arr의 사이즈:%d\n", sizeof(arr)); //24B

	int* ptr0;
	printf("ptr0의 사이즈:%d\n", sizeof(ptr0)); //4B


	ptr0 = arr;
	printf("1.값:%d ,주소:%d\n", *ptr0, ptr0); //10 
	ptr0++;
	printf("2.값:%d ,주소:%d\n", *ptr0, ptr0);//20 ,4B증가
	ptr0++;
	printf("3.값:%d ,주소:%d\n", *ptr0, ptr0); //30,4B증가
	ptr0++;
	printf("4.값:%d ,주소:%d\n", *ptr0, ptr0);//40, 4B증가

	int* ptr1[3]; //포인터 배열
	printf("ptr1의 사이즈:%d\n", sizeof(ptr1)); //12B
	ptr1[1] = arr;
	printf("11.값:%d ,주소:%d\n", **(ptr1 + 1), *(ptr1 + 1)); // 10
	printf("12.값:%d ,주소:%d\n", *(ptr1[1] + 1), ptr1[1]); // 10



	int(*ptr2)[3];
	printf("ptr2의 사이즈:%d\n", sizeof(ptr2));	//4B
	ptr2 = arr;
	printf("11.값:%d ,주소:%d\n", **ptr2, *ptr2); // 10
	ptr2 = ptr2 + 1;
	printf("12.값:%d ,주소:%d\n", **ptr2, *ptr2); // 40, 12B 증가

	ptr2 = arr;
	printf("%d\n", ptr2[0]);			//주소
	printf("%d\n", ptr2[1]);			//주소
	printf("%d\n", ptr2[0][0]);			//값(10)
	printf("%d\n", ptr2[1][0]);			//값(40)


	return 0;

}