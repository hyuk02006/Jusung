#pragma once

#include "packet.h"

// CChatDlg 대화 상자

class CChatDlg : public CDialogEx
{
	DECLARE_DYNAMIC(CChatDlg)

public:
	CChatDlg(CWnd* pParent = nullptr);   // 표준 생성자입니다.
	virtual ~CChatDlg();

// 대화 상자 데이터입니다.
#ifdef AFX_DESIGN_TIME
	enum { IDD = IDD_DIALOG_CHAT };
#endif

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 지원입니다.

	DECLARE_MESSAGE_MAP()
public:
	void SendData(CString name);

private:
	CString m_name;


public:
	virtual BOOL OnInitDialog();


	afx_msg void OnBnClickedButtonSend();
	CString m_strShortmessage;
	CEdit m_ctrlList;
	void Shortmessage(PACKETSHORTMESSAGE* msg);
	void MemoMessage(PAKETMEMO* msg);
	void DisplayText(char* fmt, ...);

	//사용자 정의 메시지 핸들러
	afx_msg LRESULT OnMyMessage(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnMyMemoMessage(WPARAM wParam, LPARAM lParam);

	afx_msg void OnBnClickedButtonSendmsg();
	CString m_strMemo;
};
