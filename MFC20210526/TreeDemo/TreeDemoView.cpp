
// TreeDemoView.cpp: CTreeDemoView 클래스의 구현
//

#include "pch.h"
#include "framework.h"
// SHARED_HANDLERS는 미리 보기, 축소판 그림 및 검색 필터 처리기를 구현하는 ATL 프로젝트에서 정의할 수 있으며
// 해당 프로젝트와 문서 코드를 공유하도록 해 줍니다.
#ifndef SHARED_HANDLERS
#include "TreeDemo.h"
#endif

#include "TreeDemoDoc.h"
#include "TreeDemoView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CTreeDemoView

IMPLEMENT_DYNCREATE(CTreeDemoView, CFormView)

BEGIN_MESSAGE_MAP(CTreeDemoView, CFormView)
	// 표준 인쇄 명령입니다.
	ON_COMMAND(ID_FILE_PRINT, &CFormView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_DIRECT, &CFormView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_PREVIEW, &CFormView::OnFilePrintPreview)
END_MESSAGE_MAP()

// CTreeDemoView 생성/소멸

CTreeDemoView::CTreeDemoView() noexcept
	: CFormView(IDD_TREEDEMO_FORM)
{
	// TODO: 여기에 생성 코드를 추가합니다.

}

CTreeDemoView::~CTreeDemoView()
{
}

void CTreeDemoView::DoDataExchange(CDataExchange* pDX)
{
	CFormView::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_FAMILY, m_ctrlFamily);
}

BOOL CTreeDemoView::PreCreateWindow(CREATESTRUCT& cs)
{
	// TODO: CREATESTRUCT cs를 수정하여 여기에서
	//  Window 클래스 또는 스타일을 수정합니다.

	return CFormView::PreCreateWindow(cs);
}

void CTreeDemoView::OnInitialUpdate()
{
	CFormView::OnInitialUpdate();
	GetParentFrame()->RecalcLayout();
	ResizeParentToFit();
	
	/// /////////////////////////////////////////
	//이미지 리스트 사용법
	//1. 빈 방을 생성한다.
	static CImageList imgList;
	imgList.Create(16, 16, ILC_COLOR24, 6,0);
	//2. 각 방에 이미지를 추가한다(ico,bmp)
	CBitmap bmpTree;
	bmpTree.LoadBitmap(IDB_TREE);
	imgList.Add(&bmpTree,RGB(0,0,0));
	//3. 컨트롤과 이미지 리스트가 결합한다.
	m_ctrlFamily.SetImageList(&imgList, TVSIL_NORMAL);
	//4. 컨트롤은 이미지를 번호(index)로 사용한다.

	//===================================================
	HTREEITEM hHal,hB,hM,hS, hMe;
	hHal = m_ctrlFamily.InsertItem(_T("할아버지"), 0, 2, TVI_ROOT);
	//===================================================
	hB = m_ctrlFamily.InsertItem(_T("큰아버지"), 2, 0, hHal);
	m_ctrlFamily.InsertItem(_T("사촌형"), 2, 0, hB);
	m_ctrlFamily.InsertItem(_T("사촌누나"), 2, 0, hB);
	//===================================================
	hM = m_ctrlFamily.InsertItem(_T("아버지"), 3, 0, hHal);
	m_ctrlFamily.InsertItem(_T("형"), 3, 0, hM);
	hMe = m_ctrlFamily.InsertItem(_T("나"), 3, 0, hM);
	m_ctrlFamily.InsertItem(_T("동생"), 3, 0, hM);
	//===================================================
	hS = m_ctrlFamily.InsertItem(_T("작은아버지"), 5, 0, hHal);
	m_ctrlFamily.InsertItem(_T("사촌 동생"), 5, 0, hS);
	//===================================================

	 m_ctrlFamily.EnsureVisible(hMe);

}


// CTreeDemoView 인쇄

BOOL CTreeDemoView::OnPreparePrinting(CPrintInfo* pInfo)
{
	// 기본적인 준비
	return DoPreparePrinting(pInfo);
}

void CTreeDemoView::OnBeginPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: 인쇄하기 전에 추가 초기화 작업을 추가합니다.
}

void CTreeDemoView::OnEndPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: 인쇄 후 정리 작업을 추가합니다.
}

void CTreeDemoView::OnPrint(CDC* pDC, CPrintInfo* /*pInfo*/)
{
	// TODO: 여기에 사용자 지정 인쇄 코드를 추가합니다.
}


// CTreeDemoView 진단

#ifdef _DEBUG
void CTreeDemoView::AssertValid() const
{
	CFormView::AssertValid();
}

void CTreeDemoView::Dump(CDumpContext& dc) const
{
	CFormView::Dump(dc);
}

CTreeDemoDoc* CTreeDemoView::GetDocument() const // 디버그되지 않은 버전은 인라인으로 지정됩니다.
{
	ASSERT(m_pDocument->IsKindOf(RUNTIME_CLASS(CTreeDemoDoc)));
	return (CTreeDemoDoc*)m_pDocument;
}
#endif //_DEBUG


// CTreeDemoView 메시지 처리기
