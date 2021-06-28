import csv


# 1. 리스트의 데이타를 csv 파일로 저장하기
data = [[1, '김', '책임'],[2, '박', '선임'],[3, '주', '연구원']]

with open('./data/imsi.csv','wt', encoding='utf-8') as f:   # w : write, t : text
    cout = csv.writer(f)
    cout.writerows(data)
# [확인] imsi.csv 파일 - 한 줄씩 개행이 더 들어가 있다


# 2. csv 파일을 읽어서 리스트 변수에 저장하기 ( 하도록 )
data = []
with open('./data/imsi.csv', 'rt', encoding='utf-8') as fin:
    cin = csv.reader(fin)
    data = [row for row in cin]
print(data)

# [결과확인] 빈 줄이 빈 리스트로 생성되어 있음
# 빈 리스트 해결 -> data = [row for row in cin if row ]
# 파일에 저장시 반드시 확인