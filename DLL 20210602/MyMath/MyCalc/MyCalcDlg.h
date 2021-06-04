﻿
// MyCalcDlg.h: 헤더 파일
//

#pragma once


// CMyCalcDlg 대화 상자
class CMyCalcDlg : public CDialogEx
{
// 생성입니다.
public:
	CMyCalcDlg(CWnd* pParent = nullptr);	// 표준 생성자입니다.

// 대화 상자 데이터입니다.
#ifdef AFX_DESIGN_TIME
	enum { IDD = IDD_MYCALC_DIALOG };
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
	int m_nA;
	int m_nB;
	int m_nC;
	afx_msg void OnBnClickedButton1();
};