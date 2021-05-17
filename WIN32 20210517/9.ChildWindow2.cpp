// WIN32 20210517.cpp : 애플리케이션에 대한 진입점을 정의합니다.
//

#include "framework.h"
#include "WIN32 20210517.h"

#define MAX_LOADSTRING 100
// 전역 변수:
HINSTANCE hInst;                                // 현재 인스턴스입니다.
WCHAR szTitle[MAX_LOADSTRING];                  // 제목 표시줄 텍스트입니다. (sz = null로 끝나는 문자앞에 붙이는 헝가리언 표기법)
WCHAR szWindowClass[MAX_LOADSTRING];            // 기본 창 클래스 이름입니다.

// 이 코드 모듈에 포함된 함수의 선언을 전달합니다:
ATOM                MyRegisterClass(HINSTANCE hInstance);
BOOL                InitInstance(HINSTANCE, int);
LRESULT CALLBACK    WndProc(HWND, UINT, WPARAM, LPARAM);
LRESULT CALLBACK    ChildProc(HWND, UINT, WPARAM, LPARAM);
LPCTSTR lpzsChildClass = _T("ChildWnd");

int APIENTRY wWinMain(_In_ HINSTANCE hInstance,
	_In_opt_ HINSTANCE hPrevInstance,
	_In_ LPWSTR    lpCmdLine,
	_In_ int       nCmdShow)
{
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);

	// TODO: 여기에 코드를 입력합니다.

	// 전역 문자열을 초기화합니다.
	LoadString(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING); //IDS_APP_TITLE= 리소스에 있는 스트링 읽어와라
	LoadStringW(hInstance, IDC_WIN3220210517, szWindowClass, MAX_LOADSTRING);
	MyRegisterClass(hInstance);


	// 애플리케이션 초기화를 수행합니다:
	if (!InitInstance(hInstance, nCmdShow))		//윈도우의 탄생
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
	wcex.lpszMenuName = MAKEINTRESOURCEW(IDC_WIN3220210517);
	wcex.lpszClassName = szWindowClass;
	wcex.hIconSm = LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));
	RegisterClassExW(&wcex);

	///////////////////////////////////////////////////////
	//1.Child class
	wcex.lpfnWndProc = ChildProc;
	wcex.lpszClassName = lpzsChildClass;
	wcex.hCursor = LoadCursor(nullptr, IDC_CROSS);
	wcex.hbrBackground = (HBRUSH)GetStockObject(LTGRAY_BRUSH);


	RegisterClassExW(&wcex);

	return 0;
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

	HWND hWnd = CreateWindowW(szWindowClass, szTitle, WS_OVERLAPPEDWINDOW,
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

	switch (message)
	{

	case WM_CREATE:	//메인 윈도우가 생성될때
	{
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				CreateWindow(lpzsChildClass, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN | WS_BORDER,
					i*100, j*100, 100, 100,
					hWnd, (HMENU)0, hInst, NULL);
			}
		}
		/*
		//Child Window
		CreateWindow(lpzsChildClass, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN | WS_BORDER,
			100, 100, 100, 100,
			hWnd, (HMENU)0, hInst, NULL);
		*/
		//PopUp Window
		/*CreateWindow(lpzsChildClass, _T("안녕난팝업"), WS_POPUP | WS_VISIBLE | WS_CLIPCHILDREN | WS_CAPTION | WS_SYSMENU,
			100, 100, 500, 500,
			hWnd, (HMENU)0, hInst, NULL);
		*/
	}
	break;
	case WM_PAINT:
	{
		PAINTSTRUCT ps;
		HDC hdc = BeginPaint(hWnd, &ps);
		// TODO: 여기에 hdc를 사용하는 그리기 코드를 추가합니다...

		EndPaint(hWnd, &ps);
	}
	break;
	case WM_DESTROY:
		PostQuitMessage(0);
		break;
	default:
		return DefWindowProc(hWnd, message, wParam, lParam);    //중요
	}
	return 0;
}

BOOL bEllipse = TRUE;

LRESULT CALLBACK ChildProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	BOOL bProp;
	switch (message)
	{
	
	case WM_CREATE:
	{
		SetProp(hWnd, _T("bEllipse"), (HANDLE)TRUE);
		SetProp(hWnd, _T("nCount"), (HANDLE)3);

	}
	break;
	case WM_LBUTTONDOWN:
	{
		//bEllipse = !bEllipse;
		bProp = (BOOL)GetProp(hWnd, _T("bEllipse"));
		SetProp(hWnd, _T("bEllipse"), (HANDLE)!bProp);
		InvalidateRect(hWnd, NULL, TRUE);
	}
	break;
	case WM_PAINT:
	{
		PAINTSTRUCT ps;
		HDC hdc = BeginPaint(hWnd, &ps);
		// TODO: 여기에 hdc를 사용하는 그리기 코드를 추가합니다...

		//if (bEllipse)
		bProp=(BOOL)GetProp(hWnd, _T("bEllipse"));
		if(bProp)
			Ellipse(hdc, 10, 10, 90, 90);
		else
		{
			MoveToEx(hdc, 10, 10, NULL);	//어디점으로 이동해라
			LineTo(hdc, 90, 90);			//라인을 떙겨라
			MoveToEx(hdc, 10, 90, NULL);
			LineTo(hdc, 90, 10);

		}
		EndPaint(hWnd, &ps);
	}
	break;
	case WM_DESTROY:
	{
		RemoveProp(hWnd, _T("bEllipse"));
	}
	break;
	default:
		return DefWindowProc(hWnd, message, wParam, lParam);    //중요
	}
	return 0;

}
