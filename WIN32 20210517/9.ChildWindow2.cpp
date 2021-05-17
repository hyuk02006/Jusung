// WIN32 20210517.cpp : ���ø����̼ǿ� ���� �������� �����մϴ�.
//

#include "framework.h"
#include "WIN32 20210517.h"

#define MAX_LOADSTRING 100
// ���� ����:
HINSTANCE hInst;                                // ���� �ν��Ͻ��Դϴ�.
WCHAR szTitle[MAX_LOADSTRING];                  // ���� ǥ���� �ؽ�Ʈ�Դϴ�. (sz = null�� ������ ���ھտ� ���̴� �밡���� ǥ���)
WCHAR szWindowClass[MAX_LOADSTRING];            // �⺻ â Ŭ���� �̸��Դϴ�.

// �� �ڵ� ��⿡ ���Ե� �Լ��� ������ �����մϴ�:
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


LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{

	switch (message)
	{

	case WM_CREATE:	//���� �����찡 �����ɶ�
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
		/*CreateWindow(lpzsChildClass, _T("�ȳ糭�˾�"), WS_POPUP | WS_VISIBLE | WS_CLIPCHILDREN | WS_CAPTION | WS_SYSMENU,
			100, 100, 500, 500,
			hWnd, (HMENU)0, hInst, NULL);
		*/
	}
	break;
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
		// TODO: ���⿡ hdc�� ����ϴ� �׸��� �ڵ带 �߰��մϴ�...

		//if (bEllipse)
		bProp=(BOOL)GetProp(hWnd, _T("bEllipse"));
		if(bProp)
			Ellipse(hdc, 10, 10, 90, 90);
		else
		{
			MoveToEx(hdc, 10, 10, NULL);	//��������� �̵��ض�
			LineTo(hdc, 90, 90);			//������ ���ܶ�
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
		return DefWindowProc(hWnd, message, wParam, lParam);    //�߿�
	}
	return 0;

}
