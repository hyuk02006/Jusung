"""
경로 이동은  Path 모듈로 안되어 os 모듈이 필요하다
"""

from pathlib import Path
import os

# print(Path.cwd())
# os.chdir('..')    # 작업경로 변경 - 당연히 존재하지 않으면 에러 발생
# print(Path.cwd())

# 리눅스에서 주로 사용할 듯
print(os.environ["HOMEPATH"])  # 파이썬 홈경로는 윈도우에서 사용자 밑에 컴퓨터명 폴더이다.
#print(os.path.expandvars("%HOMEPATH%")) # 환경변수를 사용할 때
p1 = Path("kbx/mbc/sbs")
p2 = Path( os.environ["HOMEPATH"], p1 )
p2 = Path( os.path.expandvars("%HOMEPATH%"), p1 ) # 위와 동일
print(p2)


# -----------------------------------------------------
# shutil 패키지
import shutil

# shutil.rmtree('imsi')  # 디텍토리를 지운다 (안에 파일이 있어도 삭제된다)
# Path 모듈의 rmdir() 는 디렉토리 안에 내용이 있으면 삭제가 안되었다

#shutil.copy('Ex00.txt', Path('imsi/2copy.txt'))  # imsi 폴더는 존재해야 함

# 디렉토리 간의 복사
#shutil.copytree('imsi','../copytemp')