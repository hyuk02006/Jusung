// Win32 20210513.cpp : 애플리케이션에 대한 진입점을 정의합니다.
//

#include "framework.h"
#include "Win32 20210513.h"

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
    LoadStringW(hInstance, IDC_WIN3220210513, szWindowClass, MAX_LOADSTRING);
    MyRegisterClass(hInstance);

    // 애플리케이션 초기화를 수행합니다:
    if (!InitInstance(hInstance, nCmdShow))
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
ATOM MyRegisterClass(HINSTANCE hInstance)
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
    case WM_COMMAND:
    {
        int wmId = LOWORD(wParam);
        // 메뉴 선택을 구문 분석합니다:
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
        if (0) //stockObject 사용
        {
            PAINTSTRUCT ps;
            HDC hdc = BeginPaint(hWnd, &ps);
            // TODO: 여기에 hdc를 사용하는 그리기 코드를 추가합니다...
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
        if (1)  //나만의 브러쉬 생성
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
        if (0) //나만의 펜 생성
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
        return DefWindowProc(hWnd, message, wParam, lParam);    //중요
    }
    return 0;
}

