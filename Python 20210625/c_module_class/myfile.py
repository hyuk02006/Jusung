# (1) 모듈 전체를 참조할 때는 import
# import mymodule
# today = mymodule.get_weather()
# print('오늘의 날씨는',today)
# print('오늘은',mymodule.get_date(),'요일입니다')

# (2) 별칭을 부여하여 모듈 임포트
import  mypackage.mymodule as my
today = my.get_weather()
print('오늘의 날씨는',today)
print('오늘은',my.get_date(),'요일입니다')

# (3) 모듈에서 필요한 부분만 임포트하기
from mypackage.mymodule import  get_weather
today =get_weather()
print('오늘의 날씨는',today)
# print('오늘은', mymodule.get_date) 에러