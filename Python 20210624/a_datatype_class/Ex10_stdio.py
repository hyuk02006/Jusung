"""
    [ 콘솔 입력 처리 함수 ]
    - input() : 기본적으로 문자열로 입력받음
    - eval() : 함수로 감싸면 숫자 처리됨
    - int() / float() /str() 자료형변환 (권장)
"""
"""
name = input('이름입력 ->')
print('당신은 %s 입니다.' %name)
print('당신의 이름은 {0}'.format(name))
print(type(name))
"""

# ----------------------------------
# 나이와 키를 입력받아 +1을 하여 출력한다면?
# age = int(input('나이입력 ->'))
# print('당신의 나이는 ' ,age +1)
# height = eval(input('키입력 ->'))
# print('당신의 키는', height)

# print(eval('1+2'))

# ----------------------------------

# 단을 입력받아 구구단을 구하기
# dan = int(input('단입력->'))
# for i in range(1, 10): # 1~9까지
#     print(dan, '*', i, '=' , dan*i)

# -----------------------------------------
# print() 함수
print('안녕' '친구')
print('안녕','친구')
print('안녕'+'친구')

for i in range(5):
    print(i, end=' ')   # 개챙문자 대신 공백문자 사용됨

# -------------------------------------------------------
# 명령행 매개변수 받기 - java의 main() 함수의 인자
# [ 콘솔에서 실행 ] python Ex10_stdio.py ourserver scott tiger
#                                   0         1      2      3
import sys
args =sys.argv[1:]
for i in args:
    print(i)
    print('서버에 연결합니다')