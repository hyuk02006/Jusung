
// MultiTaskView.cpp: CMultiTaskView 클래스의 구현
//

#include "pch.h"
#include "framework.h"
// SHARED_HANDLERS는 미리 보기, 축소판 그림 및 검색 필터 처리기를 구현하는 ATL 프로젝트에서 정의할 수 있으며
// 해당 프로젝트와 문서 코드를 공유하도록 해 줍니다.
#ifndef SHARED_HANDLERS
#include "MultiTask.h"
#endif

#include "MultiTaskDoc.h"
#include "MultiTaskView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CMultiTaskView

IMPLEMENT_DYNCREATE(CMultiTaskView, CView)

BEGIN_MESSAGE_MAP(CMultiTaskView, CView)
	// 표준 인쇄 명령입니다.
	ON_COMMAND(ID_FILE_PRINT, &CView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_DIRECT, &CView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_PREVIEW, &CView::OnFilePrintPreview)
	ON_WM_LBUTTONDOWN()
END_MESSAGE_MAP()

// CMultiTaskView 생성/소멸

CMultiTaskView::CMultiTaskView() noexcept
{
	// TODO: 여기에 생성 코드를 추가합니다.

}

CMultiTaskView::~CMultiTaskView()
{
}

BOOL CMultiTaskView::PreCreateWindow(CREATESTRUCT& cs)
{
	// TODO: CREATESTRUCT cs를 수정하여 여기에서
	//  Window 클래스 또는 스타일을 수정합니다.

	return CView::PreCreateWindow(cs);
}

// CMultiTaskView 그리기
int g_nBlue = 0;
void CMultiTaskView::OnDraw(CDC* pDC)
{
	CMultiTaskDoc* pDoc = GetDocument();
	ASSERT_VALID(pDoc);
	if (!pDoc)
		return;

	// TODO: 여기에 원시 데이터에 대한 그리기 코드를 추가합니다.

}


// CMultiTaskView 인쇄

BOOL CMultiTaskView::OnPreparePrinting(CPrintInfo* pInfo)
{
	// 기본적인 준비
	return DoPreparePrinting(pInfo);
}

void CMultiTaskView::OnBeginPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: 인쇄하기 전에 추가 초기화 작업을 추가합니다.
}

void CMultiTaskView::OnEndPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: 인쇄 후 정리 작업을 추가합니다.
}


// CMultiTaskView 진단

#ifdef _DEBUG
void CMultiTaskView::AssertValid() const
{
	CView::AssertValid();
}

void CMultiTaskView::Dump(CDumpContext& dc) const
{
	CView::Dump(dc);
}

CMultiTaskDoc* CMultiTaskView::GetDocument() const // 디버그되지 않은 버전은 인라인으로 지정됩니다.
{
	ASSERT(m_pDocument->IsKindOf(RUNTIME_CLASS(CMultiTaskDoc)));
	return (CMultiTaskDoc*)m_pDocument;
}
#endif //_DEBUG


// CMultiTaskView 메시지 처리기
CMultiTaskView* g_pView;
DWORD WINAPI ThreadDraw(LPVOID p)
{
	RECT rc = *(LPRECT)p;
	CDC* pDC= g_pView->GetDC();

	while (1)
	{
		g_nBlue += 5;
		if (g_nBlue > 255)g_nBlue = 0;

		CBrush BrBlue(RGB(0, 0, g_nBlue));
		CBrush* pOldBr = pDC->SelectObject(&BrBlue);
		pDC->Rectangle(rc.left, rc.top,rc.right,rc.bottom);
		pDC->SelectObject(pOldBr);
	}
	g_pView->ReleaseDC(pDC);
	return 0;
}


void CMultiTaskView::OnLButtonDown(UINT nFlags, CPoint point)
{
	g_pView = this;
	static RECT arRect[] = {{100,100,200,200},
							{300,100,400,200},
							{100,300,200,400},
							{300,300,400,400},};
	DWORD dwID;
	for(int i= 0; i<4; i++)
	CloseHandle(CreateThread(NULL, NULL, ThreadDraw, &arRect[i], 0/*dwCreationFlags즉시 일을 시작해라는 것*/,&dwID/*ID*/));

	CView::OnLButtonDown(nFlags, point);
}
