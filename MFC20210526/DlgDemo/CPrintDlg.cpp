// CPrintDlg.cpp: 구현 파일
//

#include "pch.h"
#include "DlgDemo.h"
#include "CPrintDlg.h"
#include "afxdialogex.h"


// CPrintDlg 대화 상자

IMPLEMENT_DYNAMIC(CPrintDlg, CDialogEx)

CPrintDlg::CPrintDlg(CWnd* pParent /*=nullptr*/)
	: CDialogEx(IDD_PRINT, pParent)
{

}

CPrintDlg::~CPrintDlg()
{
}

void CPrintDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
}


BEGIN_MESSAGE_MAP(CPrintDlg, CDialogEx)
END_MESSAGE_MAP()


// CPrintDlg 메시지 처리기
