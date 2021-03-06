"""
#----------------------------------------------------------
[튜플 자료형]
    1- 리스트와 유사하지만 튜플은 값을 변경 못한다.
    2- 각 값에 대해 인덱스가 부여
    3- 변경 불가능 (*****)
    4- 소괄호 () 사용
"""

# (1) 튜플 생성
print('------------------- 1. 튜플 생성-----------------')
t =(1, 2, 3)    # t =(1, 2, 3)
print(t)
print(t[0])

t2 =(1, 2, 3)    # t =(1, 2, 3)
print(t2)
print(t[-1])

# (2) 튜플은 요소를 변경하거나 삭제 안됨
# t[1] = 0;  # 블럭이 생기면서 실행 안됨
# del t[1]   # 에러 발생
print('------------------- 2 -----------------')
t3 =()  # t3 = tuple()
# t3[1] =1  튜플은 추가 불가능
# del t3[2] 튜플은 삭제 불가능
# del t2 = 실행가능


# (3) 하나의 요소를 가진 튜플
print('------------------- 3 -----------------')
t4 = (1)     # 튜플이 아니다
print(t4)
print(type(t4))

t5 = (1,)     # 하나의 요소의 튜플일라면 반드시 컴마(,)가 필요
print(t5)
print(type(t5))

# (4) 인덱싱과 연산자
print('------------------- 4 -----------------')
print(t[1])
print(t[1:])
print(t+t2)     # 튜플 합치기
print(t*2)      # 튜플 곱하기
# print(t + t4) t4는 튜플 자료형이 아니다

# (5) 튜플 요소 풀기
print('------------------- 5 -----------------')
t6 = (1, 2, 3)
n1, n2 , n3=t6
print(n1 + n2 + n3)

# (6) 튜플과 리스트 변환
print('------------------- 6 -----------------')
my_list = ['a', 'b', 'c']   # 리스트
my_tuple = ('x', 'y', 'z')   # 튜플
print(tuple(my_list))   # 튜플형식 ('a','b','c')
print(list(my_tuple))   # 리스트 형식['x','y','z']
print(my_list)          # 원본이 변경된 것은 아님
