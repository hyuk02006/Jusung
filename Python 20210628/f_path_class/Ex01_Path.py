"""
 - import pathlib 만 선언하면
        Path클래스 사용시 pathlib.Path라고 명시해야 한다. 
"""
from pathlib import Path


# (1) 해당 경로와 하위 목록들 확인

#p = Path('C:\Windows')
#p = Path('C:\Windows2') # 단순출력에서는 에러가 발생하지 않고 하위 디렉토리 찾을 때 에러 발생
#p = Path('.') # 현재 디렉토리
p = Path('..')  # 부모 디렉토리
print(p)
print(p.resolve())  # 절대경로로 출력됨
print('1----------------')

test = []
for x in p.iterdir():       # p 경로의 요소들을 반복
    if x.is_dir():          # 디렉토리인 경우라면
        test.append(x)      # 리스트에 추가
print(test)
print('----------------')

# 위의 코딩과 같은 동작 - comprehension 으로 변경
# 반복하면서 해당하는 요소가 디렉토리인 경우만 필터랑 하여 x에 담기는 것이다
i =[x for x in p.iterdir() if x.is_dir()]
print(i)
print('----------------')

#  = list(p.glob('a_datatype/*.py')) # 만일 현재디렉토리에서만 가져오려면 p.glob('*.py')
                                     # 만일 하위에서 a_datatype 폴더를 찾는다면 p.glob('**/a_datatype/*.py')
j = list(p.glob('**/data/*.*'))
print(j)


