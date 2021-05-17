// Win32 20210513.cpp : 애플리케이션에 대한 진입점을 정의합니다.
//

#include "framework.h"
#include "WIN32 20210517.h"

#define MAX_LOADSTRING 100

// 전역 변수:
HINSTANCE hInst;                                // 현재 인스턴스입니다.
WCHAR szTitle[MAX_LOADSTRING];                  // 제목 표시줄 텍스트입니다.
WCHAR szWindowClass[MAX_LOADSTRING];            // 기본 창 클래스 이름입니다.

// 이 코드 모듈에 포함된 함수의 선언을 전달합니다:
ATOM                MyRegisterClass(HINSTANCE hInstance);
BOOL                InitInstance(HINSTANCE, int);
LRESULT CALLBACK    WndProc(HWND, UINT, WPARAM, LPARAM);
INT_PTR CALLBACK    About(HWND, UINT, WPARAM, LPARAM);

int APIENTRY wWinMain(_In_ HINSTANCE hInstance,
	_In_opt_ HINSTANCE hPrevInstance,
	_In_ LPWSTR    lpCmdLine,
	_In_ int       nCmdShow)
{
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);

	// TODO: 여기에 코드를 입력합니다.

	// 전역 문자열을 초기화합니다.
	LoadString(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING);
	LoadStringW(hInstance, IDC_WIN3220210517, szWindowClass, MAX_LOADSTRING);
	MyRegisterClass(hInstance);

	// 애플리케이션 초기화를 수행합니다:
	if (!InitInstance(hInstance, nCmdShow))
	{
		return FALSE;
	}

	HACCEL hAccelTable = LoadAccelerators(hInstance, MAKEINTRESOURCE(IDC_WIN3220210517));

	MSG msg;

	// 기본 메시지 루프입니다:
	while (GetMessage(&msg, nullptr, 0, 0))
	{
		if (!TranslateAccelerator(msg.hwnd, hAccelTable, &msg))
		{
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
	}

	return (int)msg.wParam;
}



//
//  함수: MyRegisterClass()
//
//  용도: 창 클래스를 등록합니다.
//
ATOM MyRegisterClass(HINSTANCE hInstance)
{
	WNDCLASSEXW wcex;

	wcex.cbSize = sizeof(WNDCLASSEX);

	wcex.style = CS_HREDRAW | CS_VREDRAW;

	wcex.lpfnWndProc = WndProc; //포수이름(레지스터클래스에서 가장 중요함)

	wcex.cbClsExtra = 0;
	wcex.cbWndExtra = 0;
	wcex.hInstance = hInstance;
	wcex.hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_WIN3220210517));
	wcex.hCursor = LoadCursor(nullptr, IDC_HAND);
	wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
	// wcex.hbrBackground = (HBRUSH)GetStockObject(GRAY_BRUSH);
	wcex.lpszMenuName = MAKEINTRESOURCEW(IDC_WIN3220210517);
	wcex.lpszClassName = szWindowClass;
	wcex.hIconSm = LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));

	return RegisterClassExW(&wcex);
}

//
//   함수: InitInstance(HINSTANCE, int)
//
//   용도: 인스턴스 핸들을 저장하고 주 창을 만듭니다.
//
//   주석:
//
//        이 함수를 통해 인스턴스 핸들을 전역 변수에 저장하고
//        주 프로그램 창을 만든 다음 표시합니다.
//
BOOL InitInstance(HINSTANCE hInstance, int nCmdShow)
{
	hInst = hInstance; // 인스턴스 핸들을 전역 변수에 저장합니다.

	HWND hWnd = CreateWindowW(szWindowClass,_T("KeyDown"), WS_OVERLAPPEDWINDOW,
		CW_USEDEFAULT, 0, CW_USEDEFAULT, 0, nullptr, nullptr, hInstance, nullptr);


	ShowWindow(hWnd, nCmdShow);
	UpdateWindow(hWnd);

	return TRUE;
}

//
//  함수: WndProc(HWND, UINT, WPARAM, LPARAM)
//
//  용도: 주 창의 메시지를 처리합니다.
//
//  WM_COMMAND  - 애플리케이션 메뉴를 처리합니다.
//  WM_PAINT    - 주 창을 그립니다.
//  WM_DESTROY  - 종료 메시지를 게시하고 반환합니다.
//
//
LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	static TCHAR* mem;
	static TCHAR str[256] = { 0 };
	int len;

	static int x1 = 200;
	static int y1 = 200;
	static int x2 = 200;
	static int y2 = 200;

	switch (message)
	{
	case WM_CHAR:   //키보드에서 문자 키가 눌릴때
	{
		if ((TCHAR)wParam == ' ')
		{
			str[0] = 0;
		}

		else {
			len = lstrlen(str);
			str[len] = (TCHAR)wParam;
			str[len + 1] = 0;
		}
		InvalidateRect(hWnd, NULL, TRUE);//무효화 함수 WM_PAINT발생시킴 

	}
	break;
	case WM_PAINT:
	{

		PAINTSTRUCT ps;
		HDC hdc = BeginPaint(hWnd, &ps);
		// TODO: 여기에 hdc를 사용하는 그리기 코드를 추가합니다...
		TextOut(hdc, x1, y1, _T("O"), 1);
		TextOut(hdc, x2, y2, _T("안녕하세요"), 5);

		TextOut(hdc, 100, 100, str, lstrlen(str));
		EndPaint(hWnd, &ps);
	}
	break;
	case WM_LBUTTONDOWN:
		//case WM_MOUSEMOVE:

	{
		x2 = LOWORD(lParam);
		y2 = HIWORD(lParam);
		//InvalidateRect(hWnd, NULL, FALSE);//무효화 함수 WM_PAINT발생시킴 
		InvalidateRect(hWnd, NULL, FALSE);//무효화 함수 WM_PAINT발생시킴 


	}
	break;
	case WM_KEYDOWN: //키보드에서 키가 눌림
	{
		switch (wParam)
		{
		case VK_LEFT:
			x1 -= 8;
			break;
		case VK_RIGHT:
			x1 += 8;
			break;
		case VK_UP:
			y1 -= 8;
			break;
		case VK_DOWN:
			y1 += 8;
			break;
		}
		InvalidateRect(hWnd, NULL, FALSE);//무효화 함수 WM_PAINT발생시킴 

	}
	break;
	case WM_CREATE:	//윈도우가 생성됨
		mem = (TCHAR*)malloc(104834);
		break;
	case WM_DESTROY: //윈도우가 파괴됨
		free(mem);
		PostQuitMessage(0);
		break;
	default:
		return DefWindowProc(hWnd, message, wParam, lParam);    //중요
	}
	return 0;
}

