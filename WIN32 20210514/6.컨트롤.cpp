// Win32 20210513.cpp : ���ø����̼ǿ� ���� �������� �����մϴ�.
//

#include "framework.h"
#include "Win32 20210513.h"

#define MAX_LOADSTRING 100

// ���� ����:
HINSTANCE hInst;                                // ���� �ν��Ͻ��Դϴ�.
WCHAR szTitle[MAX_LOADSTRING];                  // ���� ǥ���� �ؽ�Ʈ�Դϴ�. (sz = null�� ������ ���ھտ� ���̴� �밡���� ǥ���)
WCHAR szWindowClass[MAX_LOADSTRING];            // �⺻ â Ŭ���� �̸��Դϴ�.

// �� �ڵ� ��⿡ ���Ե� �Լ��� ������ �����մϴ�:
ATOM                MyRegisterClass(HINSTANCE hInstance);
BOOL                InitInstance(HINSTANCE, int);
LRESULT CALLBACK    WndProc(HWND, UINT, WPARAM, LPARAM);	//���� (�޴��Լ�)
INT_PTR CALLBACK    About(HWND, UINT, WPARAM, LPARAM);

int APIENTRY wWinMain(_In_ HINSTANCE hInstance,
	_In_opt_ HINSTANCE hPrevInstance,
	_In_ LPWSTR    lpCmdLine,
	_In_ int       nCmdShow)
{
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);

	// TODO: ���⿡ �ڵ带 �Է��մϴ�.

	// ���� ���ڿ��� �ʱ�ȭ�մϴ�.
	LoadString(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING);	//LoadString() = IDS_APP_TITLE= ���ҽ��� �ִ� ��Ʈ�� �о�Ͷ�
	LoadStringW(hInstance, IDC_WIN3220210513, szWindowClass, MAX_LOADSTRING);
	MyRegisterClass(hInstance);

	// ���ø����̼� �ʱ�ȭ�� �����մϴ�:
	if (!InitInstance(hInstance, nCmdShow))		//�������� ź��
	{
		return FALSE;
	}

	HACCEL hAccelTable = LoadAccelerators(hInstance, MAKEINTRESOURCE(IDC_WIN3220210513));

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
ATOM MyRegisterClass(HINSTANCE hInstance)	//���� �����츦 ����ϴ� ����!
{
	WNDCLASSEXW wcex;

	wcex.cbSize = sizeof(WNDCLASSEX);

	wcex.style = CS_HREDRAW | CS_VREDRAW;
	wcex.lpfnWndProc = WndProc; //�����̸�(��������Ŭ�������� ���� �߿���)
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
	
	//HWND hWnd = CreateWindowW(_T("EDIT") //btn,edit ����   , szTitle, WS_OVERLAPPEDWINDOW,		
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
//  �Լ�: WndProc(HWND, UINT, WPARAM, LPARAM)
//
//  �뵵: �� â�� �޽����� ó���մϴ�.
//
//  WM_COMMAND  - ���ø����̼� �޴��� ó���մϴ�.
//  WM_PAINT    - �� â�� �׸��ϴ�.
//  WM_DESTROY  - ���� �޽����� �Խ��ϰ� ��ȯ�մϴ�.
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
		hStatic = CreateWindowW(_T("STATIC"), _T("�̸� : "),	//hStatic = �ڵ��� �޴°ſ�~
			WS_CHILD | WS_VISIBLE,
			20, 20, 100, 25,
			hWnd, (HMENU)-1, hInst, NULL);

		hEdit = CreateWindowW(_T("EDIT"), _T("�ٺ���"),
			WS_CHILD | WS_VISIBLE | WS_BORDER | ES_AUTOHSCROLL,
			20, 100, 200, 25,
			hWnd, (HMENU)ID_EDIT_NAME, hInst, NULL);

		hButtonSave = CreateWindowW(_T("BUTTON"), _T("����"),
			WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON,
			300, 20, 100, 25,
			hWnd, (HMENU)ID_BTN_SAVE, hInst, NULL);

		CreateWindowW(_T("BUTTON"), _T("�μ�"),
			WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON,
			300, 50, 100, 25,
			hWnd, (HMENU)ID_BTN_PRINT, hInst, NULL);
	}
	break;
	case WM_COMMAND:
	{

		// �޴� ������ ���� �м��մϴ�:
		switch (LOWORD(wParam))
		{
		case ID_BTN_SAVE:
			MessageBox(hWnd, TEXT("������ �����մϴ�."), TEXT("Button"), MB_OK);
			

			break;
		case ID_BTN_PRINT:
			MessageBox(hWnd, TEXT("�μ��� �����մϴ�."), TEXT("Button"), MB_OK);
			break;
		case ID_EDIT_NAME:
			if (HIWORD(wParam) == EN_CHANGE)
			{
				TCHAR str[128];
				GetWindowText(hEdit, str, 128);
				SetWindowText(hWnd, str);
				SetWindowText(hStatic, str);
				//SetWindowText(hEdit, str); //���ѷ����� ����


			}
			break;
		}
	}
	break;
	case WM_LBUTTONDOWN :
	{
		SetParent(hButtonSave, GetDesktopWindow()); //��ư�� �θ� ����ȭ������
		SetWindowText(hButtonSave, _T("������ ��ư"));
	}
	break;

	case WM_RBUTTONDOWN:
	{
		SetParent(hButtonSave, hWnd);
		SetWindowText(hButtonSave, _T("���ƿ� ��ư"));


	}
	break;
	case WM_KEYDOWN: //Ű���忡�� Ű�� ����
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
				SetWindowText(hWnd, _T("�������"));
				SetWindowText(hButtonSave, _T("�������"));
			}
			else
			{
				bshow = TRUE;
				ShowWindow(hEdit, SW_SHOW);
				SetWindowText(hWnd, _T("��Ÿ����"));
				SetWindowText(hButtonSave, _T("��Ÿ����"));

			}
		}
		MoveWindow(hEdit, 10, nTop, 200, 25, TRUE);
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

