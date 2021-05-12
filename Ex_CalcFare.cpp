#pragma once

#include <iostream>

//#define ON_MAIN
#ifdef ON_MAIN

using namespace std;

#define DEFAULT_FARE 45000

class CPerson
{
public:
    void CalcFare() { m_nFare = DEFAULT_FARE; };
    unsigned int GetFare() { return m_nFare; }

protected:
    unsigned int m_nFare = 0;
};

// ������(0~7��) ��� ��� --> // 0%
class CBaby {};
// ���(8~13��) ��� ��� --> // 50%
class CChild {};
// û�ҳ�(14~19��) ��� ���--> // 75%
class CTeen {};
// ����(20�� �̻�) ��� ���--> // 100%
class CAdult {};


int main()
{
    cout << "=======================================" << endl;
    cout << "\t �������� ��� ����" << endl;
    cout << "=======================================" << endl;

    int nCount = 0;
    cout << "�� �� ���� �����Ͻó���? ";
    cin >> nCount;
    cout << "------------------------------" << endl;

    CPerson** arList = NULL;
    ...

    // 1. �ڷ� �Է�: ����� �Է¿� ���� ������ ��ü ����
    int nAge = 0;
    for (int i = 0; i < nCount; i++)
    {
        cout << i + 1 << "���� ���̸� �Է��ϼ���: ";
        cin >> nAge;
        
        // ����ڰ� �Է��� ���̿� ���� ��ü�� ���� �����Ѵ�.
        ....

        // ������ ��ü�� �´� ����� �ڵ����� ���ȴ�.
        arList[i]->CalcFare();
    }

    // 2. �ڷ� ���: ����� ����� Ȱ���ϴ� �κ�
    int nFare = 0;
    int nTotal = 0;
    cout << "------------------------------" << endl;
    for (int i = 0; i < nCount; i++)
    {
        ...

        cout << i + 1 << "���� ����� " << nFare << "��" << endl;
    }
    cout << "------------------------------" << endl;
    cout << "�� ��� : " << nTotal << endl;
    cout << "------------------------------" << endl;

    // 3. �ڷ� ���� �� ����
    for (int i = 0; i < nCount; i++)
    {
        ...
    }    

    return 0;
}
#endif
