#include<iostream>

using namespace std;

//int mymax(int a, int b)
//{
//	int temp =a;
//	if (a > b)
//		temp = a;
//	else
//		temp = b;
//	return temp;
//}
//
//char mymax(char a, char b)
//{
//	return a > b ? a : b;;
//
//}
//double mymax(double a, double b)
//{
//	return a > b ? a : b;;
//
//}

//ÇÔ¼ö ÅÛÇÃ¸´ÀÇ Á¤ÀÇ
template <typename T> 
T mymax(T a, T b)
{
		return a > b ? a : b;;

}
//int myswap(int &a, int &b)
//{
//	int temp =0;
//	temp = a;
//	a = b ;
//	b = temp;
//	return temp;
//
//}

template <typename T>																																																																																																																																																																																																																																				
T myswap(T &a, T &b)
{
	T temp = 0;
	temp = a;
	a = b;
	b = temp;
	return 0;
}
int main()
{

	//ÇÔ¼ö ÅÆÇÃ¸´1
	cout << mymax(10, 20) << endl;
	cout << mymax(3.14, 2.2) << endl;
	cout << mymax('A', 'B') << endl;

	//cout << mymax(10, 13.14) << endl;//Å¸ÀÔ¿¡·¯
	cout << mymax<int>(10, 13.14) << endl;	//T = int
	cout << mymax<double>(20.78, 13.14) << endl; //T =double

	//ÇÔ¼ö ÅÆÇÃ¸´2
	int i = 50;
	int j = 10;
	cout << "Before" << "i=" << i << ", j=" << j << endl;
	myswap(i, j);
	cout << "After" << "i=" << i << ", j=" << j << endl;

	char c1 = 'A';
	char c2 = 'Z';
	cout << "Before" << "i=" << c1 << ", j=" << c2 << endl;
	myswap(c1, c2);
	cout << "After" << "i=" << c1 << ", j=" << c2 << endl;


	return 0;
}