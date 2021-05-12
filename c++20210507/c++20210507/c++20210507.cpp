#include <iostream>
using namespace std;
//using namespace TEST;

void Gugudan()
{
	cout << "Global::Gugudan()" << endl;
}
int g_nData = 1000;

namespace TEST
{
	int g_nData=100;
	void Gugudan(void)
	{
		cout << "TEST::Gugudan()" << endl;
	}

}
void TestFuncRef1(int rParam)
{
	rParam = 100;

}

void TestFuncRef2(int& rParam)
{
	rParam = 100;
}

void TestFuncRef3(int* rParam)
{
	*rParam = 100;
}

int Sum(int a, int b) 
{
	return a + b;
}

int Sum(int a, int b,int c) 
{
	return a + b+c;
}

double Sum(double a, double b) 
{
	return a + b;
}

int TestFunc2(int nParam =99)	//디폴트를 넣어줄수있다.
{
	return nParam *2;
}

int TestFunc3(int a, int b,int c = 99)	//디폴트를 넣어줄수있다.
{
	return c * 2;
}

inline int add(int a, int b)//인라인 함수
{
	return a + b;
}
#define ADD(a,b)((a)+(b))	//매크로 함수

int main()
{

	//인라인 함수 (vs 매크로 함수)
	{
		cout << add(10, 20) << endl;
		cout << ADD(30, 40) << endl;

	}
	return 0;

	//범위기반 for문
	{
		//C
		int aList[5] = { 10,20,30,40,50 };
		int sum = 0;

		for (int i = 0; i < 5; i++)
			sum += aList[i];

		cout << sum << endl;


		sum = 0;
		//C++
		for (auto n : aList)	//자동으로 type을 맞춰줌
		{
			sum += n;
		}
		cout << sum << endl;

	}
	return 0;
	//문자열
	{
		//char name1[20] = "홍길동"; //c언어 
		string name = "홍길동";

		cout << name << endl;
		cout << name.size() << endl;

		name = "박태환";//strcpy는 c에서
		if (name == "박태환")
			cout << "이름 일치" << endl;
		else
			cout << "이름 불일치" << endl;
		name = name + " 만세"; //strcat in c
		cout << name << endl;

	}
	return 0;

	//동적메모리연산자
	{
		//1.
		int* p1 = new int;
		*p1 = 10;
		cout << *p1 << endl;
		delete p1;

		//2.
		int* p2 = new int(100);
		cout << *p2 << endl;
		delete p2;

		//3.배열
		int* arr = new int[5];
		for (int i = 0; i < 5; i++)
		{
			arr[i] = i * 10;
			cout << arr[i] << endl;

		}
		delete[] arr;
	}
	return 0;
	//디폴트 매개변수
	{
		int a = TestFunc2(10);
		cout << a << endl;

		int b = TestFunc2();
		cout << b << endl;

		int c = TestFunc3(3, 5, 35);
		cout << c << endl;

	}
	return 0;

	//함수 오버로딩(=다중정의) -->by 네임맹글링(vs extern "C")
	{
		cout << Sum(3, 4) << endl;
		cout << Sum(3, 4, 5) << endl;
		cout << Sum(3.1, 4.2) << endl;
	}
	return 0;

	//Call by reference
	{

		int nData = 0;
		nData = 0;
		TestFuncRef1(nData);
		cout << "1." << nData << endl;

		nData = 0;
		TestFuncRef2(nData);
		cout << "2." << nData << endl;

		nData = 0;
		TestFuncRef3(&nData);
		cout << "3." << nData << endl;
		return 0;

	}

	//포인터 vs 레퍼런스(별명)
	{
		int nData = 10;
		int* pt = &nData; //포인터의 선언과 초기화
		const int& ref = nData; //레퍼런스의 선언과 초기화

		cout << nData << endl;
		//ref = 20;
		cout << nData << endl;
		nData = 30;
		cout << "여기 " << ref << endl;

		int nAnther = 1000;
		//ref = nAnther;
		//ref++;
		cout << ref << endl;
	}
	return 0;

	//네임스페이스
	{
	Gugudan();
	TEST::Gugudan(); //TEST::소속 함수
	cout << TEST::g_nData << endl;

	cout << g_nData << endl;
	cout << ::g_nData << endl;
	cout << TEST::g_nData << endl;
	}
	return 0;

	//auto사용
	{
	int a = 10;
	int b(a); //b=a;
	auto c(b);


	cout << a+b+c << endl;
	cout << b << endl;
	cout << c << endl;

	}
	return 0;

	//cin / cout 사용
	{
	int nAge;
	cout << "나이를 입력하세요" << endl;
	cin >> nAge;

	char szJob[32];
	cout << "직업을 입력하세요.";
	cin >> szJob;

	string strName;
	cout << "이름을 입력하세요" << endl;
	cin >> strName;

	string nickname = "천재";
	strName = strName + nickname;
	cout << "당신의 이름은" <<strName<<"이고,"<<"나이는 "<<nAge<<" 살이며"<<"직업은 "<<szJob<<"입니다."<< endl;

	}
	return 0;
	//cout << "Hello World!\n";
	//printf("Hello World!\n");
	//int a = 3;
	//cout << a << "가 뭘까?"<<"\n";
	//printf("%d\n", a);
	//printf("%d\n", __LINE__);
	//cout << 3 + 4 <<endl;
}

