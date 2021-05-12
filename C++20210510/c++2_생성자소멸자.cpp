#include <iostream>

using namespace std;
class CTest
{
public:
	int m_nA;
	const int m_nMax =50; //C++ 11부터 값을 받아줌

	CTest()
		:m_nMax(100), m_nA(0)//초기화 목록 (초기화 리스트) const변수일때 이용
	{
		
		cout << "CTest의 생성자  " << endl;
	
	}
	CTest(int a)
		:m_nA(a) // 초기화 목록
	{
		cout << "CTest의 생성자 int " << endl;
		//m_nA = a;
	}
	CTest(int a,int b)
	{
		cout << "CTest의 생성자 int,int " << endl;
		this->m_nA = a+b;
	}
	CTest(double a) = delete;
	/*{
		cout << "CTest의 생성자 doble "<<endl;
		m_nA = -1;
	}
	*/
	~CTest()
	{
		cout << "CTest의 소멸자\n";

	}
};
int main()
{
	//생성자와 초기화 목록
	{
		
		CTest a(100);
		cout << a.m_nA << endl;

		CTest b(500);
		cout << b.m_nA << endl;

		CTest c(22,99);
		cout << c.m_nA << endl;
	

		//CTest d(3.14); Error
		//cout << d.m_nA << endl;

	}
	return 0;

	//객체의 동적생성 메인이 종료되기전에 소멸
	{
		cout << "main의 시작\n";
		CTest* pData = new CTest;
		cout << "TEST" << endl;
		delete pData;
		cout << "main의 종료\n";
	}
	return 0;
	//생성자,소멸자 메인이 종료되고 소멸
	{
		cout << "main의 시작\n";
		CTest a(100);
		cout << "main의 종료\n";
	}

	return 0;
}