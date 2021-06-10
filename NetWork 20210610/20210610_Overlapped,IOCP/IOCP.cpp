#define _WINSOCK_DEPRECATED_NO_WARNINGS

#include <winsock2.h>
#pragma comment(lib, "ws2_32.lib")
#include <process.h>
#include <stdio.h>

#define BUFSIZE 1024

// 소켓 정보 저장을 위한 구조체
struct SOCKETINFO
{
	WSAOVERLAPPED overlapped;
	SOCKET sock;
	char buf[BUFSIZE];
	int recvbytes;
	int sendbytes;
	WSABUF wsabuf;
};
SOCKET listenSock;

void DisplayMessage()
{
	LPVOID pMsg;
	FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER |
		FORMAT_MESSAGE_FROM_SYSTEM,
		NULL,
		WSAGetLastError(),
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR)&pMsg,
		0, NULL);
	printf("%s\n", pMsg);
	LocalFree(pMsg);
}

// 작업 쓰레드
unsigned int WINAPI WorkerThread(void* pParam)
{
	HANDLE hcp = (HANDLE)pParam;
	int retval;
	while (1) 
	{
		// 비동기 입출력 완료 기다리기
		DWORD cbTransferred;
		SOCKET client_sock;
		SOCKETINFO* ptr;
		retval = GetQueuedCompletionStatus(hcp, &cbTransferred,
			(LPDWORD)&client_sock, (LPOVERLAPPED*)&ptr, INFINITE);
		// 클라이언트 정보 얻기
		SOCKADDR_IN clientaddr;
		int addrlen = sizeof(clientaddr);
		getpeername(ptr->sock, (SOCKADDR*)&clientaddr, &addrlen);
		// 비동기 입출력 결과 확인
		if (retval == 0 || cbTransferred == 0)
		{
			if (retval == 0) 
			{
				DWORD temp1, temp2;
				WSAGetOverlappedResult(ptr->sock,
					&(ptr->overlapped), &temp1, FALSE, &temp2);
				DisplayMessage();
			}
			closesocket(ptr->sock);
			printf("[TCP 서버] 클라이언트 종료: IP 주소=%s, 포트 번호=%d\n",
				inet_ntoa(clientaddr.sin_addr),
				ntohs(clientaddr.sin_port));
			delete ptr;
			continue;
		}

		// 데이터 전송량 갱신
		if (ptr->recvbytes == 0)
		{
			ptr->recvbytes = cbTransferred;
			ptr->sendbytes = 0;
			// 받은 데이터 출력
			ptr->buf[ptr->recvbytes] = '\0';
			printf("[TCP/%s:%d] %s\n", inet_ntoa(clientaddr.sin_addr),
				ntohs(clientaddr.sin_port), ptr->buf);
		}
		else
		{
			ptr->sendbytes += cbTransferred;
		}
		if (ptr->recvbytes > ptr->sendbytes)
		{
			// 데이터 보내기
			ZeroMemory(&(ptr->overlapped), sizeof(ptr->overlapped));
			ptr->wsabuf.buf = ptr->buf + ptr->sendbytes;
			ptr->wsabuf.len = ptr->recvbytes - ptr->sendbytes;
			DWORD sendbytes;
			retval = WSASend(ptr->sock, &(ptr->wsabuf), 1,
				&sendbytes, 0, &(ptr->overlapped), NULL);
			if (retval == SOCKET_ERROR) 
			{
				if (WSAGetLastError() != WSA_IO_PENDING) 
				{
					DisplayMessage();
				}
				continue;
			}
		}
		else
		{
			ptr->recvbytes = 0;
			// 데이터 받기
			ZeroMemory(&(ptr->overlapped), sizeof(ptr->overlapped));
			ptr->wsabuf.buf = ptr->buf;
			ptr->wsabuf.len = BUFSIZE;
			DWORD recvbytes;
			DWORD flags = 0;
			retval = WSARecv(ptr->sock, &(ptr->wsabuf), 1,
				&recvbytes, &flags, &(ptr->overlapped), NULL);
			if (retval == SOCKET_ERROR)
			{
				if (WSAGetLastError() != WSA_IO_PENDING) 
				{
					DisplayMessage();
				}
				continue;
			}
		}
	}
	return 0;
}

bool CreateListenSocket()
{
	int retval;

	// 대기 소켓 생성
	listenSock = socket(AF_INET, SOCK_STREAM, 0);
	if (listenSock == INVALID_SOCKET)
	{
		DisplayMessage();
		return false;
	}
	// 대기 소켓의 로컬 주소, 포트 설정
	SOCKADDR_IN serveraddr;
	ZeroMemory(&serveraddr, sizeof(serveraddr));
	serveraddr.sin_family = AF_INET;
	serveraddr.sin_port = htons(40100);
	serveraddr.sin_addr.s_addr = htonl(INADDR_ANY);
	retval = bind(listenSock, (SOCKADDR*)&serveraddr, sizeof(serveraddr));
	if (retval == SOCKET_ERROR)
	{
		DisplayMessage();
		return false;
	}
	// 대기 소켓의 접속 대기 큐 생성 및 클라이언트 접속 대기
	retval = listen(listenSock, SOMAXCONN);
	if (retval == SOCKET_ERROR)
	{
		DisplayMessage();
		return false;
	}
	return true;
}

int main(int argc, char* argv[])
{
	WSADATA wsa;

	if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
	{
		printf("윈도우 소켓 초기화 실패!\n");
		return -1;
	}
	// 입출력 완료 포트 생성
	HANDLE hcp = CreateIoCompletionPort(
		INVALID_HANDLE_VALUE, NULL, 0, 0);
	if (hcp == NULL) return -1;
	// CPU 개수 확인
	SYSTEM_INFO si;
	GetSystemInfo(&si);
	// 작업자 스레드 생성
	HANDLE hThread;
	unsigned int ThreadId;

	for (int i = 0; i < (int)si.dwNumberOfProcessors * 2; i++)
	{
		hThread = (HANDLE)_beginthreadex(NULL, 0, WorkerThread, (void*)hcp, 0,
			&ThreadId);
		if (hThread == NULL) return -1;
		CloseHandle(hThread);
	}

	//================================================================
	// 대기 소켓 초기화(socket()+bind()+listen())
	if (!CreateListenSocket())
	{
		printf("대기 소켓 생성 실패!\n");
		return -1;
	}
	//================================================================

	while (1) {
		int retval;
		SOCKADDR_IN clientaddr;
		int addrlen = sizeof(clientaddr);
		SOCKET client_sock = accept(listenSock, (SOCKADDR*)&clientaddr,
			&addrlen);
		if (client_sock == INVALID_SOCKET) {
			DisplayMessage();
			continue;
		}
		printf("[TCP 서버] 클라이언트 접속: IP 주소=%s, 포트 번호=%d\n",
			inet_ntoa(clientaddr.sin_addr), ntohs(clientaddr.sin_port));
		// 소켓과 입출력 완료 포트 연결
		HANDLE hResult = CreateIoCompletionPort((HANDLE)client_sock, hcp,
			(DWORD)client_sock, 0);
		if (hResult == NULL) return -1;

		// 소켓 정보 구조체 할당(비동기 호출 준비 및 호출 -WSARecv)
		SOCKETINFO* ptr = new SOCKETINFO;
		if (ptr == NULL)
		{
			printf("[오류] 메모리가 부족합니다!\n");
			break;
		}
		ZeroMemory(&(ptr->overlapped), sizeof(ptr->overlapped));
		ptr->sock = client_sock;
		ptr->recvbytes = 0;
		ptr->sendbytes = 0;
		ptr->wsabuf.buf = ptr->buf;
		ptr->wsabuf.len = BUFSIZE;
		// 비동기 입출력 시작
		DWORD recvbytes;
		DWORD flags = 0;
		retval = WSARecv(client_sock, &(ptr->wsabuf), 1, &recvbytes,
			&flags, &(ptr->overlapped), NULL);
		if (retval == SOCKET_ERROR)
		{
			if (WSAGetLastError() != ERROR_IO_PENDING) {
				DisplayMessage();
			}
			continue;
		}
	}

	// 윈속 종료
	WSACleanup();
	return 0;
}