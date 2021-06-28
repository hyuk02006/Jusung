from pathlib import Path

# ------------------------------------------------
# 1. 경로의 상태보기
'''
print(Path.cwd()) # 현재 이 파일의 경로의 위치만
print(Path.home()) # 윈도우의 사용자 홈 디렉토리는 C 드라이버 밑에 사용자 밑에 컴퓨터명 폴더

p1 = Path('Ex03_createPath.py')
print(p1.stat()) # 이 파일의 상태를 보여준다
# os.stat_result(st_mode=33206, st_ino=9007199255105176, st_dev=3561482230, st_nlink=1, st_uid=0, st_gid=0, st_size=366, st_atime=1529113596, st_mtime=1529113596, st_ctime=1529113596)
# st_atime=1529113596 : 1970년 0시 부터 시작한 시간?
'''

# ----------------------------------------------------
# 2. 경로(파일) 생성시간 알아보기
'''
import stat
import datetime

p1 = Path('Ex03_createPath.py')
tm = p1.stat()[stat.ST_CTIME]
print(tm) # 생성시간 - 1970/01/01 이후 초???

z = datetime.datetime.fromtimestamp(tm)
print(z)
'''

# ------------------------------------------------
# 3. 디렉토리 생성
'''
# p1 = Path("imsi")
# p1.mkdir()
# p2 = Path("imsi")
# p2.mkdir()
# 이미 imsi 폴더가 존재하기에 에러 발생
p1 = Path("imsi")
p1.mkdir(exist_ok=True)

# 여러경로를 한번에 하려면
# p2 = Path('temp2/imsi2/test')
# p2.mkdir() # 에러발생

p2 = Path('temp2/imsi2/test') 
p2.mkdir(parents=True) # 한번에 생성된다.
'''

# ------------------------------------------------
# 4. 파일 생성
'''
f = open('imsi/1.txt', 'w')   # 한글 깨짐 확인
f = open('imsi/1.txt', 'w', encoding='utf-8')
f.write('홍길동1')             
f.close()

# 위와동일한 코드 : with를 사용하면 close 필요없음
with open('imsi/1.txt', 'w', encoding='utf-8')  as f:
    f.write('홍길동2')

p1 = Path('imsi/2.txt')
f = p1.open('w', encoding='utf-8')
f.write('홍길동3')
f.close()

# 위와동일한 코드
p1 = Path('imsi/2.txt')
with open(p1, 'w', encoding='utf-8')  as f:
    f.write('홍길동4')

f = Path('imsi/3.txt')
f.write_text('홍길동5', encoding='utf-8')  # open 하지 않고 쉽게 쓴다
# print( f.read_text() )

# f.rename('3-1.txt') # 파일명 변경
# f.rename(Path("imsi/3-1.txt")) # 다른 경로에 파일명 변경

f = Path('imsi/1.txt')
f.replace('imsi/test.py')
# rename과 다른점은?
#   - rename 은 변경된 파일명이 이미 존재하면 에러 발생
#     rename은 여러개의 이름으로 변경 가능
#   - replace 는 변경된 파일명을 덮어버려서 에러 발생 하지 않음
# p1 = Path('imsi/2.txt')
# p1.rename('imsi/test2.py')
'''

# ------------------------------------------------
#  5. 경로 제거
# f = Path('imsi')
# f.rmdir() # 디렉토리 제거
#           # 지우고자 하는 디렉토리 안에 파일이 있으면 지울 수 없다
#           # 파일이 있어도 지우려면 shunil 패키지 이용해야 한다 (Ex04_etc.py)

# f = Path('imsi/test.py')
# f.unlink() # 파일 삭제


# ---------------------------------------------
# 빈 파일 만들거나 기존에 파일의 시간을 변경
# f = Path('imsi/2.txt')
# f.touch()   # 파일의 접근 시간을 지금으로 변경하게 된다. -> 탐색기에서 확인
# 파이썬이 리눅스 기반의 언어로 생겼다는 것을 touch()로도 알 수 있네


