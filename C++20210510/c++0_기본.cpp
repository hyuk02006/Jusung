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
	int g_nData = 100;
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

int Sum(int a, int b, int c)
{
	return a + b + c;
}

double Sum(double a, double b)
{
	return a + b;
}

int TestFunc2(int nParam = 99)	//����Ʈ�� �־��ټ��ִ�.
{
	return nParam * 2;
}

int TestFunc3(int a, int b, int c = 99)	//����Ʈ�� �־��ټ��ִ�.
{
	return c * 2;
}

inline int add(int a, int b)//�ζ��� �Լ�
{
	return a + b;
}
#define ADD(a,b)((a)+(b))	//��ũ�� �Լ�

int main()
{

	//�ζ��� �Լ� (vs ��ũ�� �Լ�)
	{
		cout << add(10, 20) << endl;
		cout << ADD(30, 40) << endl;

	}
	return 0;

	//������� for��
	{
		//C
		int aList[5] = { 10,20,30,40,50 };
		int sum = 0;

		for (int i = 0; i < 5; i++)
			sum += aList[i];

		cout << sum << endl;


		sum = 0;
		//C++
		for (auto n : aList)	//�ڵ����� type�� ������
		{
			sum += n;
		}
		cout << sum << endl;

	}
	return 0;
	//���ڿ�
	{
		//char name1[20] = "ȫ�浿"; //c��� 
		string name = "ȫ�浿";

		cout << name << endl;
		cout << name.size() << endl;

		name = "����ȯ";//strcpy�� c����
		if (name == "����ȯ")
			cout << "�̸� ��ġ" << endl;
		else
			cout << "�̸� ����ġ" << endl;
		name = name + " ����"; //strcat in c
		cout << name << endl;

	}
	return 0;

	//�����޸𸮿�����
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

		//3.�迭
		int* arr = new int[5];
		for (int i = 0; i < 5; i++)
		{
			arr[i] = i * 10;
			cout << arr[i] << endl;

		}
		delete[] arr;
	}
	return 0;
	//����Ʈ �Ű�����
	{
		int a = TestFunc2(10);
		cout << a << endl;

		int b = TestFunc2();
		cout << b << endl;

		int c = TestFunc3(3, 5, 35);
		cout << c << endl;

	}
	return 0;

	//�Լ� �����ε�(=��������) -->by ���Ӹͱ۸�(vs extern "C")
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

	//������ vs ���۷���(����)
	{
		int nData = 10;
		int* pt = &nData; //�������� ����� �ʱ�ȭ
		const int& ref = nData; //���۷����� ����� �ʱ�ȭ

		cout << nData << endl;
		//ref = 20;
		cout << nData << endl;
		nData = 30;
		cout << "���� " << ref << endl;

		int nAnther = 1000;
		//ref = nAnther;
		//ref++;
		cout << ref << endl;
	}
	return 0;

	//���ӽ����̽�
	{
		Gugudan();
		TEST::Gugudan(); //TEST::�Ҽ� �Լ�
		cout << TEST::g_nData << endl;

		cout << g_nData << endl;
		cout << ::g_nData << endl;
		cout << TEST::g_nData << endl;
	}
	return 0;

	//auto���
	{
		int a = 10;
		int b(a); //b=a;
		auto c(b);


		cout << a + b + c << endl;
		cout << b << endl;
		cout << c << endl;

	}
	return 0;

	//cin / cout ���
	{
		int nAge;
		cout << "���̸� �Է��ϼ���" << endl;
		cin >> nAge;

		char szJob[32];
		cout << "������ �Է��ϼ���.";
		cin >> szJob;

		string strName;
		cout << "�̸��� �Է��ϼ���" << endl;
		cin >> strName;

		string nickname = "õ��";
		strName = strName + nickname;
		cout << "����� �̸���" << strName << "�̰�," << "���̴� " << nAge << " ���̸�" << "������ " << szJob << "�Դϴ�." << endl;

	}
	return 0;
	//cout << "Hello World!\n";
	//printf("Hello World!\n");
	//int a = 3;
	//cout << a << "�� ����?"<<"\n";
	//printf("%d\n", a);
	//printf("%d\n", __LINE__);
	//cout << 3 + 4 <<endl;
}

