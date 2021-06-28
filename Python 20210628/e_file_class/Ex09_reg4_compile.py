'''
    # compile
     - 동일한 정규표현식을 매번 다시 쓰기 번거로움을 해결
     - compile로 해당표현식을 re.RegexObject 객체로 저장하여 사용가능
'''

import re
email_reg = re.compile(r'[\w-]+@[\w.]+')
c = email_reg.search('test@gmail.com haha good')
print(c)
if c:
    print(c.group())

#----------------------------------------
webs = ['http://www.test.co.kr',
        'https://www.test1.com',
        'http://www.test.com',
        'ftp://www.test.com',
        'http:://www.test.com',
       'htp://www.test.com',
       'http://www.google.com',
       'https://www.homepage.com.']

web_reg = re.compile(r'https?://[\w.]+\w+$')
# s? : s가 있기도 하고 없기도 하고
# [\w.]+ : 문자숫자와 점(.)이 여러개 나옴 (그러나 마지막에 점(.)이 있어도 검색대상에 포함되기에
# \w+$ : 문자숫자 여러개로 마지막 문자임을 지정
result = list(map(lambda w:web_reg.search(w) != None, webs))
print(result)