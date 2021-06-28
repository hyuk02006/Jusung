'''
    # [참고] 파이썬 정규식 표현
            - https://docs.python.org/3/library/re.html
            - https://wikidocs.net/4308 > re

    # findall(검색어, 문자열) : 문자열에서 검색어와 일하는 내용들을 리스트로 반환
                search가 최초로 매칭되는 패턴만 반환한다면, findall은 매칭되는 전체의 패턴을 반환
                매칭되는 모든 결과를 리스트 형태로 반환

'''

import re

msg = 'We_are_happy!! You are happy?? Happy2019-2020 안녕'

# a = re.findall('happy', msg)
# print(a)
# b = re.findall('Happy', msg)
# print(b)

#----------------------------------------- 같이 하자
# 소문자를 모두 찾아서 리스트로 반환
c = re.findall('[a-z]', msg)
print(c)

# 소문자가 아닌 것들을 모두 찾아서 리스트로 반환 ( 대괄호 안에 ^ )
c = re.findall('[^a-z]', msg)
print(c)

# +반복 옵셥으로 소문자를 연속해서 찾음 ( 즉, 단어 )
c = re.findall('[a-z]+', msg)
print(c)

# 대문자를 모두 찾음
c = re.findall('[A-Z]', msg)
print(c)

# 소문자와 대문자를 단어 단위로 찾음
c = re.findall('[a-zA-Z]+', msg)
print(c)

# 숫자를 모두 찾음
c = re.findall('[0-9]', msg)
print(c)

# +반복 옵션으로 숫자를 연속해서 찾음
c = re.findall('[0-9]+', msg)
print(c)

# 소문자, 대문자, 숫자가 아닌 문자들 ( 공백문자나 특수문자 )
c = re.findall('[^a-zA-Z0-9]', msg)
print(c)

# 문자 숫자 _ 를 검색
c = re.findall('[\w]', msg)
print(c)

c = re.findall('[\w]+', msg)
print(c)

# 영문자 숫자 _가 아닌 것들 검색
c = re.findall('[\W]+', msg)
print(c)