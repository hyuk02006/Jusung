str = 'HELLO'                # 문자열
li = ['a','b','c']        # 리스트
tpl = ('ㄱ','ㄴ','ㄷ')    # 튜플
di = dict(k=5, j=6)       # 딕셔너리

# (1) unpacking : 요소를 분해
c1, c2, c3 =li
print(c1)
print(c2)
print(c3)

for key, value in di.items():
    print(key, ':', value)

# (2) 리스트의요소가 튜플인 경우
# 리스트 : []
# 튜플 : ()
aList = [(1, 2), (3, 4), (5, 6)]
for (first, second) in aList:
    print(first, '+', second, '=', first+second)

# (3) 여러 시퀀스 순회하기 : zip()
# -> 나중에 기억이 안나서 자주 확인하는 함수 중에 하나

# zip() 사용 예
days = ['월', '화', '수']
doit = ['잠자기', '공부', '놀기', '밥먹기']
print(zip(days, doit))
print(list(zip(days, doit)))
print(dict(zip(days, doit)))

for yoil, halil in zip(days, doit): # zip() 가장 짧은 시퀀스가 완료되면 멈춤
    print(yoil, halil)