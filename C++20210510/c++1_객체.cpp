#include <iostream>

#define ON_MAIN
#ifdef ON_MAIN
using namespace std;


class USERDATA2
{
	public://�⺻���� private
	int nAge;
	char szName[32];
	
	void PrintData();


};
void USERDATA2::PrintData()
{
	//printf("%d,%s\n", nAge, szName);
	cout << "Ŭ����:" << nAge << ", " << szName << endl;

}

typedef struct USERDATA 
{
	int nAge;
	char szName[32];
	void (*Print)(USERDATA*);//�Լ��������߰�

	

}USERDATA;

void PrintData(USERDATA* pUser)
{
	//printf("%d,%s\n", pUser->nAge, pUser->szName);
	cout << pUser->nAge <<", "<< pUser->szName << endl;
}

int main()
{
	//����ü
	{
		//������� �ڵ�
		USERDATA user = { 20,"ö��" };
		
		//printf("%d,%s\n", user.nAge, user.szName); //1�ܰ�
		//PrintData(&user); //2�ܰ�
		user.nAge = 30;
		user.Print = PrintData;
		user.Print(&user);//3�ܰ�

	}
	//��ü
	{
		USERDATA2 his = { 33,"ȫ�浿" };
		his.PrintData();
	}

	//Getter/Setter �Լ�
	{
		class  CMyData
		{
		public :
			int m_nID;
			void PrintMoney() const //����� �޼���
			{
				cout << "�� ���� " << m_nMoney << "�� �Դϴ�" << endl;
			}
		private:
			int m_nMoney;
		public:
			int GetMoney(void) const //����� �޼���
			{
				//m_nMoney++ //Error-READ_ONLY �Լ�
				return m_nMoney;
			}
			void SetMoney(int nParam) 
			{	
		
				if(m_nMoney <0)
					m_nMoney = nParam;
		
			}

		};

		CMyData a;
		
		a.SetMoney(100);
		a.PrintMoney();
		cout << a.GetMoney() <<"�� �Դϴ�"<< endl;
		
	}
	return 0;
}

#endif