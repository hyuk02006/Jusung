"""
 [ 제어문 ]
    - 파이썬은 들여쓰기를 하여 블록을 표현한다
    - 들여쓰기는 탭과 공백을 섞어 쓰지 말자

    [ex]
    if a>b:
        print(a)
            print(b)  => 에러발생

    (1) if 문
        if 조건식A :
            문장들
        elif 조건식B :
            문장들
        else :
            문장들

        ` 조건식이나 else 뒤에는 콜론(:) 표시
        ` 조건식을 ( ) 괄호 필요없다
        ` 실행할 코드가 없으면 pass
        ` 파이썬은 switch 문 없음
"""

# 거짓(False)의 값
i = 0;
i2=0.0
i3=""
i4=str()
i5=list()
i6=tuple()
i7=set()
i8=dict()
i9={}
i10=None

# 1)-----------------------------------------------------------------------
a = -1  # a=0 a가 0일 경우만 false이고 -1인 경우도 true
if a:
    print('True')
else:
    print('False')

if not i:
    print('True2')

# 2) 논리 연산자를 이용한 조건
a = 10
b = -1
if a and b:
    print('True3')

if a or b:
    print('True4')

# 정리
print(a and b)  # True 값이 들어갈 것 같지만 -1이 들어감
                # b의 값에 의해 결정되기 때문에 b값을 지정

print(a or b)   # a값에 의해 결과가 결정되기에 a값을 지정
print('*'*50)

# 3)-----------------------------------------------------------------------
# find()는 못찾으면 -1을 리턴하고, 첫번째 인자라면 0을 리턴
word = 'korea'
if word.find('k'):
    print('1>'+word)

if word.find('z'):
    print('2>'+word)

# 해결
if word.find('k') > -1:
    print('3>'+word)

if word.find('z') > -1:
    print('4>'+word)

# 4)-----------------------------------------------------------------------
a = 10
if not a:
    print('inside if ' + str(a))     # a가 0이 아니라면

print('outside if ' + str(a))        # 블록은 반드시 탭크기에 맞춰야 한다. 한 칸이라도 달라지면 실행되지 않는다.

if a:
    c = 2
elif b:
    c = 4
else:
    c = 6
print(c)    # 지역변수 개념이 없다

# 들여쓰기 주의!
# (1)
imsi = 100
if imsi:
    print('A')
print('B')
print('C')

# (2)
imsi = 100
if imsi:
    print('A')
    print('B')
    print('C')

#  (3)에러
# imsi = 100
# if imsi:
#     print('A')
#  print('B')
#     print('C')
