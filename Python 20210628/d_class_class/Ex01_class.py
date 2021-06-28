"""
     1) 클래스 기초

     ` __init__ 함수 : 객체 초기화 함수( 생성자 역할 )
     ` self : 객체 자신을 가리킨다.
"""
class Sample:
    # 객체 생성과 동시에 데이터 초기화 할 때 반드시 생성자를 만들어야 한다.
    def __init__(self):
        self.age = 24
        self.name = '홍길동'
        print('__init__호출')
    # 소멸자 함수
    def __del__(self):
        self.age = 0
        self.name = '0'
        print('__del__호출')

s =Sample()
print(s.age,s.name)
print(dir(s))
print('-----------------------------Sample-------------------------------')

"""
    2) 
    인스턴스 함수 :  'self'인 인스턴스를 인자로 받고 인스턴스 변수와 같이 하나의 인스턴스에만 한정된 데이터를 생성, 변경, 참조
    클래스   함수 :  'cls'인 클래스를 인자로 받고 모든 인스턴스가 공유하는 클래스 변수와 같은 데이터를 생성, 변경 또는 참조
    static  함수 :  클래스명 접근을 하며 객체끼리의 공유하고자 하는 메소드
    
    - 클래스 함수와 스태틱 함수는 둘 다 클래스명 접근
    - 클래스 함수는 cls를 이용하여 객체를 이용할 수 있다
    
     - 정적 메소드는 모두 @classmethod  를 이용해서 사용하면 될 거 같습니다만 
      상속에서 사용되어 혼동을 초래할 여지가 없거나 조금이라도 더 간략하게 표현하고 싶을 경우 
      @staticmethod 를 사용하는게 더 편해 보인다 [ HAMA 블로거 의견 ]

    - 내의견은 @staticmethod를 다른 언어처럼 만들었지만 불편하여 다시 @classmethod 를 만든건 아닌가?

"""
class Book:

    cnt = 0

    def __init__(self, title):
        self.title = title

    def output(self):
        print('제목: ', self.title)
        self.cnt += 1
        print('총 갯수', self.cnt)

    @staticmethod
    def output3():
        pass
        # print(self.name+" 접근 안됨 ")
        # print( name+" 접근 안됨 ")

    @classmethod
    def output2(cls):
        cls.cnt += 1
        print('총 갯수',cls.cnt)
        # print('제목: ', cls.title)  # 실행시 에러발생

# b1 =Book()
b1 = Book('행복이란..')
b1.output2()

b2 = Book('먹고 산다는것..')
b2.output2()

'''
     3) 클래스 상속
        - 파이션은 method overriding은 있지만 method overloading 개념은 없다
        - 파이션은 다중상속이 가능
        - 부모 클래스가 2개 이상인 경우 먼저 기술한 부모클래스에서 먼저 우선 해당 멤버를 찾음
'''
class Animal:
    def move(self):
        print('동물은 움직인다')

class Wolf(Animal):         # 늑대는 동물을 상속
    def move(self):
        print('늑대는 4발로 달린다')

class Human(Animal):
    def move(self):
        print('인간은 2발로 걷는다')

class WolfHuman(Wolf, Human):
    def move(self):
        super().move()    # 부모 메소드 호출하려면
                           # 부모가 Wolf, Human 2개 있는데 먼저 기술된 Wolf 우선이고 여기에 메소드 없으면 Human
        print('늑대인간은 2발로 빠르게 달린다')

class Dog(Animal):
    def move(self):
        print('개는 4발로 달린다')
        print('그리고 멍멍짖는다')

wh =WolfHuman()
wh.move()
D= Dog()
D.move()

















