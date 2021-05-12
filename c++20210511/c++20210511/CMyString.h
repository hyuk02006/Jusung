//CMyString.h
#pragma once
class CMyString
{
public:
	//정적 멤버 
	 int m_nCount;//객체가 생성된 누적수
	// void ResetCount();

	CMyString();
	CMyString(CMyString& temp);//복사 생성자
	CMyString(const char* pszParam);//변환생성자
	~CMyString();

protected:
	// 문자열을 저장하기 위해 동적 할당한 메모리를 가리키는 포인터
	char* m_pszData;

	// 저장된 문자열의 길이
	int m_nLength;

public:
	int SetString(const char* pszParam);

	// 멤버 읽기만 수행하므로 메서드를 상수화한다.
	const char* GetString() const;

	void Release();

	void Copy(const char* aa);
	/////////////////////////////
	//연산자 오버로딩
	CMyString& operator=(const CMyString& rhs);
	CMyString& operator+(const CMyString& rhs);


};
