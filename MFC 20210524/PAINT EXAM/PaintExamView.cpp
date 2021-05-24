
// PaintExamView.cpp: CPaintExamView 클래스의 구현
//

#include "pch.h"
#include "framework.h"
// SHARED_HANDLERS는 미리 보기, 축소판 그림 및 검색 필터 처리기를 구현하는 ATL 프로젝트에서 정의할 수 있으며
// 해당 프로젝트와 문서 코드를 공유하도록 해 줍니다.
#ifndef SHARED_HANDLERS
#include "PaintExam.h"
#endif

#include "PaintExamDoc.h"
#include "PaintExamView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CPaintExamView

IMPLEMENT_DYNCREATE(CPaintExamView, CView)

BEGIN_MESSAGE_MAP(CPaintExamView, CView)
	// 표준 인쇄 명령입니다.
	ON_COMMAND(ID_FILE_PRINT, &CView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_DIRECT, &CView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_PREVIEW, &CView::OnFilePrintPreview)
	ON_WM_MOUSEMOVE()
	ON_WM_LBUTTONDOWN()
	ON_WM_LBUTTONUP()
	ON_WM_PAINT()
	ON_COMMAND(ID_LINE, &CPaintExamView::OnLine)
	ON_COMMAND(ID_RECT, &CPaintExamView::OnRect)
	ON_COMMAND(ID_EIILPSE, &CPaintExamView::OnEiilpse)
	ON_COMMAND(ID_FILL, &CPaintExamView::OnFill)
	ON_UPDATE_COMMAND_UI(ID_LINE, &CPaintExamView::OnUpdateLine)
	ON_UPDATE_COMMAND_UI(ID_RECT, &CPaintExamView::OnUpdateRect)
	ON_UPDATE_COMMAND_UI(ID_EIILPSE, &CPaintExamView::OnUpdateEiilpse)
	ON_UPDATE_COMMAND_UI(ID_FILL, &CPaintExamView::OnUpdateFill)

END_MESSAGE_MAP()

// CPaintExamView 생성/소멸

CPaintExamView::CPaintExamView() noexcept
{
	// TODO: 여기에 생성 코드를 추가합니다.
	//선===============
	m_ptStart.x = 0;
	m_ptStart.y = 0;
	m_ptEnd.x = 0;
	m_ptEnd.y = 0;
	m_bDrag=FALSE;

	//메뉴 ===============
	m_nShape = 0;
	m_bFill = FALSE;
}

CPaintExamView::~CPaintExamView()
{
}

BOOL CPaintExamView::PreCreateWindow(CREATESTRUCT& cs)
{
	// TODO: CREATESTRUCT cs를 수정하여 여기에서
	//  Window 클래스 또는 스타일을 수정합니다.

	return CView::PreCreateWindow(cs);
}

// CPaintExamView 그리기

void CPaintExamView::OnDraw(CDC* /*pDC*/)
{
	CPaintExamDoc* pDoc = GetDocument();
	ASSERT_VALID(pDoc);
	if (!pDoc)
		return;

	// TODO: 여기에 원시 데이터에 대한 그리기 코드를 추가합니다.
}


// CPaintExamView 인쇄

BOOL CPaintExamView::OnPreparePrinting(CPrintInfo* pInfo)
{
	// 기본적인 준비
	return DoPreparePrinting(pInfo);
}

void CPaintExamView::OnBeginPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: 인쇄하기 전에 추가 초기화 작업을 추가합니다.
}

void CPaintExamView::OnEndPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: 인쇄 후 정리 작업을 추가합니다.
}


// CPaintExamView 진단

#ifdef _DEBUG
void CPaintExamView::AssertValid() const
{
	CView::AssertValid();
}

void CPaintExamView::Dump(CDumpContext& dc) const
{
	CView::Dump(dc);
}

CPaintExamDoc* CPaintExamView::GetDocument() const // 디버그되지 않은 버전은 인라인으로 지정됩니다.
{
	ASSERT(m_pDocument->IsKindOf(RUNTIME_CLASS(CPaintExamDoc)));
	return (CPaintExamDoc*)m_pDocument;
}
#endif //_DEBUG


// CPaintExamView 메시지 처리기

void CPaintExamView::OnLButtonDown(UINT nFlags, CPoint point)
{
	m_bDrag = TRUE;
	m_ptStart = point;
	CView::OnLButtonDown(nFlags, point);
}


void CPaintExamView::OnLButtonUp(UINT nFlags, CPoint point)
{
	if (m_bDrag)
	{
		m_bDrag = FALSE;
		
		m_ptEnd = point;
		RedrawWindow();
	}
	CView::OnLButtonUp(nFlags, point);
}


void CPaintExamView::OnMouseMove(UINT nFlags, CPoint point)
{
	if (m_bDrag)
	{
		m_ptEnd = point;
		RedrawWindow();
	}
	CView::OnMouseMove(nFlags, point);
}



void CPaintExamView::OnPaint()
{
	CPaintDC dc(this); // device context for painting
					   // TODO: 여기에 메시지 처리기 코드를 추가합니다.
					   // 그리기 메시지에 대해서는 CView::OnPaint()을(를) 호출하지 마십시오.
	CBrush Redbr = RGB(255, 0, 0);
	CBrush* pOldbr = NULL;
	if (m_bFill == TRUE)
	{
		pOldbr = dc.SelectObject(&Redbr);
	}
	///////////////////////////////////////////////////////////////
	switch (m_nShape)
	{
	case 0:
	{
		dc.MoveTo(m_ptStart);
		dc.LineTo(m_ptEnd);
		break;
	}
	case 1:
	{
		dc.Rectangle(m_ptStart.x, m_ptStart.y, m_ptEnd.x, m_ptEnd.y);
		break;
	}
	case 2:
	{
		dc.Ellipse(m_ptStart.x, m_ptStart.y, m_ptEnd.x, m_ptEnd.y);
		break;
	}
	}
	///////////////////////////////////////////////////////////////
	if (m_bFill ==TRUE)
	{
		dc.SelectObject(pOldbr);
	}
}


void CPaintExamView::OnLine()
{
	// TODO: 여기에 명령 처리기 코드를 추가합니다.
	m_nShape = 0;
}

void CPaintExamView::OnUpdateLine(CCmdUI* pCmdUI)
{
	
	if (m_nShape == 0)
		pCmdUI->SetCheck(1);
	else
		pCmdUI->SetCheck(0);
}



void CPaintExamView::OnRect()
{
	m_nShape = 1;
}
void CPaintExamView::OnUpdateRect(CCmdUI* pCmdUI)
{
	if(m_nShape ==1)
		pCmdUI->SetCheck(1);
	else
		pCmdUI->SetCheck(0);

}

void CPaintExamView::OnEiilpse()
{
	m_nShape = 2;
}

void CPaintExamView::OnUpdateEiilpse(CCmdUI* pCmdUI)
{
	if (m_nShape == 2)
		pCmdUI->SetCheck(1);
	else
		pCmdUI->SetCheck(0);
}

void CPaintExamView::OnFill()
{
	if (m_bFill)
		m_bFill = FALSE;
	else
		m_bFill = TRUE;
}

void CPaintExamView::OnUpdateFill(CCmdUI* pCmdUI)
{
	if (m_bFill == TRUE)
		pCmdUI->SetCheck(1);
	else
		pCmdUI->SetCheck(0);
}

