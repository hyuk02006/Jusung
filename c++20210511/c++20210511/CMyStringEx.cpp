//CMyStringEx.cpp
#include <iostream>
#include "CMyStringEx.h"
#include "CMyString.h"
using namespace std;
#pragma warning (disable:4996)
CMyStringEx::CMyStringEx()//디폴트 생성자
{
	m_pszData = NULL;
	m_nLength = 0;
}

CMyStringEx::CMyStringEx(const char* pszParam)
{
	Release();
	m_nLength = strlen(pszParam);
	m_pszData = new char[m_nLength+1];

	strcpy(m_pszData, pszParam);


	
}

CMyStringEx::~CMyStringEx()
{
	Release();


}

int CMyStringEx::GetLength()const
{
	return m_nLength;

}
int CMyStringEx::Append(const char* pszParam)
{	
	int temp = strlen(m_pszData) + strlen(pszParam);
	m_nLength = temp;
	char* I_pszData = new char[m_nLength + 1];
	strcpy(I_pszData, m_pszData);
	m_pszData = new char[m_nLength + 1];
	strcpy(m_pszData, I_pszData);
	strcat(m_pszData, pszParam);

	return 0;
}
int CMyStringEx::Find(const char* pszParam)
{
	
	char* p = strstr(m_pszData, pszParam);

	if (p != NULL)
		cout << *p << endl;
	else
		cout << "x" << endl;

	int n = p - m_pszData;;
	
	return n;


}