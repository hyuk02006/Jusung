#include <stdio.h>
#pragma warning (disable:4996)
int main(void)
{
	char ch = 0;
	int age;
	char name[20];

	printf("나이 입력:");

	scanf("%d", &age);
	//fflush(stdin);//입력 버퍼를 비운다.
	getchar(); //엔터 버퍼 지움
	printf("이름 입력:");
	gets(name);

	//scanf("%s", &name);

	printf("%d,%s\n", age, name);

	//1.키보드 -> 모니터
	/*
	while (ch !=EOF) //ctrl+z (-1)
	{
		ch = getchar();//읽기
		//putchar(ch);//쓰기
	}
	*/
	//2.키보드 -> 파일
	/*
	FILE* stream;
	fopen_s(&stream, "C:\\temp\\data.txt","wt");

	while (ch != EOF) //ctrl+z (-1)
	{
		//ch = getchar();//읽기
		ch = fgetc(stdin);

		putchar(ch);//모니터 쓰기
		fputc(ch, stream);//파일 쓰기
	}
	fclose(stream);
	*/

	//3.파일 -> 파일
	/*
	FILE* stream1;
	FILE* stream2;
	int input = 0;

	fopen_s(&stream1, "C:\\temp\\data.txt", "r");
	fopen_s(&stream2, "C:\\temp\\data2.txt", "w");

	while (input != EOF) //ctrl+z (-1)
	{
		input = fgetc(stream1); //1번 파일에서 읽기
		fputc(input, stream2);  //2번 파일에 쓰기
		fputc(input, stdout);	//모니터에 쓰기
	}

	fclose(stream1);
	fclose(stream2);
	*/

	//4.키보드(문자열->파일)
	/*
	FILE* stream;
	char buffer[50];
	fopen_s(&stream, "C:\\temp\\data3.txt", "wt");


	fgets(buffer,sizeof(buffer),stdin);//키보드로 부터
	fputs(buffer, stream);

	fclose(stream);
	*/

	//5.키보드(성적입력) ->파일
	/*
	FILE* stream;
	char name[20];
	int kor, eng, math, total;
	double avg;
	int answer = 0;
	fopen_s(&stream, "C:\\temp\\data4.txt", "w");
	
	while (1)
	{
	
		printf("이름입력:");
		//scanf_s("%s", &name, 20);
		getchar(); //불필요한 enther소진
		gets(name);
		
		//puts(name);
		printf("\n");
		printf("국어점수,영어점수,수학점수 입력:");
		fscanf_s(stdin, "%d %d %d", &kor, &eng, &math);
		
		total = kor + eng + math;
		avg = total / 3;
		fprintf(stdout, "이름:%s 국어:%d 영어:%d 수학:%d 총점:%d 평균:%f\n", name, kor, eng, math, total, avg);
	
		fprintf(stream, "이름:%s 국어:%d 영어:%d 수학:%d 총점:%d 평균:%f\n", name, kor, eng, math, total, avg);

		puts("계속하시겠습니까?(1:yes,0:no)");
		scanf_s("%d", &answer);
		if (answer == 0)
			break;

		printf("\n");
	}
	fclose(stream);
	*/


	return 0;

}








