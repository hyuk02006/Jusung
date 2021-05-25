// ListBoxDemoDlg.cpp: 구현 파일
//
#include "pch.h"
#include "framework.h"
#include "ListBoxDemo.h"
#include "ListBoxDemoDlg.h"
#include "afxdialogex.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// 응용 프로그램 정보에 사용되는 CAboutDlg 대화 상자입니다.

class CAboutDlg : public CDialogEx
{
public:
	CAboutDlg();

	// 대화 상자 데이터입니다.
#ifdef AFX_DESIGN_TIME
	enum { IDD = IDD_ABOUTBOX };
#endif

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 지원입니다.

// 구현입니다.
protected:
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialogEx(IDD_ABOUTBOX)
{
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialogEx)
END_MESSAGE_MAP()


// CListBoxDemoDlg 대화 상자



CListBoxDemoDlg::CListBoxDemoDlg(CWnd* pParent /*=nullptr*/)
	: CDialogEx(IDD_LISTBOXDEMO_DIALOG, pParent)
	, m_strName(_T(""))
	, m_strComboName(_T(""))
{mfc 문자열 찾기getCount()
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CListBoxDemoDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_LIST_NAME, m_ctrlNameBox);
	DDX_Text(pDX, IDC_EDIT_NAME, m_strName);
	DDX_Control(pDX, IDC_COMBO_NAME, m_crtlNameCombo);
	DDX_CBString(pDX, IDC_COMBO_NAME, m_strComboName);
}

BEGIN_MESSAGE_MAP(CListBoxDemoDlg, CDialogEx)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BUTTON_ADD, &CListBoxDemoDlg::OnBnClickedButtonAdd)
	ON_BN_CLICKED(IDC_BUTTON_INSERT, &CListBoxDemoDlg::OnBnClickedButtonInsert)
	ON_BN_CLICKED(IDC_BUTTON_FIND, &CListBoxDemoDlg::OnBnClickedButtonFind)
	ON_LBN_SELCHANGE(IDC_LIST_NAME, &CListBoxDemoDlg::OnLbnSelchangeListName)
	ON_BN_CLICKED(IDC_BUTTON4, &CListBoxDemoDlg::OnBnClickedButton4)
	ON_BN_CLICKED(IDC_BUTTON_ADDCOMBO, &CListBoxDemoDlg::OnBnClickedButtonAddcombo)
END_MESSAGE_MAP()


// CListBoxDemoDlg 메시지 처리기

BOOL CListBoxDemoDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	// 시스템 메뉴에 "정보..." 메뉴 항목을 추가합니다.

	// IDM_ABOUTBOX는 시스템 명령 범위에 있어야 합니다.
	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != nullptr)
	{
		BOOL bNameValid;
		CString strAboutMenu;
		bNameValid = strAboutMenu.LoadString(IDS_ABOUTBOX);
		ASSERT(bNameValid);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	// 이 대화 상자의 아이콘을 설정합니다.  응용 프로그램의 주 창이 대화 상자가 아닐 경우에는
	//  프레임워크가 이 작업을 자동으로 수행합니다.
	SetIcon(m_hIcon, TRUE);			// 큰 아이콘을 설정합니다.
	SetIcon(m_hIcon, FALSE);		// 작은 아이콘을 설정합니다.

	// TODO: 여기에 추가 초기화 작업을 추가합니다.

	return TRUE;  // 포커스를 컨트롤에 설정하지 않으면 TRUE를 반환합니다.
}

void CListBoxDemoDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialogEx::OnSysCommand(nID, lParam);
	}
}

// 대화 상자에 최소화 단추를 추가할 경우 아이콘을 그리려면
//  아래 코드가 필요합니다.  문서/뷰 모델을 사용하는 MFC 애플리케이션의 경우에는
//  프레임워크에서 이 작업을 자동으로 수행합니다.

void CListBoxDemoDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // 그리기를 위한 디바이스 컨텍스트입니다.

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// 클라이언트 사각형에서 아이콘을 가운데에 맞춥니다.
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// 아이콘을 그립니다.
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialogEx::OnPaint();
	}
}

// 사용자가 최소화된 창을 끄는 동안에 커서가 표시되도록 시스템에서
//  이 함수를 호출합니다.
HCURSOR CListBoxDemoDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}


void CListBoxDemoDlg::OnBnClickedButtonAdd()
{
	//1. 에디트 창의 글자를 읽어온다.
	//UpdateData(1);//컨트롤 --> 변수(m_strName)
	GetDlgItem(IDC_EDIT_NAME)->GetWindowText(m_strName);
	//2. 그 글자를 리스트 박스에 추가한다.
	m_ctrlNameBox.AddString(m_strName);
	//m_ctrlNameBox.SetWindowTextW(m_strName);
	//3. 에디트 창의 글자를 초기화한다.
	m_strName = _T("");
	//UpdateData(0); //컨트롤 <-- 변수(m_strName)
	GetDlgItem(IDC_EDIT_NAME)->SetWindowText(m_strName);
}


void CListBoxDemoDlg::OnBnClickedButtonInsert()
{
	//1. 에디트 창의 글자를 읽어온다.
	UpdateData(1);//컨트롤 --> 변수(m_strName)
	//2. 그 글자를 리스트 박스에 삽입 한다.
	m_ctrlNameBox.InsertString(0, m_strName);
	//3. 에디트 창의 글자를 초기화한다.
	m_strName = _T("");
	UpdateData(0); //컨트롤 <-- 변수(m_strName)
}


void CListBoxDemoDlg::OnBnClickedButtonFind()
{
	//1. 에디트 창의 글씨를 읽어온다.
	UpdateData(1);//컨트롤 --> 변수(m_strName)
	//2. 그 글자로 리스트박스에서 검색한다.
	int nIndex = m_ctrlNameBox.FindString(-1, m_strName);
	for (int nIndex = 0; nIndex <= m_ctrlNameBox.GetCount();nIndex++)
	{
		if (nIndex == LB_ERR)
		{
			AfxMessageBox(_T("그런 사람 없어요."));
		}
		else
		{
			CString strTemp;
			m_ctrlNameBox.GetText(nIndex, strTemp);
			if (m_strName == strTemp)
			{
				AfxMessageBox(strTemp);
			}
		}
		
	}
	//3. 에디트 창을 초기화한다.
	m_strName = _T("");
	UpdateData(0); //컨트롤 <-- 변수(m_strName)
}


void CListBoxDemoDlg::OnLbnSelchangeListName()
{
	//1. 현재 선택된 셀렉션을 구한다(int index)
	int index = m_ctrlNameBox.GetCurSel();
	if (index == LB_ERR) return;
	//2. 구한 index의 글자를 구한다.
	CString strTemp;
	m_ctrlNameBox.GetText(index, strTemp);
	//3. 그 글자를 대화 상자에 쓴다
	this->SetWindowText(strTemp);
}


void CListBoxDemoDlg::OnBnClickedButton4()
{
	//1. 현재 선택된 셀렉션을 구한다(int index)
	int index = m_ctrlNameBox.GetCurSel();
	if (index == LB_ERR) return;
	//2. 구한 index의 글자를 구한다.
	CString strTemp;
	m_ctrlNameBox.GetText(index, strTemp);
	//3. 그 글자를 대화 상자에 쓴다
	this->SetWindowText(strTemp);
	AfxMessageBox(_T("전화걸기") + strTemp);
}


void CListBoxDemoDlg::OnBnClickedButtonAddcombo()
{
	//1. 콤보의 에디트 창의 글자를 읽어온다
	//UpdateData(1);
	GetDlgItem(IDC_COMBO_NAME)->GetWindowText(m_strComboName);
	//2. 그 글자를 콤보박스에 추가한다.
	m_crtlNameCombo.AddString(m_strComboName);
	m_ctrlNameBox.AddString(m_strComboName);
	//3.콤보의 에디트 창을 초기화한다.
	m_strComboName = _T("");
	//GetDlgItem(IDC_COMBO_NAME)->SetWindowText(m_strComboName);
	//UpdateData(0); //컨트롤 <-- 변수(m_strName)
	m_crtlNameCombo.SetEditSel(0, -1);
	m_crtlNameCombo.Clear();
}
