"""
import datetime
today = datetime.date.today();
print('today is ', today)
"""
from datetime import date, datetime, timedelta

# 날짜 구하기
today = date.today()
print('오늘 :', today)

# 년 월 일 출력
print(today.year, '년', today.month, '일', today.day,'일' )

# 현재의 날짜와 시간구하기(datetime)
current_time =datetime.today()
print(current_time)

# 날짜 계산(timedelta)
today = date.today()
print("어제:",today + timedelta(days=-1))
print("일주일전:",today + timedelta(weeks=-1))
print("10일후:",today + timedelta(days=10))
print()

# 날짜출력형식(strftime 함수이용)
today =date.today()
print(today.strftime('%Y %m %d %H'))
print()

# 문자열을 날짜 형식으로 변환(strptime 함수이용)
str = '2021-06-24 16:14:30'
mydate = datetime.strptime(str,'%Y-%m-%d %H:%M:%S')
print(mydate)
print(type(mydate))