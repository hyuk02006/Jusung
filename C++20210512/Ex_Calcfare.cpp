#pragma once

#include <iostream>

#define ON_MAIN
#ifdef ON_MAIN

using namespace std;

#define DEFAULT_FARE 45000

class CPerson
{
public:
	CPerson() : m_nFare(0) { m_nFare = 0; }
	~CPerson();
	virtual  void CalcFare() { m_nFare = DEFAULT_FARE; };
	virtual  unsigned int GetFare() const{ return m_nFare; }

protected:
	unsigned int m_nFare ;
};

// ������(0~7��) ��� ��� --> // 0%
class CBaby :public CPerson
{
public:
	void CalcFare() { m_nFare = DEFAULT_FARE * 0; };


};
// ���(8~13��) ��� ��� --> // 50%
class CChild :public CPerson
{
public:
	void CalcFare() { m_nFare = DEFAULT_FARE * 0.5; }


};

// û�ҳ�(14~19��) ��� ���--> // 75%
class CTeen : public  CPerson
{
public:
	void CalcFare() { m_nFare = DEFAULT_FARE * 0.75; };

};

// ����(20�� �̻�) ��� ���--> // 100%
class CAdult :public CPerson
{
public:
	void CalcFare() { m_nFare = DEFAULT_FARE; };
};


int main()
{

		cout << "=======================================" << endl;
		cout << "\t �������� ��� ����" << endl;
		cout << "=======================================" << endl;

		int nCount = 0;
		cout << "�� �� ���� �����Ͻó���? ";
		cin >> nCount;
		cout << "------------------------------" << endl;

		CPerson** arList = new CPerson * [nCount];


		// 1. �ڷ� �Է�: ����� �Է¿� ���� ������ ��ü ����
		int nAge = 0;
		for (int i = 0; i < nCount; i++)
		{
			cout << i + 1 << "���� ���̸� �Է��ϼ���: ";
			cin >> nAge;

			// ����ڰ� �Է��� ���̿� ���� ��ü�� ���� �����Ѵ�.
			if (nAge < 7)
			{
				arList[i] = new CBaby;
			}

			else if (nAge >= 8 && nAge <= 13)
			{
				arList[i] = new CChild;
			}

			else if (nAge >= 14 && nAge < 20)
			{
				arList[i] = new CTeen;
			}

			else
				arList[i] = new CAdult;


			// ������ ��ü�� �´� ����� �ڵ����� ���ȴ�.
			arList[i]->CalcFare();
		}

		// 2. �ڷ� ���: ����� ����� Ȱ���ϴ� �κ�
		int nFare = 0;
		int nTotal = 0;
		cout << "------------------------------" << endl;
		for (int i = 0; i < nCount; i++)
		{
			nFare = arList[i]->GetFare();
			nTotal += nFare;
			cout << i + 1 << "���� ����� " << nFare << "��" << endl;
		}
		cout << "------------------------------" << endl;
		cout << "�� ��� : " << nTotal << endl;
		cout << "------------------------------" << endl;

		// 3. �ڷ� ���� �� ����
		for (int i = 0; i < nCount; i++)
		{
			delete arList[i];
		}
		delete[] arList;

	return 0;
}
#endif
