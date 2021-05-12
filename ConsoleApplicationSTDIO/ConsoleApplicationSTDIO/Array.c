#include <stdio.h>


int main(void)
{


	int aList[4][5] = {
		{10,20,30,40},
		{50,60,70,80},
		{90,100,110,120}
	};

	int r_sum, total = 0;
	int c_sum[4] = { 0 };
	int arrLength = sizeof(aList) / 5 / sizeof(int);

	for (int i = 0; i < arrLength - 1; i++) {
		r_sum = 0;
		for (int j = 0; j < arrLength; j++) {
			printf("%d\t", aList[i][j]);
			r_sum += aList[i][j];
			c_sum[j] += aList[i][j];
		}
		printf("%d\t", r_sum);
		total += r_sum;

		printf("\n");
	}
	for (int i = 0; i < arrLength; i++) {
		printf("%d\t", c_sum[i]);

	}
	printf("%d\n", total);



	//int arrGrade[2][3] = { 1,2,3,4,5,6 };
	//printf("林家1:%d\n", arrGrade);
	//printf("林家2: %d\n", arrGrade[0]);
	//printf("林家3: %d\n", arrGrade[1]);
	//printf("林家4: %d\n", *(*(arrGrade)+1)); //2
	//printf("林家5: %d\n", *(*(arrGrade + 1) + 1)); //5







	//////////////////////////////////////////////
	//int arNumbers[] = { 10,22,32,5,8,-1,10 };
	//int max = arNumbers[0];

	//for (int i = 0; i < sizeof(arNumbers) / sizeof(int); i++)
	//{
	//	if (max < arNumbers[i])
	//		max = arNumbers[i];
	//}
	//printf("%d\n", max);


	//int min = *arNumbers;

	//for (int i = 0; i < sizeof(arNumbers) / sizeof(int); i++)
	//{
	//	if (min > *(arNumbers + i))
	//		min = *(arNumbers + i);
	//}
	//printf("%d\n", min);


	//int arr[3] = { 1,2,3 };
	//char latter[3] = { 'A','B','C' };

	//printf("蔼: %d, %d, %d\n", arr[0], arr[1], arr[2]);
	//printf("林家: %d, %d, %d\n", &arr[0], &arr[1], &arr[2]);
	//printf("蔼1: %d, %d, %d\n", *arr, *(arr + 1), *(arr + 2));
	//printf("林家1: %d, %d, %d\n", arr, arr + 1, arr + 2);

	//printf("===============================\n");

	//printf("蔼: %d, %d, %d\n", latter[0], latter[1], latter[2]);
	//printf("林家: %d, %d, %d\n", &latter[0], &latter[1], &latter[2]);
	//printf("蔼1: %d, %d, %d\n", *latter, *(latter + 1), *(latter + 2));
	//printf("林家1: %d, %d, %d\n", latter, latter + 1, latter + 2);



	return 0;
}
