
// DlgDemoView.cpp: CDlgDemoView 클래스의 구현
//

#include "pch.h"
#include "framework.h"
// SHARED_HANDLERS는 미리 보기, 축소판 그림 및 검색 필터 처리기를 구현하는 ATL 프로젝트에서 정의할 수 있으며
// 해당 프로젝트와 문서 코드를 공유하도록 해 줍니다.
#ifndef SHARED_HANDLERS
#include "DlgDemo.h"
#endif

#include "DlgDemoDoc.h"
#include "DlgDemoView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif
#include "CLoginDlg.h"
#include "CPrintDlg.h"


// CDlgDemoView

IMPLEMENT_DYNCREATE(CDlgDemoView, CView)

BEGIN_MESSAGE_MAP(CDlgDemoView, CView)
	// 표준 인쇄 명령입니다.
	ON_COMMAND(ID_FILE_PRINT, &CView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_DIRECT, &CView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_PREVIEW, &CView::OnFilePrintPreview)
	ON_COMMAND(ID_MODAL, &CDlgDemoView::OnModal)
	ON_COMMAND(ID_MODALESS, &CDlgDemoView::OnModaless)
	ON_COMMAND(ID_AB, &CDlgDemoView::OnAb)
END_MESSAGE_MAP()

// CDlgDemoView 생성/소멸

CDlgDemoView::CDlgDemoView() noexcept
{
	// TODO: 여기에 생성 코드를 추가합니다.


}

CDlgDemoView::~CDlgDemoView()
{
}

BOOL CDlgDemoView::PreCreateWindow(CREATESTRUCT& cs)
{
	// TODO: CREATESTRUCT cs를 수정하여 여기에서
	//  Window 클래스 또는 스타일을 수정합니다.

	return CView::PreCreateWindow(cs);
}

// CDlgDemoView 그리기

void CDlgDemoView::OnDraw(CDC* /*pDC*/)
{
	CDlgDemoDoc* pDoc = GetDocument();
	ASSERT_VALID(pDoc);
	if (!pDoc)
		return;

	// TODO: 여기에 원시 데이터에 대한 그리기 코드를 추가합니다.
}


// CDlgDemoView 인쇄

BOOL CDlgDemoView::OnPreparePrinting(CPrintInfo* pInfo)
{
	// 기본적인 준비
	return DoPreparePrinting(pInfo);
}

void CDlgDemoView::OnBeginPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: 인쇄하기 전에 추가 초기화 작업을 추가합니다.
}

void CDlgDemoView::OnEndPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: 인쇄 후 정리 작업을 추가합니다.
}


// CDlgDemoView 진단

#ifdef _DEBUG
void CDlgDemoView::AssertValid() const
{
	CView::AssertValid();
}

void CDlgDemoView::Dump(CDumpContext& dc) const
{
	CView::Dump(dc);
}

CDlgDemoDoc* CDlgDemoView::GetDocument() const // 디버그되지 않은 버전은 인라인으로 지정됩니다.
{
	ASSERT(m_pDocument->IsKindOf(RUNTIME_CLASS(CDlgDemoDoc)));
	return (CDlgDemoDoc*)m_pDocument;
}
#endif //_DEBUG


// CDlgDemoView 메시지 처리기

void CDlgDemoView::OnModal()
{
	CLoginDlg LoginDlg;
	int nResult =LoginDlg.DoModal();

	if (nResult == IDOK) //IDOK는 UpdateData를 자동으로 해줌
	{
		AfxMessageBox(_T("밖: ")+LoginDlg.m_strDB);
	}
}


void CDlgDemoView::OnModaless()
{
	static CLoginDlg LoginDlg;
	if (LoginDlg.GetSafeHwnd()==NULL)
	{
		LoginDlg.Create(IDD_LOGON);
	}
	LoginDlg.ShowWindow(SW_SHOW);
}


void CDlgDemoView::OnAb()
{
	CFileDialog LoginDlg(FALSE);  //TRUE = 열기/FALSE = 저장
	int nResult = LoginDlg.DoModal();

	if (nResult == IDOK) //IDOK는 UpdateData를 자동으로 해줌
	{
		AfxMessageBox(LoginDlg.GetFileExt());
		AfxMessageBox(LoginDlg.GetFileName());
		AfxMessageBox(LoginDlg.GetPathName());
	}
}
