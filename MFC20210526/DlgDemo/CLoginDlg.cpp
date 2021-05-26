// CLoginDlg.cpp: 구현 파일
//

#include "pch.h"
#include "DlgDemo.h"
#include "CLoginDlg.h"
#include "afxdialogex.h"


// CLoginDlg 대화 상자

IMPLEMENT_DYNAMIC(CLoginDlg, CDialogEx)

CLoginDlg::CLoginDlg(CWnd* pParent /*=nullptr*/)
	: CDialogEx(IDD_LOGON, pParent)
	, m_strDB(_T(""))
{

}

CLoginDlg::~CLoginDlg()
{
}

void CLoginDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT1, m_strDB);
}


BEGIN_MESSAGE_MAP(CLoginDlg, CDialogEx)
	ON_BN_CLICKED(IDC_BUTTON_DB, &CLoginDlg::OnBnClickedButtonDb)
END_MESSAGE_MAP()


// CLoginDlg 메시지 처리기


void CLoginDlg::OnBnClickedButtonDb()
{
		GetDlgItem(IDC_EDIT1)->GetWindowText(m_strDB);
		AfxMessageBox(m_strDB);
}
