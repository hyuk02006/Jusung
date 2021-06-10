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
SOCKET clientSock;
HANDLE hEvent;

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

// 완료 루틴
void CALLBACK CompletionRoutine(DWORD dwError, DWORD cbTransferred,LPWSAOVERLAPPED lpOverlapped, DWORD dwFlags)
{
	int retval;
	// 클라이언트 정보 얻기
	SOCKETINFO* ptr = (SOCKETINFO*)lpOverlapped;
	SOCKADDR_IN clientaddr;
	int addrlen = sizeof(clientaddr);
	getpeername(ptr->sock, (SOCKADDR*)&clientaddr, &addrlen);
	// 비동기 입출력 결과 확인
	if (dwError != 0 || cbTransferred == 0) {
		if (dwError != 0) DisplayMessage();
		closesocket(ptr->sock);
		printf("[TCP 서버] 클라이언트 종료: IP 주소=%s, 포트 번호=%d\n",
			inet_ntoa(clientaddr.sin_addr), ntohs(clientaddr.sin_port));
		delete ptr;
		return;
	}

	// 데이터 전송량 갱신
	if (ptr->recvbytes == 0) {
		ptr->recvbytes = cbTransferred;
		ptr->sendbytes = 0;
		// 받은 데이터 출력
		ptr->buf[ptr->recvbytes] = '\0';
		printf("[TCP/%s:%d] %s\n", inet_ntoa(clientaddr.sin_addr),
			ntohs(clientaddr.sin_port), ptr->buf);
	}
	else {
		ptr->sendbytes += cbTransferred;
	}
	if (ptr->recvbytes > ptr->sendbytes) {
		// 데이터 보내기
		ZeroMemory(&(ptr->overlapped), sizeof(ptr->overlapped));
		ptr->wsabuf.buf = ptr->buf + ptr->sendbytes;
		ptr->wsabuf.len = ptr->recvbytes - ptr->sendbytes;
		DWORD sendbytes;
		retval = WSASend(ptr->sock, &(ptr->wsabuf), 1, &sendbytes,
			0, &(ptr->overlapped), CompletionRoutine);
		if (retval == SOCKET_ERROR) {
			if (WSAGetLastError() != WSA_IO_PENDING) {
				DisplayMessage();
				return;
			}
		}
	}
	else {
		ptr->recvbytes = 0;
		// 데이터 받기
		ZeroMemory(&(ptr->overlapped), sizeof(ptr->overlapped));
		ptr->wsabuf.buf = ptr->buf;
		ptr->wsabuf.len = BUFSIZE;
		DWORD recvbytes;
		DWORD flags = 0;
		retval = WSARecv(ptr->sock, &(ptr->wsabuf), 1, &recvbytes,
			&flags, &(ptr->overlapped), CompletionRoutine);
		if (retval == SOCKET_ERROR) {
			if (WSAGetLastError() != WSA_IO_PENDING) {
				DisplayMessage();
				return;
			}
		}
	}
}

DWORD WINAPI WorkerThread(LPVOID pParam)
{
	HANDLE hEvent = (HANDLE)pParam;
	int retval;
	while (1) {
		while (1) {
			// alertable wait
			// 클라이언트 접속~
			// 비동기 결과 발생(WAIT_IO_COMPLETION)
			// =이때 콜백 함수가 호출
			DWORD result = WaitForSingleObjectEx(hEvent,
				INFINITE, TRUE);
			if (result == WAIT_OBJECT_0) break;
			if (result != WAIT_IO_COMPLETION) return -1;
		}
		// 접속한 클라이언트 정보 출력
		SOCKADDR_IN clientaddr;
		int addrlen = sizeof(clientaddr);
		getpeername(clientSock, (SOCKADDR*)&clientaddr, &addrlen);
		printf("[TCP 서버] 클라이언트 접속: IP 주소=%s, 포트 번호=%d\n",
			inet_ntoa(clientaddr.sin_addr),
			ntohs(clientaddr.sin_port));
		// 소켓 정보 구조체 할당과 초기화
		SOCKETINFO* ptr = new SOCKETINFO;
		if (ptr == NULL) {
			printf("[오류] 메모리가 부족합니다!\n");
			return -1;
		}
		ZeroMemory(&(ptr->overlapped), sizeof(ptr->overlapped));
		ptr->sock = clientSock;
		ptr->recvbytes = 0;
		ptr->sendbytes = 0;
		ptr->wsabuf.buf = ptr->buf;
		ptr->wsabuf.len = BUFSIZE;
		// 비동기 입출력 시작
		DWORD recvbytes;
		DWORD flags = 0;
		retval = WSARecv(ptr->sock, &(ptr->wsabuf), 1, &recvbytes,
			&flags, &(ptr->overlapped), CompletionRoutine);
		if (retval == SOCKET_ERROR) {
			if (WSAGetLastError() != WSA_IO_PENDING) {
				DisplayMessage();
				return -1;
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

	// 임시 이벤트 객체 생성
	hEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
	if (hEvent == NULL) false;

	// 대기 소켓의 로컬 주소, 포트 설정
	SOCKADDR_IN serveraddr;
	ZeroMemory(&serveraddr, sizeof(serveraddr));
	serveraddr.sin_family = AF_INET;
	serveraddr.sin_port = htons(9000);
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
	// 대기 소켓 초기화(socket()+bind()+listen())
	if (!CreateListenSocket())
	{
		printf("대기 소켓 생성 실패!\n");
		return -1;
	}
	// 대기 쓰레드 종료를 기다림.
	DWORD threadID;
	CloseHandle(CreateThread(0, 0, WorkerThread, (void*)hEvent, 0,
		&threadID));
	while (1) {
		// accept()
		clientSock = accept(listenSock, NULL, NULL);
		if (clientSock == INVALID_SOCKET) {
			DisplayMessage();
			continue;
		}
		if (!SetEvent(hEvent)) break;
	}
	// 윈속 종료
	WSACleanup();
	return 0;
}