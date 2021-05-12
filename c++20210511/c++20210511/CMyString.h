//CMyString.h
#pragma once
class CMyString
{
public:
	//���� ��� 
	 int m_nCount;//��ü�� ������ ������
	// void ResetCount();

	CMyString();
	CMyString(CMyString& temp);//���� ������
	CMyString(const char* pszParam);//��ȯ������
	~CMyString();

protected:
	// ���ڿ��� �����ϱ� ���� ���� �Ҵ��� �޸𸮸� ����Ű�� ������
	char* m_pszData;

	// ����� ���ڿ��� ����
	int m_nLength;

public:
	int SetString(const char* pszParam);

	// ��� �б⸸ �����ϹǷ� �޼��带 ���ȭ�Ѵ�.
	const char* GetString() const;

	void Release();

	void Copy(const char* aa);
	/////////////////////////////
	//������ �����ε�
	CMyString& operator=(const CMyString& rhs);
	CMyString& operator+(const CMyString& rhs);


};
