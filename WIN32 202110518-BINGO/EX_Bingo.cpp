// WIN32 20210517.cpp : ���ø����̼ǿ� ���� �������� �����մϴ�.
//

#include "framework.h"
#include "WIN32 20210518.h"


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

	// TODO: ���⿡ �ڵ带 �Է��մϴ�.

	// ���� ���ڿ��� �ʱ�ȭ�մϴ�.
	LoadString(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING); //IDS_APP_TITLE= ���ҽ��� �ִ� ��Ʈ�� �о�Ͷ�
	LoadStringW(hInstance, IDC_WIN3220210518, szWindowClass, MAX_LOADSTRING);
	MyRegisterClass(hInstance);


	// ���ø����̼� �ʱ�ȭ�� �����մϴ�:
	if (!InitInstance(hInstance, nCmdShow))		//�������� ź��
	{
		return FALSE;
	}


	HACCEL hAccelTable = LoadAccelerators(hInstance, MAKEINTRESOURCE(IDC_WIN3220210518));

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
	g_Main = hWnd;
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
		//1. ���� ���ϵ��� ����
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
		//2. ���� üũ�Ѵ�.
		int nBingoCount;
	/*
		//����
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
		//����
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

		//����
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

		//�밢��
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
		// TODO: ���⿡ hdc�� ����ϴ� �׸��� �ڵ带 �߰��մϴ�...

		EndPaint(hWnd, &ps);
	}
	break;

	break;
	case WM_DESTROY:
		PostQuitMessage(0);
		break;
	default:
		return DefWindowProc(hWnd, message, wParam, lParam);    //�߿�
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
			InvalidateRect(hWnd, NULL, TRUE);//������ �׸��� �ٽ� �׸�
			g_bMyTurn = !g_bMyTurn;
			ckProp = (int)GetProp(hWnd, TEXT("CkBingo"));
			SendMessage(g_Main, WM_CHECKBINGO, (WPARAM)hWnd, ckProp);//��ο� ����üũ ��û


		}
	}
	break;
	case WM_PAINT:
	{
		PAINTSTRUCT ps;
		HDC hdc = BeginPaint(hWnd, &ps);
		// TODO: ���⿡ hdc�� ����ϴ� �׸��� �ڵ带 �߰��մϴ�...

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
			TextOut(hdc, 10, 10, _T("���� ����"), 5);
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
			TextOut(hdc, 10, 10, _T("���� ����"), 5);
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
		return DefWindowProc(hWnd, message, wParam, lParam);    //�߿�
	}
	return 0;

}

