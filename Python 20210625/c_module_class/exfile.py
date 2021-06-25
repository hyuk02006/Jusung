"""" [과제] 패키지 만든 후에 하자
    모듈 생성 : mypackage 패키지 안에 exmodule
    인자 2개를 받아서 더한 값을 리턴한다.
    그러나 다른 자료형이면 "자료형이 다르면 계산 할 수 없다"는 문장 출력한다
"""
# exmodule을 사용하는 예
import mypackage.exmodule as ex

print(ex.sum(3, 1))
print(ex.sum(3, 1.5))
ex.sum(3, 'a')