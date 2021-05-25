﻿
// CreateDemoView.h: CCreateDemoView 클래스의 인터페이스
//

#pragma once


class CCreateDemoView : public CView
{

protected: // serialization에서만 만들어집니다.
	CCreateDemoView() noexcept;
	DECLARE_DYNCREATE(CCreateDemoView)
	
// 특성입니다.
public:
	CCreateDemoDoc* GetDocument() const;
	
// 작업입니다.
public:
	int m_nAge;
	CWnd m_wndChild;
// 재정의입니다.
public:
	virtual void OnDraw(CDC* pDC);  // 이 뷰를 그리기 위해 재정의되었습니다.
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
protected:
	virtual BOOL OnPreparePrinting(CPrintInfo* pInfo);
	virtual void OnBeginPrinting(CDC* pDC, CPrintInfo* pInfo);
	virtual void OnEndPrinting(CDC* pDC, CPrintInfo* pInfo);

// 구현입니다.
public:
	virtual ~CCreateDemoView();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:

// 생성된 메시지 맵 함수
protected:
	DECLARE_MESSAGE_MAP()
public:
//	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	afx_msg void OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void OnChar(UINT nChar, UINT nRepCnt, UINT nFlags);
};

#ifndef _DEBUG  // CreateDemoView.cpp의 디버그 버전
inline CCreateDemoDoc* CCreateDemoView::GetDocument() const
   { return reinterpret_cast<CCreateDemoDoc*>(m_pDocument); }
#endif

