#include <iostream>

using namespace std;
class CTest
{
public:
	int m_nA;
	const int m_nMax =50; //C++ 11���� ���� �޾���

	CTest()
		:m_nMax(100), m_nA(0)//�ʱ�ȭ ��� (�ʱ�ȭ ����Ʈ) const�����϶� �̿�
	{
		
		cout << "CTest�� ������  " << endl;
	
	}
	CTest(int a)
		:m_nA(a) // �ʱ�ȭ ���
	{
		cout << "CTest�� ������ int " << endl;
		//m_nA = a;
	}
	CTest(int a,int b)
	{
		cout << "CTest�� ������ int,int " << endl;
		this->m_nA = a+b;
	}
	CTest(double a) = delete;
	/*{
		cout << "CTest�� ������ doble "<<endl;
		m_nA = -1;
	}
	*/
	~CTest()
	{
		cout << "CTest�� �Ҹ���\n";

	}
};
int main()
{
	//�����ڿ� �ʱ�ȭ ���
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

	//��ü�� �������� ������ ����Ǳ����� �Ҹ�
	{
		cout << "main�� ����\n";
		CTest* pData = new CTest;
		cout << "TEST" << endl;
		delete pData;
		cout << "main�� ����\n";
	}
	return 0;
	//������,�Ҹ��� ������ ����ǰ� �Ҹ�
	{
		cout << "main�� ����\n";
		CTest a(100);
		cout << "main�� ����\n";
	}

	return 0;
}