"""
    이메일 주소의 적합성 체크
        kim@encore.com   : 올바른 이메일
        kim@encore       : 잘못된 이메일 ( . 하나 없어서 )
        k!m@encore.com  : 잘못된 이메일 ( 특수문자 ! 안됨 )

     [참고]
        ^[]: 시작
        [^] : not
        {2,9} : 최소 2개 최대 9개
        {2,} : 최소 2개만 지정하고 최대는 지정하지 않음
        $ 끝
"""
import re
def email_check(email):
    # exp = re.findall('^[a-z0-9]{2,}@[a-z0-9]{2,}\.[a-z]{2,}$', email)
    # exp = re.findall('^[\w]{2,}@[\w]{2,}\.[\w]{2,}$', email)
    exp = re.findall('^[\w]+@[\w]+\.[\w]+$', email)
    # 소문자와 숫자만 {2,} 최소 2개에서 최대는 지정하지 않음
    if len(exp) == 0:
        print('잘못된 이메일입니다.')
    else:
        print('올바른 이메일입니다')

email_check('kim@encore.com')       # 올바른 이메일
email_check('kim@encore')           # 잘못된 이메일 ( . 하나 없어서 )
email_check('k!m@encore.com')      # 잘못된 이메일 ( 특수문자 ! 안됨 )