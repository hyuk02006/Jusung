
// BackEventView.cpp: CBackEventView 클래스의 구현
//

#include "pch.h"
#include "framework.h"
// SHARED_HANDLERS는 미리 보기, 축소판 그림 및 검색 필터 처리기를 구현하는 ATL 프로젝트에서 정의할 수 있으며
// 해당 프로젝트와 문서 코드를 공유하도록 해 줍니다.
#ifndef SHARED_HANDLERS
#include "BackEvent.h"
#endif

#include "BackEventDoc.h"
#include "BackEventView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CBackEventView

IMPLEMENT_DYNCREATE(CBackEventView, CView)

BEGIN_MESSAGE_MAP(CBackEventView, CView)
	// 표준 인쇄 명령입니다.
	ON_COMMAND(ID_FILE_PRINT, &CView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_DIRECT, &CView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_PREVIEW, &CView::OnFilePrintPreview)
	ON_WM_LBUTTONDOWN()
	ON_WM_RBUTTONDOWN()
END_MESSAGE_MAP()

// CBackEventView 생성/소멸
HANDLE hEvent30;
HANDLE hEventCalc;

CBackEventView::CBackEventView() noexcept
{
	// TODO: 여기에 생성 코드를 추가합니다.
	hEvent30 = CreateEvent(NULL, FALSE/*자동 리셋 이벤트*/, FALSE, NULL);
	hEventCalc = CreateEvent(NULL, TRUE/*수동 리셋 이벤트*/, FALSE, NULL);//메뉴얼 리셋

}

CBackEventView::~CBackEventView()
{
	CloseHandle(hEvent30);
	CloseHandle(hEventCalc);
}

BOOL CBackEventView::PreCreateWindow(CREATESTRUCT& cs)
{
	// TODO: CREATESTRUCT cs를 수정하여 여기에서
	//  Window 클래스 또는 스타일을 수정합니다.

	return CView::PreCreateWindow(cs);
}

// CBackEventView 그리기

void CBackEventView::OnDraw(CDC* /*pDC*/)
{
	CBackEventDoc* pDoc = GetDocument();
	ASSERT_VALID(pDoc);
	if (!pDoc)
		return;

	// TODO: 여기에 원시 데이터에 대한 그리기 코드를 추가합니다.
}


// CBackEventView 인쇄

BOOL CBackEventView::OnPreparePrinting(CPrintInfo* pInfo)
{
	// 기본적인 준비
	return DoPreparePrinting(pInfo);
}

void CBackEventView::OnBeginPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: 인쇄하기 전에 추가 초기화 작업을 추가합니다.
}

void CBackEventView::OnEndPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: 인쇄 후 정리 작업을 추가합니다.
}


// CBackEventView 진단

#ifdef _DEBUG
void CBackEventView::AssertValid() const
{
	CView::AssertValid();
}

void CBackEventView::Dump(CDumpContext& dc) const
{
	CView::Dump(dc);
}

CBackEventDoc* CBackEventView::GetDocument() const // 디버그되지 않은 버전은 인라인으로 지정됩니다.
{
	ASSERT(m_pDocument->IsKindOf(RUNTIME_CLASS(CBackEventDoc)));
	return (CBackEventDoc*)m_pDocument;
}
#endif //_DEBUG


// CBackEventView 메시지 처리기
int buf[100];
DWORD WINAPI ThreadFunc(LPVOID p)
{
	for (int i = 0; i < 100; i++)
	{
		Sleep(30);
		buf[i] = i;
		if (i == 30)
			SetEvent(hEvent30); 
	}
	MessageBeep(0);
	AfxMessageBox(_T("100만원"));
	return 0;
}

void CBackEventView::OnLButtonDown(UINT nFlags, CPoint point)
{
	// TODO: 여기에 메시지 처리기 코드를 추가 및/또는 기본값을 호출합니다.

	DWORD dwID;
	HANDLE hTh=CreateThread(NULL, 0, ThreadFunc, 0, 0, &dwID);
	/////////////////////////////////////////////////
	//기다려
	//WaitForSingleObject(hTh, INFINITE);
	WaitForSingleObject(hEvent30, INFINITE);
	/////////////////////////////////////////////////
	CDC* pDC = GetDC();
	CString str;
	for (int i = 0; i <= 30; i++)
	{
		str.Format(_T("%d 라인은 %d 입니다"), i, buf[i]);
		pDC->TextOut(10, i * 20, str);
	}
	ReleaseDC(pDC);
	CloseHandle(hTh);
	CView::OnLButtonDown(nFlags, point);
}


DWORD WINAPI ThreadCalc(LPVOID p)
{

	CBackEventView* pView = (CBackEventView*)p;
	CDC* pDC = pView->GetDC();

	for (int i = 0; i < 10; i++)
	{
		pDC->TextOutW(500, 100, _T("계산중"));
		GdiFlush();
		Sleep(300);
		pDC->TextOut(500, 100, _T("기다려"));
		GdiFlush();
		Sleep(300);
	}

	MessageBeep(0);
	pDC->TextOut(500,100, _T("계산완료"));
	pView->ReleaseDC(pDC);
	SetEvent(hEventCalc);

	return 0;
}
DWORD WINAPI ThreadSave(LPVOID p)
{
	WaitForSingleObject(hEventCalc, INFINITE);

	CBackEventView* pView =(CBackEventView*)p;
	CDC* pDC = pView->GetDC();
	pDC->TextOut(500, 200, _T("저장완료"));
	pView->ReleaseDC(pDC);
	return 0;
}
DWORD WINAPI ThreadSend(LPVOID p)
{
	WaitForSingleObject(hEventCalc, INFINITE);

	CBackEventView* pView = (CBackEventView*)p;
	CDC* pDC = pView->GetDC();
	pDC->TextOut(500, 300, _T("전송완료"));
	pView->ReleaseDC(pDC);
	return 0;
}
DWORD WINAPI ThreadPrint(LPVOID p)
{
	WaitForSingleObject(hEventCalc, INFINITE);

	CBackEventView* pView = (CBackEventView*)p;
	CDC* pDC = pView->GetDC();
	pDC->TextOut(500, 400, _T("인쇄완료"));
	pView->ReleaseDC(pDC);
	return 0;
}
void CBackEventView::OnRButtonDown(UINT nFlags, CPoint point)
{
	ResetEvent(hEventCalc);
	DWORD dwID;

	CloseHandle(CreateThread(NULL, 0, ThreadCalc, this, 0, &dwID));
	CloseHandle(CreateThread(NULL, 0, ThreadSave, this, 0, &dwID));
	CloseHandle(CreateThread(NULL, 0, ThreadSend, this, 0, &dwID));
	CloseHandle(CreateThread(NULL, 0, ThreadPrint, this, 0, &dwID));

	CView::OnRButtonDown(nFlags, point);
}
