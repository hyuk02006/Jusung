// Win32 20210513.cpp : 애플리케이션에 대한 진입점을 정의합니다.
//

#include "framework.h"
#include "Win32 20210513.h"

#define MAX_LOADSTRING 100

// 전역 변수:
HINSTANCE hInst;                                // 현재 인스턴스입니다.
WCHAR szTitle[MAX_LOADSTRING];                  // 제목 표시줄 텍스트입니다. (sz = null로 끝나는 문자앞에 붙이는 헝가리언 표기법)
WCHAR szWindowClass[MAX_LOADSTRING];            // 기본 창 클래스 이름입니다.

// 이 코드 모듈에 포함된 함수의 선언을 전달합니다:
ATOM                MyRegisterClass(HINSTANCE hInstance);
BOOL                InitInstance(HINSTANCE, int);
LRESULT CALLBACK    WndProc(HWND, UINT, WPARAM, LPARAM);	//포수 (받는함수)
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
	LoadString(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING);	//LoadString() = IDS_APP_TITLE= 리소스에 있는 스트링 읽어와라
	LoadStringW(hInstance, IDC_WIN3220210513, szWindowClass, MAX_LOADSTRING);
	MyRegisterClass(hInstance);

	// 애플리케이션 초기화를 수행합니다:
	if (!InitInstance(hInstance, nCmdShow))		//윈도우의 탄생
	{
		return FALSE;
	}

	HACCEL hAccelTable = LoadAccelerators(hInstance, MAKEINTRESOURCE(IDC_WIN3220210513));

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
ATOM MyRegisterClass(HINSTANCE hInstance)	//나의 윈도우를 등록하는 과정!
{
	WNDCLASSEXW wcex;

	wcex.cbSize = sizeof(WNDCLASSEX);

	wcex.style = CS_HREDRAW | CS_VREDRAW;
	wcex.lpfnWndProc = WndProc; //포수이름(레지스터클래스에서 가장 중요함)
	wcex.cbClsExtra = 0;
	wcex.cbWndExtra = 0;
	wcex.hInstance = hInstance;
	wcex.hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_WIN3220210513));
	wcex.hCursor = LoadCursor(nullptr, IDC_HAND);
	wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
	// wcex.hbrBackground = (HBRUSH)GetStockObject(GRAY_BRUSH);
	wcex.lpszMenuName = MAKEINTRESOURCEW(IDC_WIN3220210513);
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

	HWND hWnd = CreateWindowW(szWindowClass, szTitle, WS_OVERLAPPEDWINDOW,
		CW_USEDEFAULT, 0, CW_USEDEFAULT, 0, nullptr, nullptr, hInstance, nullptr);
	
	//HWND hWnd = CreateWindowW(_T("EDIT") //btn,edit 가능   , szTitle, WS_OVERLAPPEDWINDOW,		
	//	CW_USEDEFAULT, 0, CW_USEDEFAULT, 0, nullptr, nullptr, hInstance, nullptr);

	if (!hWnd)
	{
		return FALSE;
	}

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
HWND hStatic;
HWND hEdit;
HWND hButtonSave;
#define ID_EDIT_NAME 100
#define ID_BTN_SAVE 101
#define ID_BTN_PRINT 102

int nTop = 100;

LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{

	switch (message)
	{

	case WM_CREATE:
	{
		hStatic = CreateWindowW(_T("STATIC"), _T("이름 : "),	//hStatic = 핸들을 받는거요~
			WS_CHILD | WS_VISIBLE,
			20, 20, 100, 25,
			hWnd, (HMENU)-1, hInst, NULL);

		hEdit = CreateWindowW(_T("EDIT"), _T("바보야"),
			WS_CHILD | WS_VISIBLE | WS_BORDER | ES_AUTOHSCROLL,
			20, 100, 200, 25,
			hWnd, (HMENU)ID_EDIT_NAME, hInst, NULL);

		hButtonSave = CreateWindowW(_T("BUTTON"), _T("저장"),
			WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON,
			300, 20, 100, 25,
			hWnd, (HMENU)ID_BTN_SAVE, hInst, NULL);

		CreateWindowW(_T("BUTTON"), _T("인쇄"),
			WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON,
			300, 50, 100, 25,
			hWnd, (HMENU)ID_BTN_PRINT, hInst, NULL);
	}
	break;
	case WM_COMMAND:
	{

		// 메뉴 선택을 구문 분석합니다:
		switch (LOWORD(wParam))
		{
		case ID_BTN_SAVE:
			MessageBox(hWnd, TEXT("저장을 시작합니다."), TEXT("Button"), MB_OK);
			

			break;
		case ID_BTN_PRINT:
			MessageBox(hWnd, TEXT("인쇄을 시작합니다."), TEXT("Button"), MB_OK);
			break;
		case ID_EDIT_NAME:
			if (HIWORD(wParam) == EN_CHANGE)
			{
				TCHAR str[128];
				GetWindowText(hEdit, str, 128);
				SetWindowText(hWnd, str);
				SetWindowText(hStatic, str);
				//SetWindowText(hEdit, str); //무한루프에 빠짐


			}
			break;
		}
	}
	break;
	case WM_LBUTTONDOWN :
	{
		SetParent(hButtonSave, GetDesktopWindow()); //버튼의 부모를 바탕화면으로
		SetWindowText(hButtonSave, _T("가출한 버튼"));
	}
	break;

	case WM_RBUTTONDOWN:
	{
		SetParent(hButtonSave, hWnd);
		SetWindowText(hButtonSave, _T("돌아온 버튼"));


	}
	break;
	case WM_KEYDOWN: //키보드에서 키가 눌림
	{
		switch (wParam)
		{

		case VK_UP:
			nTop -= 10;
			break;
		case VK_DOWN:
			nTop += 10;
			break;
		case VK_LEFT:
			BOOL static bshow = TRUE;
			if (bshow)
			{
				bshow = FALSE;
				ShowWindow(hEdit, SW_HIDE);
				SetWindowText(hWnd, _T("사라졌다"));
				SetWindowText(hButtonSave, _T("사라졌다"));
			}
			else
			{
				bshow = TRUE;
				ShowWindow(hEdit, SW_SHOW);
				SetWindowText(hWnd, _T("나타났다"));
				SetWindowText(hButtonSave, _T("나타났다"));

			}
		}
		MoveWindow(hEdit, 10, nTop, 200, 25, TRUE);
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

