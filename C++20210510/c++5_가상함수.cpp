#include <iostream>
using namespace std;
class CAnimal
{
public:
	//鉴荐啊惑窃荐(Pure virtual function)
	// : 牢磐其捞胶 汲拌甫 窃 
	// -> 磊侥甸捞 备泅秦具窃
	virtual void Cry() = 0;
	/*
	//啊惑窃荐
	 virtual void Cry()
	{
		cout << "款促" << endl;
	}
	*/

	void TestFunc()
	{
		cout << "****TestFunc()********" << endl;
		Cry();
		cout << "************" << endl;


	}
};

class CDog : public CAnimal
{
public:
	void Cry()
	{
		cout << "港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港港" << endl;
	}
};

class CCat : public CAnimal
{
public:
	void Cry()
	{
		cout << "谎酒酒酒澬谎酒酒酒澬谎酒酒酒澬谎酒酒酒澬谎酒酒酒澬谎酒酒酒澬谎酒酒酒澬谎酒酒酒澬谎酒酒酒澬谎酒酒酒澬谎酒酒酒澬谎酒酒酒澬谎酒酒酒澬" << endl;
	}
};

int main()
{
	//virtual class (pure virtual function)
	{
		//virtual 努贰胶绰 按眉甫 积己且 荐 绝促
		//CAnimal zzz; //virtual 努贰胶绰 函荐甫 急攫且 荐 绝促
	}

	//Reference + virtual
	{
		CCat a;
		a.Cry();

		CAnimal& ref = a;
		ref.Cry();

	}
	//return 0;
	//犁沥狼 +virtual
	{
		CAnimal* p1 = new CCat;

		p1->Cry();
		delete p1;

		CAnimal* p2 = new CDog;
		p2->Cry();
		delete p2;
	}
	//return 0;
	//犁沥狼
	{
		CCat* a = new CCat;
		a->Cry();
		a->TestFunc();

		delete a;
		CDog* b = new CDog;
		b->Cry();
		b->TestFunc();
		delete b;


	}
	return 0;
}