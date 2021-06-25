"""
    [ 함수 ]

     반복적인 구문을 만들어 함수로 선언하여 간단하게 호출만 하고자 한다

     def 함수명(매개변수):
        수행할 문장들
"""
# (0) 인자도 리턴값도 없는 함수
# def func(): # 함수 정의부
#     print('inside func')
#
# func()
# print(func())   # 리턴값이 없으면 None을 반환한다

# (1) 리턴값이 2개인 함수
# print('(1)-----------------------------------------')

# def func(arg1, arg2):
#     return arg1+5, arg2-5
#
# print(func(2, 10))
# a, b =func(2,10)
# print(a+b)

# (2) 위치인자 (positional argument)
print('(2)-----------------------------------------')
def func(greeting, name):
    print(greeting, '!!!', name, '님~')

func('하이', '송기혁')
func('김길동', '헬로우')
print()

# (3) 키워드 인자 (Keyword argument)
print('(3)-----------------------------------------')
func(name ='송기혁', greeting='안녕')
print()

# (4) 기본 매개변수값 지정하기
print('(4)-----------------------------------------')
# func('하이') 오류

def func(greeting, name="홍동우"):
    print(greeting, '!!!', name, '님~')
func('하이')


# [확인]
# def fun(i1, i2, i3=100):
#      print(i1)
#      print(i2)
#      print(i3)
# func(i3=i3, i1=i1, i2=i2)
# print()

# [참고] 기본 인자값은 함수가 실행될 때 생성하지 않고, 함수를 정의할 때 지정한다
def buggy(arg, result =[]):
    result.append(arg)
    print(result)

buggy('A')
buggy('B')
buggy('Z', [1, 2, 3, 4])
buggy('가')
print(

)
# (5) 위치 인자 모으기(*)
print('(5)-----------------------------------------')

'''
1번째와 2번째는 인자가 반드시 들어가고 3번째는 인자가 들어갈 수도 있고 없으면 0으로 초기화한다
그러나 4번째 인자부터는 정확히 모른다면?
print(func(4, 5))
print(func(4, 5, 6))
print(func(4, 5, 6, 7))
print(func(4, 5, 6, 7, 8, 9))       # i9에 7,8,9가 튜플로 들어간다
'''
# 모든 인자값의 합계를 리턴하는 함수
def func(a,b,c =0,*args):   # * : positional arguments만 받을때
    sum = a+b+c             # args : 일반적으로 사용
    for i in args:
        sum+=i
    return sum

print(func(4, 5))
print(func(4, 5, 6))
print(func(4, 5, 6, 7))
print(func(4, 5, 6, 7, 8, 9))

# (6) 키워드 인자 모으기
# * : positional arguments만 받을때
# ** : keyword arguments만 받을 때
print('(6)-----------------------------------------')
def func(i, j, k=100, *args, **kwargs): # args : 튜플형 , kwargs : 딕셔너리형
        print(i, j, k)
        print(args)
        print(kwargs)

func(10,20)
func(1, 2, 3)
func(1, 2, 3, 4, 5, 6)
func(1, 2, 3, a=10, b=11, c=12)
func(1, 2, 3, 4, a=10, b=11, c=12)





