#include <iostream>

using namespace std;

class CTest
{
private:
	//인스턴스 멤버변수
	int m_nData;
public: 
	CTest(int a) :m_nData(a) { m_nCount++; }
	int GetData() 
	{ 
		return this->m_nData;
	}
	void SetData(int a)
	{
		//체크
		this->m_nData = a;
	}
	void ResetCount() { m_nCount =0; }
	//정적 멤버함수(=클래스 멤버함수)
	static int GetCount() 
	{
		//m_nData++; //error
		return m_nCount; 
	}
	void PrintData();
	//정적 멤버변수
	static int m_nCount;//누적횟수 관리용
	~CTest(){ }
};

void CTest::PrintData() { this->m_nData; }
int CTest::m_nCount = 0;

int main()
{


	cout << CTest::GetCount() << endl;
	CTest a(100);
	//a.ResetCount();

	cout << a.GetData() << endl;
	cout << a.GetCount() << endl;

	CTest b(50);
	//b.ResetCount();

	cout << b.GetData() << endl;
	cout << b.GetCount() << endl;
	CTest::GetCount();

	return 0;

}