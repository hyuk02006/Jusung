using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;
namespace _20210706.NET
{

    //옵저버 패턴
    class C
    {
        // 고객관리 리스트
        protected ArrayList al = new ArrayList();
        public void add(Animal tiger) { al.Add(tiger); }
        public void remove(int num) { al.RemoveAt(num); }

    }
    // A클래스는 신문사(경향일보)이다.
    class A : C
    {
        string news;
        public void setNews(string news)
        {
            this.news = news;
            notify();
        }
        public void notify()
        {
            foreach (Animal item in al)
            {
                item.update(news);
            }
        }

        public void showInfo()
        {
            foreach (Tiger item in al)
            {
                WriteLine(item.name);
            }

        }
    }

    //중앙일보
    class B : C
    {

        int 온도, 습도;

        public void setNews(int 온도, int 습도)
        {
            this.온도 = 온도;
            this.습도 = 습도;
            notify();
        }
        public void notify()
        {
            //foreach (Animal item in al)
            //{
            //   item.update(news);
            //}
        }

        public void showInfo()
        {
            foreach (Tiger item in al)
            {
                WriteLine(item.name);
            }

        }
    }

    interface Animal
    {
        void update(string name);
        void update(int 온도, int 습도);
    }

    class Tiger : Animal
    {
        public string name;
        public Tiger(string name) { this.name = name; }
        public void update(string news)
        {
            WriteLine(name + ":" + news + "에 대하여 딥러닝을 합니다.");
        }
        public void update(int 온도, int 습도)
        {
                WriteLine("온도:{0} ,습도:{1}",온도,습도);
        }
        // 성격이 다른 코드 10000줄
    }

    class Lion : Animal
    {
        public string name;
        public Lion(string name) { this.name = name; }
        public void update(string news)
        {
            WriteLine(name + ":" + news + "에 대하여 딥러닝을 합니다.");
        }
        public void update(int 온도, int 습도)
        {
            for (int i = 0; i < 2; i++)
            {
                WriteLine($"온도:",온도,"습도:",습도);

            }
        }
        // 성격이 다른 코드 10000줄
    }

    class Program
    {
        static void Main(string[] args)
        {
            A a = new A();
            a.add(new Tiger("호랑이1"));
            a.add(new Lion("사자1"));
            a.setNews("장마");


            B b = new B();
            b.add(new Tiger("호랑이1"));
            b.add(new Lion("사자1"));
            b.setNews(20, 30);
        }
    }
}
