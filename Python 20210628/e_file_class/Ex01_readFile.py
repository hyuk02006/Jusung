"""
@ 파일 읽고 쓰기
    - 파일을 읽고 쓰기 전에 파일을 열어야 한다
    - fileObj = open ( filename, mode )
            mode 첫번째 글자 - 작업 표시
            r(read)  : 파일 읽기
            w(write) : 파일 쓰기 ( 파일이 없으면 생성하고 파일이 있으면 덮어쓴다 )
            x(write) : 파일 쓰기 ( 파일이 없을 때만 생성하고 쓴다 )
            a(append) : 파일 추가 ( 파일이 있으면 파일의 끝에서부터 추가하여 쓴다 )

            mode 두번째 글자 - 파일 타입
            t : 텍스트(text) 타입 ( 기본값 )
            b : 이진(binary) 타입
            두번째 글자가 없으면 텍스트 타입이다.

            encoding='utf-8' : 한글

    - 파일을 열고 사용 후에는 반드시 닫아야 한다
    - 자동으로 닫아주는 with 형식을 많이 사용 ***
"""

# [2] with 이용하여 자동으로 파일 닫기

try:
    f = open('./data/data.txt', 'r', encoding='utf-8')
except FileNotFoundError as e:
    print(e)
    print('파일을 찾을 수 없습니다')
else:  # 예외가 발생하지 않을 경우
    while True:
        line = f.readline()  # 한 줄씩 읽기
        if not line: break
        print(line)  # 먼저확인후 end 속성 추가
        # print(line, end='')
    f.close()
finally:
    print('\n종료')


# d:/Myclass/Python/aBasic/e_file/data/data.txt
# 파일을 생성하고 다시 확인

with open('./data/data.txt', 'r', encoding='utf-8') as f:
    while True:
        line = f.readline()  # 한 줄씩 읽기
        if not line: break
        print(line, end='')
print('\n종료2')

filename ='./data/data.txt'

try:
    with open(filename,'r',encoding='utf-8') as f:
        contents =f.read()
        words =contents.split()
        num = len(words)
except FileNotFoundError as e:
    print(e)
    print('파일을 찾을 수 없습니다')
print('파일명:'+filename+',총 단어수:'+str(num))
print('\n종료3')