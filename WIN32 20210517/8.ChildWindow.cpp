// WIN32 20210517.cpp : ���ø����̼ǿ� ���� �������� �����մϴ�.
//

#include "framework.h"
#include "WIN32 20210517.h"

#define MAX_LOADSTRING 100
#define ID_LISTBOX 100
// ���� ����:
HINSTANCE hInst;                                // ���� �ν��Ͻ��Դϴ�.
WCHAR szTitle[MAX_LOADSTRING];                  // ���� ǥ���� �ؽ�Ʈ�Դϴ�. (sz = null�� ������ ���ھտ� ���̴� �밡���� ǥ���)
WCHAR szWindowClass[MAX_LOADSTRING];            // �⺻ â Ŭ���� �̸��Դϴ�.

// �� �ڵ� ��⿡ ���Ե� �Լ��� ������ �����մϴ�:
ATOM                MyRegisterClass(HINSTANCE hInstance);
BOOL                InitInstance(HINSTANCE, int);
LRESULT CALLBACK    WndProc(HWND, UINT, WPARAM, LPARAM);
INT_PTR CALLBACK    About(HWND, UINT, WPARAM, LPARAM);
//������ ���ν���
LRESULT CALLBACK    ChildLeftProc(HWND, UINT, WPARAM, LPARAM);
LRESULT CALLBACK    ChildTopProc(HWND, UINT, WPARAM, LPARAM);
LRESULT CALLBACK    ChildBottomProc(HWND, UINT, WPARAM, LPARAM);
//������ �ڵ�
HWND hC1, hC2, hC3;
HWND g_hWnd;

int APIENTRY wWinMain(_In_ HINSTANCE hInstance,
	_In_opt_ HINSTANCE hPrevInstance,
	_In_ LPWSTR    lpCmdLine,
	_In_ int       nCmdShow)
{
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);

	// TODO: ���⿡ �ڵ带 �Է��մϴ�.

	// ���� ���ڿ��� �ʱ�ȭ�մϴ�.
	LoadString(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING); //IDS_APP_TITLE= ���ҽ��� �ִ� ��Ʈ�� �о�Ͷ�
	LoadStringW(hInstance, IDC_WIN3220210517, szWindowClass, MAX_LOADSTRING);
	MyRegisterClass(hInstance);


	// ���ø����̼� �ʱ�ȭ�� �����մϴ�:
	if (!InitInstance(hInstance, nCmdShow))		//�������� ź��
	{
		return FALSE;
	}


	HACCEL hAccelTable = LoadAccelerators(hInstance, MAKEINTRESOURCE(IDC_WIN3220210517));

	MSG msg;

	// �⺻ �޽��� �����Դϴ�:
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
//  �Լ�: MyRegisterClass()
//
//  �뵵: â Ŭ������ ����մϴ�.
//
ATOM MyRegisterClass(HINSTANCE hInstance)
{
	WNDCLASSEXW wcex;

	wcex.cbSize = sizeof(WNDCLASSEX);

	wcex.style = CS_HREDRAW | CS_VREDRAW;

	wcex.lpfnWndProc = WndProc; //�����̸�(��������Ŭ�������� ���� �߿���)

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

	RegisterClassExW(&wcex);
	////////////////////////////////////////////////////////////////
	//ChildWindow ���
	//1.Left child class
	static HBRUSH hRed = CreateSolidBrush(RGB(255, 0, 0));
	wcex.hbrBackground = hRed;
	wcex.lpfnWndProc = ChildLeftProc;
	wcex.lpszClassName = _T("ChildLeft");
	RegisterClassExW(&wcex);

	//2.Top child class
	static HBRUSH hGreen = CreateSolidBrush(RGB(0, 255, 0));
	wcex.hbrBackground = hGreen;
	wcex.lpfnWndProc = ChildTopProc;
	wcex.lpszClassName = _T("ChildTop");
	RegisterClassExW(&wcex);

	//3.Bottom child class
	static HBRUSH hBlue = CreateSolidBrush(RGB(0, 0, 255));
	wcex.hbrBackground = hBlue;
	wcex.lpfnWndProc = ChildBottomProc;
	wcex.lpszClassName = _T("ChildBottom");
	RegisterClassExW(&wcex);


	return 0;
}


//
//   �Լ�: InitInstance(HINSTANCE, int)
//
//   �뵵: �ν��Ͻ� �ڵ��� �����ϰ� �� â�� ����ϴ�.
//
//   �ּ�:
//
//        �� �Լ��� ���� �ν��Ͻ� �ڵ��� ���� ������ �����ϰ�
//        �� ���α׷� â�� ���� ���� ǥ���մϴ�.
//
BOOL InitInstance(HINSTANCE hInstance, int nCmdShow)
{
	hInst = hInstance; // �ν��Ͻ� �ڵ��� ���� ������ �����մϴ�.

	HWND hWnd = CreateWindowW(szWindowClass, szTitle, WS_OVERLAPPEDWINDOW,
		CW_USEDEFAULT, 0, CW_USEDEFAULT, 0, nullptr, nullptr, hInstance, nullptr);


	g_hWnd = hWnd;
	ShowWindow(hWnd, nCmdShow);


	UpdateWindow(hWnd);


	return TRUE;
}

//
//  �Լ�: WndProc(HWND, UINT, WPARAM, LPARAM)
//
//  �뵵: �� â�� �޽����� ó���մϴ�.
//
//  WM_COMMAND  - ���ø����̼� �޴��� ó���մϴ�.
//  WM_PAINT    - �� â�� �׸��ϴ�.
//  WM_DESTROY  - ���� �޽����� �Խ��ϰ� ��ȯ�մϴ�.
//
//
static HWND hList;
TCHAR str[256];

LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{

	static HBRUSH rBrush, gBrush, bBrush;
	static HBRUSH NowBrush;

	switch (message)
	{
	case WM_CREATE:
	{
		hList = CreateWindow(_T("listbox"), NULL, WS_CHILD | WS_VISIBLE | WS_BORDER |
			LBS_NOTIFY, 100, 100, 100, 200, hWnd, (HMENU)ID_LISTBOX, hInst, NULL);


		hC1 = CreateWindow(_T("ChildLeft"), NULL, WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN,
			0, 0, 0, 0,
			hWnd, (HMENU)0, hInst, NULL);

		hC2 = CreateWindow(_T("ChildTop"), NULL, WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN,
			0, 0, 0, 0,
			hWnd, (HMENU)1, hInst, NULL);
		hC3 = CreateWindow(_T("ChildBottom"), NULL, WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN,
			0, 0, 0, 0,
			hWnd, (HMENU)2, hInst, NULL);
	}
	break;


	case WM_SIZE:
		if (wParam != SIZE_MINIMIZED)
		{
			RECT crt;
			GetClientRect(hWnd, &crt);
			MoveWindow(hC1, 0, 0, 300, crt.bottom, TRUE);
			MoveWindow(hC2, 300, 0, crt.right - 300, 150, TRUE);
			MoveWindow(hC3, 300, 150, crt.right - 300, crt.bottom - 150, TRUE);

		}
	case WM_PAINT:
	{
		PAINTSTRUCT ps;
		HDC hdc = BeginPaint(hWnd, &ps);
		// TODO: ���⿡ hdc�� ����ϴ� �׸��� �ڵ带 �߰��մϴ�...

		EndPaint(hWnd, &ps);
	}
	break;
	case WM_DESTROY:
		PostQuitMessage(0);
		break;
	default:
		return DefWindowProc(hWnd, message, wParam, lParam);    //�߿�
	}
	return 0;
}

LRESULT CALLBACK ChildLeftProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	static HWND hEdit, hBtn;

	switch (message)
	{
	case WM_CREATE:
	{
		hEdit = CreateWindow(_T("edit"),
			NULL, WS_CHILD | WS_VISIBLE | WS_BORDER | ES_AUTOHSCROLL,
			10, 10, 80, 25, hWnd, (HMENU)1, hInst, NULL);
		hBtn = CreateWindow(_T("button"), _T("Add"),
			WS_CHILD | WS_VISIBLE | BS_DEFPUSHBUTTON,
			10, 50, 80, 25, hWnd, (HMENU)2, hInst, NULL);


	}
	break;
	case WM_COMMAND://����� ������
	{
		switch (LOWORD(wParam))
		{
		case 2:
		{
			TCHAR str[256];
			//1. ����Ʈ â�� ���ڸ� �д´�.

			GetWindowText(hEdit, str, 128);
			//2. �� ���ڸ� ���� â�� ����.

			SetWindowTextW(g_hWnd, str);

			//3.����Ʈ â �ʱ�ȭ
			SetWindowText(hEdit, _T(""));
		}

		}
	}
	break;
	default:
		return DefWindowProc(hWnd, message, wParam, lParam);    //�߿�
	}
	return 0;
}

LRESULT CALLBACK ChildTopProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{

	switch (message)
	{
	case WM_PAINT:
	{
		PAINTSTRUCT ps;
		HDC hdc = BeginPaint(hWnd, &ps);
		// TODO: ���⿡ hdc�� ����ϴ� �׸��� �ڵ带 �߰��մϴ�...
		SetBkMode(hdc, TRANSPARENT);
		TextOut(hdc, 10, 10, _T("�ȳ��ϼ���.�ݰ�����"), 20);
		EndPaint(hWnd, &ps);
	}

	default:
		return DefWindowProc(hWnd, message, wParam, lParam);    //�߿�
	}
	return 0;
}


LRESULT CALLBACK ChildBottomProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{

	static HWND hEdit;
	switch (message)
	{
	case WM_CREATE:
	{
		hEdit = CreateWindow(_T("edit"),
			NULL, WS_CHILD | WS_VISIBLE | WS_BORDER | ES_AUTOHSCROLL | ES_MULTILINE,
			10, 10, 500, 500, hWnd, (HMENU)1, hInst, NULL);
		SetWindowText(hEdit, _T("�������� �̾߱⸦ ���ּ���"));


	}
	break;

	default:
		return DefWindowProc(hWnd, message, wParam, lParam);    //�߿�
	}
	return 0;
}