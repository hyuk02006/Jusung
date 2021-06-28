"""
# Path는 파일 시스템에 접근하기 때문에, 기본적으로 운영체제 상에 조작 대상 파일 경로가 존재해야 합니다.
# PurePath는 순수 경로의 기반 클래스입니다.
# 파일 시스템에 접근하지 않기 때문에, 운영체제 상에 존재하지 않는 파일 경로를 다룰 수도 있습니다.
"""
from pathlib import PurePath
from pathlib import PurePosixPath, PureWindowsPath


# 1 - 존재하지 않는 파일
print(PurePath("babo/myclass/myjob"))   # 실제 경로를 찾지못함

#-------------------------------------------------------------------
# 2. 실제 경로로 아닌 가짜 경로를 관리하는 PurePath를 어디에 사용할까?
# 아마도 경로나 파일명만 조작할 때 사용하지 않을까?
# 해당 경로나 파일명이 현재 컴퓨터가 아니기에 이름만 관리하는 작업이 필요할 듯 싶다
j1 = PurePath('a:/Python36/bin/python') # 실제 존재여부는 관련없이 경로만 확인함
print(j1.parts)
print(j1.drive)     # 해당 드라이브

j1 = PurePath("\\192.0.0.111\Share\mbc")    # UNC경로 : 원격 컴퓨터의 공유폴더에 직접 접근하기 위한 경로
print(j1)
print(j1.parts)

# --------------------------------------------------
# 3. 경로 붙이기 연산
b =  PurePath("mywork")
c  = b / 'python' / 'imsi' / 'imsi'
print(c)

d = b.joinpath('python', 'imsi', 'imsi2')
print(d)

data = ('python', 'imsi', 'imsi3')
e = b.joinpath(*data)
print(e)

# 정규식처럼 - 정규표현식은 아니다. 비슷할 뿐
b = PurePath('Ex02_PurePath.py')
print(b.with_suffix(".py"))
print(b.with_suffix(".exe"))
print(b.match("Ex*.py"))
print(b.match("Ex[0-9]*.py"))
# python glob 을 검색해서 확인


# ---------------------------------------------------------------
# 4.부모경로찾기
j1 = PurePath('C:/Windows/System32/drivers/etc/hosts')
print(j1.parts)
print(j1.parts[3])

print(j1.parents[0])
print(j1.parents[1])
print(j1.parents[2])
# print(j1.as_uri()) # 파일 시스템 경로를 uri 형태로 ( 두번째 경로에서만 실행됨 )