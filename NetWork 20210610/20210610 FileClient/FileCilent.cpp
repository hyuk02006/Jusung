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
	hEvent=CreateEvent(0, FALSE, FALSE, "e");//�ڵ� , nonsignal
	printf("=================================\n");
	printf("[1]Server [2]Client\n");
	printf("=================================\n");
	char idx = _getch(); //#include <conio.h>

	if (idx == '1')
	{
		CServer server; //����
		server.CreateSocket(PORT);
		printf("Ŭ���̾�Ʈ ���Ӵ��\n");
		WaitForSingleObject(hEvent, INFINITE);
		printf("Ŭ���̾�Ʈ ����\n");
		Sleep(1000);
		CFileSend send(server.getSock());
		send.FileSend(FILE_PATH);
		
	}
	else if (idx == '2')
	{
		CClient client; //����
		client.CreateSocket(IP, PORT);
		CFileRecv recv(client.getSock());
		if(recv.FileRecv() ==true)
			printf("���� ���ſϷ�\n");
		else
			printf("���� ���Ž���\n");

	}
	CloseHandle(hEvent);
	return 0;
}