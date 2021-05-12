#include <iostream>
using namespace std;

class CMyDataA
{
//private:
	protected:
	char* m_pData;
public:
	CMyDataA() 
	{
		cout << "CMyDataA()" << endl;

		m_pData = new char[32];
	}
	~CMyDataA() 
	{
		cout << "~CMyDataA()" << endl;
		if(m_pData != NULL)
			delete[] m_pData;
	}

};

class CMyDataB :public CMyDataA
{
public:
	CMyDataB() { cout << "CMyDataB()" << endl; }
	~CMyDataB() { cout << "~CMyDataB()" << endl; }

};


class CMyDataC :public CMyDataB
{
public:
	CMyDataC() { cout << "CMyDataC()" << endl; }
	~CMyDataC() 
	{ 
		cout << "~CMyDataC()" << endl; 
		delete[] m_pData;
		m_pData = NULL;
	}

};

/////////////////////////////////////////////////////////////////

class CParent
{
public://������ ���ٰ���
	CParent() { cout << "CParent()" << endl; }
	CParent(int a) { cout << "CParent(int) " << endl; }
	CParent(double a) { cout << "CParent(double) " << endl; }

	int GetData() {return m_nData;	}
	void SetData(int nParam)  {m_nData = nParam; }


private:	//������ ���� �Ұ���
	int m_nData = 0;
protected:
	void PrintData()
	{
		cout << "CParent :: PrintData() :" << m_nData << endl; 
	}
};
class CChild :public CParent
{
public:	//������ ���ٰ���
	CChild() { cout << "CChild()" << endl; }
	CChild(int a) :CParent(a)
	{ 
		cout << "CChild(int) " << endl; 
	}
	CChild(double a) :CParent()
	{
		cout << "CChild(double) " << endl; 
	}
	void TextFunc()
	{
		SetData(100);//public
		PrintData();//protected
	}

	//������(�������̵�)
	void SetData(int nParam) 
	{
		//�Է� ������ ���� �����ϴ� ���ο� ����� �߰��Ѵ�.
		if (nParam < 0)
			CParent::SetData(0);
		else if(nParam>10)
			CParent::SetData(10);
		else
			CParent::SetData(nParam);


	}

};

int main()
{
	//�����ڸ� ������
	{

		CChild a;
		cout << "**************"<< endl;
		CChild b(5);
		cout << "**************" << endl;
		CChild c(3.3);
		cout << "**************" << endl;

	}
	return 0;
	{
		//���� Ŭ���� - ���� �����ϴ� ����� ����.
		CParent a;
		a.SetData(15);
		cout << a.GetData() << endl;

		//���� Ŭ����(���) - ���� �����ϴ� ����� �ִ�.
		CChild b;
		b.SetData(15);
		cout << b.GetData() << endl;

		CParent& ref = b;	//���� ���� �ȵ� ref�� ���� ������ �ҷ����� ����
		ref.SetData(20);
		cout << ref.GetData() << endl;

		//protected�� ��������
		//a.PrintData(); //protected �̹Ƿ� Error �߻�
		//a.TextFunc();


	}

	return 0;
	//���
	{
		cout << "***********Main Begin ************" << endl;
		CMyDataC xxx;
		cout << "***********Main END ************" << endl;

	}
	return 0;
}