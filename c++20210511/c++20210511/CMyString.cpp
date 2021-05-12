//CMyString.cpp
#include <iostream>
using namespace std;
#include "CMyString.h"

#pragma warning(disable:4996)

CMyString::CMyString()//디폴트 생성자
	:m_pszData(NULL), m_nLength(0)
{
	//ResetCount();
	//m_nCount++;
	//cout << "현재 누적 수 :" << m_nCount << endl;
}

CMyString::CMyString(CMyString& temp)//복사 생성자
{
	//m_pszData = temp.m_pszData;//얕은복사

	this->SetString(temp.GetString());
	m_nCount++;
	cout << "현재 누적 수 :" << m_nCount << endl;

	/*
	m_nLength = temp.m_nLength;
	m_pszData = new char[m_nLength + 1];
	strcpy(m_pszData, temp.m_pszData);
	*/
}
CMyString::CMyString(const char* pszParam) //변환 생정자
{
	SetString(pszParam);

	/*m_nCount++;
	cout << "현재 누적 수 :" << m_nCount << endl;*/
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

// 멤버 읽기만 수행하므로 메서드를 상수화한다.
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