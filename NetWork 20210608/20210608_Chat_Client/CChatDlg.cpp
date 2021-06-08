// CChatDlg.cpp: 구현 파일
//

#include "pch.h"
#include "20210608_Chat_Client.h"
#include "CChatDlg.h"
#include "afxdialogex.h"
#include "ChatControl.h"


// CChatDlg 대화 상자

CChatDlg* g_pDlg = NULL;
IMPLEMENT_DYNAMIC(CChatDlg, CDialogEx)

CChatDlg::CChatDlg(CWnd* pParent /*=nullptr*/)
	: CDialogEx(IDD_DIALOG_CHAT, pParent)
	, m_strShortmessage(_T(""))
	, m_strMemo(_T(""))
{
	g_pDlg = this;
}

CChatDlg::~CChatDlg()
{
}

void CChatDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT3, m_strShortmessage);
	DDX_Control(pDX, IDC_EDIT_DIG, m_ctrlList);
	DDX_Text(pDX, IDC_EDIT_NOTE, m_strMemo);
}


BEGIN_MESSAGE_MAP(CChatDlg, CDialogEx)
	ON_BN_CLICKED(IDC_BUTTON_SEND, &CChatDlg::OnBnClickedButtonSend)
	ON_MESSAGE(EVENT_SENDMESSAGE, &CChatDlg::OnMyMessage)
	ON_MESSAGE(EVENT_MEMOMESSAGE, &CChatDlg::OnMyMemoMessage)
	ON_BN_CLICKED(IDC_BUTTON_SENDMSG, &CChatDlg::OnBnClickedButtonSendmsg)
END_MESSAGE_MAP()


// CChatDlg 메시지 처리기


void CChatDlg::SendData(CString name)
{
	m_name = name;
}

BOOL CChatDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	// TODO:  여기에 추가 초기화 작업을 추가합니다.
	CString strTemp;
	strTemp.Format("%s님이 로그인 하셨습니다.", m_name);
	SetWindowText(strTemp);
	return TRUE;  // return TRUE unless you set the focus to a control
				  // 예외: OCX 속성 페이지는 FALSE를 반환해야 합니다.
}

//채팅에 대한 전송 버튼
void CChatDlg::OnBnClickedButtonSend()
{
	UpdateData(1);

	if (m_strShortmessage == "")
	{
		return;
	}
	ChatControl::getInstance()->ShortMesaage(m_name, m_strShortmessage);
	m_strShortmessage = "";
	UpdateData(0);
}

void CChatDlg::Shortmessage(PACKETSHORTMESSAGE* msg)
{
	g_pDlg->PostMessage(EVENT_SENDMESSAGE, (WPARAM)msg, 0);

	//DisplayText("[%s]%s(%d:%d:%d)", msg->name, msg->msg, msg->hour, msg->min, msg->second);
}

void CChatDlg::DisplayText(char* fmt, ...)
{
	va_list arg;			va_start(arg, fmt);

	char cbuf[512 + 256];	vsprintf_s(cbuf, fmt, arg);
	
	//크로스 쓰레드 문제 발생
	//컨트롤을 생성한 쓰레드 (primary thread)
	//컨트롤을 사용하는 쓰레드가 다를경우 발생하는 문제.
	CEdit* pEdit = (CEdit*)GetDlgItem(IDC_EDIT_DIG);
	int nLength = pEdit->GetWindowTextLength();
	pEdit->SetSel(nLength, nLength);
	pEdit->ReplaceSel(cbuf);

	va_end(arg);
}


void CChatDlg::MemoMessage(PAKETMEMO* msg)
{
	g_pDlg->PostMessage(EVENT_MEMOMESSAGE, (WPARAM)msg, 0);
}

LRESULT CChatDlg::OnMyMessage(WPARAM wParam, LPARAM lParam)
{
	PACKETSHORTMESSAGE* p = (PACKETSHORTMESSAGE*)wParam;
	DisplayText("[%s]%s(%d:%d:%d)\r\n",
		p->name, p->msg, p->hour, p->min, p->second);
	return 0;
}

LRESULT CChatDlg::OnMyMemoMessage(WPARAM wParam, LPARAM lParam)
{
	PAKETMEMO* pmsg = (PAKETMEMO*)wParam;
	m_strMemo = pmsg->msg;
	UpdateData(FALSE);	//<-----
	return 0;
}



//쪽지 보내기
void CChatDlg::OnBnClickedButtonSendmsg()
{
	UpdateData(1);

	if (m_strMemo == "")
	{
		return;
	}
	ChatControl::getInstance()->MemoMessage(m_name, m_strMemo);
	m_strMemo = "";
	UpdateData(0);

}
