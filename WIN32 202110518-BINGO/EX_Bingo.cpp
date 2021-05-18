// WIN32 20210517.cpp : 애플리케이션에 대한 진입점을 정의합니다.
//

#include "framework.h"
#include "WIN32 20210518.h"


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


#define MAX 4
BOOL g_bMyTurn = TRUE;
HWND g_Main;
HWND g_Child[MAX][MAX] = { 0, };
#define WM_CHECKBINGO WM_USER +1


enum modeBINGO {
	bingoNONE,
	bingoMINE,
	bingoMyBINGO,
	bingoYOURS,
	bingoYourBINGO
};

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
	LoadStringW(hInstance, IDC_WIN3220210518, szWindowClass, MAX_LOADSTRING);
	MyRegisterClass(hInstance);


	// 애플리케이션 초기화를 수행합니다:
	if (!InitInstance(hInstance, nCmdShow))		//윈도우의 탄생
	{
		return FALSE;
	}


	HACCEL hAccelTable = LoadAccelerators(hInstance, MAKEINTRESOURCE(IDC_WIN3220210518));

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
	wcex.hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_WIN3220210518));
	wcex.hCursor = LoadCursor(nullptr, IDC_HAND);
	wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
	wcex.lpszMenuName = MAKEINTRESOURCEW(IDC_WIN3220210518);
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
	g_Main = hWnd;
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
				g_Child[i][j] = CreateWindow(lpzsChildClass, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN | WS_BORDER,
					i * 100, j * 100, 100, 100,
					hWnd, (HMENU)0, hInst, NULL);
			}
		}
		SetProp(hWnd, _T("CkBingo"), (HANDLE)bingoNONE);

	}
	break;
	case WM_CHECKBINGO:
	{
		/////////////////////////////////////
		//1. 현재 차일드의 정보
		HWND hCurChild = (HWND)wParam;
		int nCurMode = (int)lParam;

		int x = 0;
		int y = 0;
		for (int i = 0; i < MAX; i++)
		{
			for (int j = 0; j < MAX; j++)
			{
				if (g_Child[i][j] == hCurChild)
				{
					x = i;
					y = j;
					break;
				}
			}
		}

		/////////////////////////////////////
		//2. 빙고를 체크한다.
		int nBingoCount;
	/*
		//세로
		for (int i = 0; i < MAX; i++)
			if ((int)GetProp(g_Child[x][i], _T("CkBingo")) == bingoMINE)
				Mine++;
			else if ((int)GetProp(g_Child[x][i], _T("CkBingo")) == bingoYOURS)
				you++;

		if (Mine == 3)
			for (int i = 0; i < MAX; i++)
			{
				SetProp(g_Child[x][i], _T("CkBingo"), (HANDLE)bingoMyBINGO);
				InvalidateRect(hWnd, NULL, TRUE);
			}
		else if (you == 3)
			for (int i = 0; i < MAX; i++)
			{
				SetProp(g_Child[x][i], _T("CkBingo"), (HANDLE)bingoYourBINGO);
				InvalidateRect(hWnd, NULL, TRUE);
			}
*/
		//세로
		nBingoCount = 0;
		for (int i = 0; i < MAX; i++)
		{
			int tmp = (int)GetProp(g_Child[x][i], _T("CkBingo"));
			if (tmp == nCurMode)
				nBingoCount++;
		}
		if (nBingoCount == 3)
		{
			for (int i = 0; i < MAX; i++)
			{
				SetProp(g_Child[x][i], _T("CkBingo"), (HANDLE)(nCurMode + 1));//enum
				InvalidateRect(g_Child[x][i], NULL, TRUE);
			}
		}

		//가로
		nBingoCount = 0;
		for (int i = 0; i < MAX; i++)
		{
			int tmp = (int)GetProp(g_Child[i][y], _T("CkBingo"));
			if (tmp == nCurMode)
				nBingoCount++;

		}
		if (nBingoCount == 3)
		{
			for (int i = 0; i < MAX; i++)
			{
				SetProp(g_Child[i][y], _T("CkBingo"), (HANDLE)(nCurMode+1));//enum
				InvalidateRect(g_Child[i][y], NULL, TRUE);
			}
		}

		//대각선
		nBingoCount = 0;
		for (int i = 0; i < MAX; ++i)
		{
			int tmp = (int)GetProp(g_Child[i][i], _T("CkBingo"));

			if (tmp == nCurMode)
				nBingoCount++;
		}
		
		for (int i = 0; i < 5; ++i)
		{
			int tmp = (int)GetProp(g_Child[x-i-1][i], _T("CkBingo"));
			if (tmp==nCurMode)
				nBingoCount++;
		}
		if (nBingoCount == 3)
		{
			for (int i = 0; i < MAX; i++)
			{
				SetProp(g_Child[i][y], _T("CkBingo"), (HANDLE)(nCurMode + 1));//enum
				InvalidateRect(g_Child[i][y], NULL, TRUE);
			}
		}


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

	break;
	case WM_DESTROY:
		PostQuitMessage(0);
		break;
	default:
		return DefWindowProc(hWnd, message, wParam, lParam);    //중요
	}
	return 0;
}

LRESULT CALLBACK ChildProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	int ckProp;
	switch (message)
	{
	case WM_CREATE:
	{
		SetProp(hWnd, _T("CkBingo"), (HANDLE)bingoNONE);

	}
	break;


	case WM_LBUTTONDOWN:
	{
		ckProp = (int)GetProp(hWnd, _T("CkBingo"));
		if (ckProp == bingoNONE)
		{
			if (g_bMyTurn == TRUE)
			{
				SetProp(hWnd, _T("CkBingo"), (HANDLE)bingoMINE);

			}
			else
			{
				SetProp(hWnd, _T("CkBingo"), (HANDLE)bingoYOURS);

			}
			InvalidateRect(hWnd, NULL, TRUE);//본인이 그림을 다시 그림
			g_bMyTurn = !g_bMyTurn;
			ckProp = (int)GetProp(hWnd, TEXT("CkBingo"));
			SendMessage(g_Main, WM_CHECKBINGO, (WPARAM)hWnd, ckProp);//상부에 빙고체크 요청


		}
	}
	break;
	case WM_PAINT:
	{
		PAINTSTRUCT ps;
		HDC hdc = BeginPaint(hWnd, &ps);
		// TODO: 여기에 hdc를 사용하는 그리기 코드를 추가합니다...

		ckProp = (int)GetProp(hWnd, _T("CkBingo"));
		if (ckProp == bingoNONE) {
			Rectangle(hdc, 10, 10, 90, 90);
		}
		else if (ckProp == bingoMINE)
		{
			Ellipse(hdc, 10, 10, 90, 90);
		}
		else if (ckProp == bingoMyBINGO)
		{
			HBRUSH hbr, hdrOld;
			hbr = CreateSolidBrush(RGB(255, 0, 0));
			hdrOld = (HBRUSH)SelectObject(hdc, hbr);
			Rectangle(hdc, 10, 10, 90, 90);
			SetBkMode(hdc, TRANSPARENT);
			TextOut(hdc, 10, 10, _T("나의 빙고"), 5);
			DeleteObject(hbr);
		}
		else if (ckProp == bingoYOURS)
		{
			MoveToEx(hdc, 10, 10, NULL);
			LineTo(hdc, 90, 90);
			MoveToEx(hdc, 10, 90, NULL);
			LineTo(hdc, 90, 10);
		}
		else
		{
			HBRUSH hbr1, hdrOld1;
			hbr1 = CreateSolidBrush(RGB(0, 0, 255));
			hdrOld1 = (HBRUSH)::SelectObject(hdc, hbr1);
			Rectangle(hdc, 10, 10, 90, 90);
			SetBkMode(hdc, TRANSPARENT);
			TextOut(hdc, 10, 10, _T("너의 빙고"), 5);
			DeleteObject(hbr1);

		}
		EndPaint(hWnd, &ps);


	}
	break;

	case WM_DESTROY:
	{

		RemoveProp(hWnd, _T("CkBingo"));

	}
	break;
	default:
		return DefWindowProc(hWnd, message, wParam, lParam);    //중요
	}
	return 0;

}

