#pragma once
#include <afx.h>
class CPerson :
    public CObject
{
public:
    CPerson();
    ~CPerson();

    int m_nAge;
    BOOL m_bGender;
    CString m_strName;
    virtual void Serialize(CArchive& ar);
};
 