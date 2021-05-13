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

void CALLBACK MyTimerproc(HWND hWnd,UINT unnamedParam2,UINT_PTR unnamedParam3, DWORD unnamedParam4)
{
    HDC hdc;
    hdc =GetDC(hWnd);
    for (int i = 0; i < 1000; i++)
    {
        SetPixel(hdc, rand() % 500,rand()%500, RGB(rand() % 256, rand() % 256, rand() % 256));
    }
    ReleaseDC(hWnd,hdc);
    
}
LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    static TCHAR sTime[128];

    switch (message)
    {
    case WM_CREATE:
    {
        //SetTimer(hWnd, 1, 1000, NULL);
        //SetTimer(hWnd, 2, 5000, NULL);
       // SetTimer(hWnd, 3, 0.00001, MyTimerproc);

    }
    break;
    case WM_TIMER:
    {
        switch (wParam)
        {
        case 1: //현재 시각을 구해서 화면에 표시한다
            SYSTEMTIME st;
            GetLocalTime(&st);
            wsprintf(sTime, TEXT("지금 시간은 %d:%d:%d입니다."), st.wHour, st.wMinute, st.wSecond);
            InvalidateRect(hWnd, NULL, TRUE);
            break;
        case 2: //공지 대화상자를 출력한다
            MessageBox(hWnd, TEXT("지금은 쉬는 시간입니다"), _T("중요알림"), MB_YESNO);
            MessageBeep(0);
            InvalidateRect(hWnd, NULL, TRUE);
            break;
        }
    }
    break;
    case WM_LBUTTONDOWN:
    {
        //KillTimer(hWnd, 2);
        HDC hdc;
        hdc = GetDC(hWnd);
        int x = LOWORD(lParam);
        int y = HIWORD(lParam);
        Ellipse(hdc, x - 10, y - 10, x + 10, y + 10);
        ReleaseDC(hWnd, hdc);
    }
        break;
    case WM_MOUSEMOVE:
    {
        //KillTimer(hWnd, 2);
        HDC hdc;
        hdc = GetDC(hWnd);
        int x = LOWORD(lParam);
        int y = HIWORD(lParam);
        Ellipse(hdc, x - 100, y - 100, x + 100, y + 100);
        ReleaseDC(hWnd, hdc);
    }
    break;
    case WM_RBUTTONDOWN:
        //SetTimer(hWnd, 2, 5000, NULL);
        break;
    case WM_PAINT:
    {
        PAINTSTRUCT ps;
        HDC hdc = BeginPaint(hWnd, &ps);
        // TODO: 여기에 hdc를 사용하는 그리기 코드를 추가합니다...
        TextOut(hdc, 100, 100, sTime, lstrlen(sTime));
  
        EndPaint(hWnd, &ps);
    }
    break;
    case WM_DESTROY:
        PostQuitMessage(0);
        KillTimer(hWnd, 1);
        KillTimer(hWnd, 2);

        break;
    default:
        return DefWindowProc(hWnd, message, wParam, lParam);
    }
    return 0;
}