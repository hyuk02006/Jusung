//CMyString.cpp
#include <iostream>
using namespace std;
#include "CMyString.h"

#pragma warning(disable:4996)

CMyString::CMyString()//����Ʈ ������
	:m_pszData(NULL), m_nLength(0)
{
	//ResetCount();
	//m_nCount++;
	//cout << "���� ���� �� :" << m_nCount << endl;
}

CMyString::CMyString(CMyString& temp)//���� ������
{
	//m_pszData = temp.m_pszData;//��������

	this->SetString(temp.GetString());
	m_nCount++;
	cout << "���� ���� �� :" << m_nCount << endl;

	/*
	m_nLength = temp.m_nLength;
	m_pszData = new char[m_nLength + 1];
	strcpy(m_pszData, temp.m_pszData);
	*/
}
CMyString::CMyString(const char* pszParam) //��ȯ ������
{
	SetString(pszParam);

	/*m_nCount++;
	cout << "���� ���� �� :" << m_nCount << endl;*/
}


CMyString::~CMyString()
{
	Release();
}

int CMyString::SetString(const char* pszParam)
{
	Release();

	m_nLength = strlen(pszParam);
	m_pszData = new char[m_nLength + 1];
	strcpy(m_pszData, pszParam);

	return m_nLength;
}

// ��� �б⸸ �����ϹǷ� �޼��带 ���ȭ�Ѵ�.
const char* CMyString::GetString() const
{
	return m_pszData;
}

void CMyString::Release()
{
	delete[] m_pszData;

	m_pszData = NULL;
	m_nLength = 0;
}
void CMyString::Copy(const char* aa)
{
	m_pszData = new char[m_nLength + 1];
	strcpy(m_pszData, aa);

}
CMyString& CMyString::operator=(const CMyString& rhs)
{
	if (this != &rhs)
		this->SetString(rhs.GetString());

	return *this;
}
// void CMyString::ResetCount()
//{
//	m_nCount = 0;
//	
//}