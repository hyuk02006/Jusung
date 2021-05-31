
// MemoryMapFileDlg.cpp: 구현 파일
//

#include "pch.h"
#include "framework.h"
#include "MemoryMapFile.h"
#include "MemoryMapFileDlg.h"
#include "afxdialogex.h"
#pragma warning (disable:4996)
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


// CMemoryMapFileDlg 대화 상자



CMemoryMapFileDlg::CMemoryMapFileDlg(CWnd* pParent /*=nullptr*/)
	: CDialogEx(IDD_MEMORYMAPFILE_DIALOG, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CMemoryMapFileDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CMemoryMapFileDlg, CDialogEx)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BUTTON_READ, &CMemoryMapFileDlg::OnBnClickedButtonRead)
	ON_BN_CLICKED(IDC_BUTTON_WRITE, &CMemoryMapFileDlg::OnBnClickedButtonWrite)
END_MESSAGE_MAP()


// CMemoryMapFileDlg 메시지 처리기

BOOL CMemoryMapFileDlg::OnInitDialog()
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

void CMemoryMapFileDlg::OnSysCommand(UINT nID, LPARAM lParam)
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

void CMemoryMapFileDlg::OnPaint()
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
HCURSOR CMemoryMapFileDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}



void CMemoryMapFileDlg::OnBnClickedButtonRead()
{
	HANDLE hFile=CreateFile(_T("c:\\temp\\Naru222.txt"), GENERIC_READ, 0, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);

	//1. 파일 매핑 오브젝트를 생성한다.
	HANDLE hFMap = CreateFileMapping(hFile, NULL, PAGE_READONLY, 0, 0, NULL);

	//2. 파일 매핑 오브젝트를 가상메모리 주소공간에 연결한다.
	TCHAR* pText = (TCHAR*)MapViewOfFile(hFMap, FILE_MAP_READ, 0, 0, 0);

	//3. 파일을 변수처럼 사용한다.
	if (IsTextUnicode(pText, 10, NULL))
	{
		//유니코드
		AfxMessageBox(pText);
	}
	else
	{
		//ANCII 코드
		char* pAnsiTExt =(char*)pText;
		MessageBoxA(NULL,pAnsiTExt,"송기혁",MB_OK);
	}

	//4. 뒷정리한다.
	UnmapViewOfFile(pText);  
	CloseHandle(hFMap);
	CloseHandle(hFile);

}


void CMemoryMapFileDlg::OnBnClickedButtonWrite()
{
	HANDLE hFile;
	hFile = CreateFile(_T("c:\\temp\\Alpha.txt"), GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);

	//1.
	HANDLE hFMap;
	hFMap = CreateFileMapping(hFile, NULL, PAGE_READWRITE, 0, 0, NULL);

	//2.
	char* pText = (char*)MapViewOfFile(hFMap, FILE_MAP_WRITE, 0, 0, 0);

	//3.파일을 메모리 처럼 사용한다.
	strcpy(pText,"HELLO");

	//4.뒷정리 한다
	UnmapViewOfFile(pText);
	CloseHandle(hFMap);
	CloseHandle(hFile);

}
