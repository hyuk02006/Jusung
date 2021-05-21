
// GDIDeomoView.cpp: CGDIDeomoView 클래스의 구현
//

#include "pch.h"
#include "framework.h"
// SHARED_HANDLERS는 미리 보기, 축소판 그림 및 검색 필터 처리기를 구현하는 ATL 프로젝트에서 정의할 수 있으며
// 해당 프로젝트와 문서 코드를 공유하도록 해 줍니다.
#ifndef SHARED_HANDLERS
#include "GDIDeomo.h"
#endif

#include "GDIDeomoDoc.h"
#include "GDIDeomoView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CGDIDeomoView

IMPLEMENT_DYNCREATE(CGDIDeomoView, CView)

BEGIN_MESSAGE_MAP(CGDIDeomoView, CView)
	// 표준 인쇄 명령입니다.
	ON_COMMAND(ID_FILE_PRINT, &CView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_DIRECT, &CView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_PREVIEW, &CView::OnFilePrintPreview)
	ON_WM_LBUTTONDOWN()
	ON_WM_PAINT()
END_MESSAGE_MAP()

// CGDIDeomoView 생성/소멸

CGDIDeomoView::CGDIDeomoView() noexcept
{
	// TODO: 여기에 생성 코드를 추가합니다.

}

CGDIDeomoView::~CGDIDeomoView()
{
}

BOOL CGDIDeomoView::PreCreateWindow(CREATESTRUCT& cs)
{
	// TODO: CREATESTRUCT cs를 수정하여 여기에서
	//  Window 클래스 또는 스타일을 수정합니다.

	return CView::PreCreateWindow(cs);
}

// CGDIDeomoView 그리기

void CGDIDeomoView::OnDraw(CDC* /*pDC*/)
{
	CGDIDeomoDoc* pDoc = GetDocument();
	ASSERT_VALID(pDoc);
	if (!pDoc)
		return;

	// TODO: 여기에 원시 데이터에 대한 그리기 코드를 추가합니다.
}


// CGDIDeomoView 인쇄

BOOL CGDIDeomoView::OnPreparePrinting(CPrintInfo* pInfo)
{
	// 기본적인 준비
	return DoPreparePrinting(pInfo);
}

void CGDIDeomoView::OnBeginPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: 인쇄하기 전에 추가 초기화 작업을 추가합니다.
}

void CGDIDeomoView::OnEndPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: 인쇄 후 정리 작업을 추가합니다.
}


// CGDIDeomoView 진단

#ifdef _DEBUG
void CGDIDeomoView::AssertValid() const
{
	CView::AssertValid();
}

void CGDIDeomoView::Dump(CDumpContext& dc) const
{
	CView::Dump(dc);
}

CGDIDeomoDoc* CGDIDeomoView::GetDocument() const // 디버그되지 않은 버전은 인라인으로 지정됩니다.
{
	ASSERT(m_pDocument->IsKindOf(RUNTIME_CLASS(CGDIDeomoDoc)));
	return (CGDIDeomoDoc*)m_pDocument;
}
#endif //_DEBUG


// CGDIDeomoView 메시지 처리기


void CGDIDeomoView::OnLButtonDown(UINT nFlags, CPoint point)
{
	// TODO: 여기에 메시지 처리기 코드를 추가 및/또는 기본값을 호출합니다.
	CDC* pDC =GetDC();

	pDC->Rectangle(100, 100, 300, 300);
	ReleaseDC(pDC);
	CView::OnLButtonDown(nFlags, point);
}


void CGDIDeomoView::OnPaint()
{
	CPaintDC dc(this); // device context for painting
					   // TODO: 여기에 메시지 처리기 코드를 추가합니다.
					   // 그리기 메시지에 대해서는 CView::OnPaint()을(를) 호출하지 마십시오.

	//========================================================
	//Pen
	/*
	CPen myPen(PS_DOT, 1, RGB(255, 50, 200));

	CPen* pOldPen = dc.SelectObject(&myPen);
	dc.Rectangle(400, 400, 200, 200);
	dc.Ellipse(400, 400, 200, 200);

	dc.MoveTo(200, 200);
	dc.LineTo(400, 400);
	
	dc.SelectObject(pOldPen);
	*/
	//========================================================
	//Brush
	/*
	CBrush myBr(RGB(0, 255, 0)); //솔리드 브러쉬
	CBrush myBr(HS_CROSS, RGB(0, 255, 0)); // 해치 브러쉬
	CBitmap bmpFlower;
	bmpFlower.LoadBitmap(IDB_FLOWER);
	CBrush myBr(&bmpFlower);// 패턴 브러쉬
	CBrush* pOldBr = dc.SelectObject(&myBr);
	CRect rectView;
	GetClientRect(&rectView);
	dc.Ellipse(&rectView);
	//dc.Ellipse(100, 100, 500, 500);
	dc.SelectObject(pOldBr);
	*/
	//========================================================
	//Font
	LOGFONT lf = { 0, };
	lf.lfHeight = 30;
	lf.lfItalic = 1;
	lf.lfUnderline = 1;
	wsprintf(lf.lfFaceName, _T("%s"), _T("궁서체"));

	CFont myFont;
	myFont.CreateFontIndirect(&lf);
	CFont* pOldFt =dc.SelectObject(&myFont);
		
	dc.TextOut(10, 10, _T("안녕하세요 반가워요"));

	dc.SelectObject(pOldFt);
	//========================================================


}
