using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace _20210729.NET
{
    class MyList<T>
    {
        private T[] arr;
        public MyList()
        {
            arr = new T[3];
        }

        public T this[int index]
        {
            get 
            {
                WriteLine("get 사용됨");
                return arr[index];
            } 
            set 
            {
                WriteLine("set 사용됨");
                arr[index] = value;
            }
        }

        //Property
        public int Length
        {
            get 
            {
                return arr.Length;
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            MyList<int> mm = new MyList<int>();
            //mm[0] = 10;
            //WriteLine(mm[0]);
            MyList<string> mm2 = new MyList<string>();
            mm2[0] = "호랑이1";
            mm2[1] = "호랑이2";
            mm2[2] = "호랑이3";

            for (int i = 0; i < mm2.Length; i++)
            {
                WriteLine(mm2[i]);
            }
        }
    }
}
