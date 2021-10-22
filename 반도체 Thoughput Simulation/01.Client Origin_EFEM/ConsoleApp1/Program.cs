

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static System.Console;
namespace ConsoleApp1
{
    class Program
    {
        public Thread vacThread; // ATM용 스레드

        static public Thread LLThread1;
        static public Thread LLThread2;
        static public Thread LLThread3;
        static public Thread LLThread4;
        public static void run1()
        {
            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(100);
                WriteLine("run1");
            }
        }
        private static void run2()
        {
            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(100);
                WriteLine("run2");
            }
        }
        private static void run3()
        {

            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(100);
                WriteLine("run3");
            }
        }
        private static void run4()
        {
            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(100);
                WriteLine("run4");
            }
        }
        public static void Main(string[] args)

        {

            Thread thread1 = new Thread(() => Run(1));
            Thread thread2 = new Thread(() => Run(2));
            Thread thread3 = new Thread(() => Run(3));
            Thread thread4 = new Thread(() => Run(4));

            thread1.Start();
            thread2.Start();
            thread3.Start();
            thread4.Start();

        }



        public static void Run(int idx)

        {

            Console.WriteLine(string.Format("Run {0} Start", idx));

            for (int i = 0; i < 100; i++)

            {

                Console.WriteLine(string.Format("Run {0} :: {1}", idx, i));

            }

            Console.WriteLine(string.Format("Run {0} End", idx));

        }
       
    }
}
