#include <iostream>

#define ON_MAIN
#ifdef ON_MAIN
using namespace std;


class USERDATA2
{
	public://기본값은 private
	int nAge;
	char szName[32];
	
	void PrintData();


};
void USERDATA2::PrintData()
{
	//printf("%d,%s\n", nAge, szName);
	cout << "클래스:" << nAge << ", " << szName << endl;

}

typedef struct USERDATA 
{
	int nAge;
	char szName[32];
	void (*Print)(USERDATA*);//함수포인터추가

	

}USERDATA;

void PrintData(USERDATA* pUser)
{
	//printf("%d,%s\n", pUser->nAge, pUser->szName);
	cout << pUser->nAge <<", "<< pUser->szName << endl;
}

int main()
{
	//구조체
	{
		//사용자의 코드
		USERDATA user = { 20,"철수" };
		
		//printf("%d,%s\n", user.nAge, user.szName); //1단계
		//PrintData(&user); //2단계
		user.nAge = 30;
		user.Print = PrintData;
		user.Print(&user);//3단계

	}
	//객체
	{
		USERDATA2 his = { 33,"홍길동" };
		his.PrintData();
	}

	//Getter/Setter 함수
	{
		class  CMyData
		{
		public :
			int m_nID;
			void PrintMoney() const //상수형 메서드
			{
				cout << "내 돈은 " << m_nMoney << "원 입니다" << endl;
			}
		private:
			int m_nMoney;
		public:
			int GetMoney(void) const //상수형 메서드
			{
				//m_nMoney++ //Error-READ_ONLY 함수
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
		cout << a.GetMoney() <<"원 입니다"<< endl;
		
	}
	return 0;
}

#endif