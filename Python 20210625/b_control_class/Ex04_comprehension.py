"""
    @ 컴프리핸션 (comprehension)
    ` 하나 이상의 이터레이터로부터 파이썬 자료구조를 만드는 컴팩트한 방법
    ` 비교적 간단한 구문으로 반복문과 조건 테스트를 결합

    * 리스트 컨프리핸션
        [ 표현식 for 항목 in 순회가능객체 ]
        [ 표현식 for 항목 in 순회가능객체 if 조건 ]

    * 딕셔러리 컨프리핸션
        { 키_표현식: 값_표현식 for 표현식 in 순회가능객체 }

    * 셋 컨프리핸션
        { 표현식 for 표현식 in 순회가능객체 }

"""

'''
# 컨프리핸션 사용하지 않은 리스트 생성
alist = []
alist.append(1)
alist.append(2)
alist.append(3)
alist.append(4)
alist.append(5)
alist.append(6)
print(alist)

alist = []
for n in range(1,6):
    alist.append(n)
print(alist)

alist = list(range(1,6))
print(alist)
'''

#------------------------------------------------
# 리스트 컨프리핸션
blist = [n for n in range(1, 6)]
print(blist)

blist = [n-1 for n in range(1, 6)]
print(blist)

blist = [n*2 for n in range(1, 6)]
print(blist)

clist = [n for n in range(1, 6) if n % 2 == 1]
print(clist)

rows = range(1, 4)
cols = range(1, 3)
dlist = [(r, c) for r in rows  for c in cols]
print(dlist)    # 튜플 리스트

for r2, c2 in dlist: # 튜플 요소를 순회하여 출력
    print(r2, c2, end=' ')
print()

#-------------------------------------------
# 딕셔러니 컨프리핸션
a = {x: x**2 for x in (2, 3, 4)}
print(a)

# word에서 글자 하나씩을 순회하여 letter에 담고
# 키: 그 글자(letter) / 값 : word에 들어있는 그 글자의 수
word = 'LOVE LOL'
wcnt = {letter: word.count(letter) for letter in word}
print(wcnt)

# 파이써닉한 방식
wcnt = {letter: word.count(letter) for letter in set(word)}
print(wcnt)


#------------------------------------------------
# 셋 컨프리핸션
# 셋 : {a, b, c}

aset = {n for n in range(1, 6) if n % 2 == 1}
print(aset)

data = [1, 2, 3, 1, 4, 5, 2]
alist = [n for n in data]
print(alist)

blist = {n for n in data}
print(blist)
print()

# -------------------------------------------------
# [참고] 제너레이터 컨프리핸션
# ( ) 를 사용하면 튜플이라 생각하지만 튜플은 컨프리핸션이 없다.
data = [1, 2, 3, 1, 4, 5, 2]
glist = (n for n in data if n % 2 == 1)
print(glist)
print(type(glist))
print(list(glist))

print(list(glist))  # 한 번 순회하려고 리스트 구조로 결과를 확인 -> 내용은 없어짐(제너레이터는 한 번만 실행)
                    # 즉석에서 그 값을 생성하고 이터레이터를 통해 한 번에 값을 하나씩만 처리하고 저장하지 않음


