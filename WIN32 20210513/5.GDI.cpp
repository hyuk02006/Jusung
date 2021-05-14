// Win32 20210513.cpp : ���ø����̼ǿ� ���� �������� �����մϴ�.
//

#include "framework.h"
#include "Win32 20210513.h"

#define MAX_LOADSTRING 100

// ���� ����:
HINSTANCE hInst;                                // ���� �ν��Ͻ��Դϴ�.
WCHAR szTitle[MAX_LOADSTRING];                  // ���� ǥ���� �ؽ�Ʈ�Դϴ�.
WCHAR szWindowClass[MAX_LOADSTRING];            // �⺻ â Ŭ���� �̸��Դϴ�.

// �� �ڵ� ��⿡ ���Ե� �Լ��� ������ �����մϴ�:
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

    // TODO: ���⿡ �ڵ带 �Է��մϴ�.

    // ���� ���ڿ��� �ʱ�ȭ�մϴ�.
    LoadString(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING);
    LoadStringW(hInstance, IDC_WIN3220210513, szWindowClass, MAX_LOADSTRING);
    MyRegisterClass(hInstance);

    // ���ø����̼� �ʱ�ȭ�� �����մϴ�:
    if (!InitInstance(hInstance, nCmdShow))
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
ATOM MyRegisterClass(HINSTANCE hInstance)
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
    case WM_COMMAND:
    {
        int wmId = LOWORD(wParam);
        // �޴� ������ ���� �м��մϴ�:
        switch (wmId)
        {
        case IDM_ABOUT:
            //  DialogBox(hInst, MAKEINTRESOURCE(IDD_ABOUTBOX), hWnd, About);
            break;
        case IDM_EXIT:
            DestroyWindow(hWnd);
            break;
        default:
            return DefWindowProc(hWnd, message, wParam, lParam);
        }
    }
    break;
    case WM_PAINT:
    {
        if (0) //stockObject ���
        {
            PAINTSTRUCT ps;
            HDC hdc = BeginPaint(hWnd, &ps);
            // TODO: ���⿡ hdc�� ����ϴ� �׸��� �ڵ带 �߰��մϴ�...
            HBRUSH MyBr = (HBRUSH)GetStockObject(GRAY_BRUSH);
            HBRUSH OldBr = (HBRUSH)SelectObject(hdc, MyBr);
            Rectangle(hdc, 500, 500, 100, 100);
            SelectObject(hdc, OldBr);

            HBRUSH MyBr2 = (HBRUSH)GetStockObject(DKGRAY_BRUSH);
            SelectObject(hdc, MyBr);

            Rectangle(hdc, 0, 0, 50, 50);
            SelectObject(hdc, OldBr);


            Rectangle(hdc, 300, 0, 50, 50);

            EndPaint(hWnd, &ps);
        }
        if (1)  //������ �귯�� ����
        {
            PAINTSTRUCT ps;
            HDC hdc = BeginPaint(hWnd, &ps);
            //HBRUSH Mybr =CreateSolidBrush(RGB(255, 0, 0));
            HBRUSH Mybr = CreateHatchBrush(HS_DIAGCROSS,RGB(255, 0, 0));

            HBRUSH OldBr = (HBRUSH)SelectObject(hdc, Mybr);
            Rectangle(hdc, 500, 500, 100, 100);

            SelectObject(hdc, OldBr);

            Rectangle(hdc, 0, 0, 50, 50);

            EndPaint(hWnd, &ps);
        }
        if (0) //������ �� ����
        {
            PAINTSTRUCT ps;
            HDC hdc = BeginPaint(hWnd, &ps);
            HPEN MyPen = CreatePen(PS_DASHDOTDOT,1,RGB(0, 0, 255));
            HPEN OldPen = (HPEN)SelectObject(hdc, MyPen);
            Rectangle(hdc, 500, 500, 100, 100);

            SelectObject(hdc, OldPen);

            Rectangle(hdc, 0, 0, 50, 50);

            EndPaint(hWnd, &ps);
        }
    }
    break;
    case WM_MOUSEMOVE:
    {
        HDC hdc;
        hdc = GetDC(hWnd);

        int x, y, r;
        x = rand() % 640;
        y = rand() % 480;
        r = rand() % 10 + 10;
        
        HBRUSH Mybr = CreateSolidBrush(RGB(rand() %256, rand() % 256, rand() % 256));
        HBRUSH OldBr = (HBRUSH)SelectObject(hdc, Mybr);

        Ellipse(hdc, x - r, y - r, 500, 600);
       // SelectObject(hdc, OldBr);

        ReleaseDC(hWnd, hdc);
    }
    break;
    case WM_LBUTTONDOWN:
    {
        InvalidateRect(hWnd, NULL, TRUE);

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

