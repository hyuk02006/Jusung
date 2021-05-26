
// MyFriendDlg.h: 헤더 파일
//

#pragma once


// CMyFriendDlg 대화 상자
class CMyFriendDlg : public CDialogEx
{
// 생성입니다.
public:
	CMyFriendDlg(CWnd* pParent = nullptr);	// 표준 생성자입니다.

// 대화 상자 데이터입니다.
#ifdef AFX_DESIGN_TIME
	enum { IDD = IDD_MYFRIEND_DIALOG };
#endif

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV 지원입니다.


// 구현입니다.
protected:
	HICON m_hIcon;

	// 생성된 메시지 맵 함수
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
	CString m_strName;
	int m_nAge;
	BOOL m_bGender;

	CListCtrl m_ctrlFriend;
	CSpinButtonCtrl m_ctrlSpin;

	afx_msg void OnBnClickedButtonAdd();
	
	afx_msg void OnBnClickedButtonDelete();
	afx_msg void OnLvnEndlabeleditListFriend(NMHDR* pNMHDR, LRESULT* pResult);
};
