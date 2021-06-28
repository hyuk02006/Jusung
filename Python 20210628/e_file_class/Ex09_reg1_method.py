"""
    # [참고] 파이썬 정규식 표현
            - https://docs.python.org/3/library/re.html
            - https://wikidocs.net/4308 > re

    # raw string
        - 문자열 앞에 r이 붙으면 해당 문자열이 구성된 그대로 문자열로 변환

    # 패턴과 소스를 비교하는 메소드
    - match() : 패턴의 일치여부 확인
                search와 유사하나, 주어진 문자열의 시작부터 비교하여 패턴이 있는지 확인
                시작부터 해당 패턴이 존재하지 않다면 None 반환
    - findall() : 일치하는 모든 문자열 리스트 반환
                search가 최초로 매칭되는 패턴만 반환한다면, findall은 매칭되는 전체의 패턴을 반환
                매칭되는 모든 결과를 리스트 형태로 반환
    - search() : 첫번째 일치하는 객체 반환

    - split() : 패턴에 맞게 소스를 분리한 후 문자열 조각의 리스트 반환
    - sub() : 주어진 문자열에서 일치하는 모든 패턴을 replace
            그 결과를 문자열로 다시 반환함
            count가 0인 경우는 전체를, 1이상이면 해당 숫자만큼 치환 됨

"""

import re

# r : raw string
a = 'abcdef\n' # escapce 문자열
print(a)
b = r'abcdef\n'
print(b)


# match()
msg = 'We are happy. You are happy. Happy2019 2020'

result = re.match('We', msg)    # '^We' 시작이 We인 패턴과 동일
if result:
    print(result.group())       # groups()는 그룹의 튜플을 출력
print('1------------------------------')

result = re.match('Happy', msg) # false라서 결과 출력 없음
if result:
    print(result.group())

print('2------------------------------')

result = re.match('.*Happy', msg)
if result:
    print(result.group())
print('3------------------------------')

# search()
result = re.search('Happy', msg)  # search() : 패턴이 아무데나 있어도 찾아줌
if result:
    print(result.group())
print('4------------------------------')

# split()
result = re.split('a', msg) # 문자 'a'로 나눠서 문자열 리스트 반환
print(result)
print('5------------------------------')

# sub()
result = re.sub('a', '@', msg) # 문자 'a'를 '?'로 변환
print(result)
print('6------------------------------')