#include <iostream>

using namespace std;

class CTest
{
private:
	//�ν��Ͻ� �������
	int m_nData;
public: 
	CTest(int a) :m_nData(a) { m_nCount++; }
	int GetData() 
	{ 
		return this->m_nData;
	}
	void SetData(int a)
	{
		//üũ
		this->m_nData = a;
	}
	void ResetCount() { m_nCount =0; }
	//���� ����Լ�(=Ŭ���� ����Լ�)
	static int GetCount() 
	{
		//m_nData++; //error
		return m_nCount; 
	}
	void PrintData();
	//���� �������
	static int m_nCount;//����Ƚ�� ������
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