
// BackThreadDlg.cpp: 구현 파일
//

#include "pch.h"
#include "framework.h"
#include "BackThread.h"
#include "BackThreadDlg.h"
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


// CBackThreadDlg 대화 상자



CBackThreadDlg::CBackThreadDlg(CWnd* pParent /*=nullptr*/)
	: CDialogEx(IDD_BACKTHREAD_DIALOG, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CBackThreadDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CBackThreadDlg, CDialogEx)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BUTTON_OPEN, &CBackThreadDlg::OnBnClickedButtonOpen)
	ON_BN_CLICKED(IDC_BUTTON_CLOSE, &CBackThreadDlg::OnBnClickedButtonClose)
	ON_WM_TIMER()
END_MESSAGE_MAP()


// CBackThreadDlg 메시지 처리기

BOOL CBackThreadDlg::OnInitDialog()
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

void CBackThreadDlg::OnSysCommand(UINT nID, LPARAM lParam)
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

void CBackThreadDlg::OnPaint()
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
HCURSOR CBackThreadDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

DWORD g_dwTlsIndex;

void CBackThreadDlg::OnBnClickedButtonOpen()
{
	g_dwTlsIndex =TlsAlloc();

	//SetTimer(1, 100, NULL);
	SetTimer(1, 20, NULL);

}


void CBackThreadDlg::OnBnClickedButtonClose()
{
	KillTimer(1);
	TlsFree(g_dwTlsIndex);
}

int nIndex;
int nCount;
int arData[100];
int nFileIndex;

int GetAddNum()
{
	//static int m = 0; //정적변수는 경쟁이 발생함
	//__declspec (thread)static int m = 0;	//TLS로 설정함( MS에서만 가능한 것 딴곳에서는 못씀)
	int m = (int)TlsGetValue(g_dwTlsIndex);

	if (m == 5000) m = 0;
	m = m + 1000;

	TlsSetValue(g_dwTlsIndex, (LPVOID)m);
	return m;
}
DWORD WINAPI CalcThread(LPVOID p)
{
	int* arWork = (int*)p;

	//arWork의 데이터를 가공한다 (정렬,통계 등)
	for (int i = 0; i < 100; i++)
	{
		arWork[i] = arWork[i] * 2;
		arWork[i] = arWork[i] +GetAddNum();
		Sleep(50); //정렬 통계 등의 복잡한 작업을 가정함
	}

	//계산 완료된 데이터를 파일에 저장한다.
	CString strFileName;
	strFileName.Format(_T("c:\\temp\\Result%d.txt"), ++nFileIndex);

	CFile myFile(strFileName, CFile::modeCreate | CFile::modeReadWrite);
	CString strTemp;
	for (int i = 0; i < 100; i++)
	{
		strTemp.Format(_T("[%d] = %d\r\n"), i, arWork[i]);
		myFile.Write(strTemp, lstrlen(strTemp) * sizeof(TCHAR));
	}
	myFile.Close();
	MessageBeep(0);
	free(arWork);

	return 0;
}
void CBackThreadDlg::OnTimer(UINT_PTR nIDEvent)
{

	if (nIDEvent == 1)
	{
		//Air에서 데이터를 모은다.
		arData[nIndex++] = ++nCount;

		if (nIndex == 100)
		{
			SetDlgItemText(IDC_STATIC_MSG, _T("100개의 데이터 입력완료"));

			//작업 복사본을 만든다.
			int* arCopy=(int*)malloc(sizeof(arData));
			memcpy(arCopy, arData, sizeof(arData));

			DWORD ID;
			//CloseHandle(CreateThread(NULL, NULL, CalcThread, arData, 0, &ID)); //원본
			CloseHandle(CreateThread(NULL, NULL, CalcThread, arCopy, 0, &ID));	//복사본

			Sleep(1000); //다음 데이터를 받아들이기 위한 약간의 준비를 가정함
			//free(arCopy);
			nIndex = 0;
		}
		else
		{
			CString strTemp;
			strTemp.Format(_T("%d번째 데이터(%d)를 입력중"),
				nIndex, nCount);
			SetDlgItemText(IDC_STATIC_MSG, strTemp);
		}
	}
	CDialogEx::OnTimer(nIDEvent);
}
