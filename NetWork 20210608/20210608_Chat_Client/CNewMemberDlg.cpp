// CNewMemberDlg.cpp: 구현 파일
//

#include "pch.h"
#include "20210608_Chat_Client.h"
#include "CNewMemberDlg.h"
#include "afxdialogex.h"


// CNewMemberDlg 대화 상자

IMPLEMENT_DYNAMIC(CNewMemberDlg, CDialogEx)

CNewMemberDlg::CNewMemberDlg(CWnd* pParent /*=nullptr*/)
	: CDialogEx(IDD_DIALOG_SIGN, pParent)
	, m_strID(_T(""))
	, m_strPW(_T(""))
	, m_strName(_T(""))
{

}

CNewMemberDlg::~CNewMemberDlg()
{
}

void CNewMemberDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT_ID, m_strID);
	DDX_Text(pDX, IDC_EDIT_PW, m_strPW);
	DDX_Text(pDX, IDC_EDIT_NAME, m_strName);
}


BEGIN_MESSAGE_MAP(CNewMemberDlg, CDialogEx)
	ON_BN_CLICKED(IDC_BUTTON_OVERLAP, &CNewMemberDlg::OnBnClickedButtonOverlap)
	ON_BN_CLICKED(IDC_BUTTON_SIGNUP, &CNewMemberDlg::OnBnClickedButtonSignup)
END_MESSAGE_MAP()


// CNewMemberDlg 메시지 처리기


//중복체크
void CNewMemberDlg::OnBnClickedButtonOverlap()
{
	UpdateData(1);
	m_strID;
	
	//서버로 전송
}


//회원가입
void CNewMemberDlg::OnBnClickedButtonSignup()
{
	//UpdateData(1).
	//IDOK 반환 설정
	CDialog::OnOK();
}


void CNewMemberDlg::GetData(CString& id, CString& pw, CString& name)
{
	id = m_strID;
	pw = m_strPW;
	name = m_strName;
}
