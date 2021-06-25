
# ------------------------------------------------------
"""
   (2) for문
        for <타켓변수> in 집합객체 :
            문장들
        else:
            문장들

        ` 반복문 뒤에 else는 반복하는 조건에 만족하지않으면 실행

   (3) while 문
        while 조건문 :
            문장들
        else :
            문장들

"""
a = 112                  # 숫자형
b = ['1','2','3']       # 리스트
c = '987'                # 문자열
d = tuple(b)             # 튜플
e = dict(k=5, j=6)       # 딕셔너리

# for entry in a: # a는 반복이 안되지만 b부터 e까지는 반복된다.
#    print(entry)

for entry in b:
    print(entry, end=' ')
print()
for entry in c:
    print(entry, end=' ')
print()
for entry in d:
    print(entry, end=' ')
print()
for entry in e.items():
    print(entry, end=' ')
print()

#---------------------------------------------
sum = 0
for i in range(1, 11):
    sum += i
    print(i, end='+')
print('=', sum)

sum = 0
for i in range(1, 11):
    if i % 2:
        sum += i
        print(i, end='')
print('=', sum)

"""
[과제] 2단부터 9단까지 이중 반복문으로 출력
"""
for i in range(2, 10):
    print('************', i, '단', '****************')
    for j in range(1, 10):
        print(i, '*', j, '=', i*j)


# =========================================================
# while

# a = 1
# while True:  # 무한루프
#    if a == 1:
#        print(a)
#        break

a = ['z', 'y', 'x']
while a:    # 조건을 거짓으로 만들면 반복이 멈추도록 한다
    data =a.pop()
    # if data == 'y': break
else:   #break 문에 의해 중단되지 않는 경우
     print("end")
