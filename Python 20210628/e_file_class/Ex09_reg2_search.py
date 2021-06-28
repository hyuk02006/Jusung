'''
   다른 모든 함수들도 정규식 표현으로 추출 및 검색 가능

   # 기본 패턴
     - a, X, 9 등등 문자 하나하나의 character들은 정확히 해당 문자와 일치
       e.g) 패턴 test는 test 문자열과 일치
       대소문자의 경우 기본적으로 구별하나, 구별하지 않도록 설정 가능

     - 몇몇 문자들에 대해서는 예외가 존재하는데, 이들은 틀별한 의미로 사용 됨
       . ^ $ * + ? { } [ ] \ | ( )

     - . (마침표) - 어떤 한개의 character와 일치 (newline(엔터) 제외)

     - \w - 문자 character와 일치 [a-zA-Z0-9_]
     - \s - 공백문자와 일치
     - \t, \n, \r - tab, newline, return
     - \d - 숫자 character와 일치 [0-9]
     - ^ (시작), $(끝) :  각각 문자열의 시작과 끝을 의미
     - \가 붙으면 스페셜한 의미가 없어짐. 예를들어 \\.는 .자체를 의미 \\\는 \를 의미


    # [] 문자들의 범위를 나타내기 위해 사용
       - [] 내부의 메타 캐릭터는 캐릭터 자체를 나타냄
       - e.g)
       - [abck] : a or b or c or k
       - [abc.^] : a or b or c or . or ^
       - [a-d]  : -와 함께 사용되면 해당 문자 사이의 범위에 속하는 문자 중 하나
       - [0-9]  : 모든 숫자
       - [a-z]  : 모든 소문자
       - [A-Z]  : 모든 대문자
       - [a-zA-Z0-9] : 모든 알파벳 문자 및 숫자
       - [^0-9] : ^가 맨 앞에 사용 되는 경우 해당 문자 패턴이 아닌 것과 매칭


    # \
     1. 다른 문자와 함께 사용되어 특수한 의미를 지님
       - \d : 숫자를          [0-9]와 동일
       - \D : 숫자가 아닌 문자  [^0-9]와 동일
       - \s : 공백 문자(띄어쓰기, 탭, 엔터 등)
       - \S : 공백이 아닌 문자
       - \w : 알파벳대소문자, 숫자 [0-9a-zA-Z]와 동일
       - \W : non alpha-numeric 문자 [^0-9a-zA-Z]와 동일
     2. 메타 캐릭터가 캐릭터 자체를 표현하도록 할 경우 사용
       - \\. , \\\

     # 반복패턴
     - 패턴 뒤에 위치하는 *, +, ?는 해당 패턴이 반복적으로 존재하는지 검사
       - '+' -> 1번 이상의 패턴이 발생
       - '*' -> 0번 이상의 패턴이 발생
       - '?' -> 0 혹은 1번의 패턴이 발생
     - 반복을 패턴의 경우 greedy하게 검색 함, 즉 가능한 많은 부분이 매칭되도록 함
      - e.g) a[bcd]*b  패턴을 abcbdccb에서 검색하는 경우
        - ab, abcb, abcbdccb 전부 가능 하지만 최대한 많은 부분이 매칭된 abcbdccb가 검색된 패턴
'''

import re

'''
msg = '123abc'
c = re.search(r'123', msg)
if c:
    print(c.group())

c = re.search(r'\d\d\d', msg)
if c:
    print(c.group())

c = re.search(r'..\w', msg)   # \w : [a-zA-Z0-9_]
if c:
    print(c.group())
print('-'*40,'1')
'''

#--------------------------------------------------------
# 소문자를 찾아서  반환
msg = '2025 Happy year! We are so happy?'

c = re.search('[a-z]', msg)
print(c)
if c:
    print(c.group())

# 소문자가 아닌 것들을 찾아서 반환 ( 대괄호 안에 ^ )
c = re.search('[^a-z]', msg)
if c:
    print(c.group())

# 대문자를 찾음
c = re.search('[A-Z]', msg)
if c:
    print(c.group())

# +반복 옵셥으로 소문자를 연속해서 찾음 ( 즉, 단어 )
c = re.search('[a-z]+', msg)
if c:
    print(c.group())

# 소문자와 대문자를 단어 단위로 찾음
c = re.search('[a-zA-Z]+', msg)
if c:
    print(c.group())

# 숫자를 찾음
c = re.search('[0-9]', msg)
if c:
    print(c.group())

# +반복 옵션으로 숫자를 찾음
c = re.search('[0-9]+', msg)
if c:
    print(c.group())

# 소문자, 대문자, 숫자가 아닌 문자들 ( 공백문자나 특수문자 )
c = re.search('[^a-zA-Z0-9]', msg)
print(c)
if c:
    print('<', c.group(),'>')

print('-'*40, '2')


# 문자 숫자 _ 를 검색
c = re.search('[\w]', msg)
if c:
    print(c.group())

c = re.search('[\w]+', msg)
if c:
    print(c.group())

# 영문자 숫자 _가 아닌 것들 검색
c = re.search('[\W]+', msg)
if c:
    print('<',c.group(),'>')

# 숫자 검색
c = re.search('[\d]+', msg)
if c:
    print(c.group())







