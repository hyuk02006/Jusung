#include<stdio.h>
#define SWAP(a,b) {int t=a; a=b; b=t;}

void sort(int* base, int n);
int main()
{
    int arlumbers[7] = { 10,40,30,20,35,1,5 };
    sort(arlumbers, 7);
    return 0;

}

void sort(int* base, int n)
{
    int i, j;
    for (i = 0; i < n; i++)
    {
        for (j = i; j < n; j++)
        {
            if (base[i] > base[j])
                SWAP(base[i], base[j]);
        }
    }
    for (i = 0; i < n; i++)
        printf("%d : %d\n", i, base[i]);
}