//filecilent.cpp

#include<stdio.h>
#include <conio.h>
#include "CServer.h"
#include "CClient.h"
#include "CFileSend.h"
#include "CFileRecv.h"

#define IP "192.168.0.95"
#define PORT 9000
#define FILE_PATH "C:\\Users\\bit\\bit391\\Network Programing\\20210610 FileClient\\sample.pdf"


int main()
{
	HANDLE hEvent;
	hEvent=CreateEvent(0, FALSE, FALSE, "e");//자동 , nonsignal
	printf("=================================\n");
	printf("[1]Server [2]Client\n");
	printf("=================================\n");
	char idx = _getch(); //#include <conio.h>

	if (idx == '1')
	{
		CServer server; //전송
		server.CreateSocket(PORT);
		printf("클라이언트 접속대기\n");
		WaitForSingleObject(hEvent, INFINITE);
		printf("클라이언트 접속\n");
		Sleep(1000);
		CFileSend send(server.getSock());
		send.FileSend(FILE_PATH);
		
	}
	else if (idx == '2')
	{
		CClient client; //수신
		client.CreateSocket(IP, PORT);
		CFileRecv recv(client.getSock());
		if(recv.FileRecv() ==true)
			printf("파일 수신완료\n");
		else
			printf("파일 수신실패\n");

	}
	CloseHandle(hEvent);
	return 0;
}