//FileSender.cpp
// [ File Sender ]
#define _WINSOCK_DEPRECATED_NO_WARNINGS
#define _CRT_SECURE_NO_WARNINGS

#include <winsock2.h>	
#pragma comment(lib,"Ws2_32.lib")
#include <stdlib.h>		
#include <stdio.h>

#define BUFSIZE 4096
#define FILE_NAME "sample.pdf"
// 소켓 함수 오류 출력 후 종료
void err_quit(const char* msg);
// 소켓 함수 오류 출력
void err_display(const char* msg);

int main(int argc, char* argv[])
{
	int retval;

	// 윈속 초기화
	WSADATA wsa;
	if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
		return -1;

	// socket()
	SOCKET sock = socket(AF_INET, SOCK_STREAM, 0);
	if (sock == INVALID_SOCKET) err_quit("socket()");

	// connect()
	SOCKADDR_IN serveraddr;
	ZeroMemory(&serveraddr, sizeof(serveraddr));
	serveraddr.sin_family = AF_INET;
	serveraddr.sin_port = htons(9000);

	serveraddr.sin_addr.s_addr = inet_addr("127.0.0.1");
	retval = connect(sock, (SOCKADDR*)&serveraddr, sizeof(serveraddr));
	if (retval == SOCKET_ERROR) err_quit("connect()");


	//------------------파일 IO---------------------------------

	// 파일 열기
	FILE* fp = fopen(FILE_NAME, "rb");
	if (fp == NULL) { perror("파일 입출력 오류");		return -1; }

	// 1. 파일 이름 보내기(100byte)
	char filename[100];
	ZeroMemory(filename, 100);
	sprintf_s(filename, FILE_NAME);
	retval = send(sock, filename, 100, 0);
	if (retval == SOCKET_ERROR) err_quit("send()");


	// 2. 파일 크기 얻기
	fseek(fp, 0, SEEK_END);//파일포인터를 파일의 끝으로 이동!
	int totalbytes = ftell(fp);//해당 위치의 값 반환

	//3.파일 크기 보내기(4byte)
	retval = send(sock, (char*)&totalbytes, sizeof(totalbytes), 0);
	if (retval == SOCKET_ERROR) err_quit("send()");



	// 4. 파일 데이터 전송에 사용할 변수
	char buf[BUFSIZE];	//4096byte	
	int numread;
	int numtotal = 0;

	// 파일 데이터 보내기
	rewind(fp); // 파일 포인터를 제일 앞으로 이동 //1*BUFSIZE
	//1.시나리오 : buf의 크기 40696, 파일크기 : 4096보다 적을 수 있다.
	//			  한번의 Read로 파일의 전체 데이터 획득.. 이때 반환값은 실제 파일의 크기!
	//2.시나리오 : buf의 크기 40696, 파일크기 : 4096보다 클 수 있다.
	//			  여러번의 Read로 파일의 전체 데이터 획득.. 예) 4096 + 4096 + 4096 + 10

	while (1) {
		numread = fread(buf, 1, BUFSIZE, fp);
		if (numread > 0) {
			retval = send(sock, buf, numread, 0);
			if (retval == SOCKET_ERROR) {
				err_display("send()");				break;
			}
			numtotal += numread;
		}
		else if (numread == 0 && numtotal == totalbytes) {
			printf("파일 전송 완료!: %d 바이트\n", numtotal);			break;
		}
		else {
			perror("파일 입출력 오류");			break;
		}
	}
	fclose(fp);
	// closesocket()
	closesocket(sock);
	// 윈속 종료	
	WSACleanup();
	return 0;
}



// 소켓 함수 오류 출력 후 종료
void err_quit(const char* msg)
{
	LPVOID lpMsgBuf;
	FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER |
		FORMAT_MESSAGE_FROM_SYSTEM,
		NULL, WSAGetLastError(),
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR)&lpMsgBuf, 0, NULL);
	MessageBox(NULL, (LPCTSTR)lpMsgBuf, msg, MB_ICONERROR);
	LocalFree(lpMsgBuf);
	exit(-1);
}

// 소켓 함수 오류 출력
void err_display(const char* msg)
{
	LPVOID lpMsgBuf;
	FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER |
		FORMAT_MESSAGE_FROM_SYSTEM,
		NULL, WSAGetLastError(),
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR)&lpMsgBuf, 0, NULL);
	printf("[%s] %s", msg, (LPCTSTR)lpMsgBuf);
	LocalFree(lpMsgBuf);
}



