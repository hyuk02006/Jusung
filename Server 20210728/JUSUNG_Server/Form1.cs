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
using System.Windows.Forms;

namespace JUSUNG_Server
{
    public partial class Form1 : Form
    {
        private string ip = "192.168.1.28";

        //port
        private int port = 9000;
        private int port2 = 9001;
        private int port3 = 9002;
        private int port4 = 9003;

        //listen
        private Thread listenThreadTMC;
        private Thread listenThreadEFEM;
        private Thread listenThreadPM1;
        private Thread listenThreadPM2;

        //receive
        private Thread receiveThreadTMC;
        private Thread receiveThreadEFEM;
        private Thread receiveThreadPM1;
        private Thread receiveThreadPM2;

        //socket
        public Socket clientSocket;
        public Socket clientSocket1;
        public Socket clientSocket2;
        public Socket clientSocket3;


        public Form1()
        {
            InitializeComponent();
        }

        delegate void LogSafety(string msg);


        private void Log(string msg)
        {
            //Cross Thread
            if (listBox1.InvokeRequired)
                listBox1.Invoke(new LogSafety(Log), msg);
            else
                listBox1.Items.Add(string.Format("[{0}]{1}", DateTime.Now.ToString(), msg));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void ListenTMC()
        {
            // IP 주소 문자열을 IPAddress 인스턴스로 변환
            IPAddress ipaddress = IPAddress.Parse(ip);

            // 네트워크 끝점(종단점)을 IP주소 및 포트번호로 나타냄
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);

            // 소켓 생성
            try
            {
                Socket listenSocket = new Socket(
                    // Socket 클래스의 인스턴스가 사용할 수 있는 주소지정 체계 지정
                    AddressFamily.InterNetwork,
                    SocketType.Stream,              // 소켓 유형 지정
                    ProtocolType.Tcp                // 프로토콜 지정
                    );

                // Socket을 endPoint와 연결(IP주소, 포트번호 할당)
                listenSocket.Bind(endPoint);

                // Socket을 수신 상태로 둔다.(연결 가능한 상태)
                // 클라이언트에 의한 연결 요청이 수신될때까지 기다린다.
                listenSocket.Listen(10);

                Log("TMC클라이언트 요청 대기중...");

                // 연결 요청에 대한 수락

                while (true)
                {
                    clientSocket = listenSocket.Accept(); //Accept를 반복해야지 1:N 통신가능


                    Log("TMC클라이언트 접속됨 ");

                    // Receive 스레드 호출
                    receiveThreadTMC = new Thread(new ThreadStart(ReceiveTM));
                    receiveThreadTMC.IsBackground = true;
                    receiveThreadTMC.Start();      // Receive() 호출              
                }
            }
            catch (SocketException e)
            {

            }
        }

        private void ListenEFEM()
        {
            // IP 주소 문자열을 IPAddress 인스턴스로 변환
            IPAddress ipaddress = IPAddress.Parse(ip);

            // 네트워크 끝점(종단점)을 IP주소 및 포트번호로 나타냄
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port2);

            // 소켓 생성
            try
            {
                Socket listenSocket1 = new Socket(
                    // Socket 클래스의 인스턴스가 사용할 수 있는 주소지정 체계 지정
                    AddressFamily.InterNetwork,
                    SocketType.Stream,              // 소켓 유형 지정
                    ProtocolType.Tcp                // 프로토콜 지정
                    );

                // Socket을 endPoint와 연결(IP주소, 포트번호 할당)
                listenSocket1.Bind(endPoint);

                // Socket을 수신 상태로 둔다.(연결 가능한 상태)
                // 클라이언트에 의한 연결 요청이 수신될때까지 기다린다.
                listenSocket1.Listen(5);

                Log("EFEM클라이언트 요청 대기중...");

                // 연결 요청에 대한 수락
                while (true)
                {
                    clientSocket1 = listenSocket1.Accept(); //Accept를 반복해야지 1:N 통신가능

                    Log("EFEM클라이언트 접속됨 ");

                    // Receive 스레드 호출
                    receiveThreadEFEM = new Thread(new ThreadStart(ReceiveEFEM));
                    receiveThreadEFEM.IsBackground = true;
                    receiveThreadEFEM.Start();      // Receive() 호출
                }
            }
            catch (SocketException e)
            {

            }
        }

        private void ListenPM1()
        {
            // IP 주소 문자열을 IPAddress 인스턴스로 변환
            IPAddress ipaddress = IPAddress.Parse(ip);

            // 네트워크 끝점(종단점)을 IP주소 및 포트번호로 나타냄
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port3);

            Socket listenSocket2 = new Socket(
             // Socket 클래스의 인스턴스가 사용할 수 있는 주소지정 체계 지정
             AddressFamily.InterNetwork,
             SocketType.Stream,              // 소켓 유형 지정
             ProtocolType.Tcp                // 프로토콜 지정
             );
            // 소켓 생성
            try
            {             
                // Socket을 endPoint와 연결(IP주소, 포트번호 할당)
                listenSocket2.Bind(endPoint);

                // Socket을 수신 상태로 둔다.(연결 가능한 상태)
                // 클라이언트에 의한 연결 요청이 수신될때까지 기다린다.
                listenSocket2.Listen(5);

                Log("PM1클라이언트 요청 대기중...");

                // 연결 요청에 대한 수락
                while (true)
                {
                    clientSocket2 = listenSocket2.Accept(); //Accept를 반복해야지 1:N 통신가능

                    Log("PM1클라이언트 접속됨 ");

                    // Receive 스레드 호출
                    receiveThreadPM1 = new Thread(new ThreadStart(ReceivePM1));
                    receiveThreadPM1.IsBackground = true;
                    receiveThreadPM1.Start();      // Receive() 호출
                }
            }
            catch (SocketException e)
            {

            }
        }

        private void ListenPM2()
        {
            // IP 주소 문자열을 IPAddress 인스턴스로 변환
            IPAddress ipaddress = IPAddress.Parse(ip);

            // 네트워크 끝점(종단점)을 IP주소 및 포트번호로 나타냄
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port4);

            // 소켓 생성
            try
            {
                Socket listenSocket3 = new Socket(
                    // Socket 클래스의 인스턴스가 사용할 수 있는 주소지정 체계 지정
                    AddressFamily.InterNetwork,
                    SocketType.Stream,              // 소켓 유형 지정
                    ProtocolType.Tcp                // 프로토콜 지정
                    );

                // Socket을 endPoint와 연결(IP주소, 포트번호 할당)
                listenSocket3.Bind(endPoint);

                // Socket을 수신 상태로 둔다.(연결 가능한 상태)
                // 클라이언트에 의한 연결 요청이 수신될때까지 기다린다.
                listenSocket3.Listen(5);

                Log("PM2클라이언트 요청 대기중...");

                // 연결 요청에 대한 수락
                while (true)
                {
                    clientSocket3 = listenSocket3.Accept(); //Accept를 반복해야지 1:N 통신가능

                    Log("PM2클라이언트 접속됨");

                    // Receive 스레드 호출
                    receiveThreadPM2 = new Thread(new ThreadStart(ReceivePM2));
                    receiveThreadPM2.IsBackground = true;
                    receiveThreadPM2.Start();      // Receive() 호출
                }
            }
            catch (SocketException e)
            {

            }
        }

        // 수신 처리...
        private void ReceiveTM()
        {
            try
            {
                while (true)
                {
                    // 연결된 클라이언트가 보낸 데이터 수신
                    byte[] receiveBuffer = new byte[512];

                    int length = clientSocket.Receive(receiveBuffer,
                        receiveBuffer.Length, SocketFlags.None);

                    // 디코딩
                    string msg = Encoding.UTF8.GetString(receiveBuffer);

                    //
                    Showmsg("상대: " + msg);
                    Log("메시지 수신함");
                }
            }
            catch (SocketException e)
            {

            }
            clientSocket.Close();
        }

        // 수신 처리...
        private void ReceiveEFEM()
        {
            try
            {
                while (true)
                {
                    // 연결된 클라이언트가 보낸 데이터 수신
                    byte[] receiveBuffer = new byte[512];

                    int length = clientSocket1.Receive(receiveBuffer,
                        receiveBuffer.Length, SocketFlags.None);

                    // 디코딩
                    string msg = Encoding.UTF8.GetString(receiveBuffer);

                    //                   
                    ShowmsgEFEM(": " + msg);
                    Log("메시지 수신함");
                }
            }
            catch (SocketException e)
            {
            }
            clientSocket1.Close();
        }

        // 수신 처리...
        private void ReceivePM1()
        {
            try
            {
                while (true)
                {
                    // 연결된 클라이언트가 보낸 데이터 수신
                    byte[] receiveBuffer = new byte[512];

                    int length = clientSocket2.Receive(receiveBuffer,
                        receiveBuffer.Length, SocketFlags.None);

                    // 디코딩
                    string msg = Encoding.UTF8.GetString(receiveBuffer);

                    //                   
                    ShowmsgPM1(": " + msg);
                    Log("메시지 수신함");
                }
            }
            catch (SocketException e)
            {

            }
            clientSocket2.Close();
        }

        // 수신 처리...
        private void ReceivePM2()
        {

            try
            {
                while (true)
                {
                    // 연결된 클라이언트가 보낸 데이터 수신
                    byte[] receiveBuffer = new byte[512];

                    int length = clientSocket3.Receive(receiveBuffer,
                        receiveBuffer.Length, SocketFlags.None);

                    // 디코딩
                    string msg = Encoding.UTF8.GetString(receiveBuffer);

                    //                   
                    ShowmsgPM2(": " + msg);
                    Log("메시지 수신함");
                }
            }
            catch (SocketException e)
            {

            }
            clientSocket3.Close();
        }

        delegate void Showmessage(string msg);

        private void Showmsg(string msg)
        {

            IPEndPoint ip_point = (IPEndPoint)clientSocket.RemoteEndPoint;
            string ip = ip_point.Address.ToString();

            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke(new Showmessage(Showmsg), msg);
            }
            else
            {
                richTextBox1.AppendText(string.Format("{0}:{1}", ip, port));
                richTextBox1.AppendText(msg);
                richTextBox1.AppendText("\r\n");
            }
            // 입력된 텍스트에 맞게 스크롤을 내려준다.
            this.Activate();


            // 캐럿(커서)를 텍스트박스의 끝으로 내려준다.
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();   // 스크럴을 캐럿(커서)위치에 맞춰준다.
        }

        private void ShowmsgEFEM(string msg)
        {
            // richTextBOX에서 개행이 정상적으로 작용되지 않으면
            // 아래처럼 따로따로          
            IPEndPoint ip_point = (IPEndPoint)clientSocket1.RemoteEndPoint;
            string ip = ip_point.Address.ToString();

            if (richTextBox2.InvokeRequired)
            {
                richTextBox2.Invoke(new Showmessage(ShowmsgEFEM), msg);
            }
            else
            {
                richTextBox2.AppendText(string.Format("{0}:{1}", ip, port2));
                richTextBox2.AppendText(msg);
                richTextBox2.AppendText("\r\n");
            }

            // 입력된 텍스트에 맞게 스크롤을 내려준다.
            this.Activate();

            // 캐럿(커서)를 텍스트박스의 끝으로 내려준다.
            richTextBox2.SelectionStart = richTextBox2.Text.Length;
            richTextBox2.ScrollToCaret();   // 스크럴을 캐럿(커서)위치에 맞춰준다.
        }


        private void ShowmsgPM1(string msg)
        {
            // richTextBOX에서 개행이 정상적으로 작용되지 않으면
            // 아래처럼 따로따로          
            IPEndPoint ip_point = (IPEndPoint)clientSocket2.RemoteEndPoint;
            string ip = ip_point.Address.ToString();

            if (richTextBox3.InvokeRequired)
            {
                richTextBox3.Invoke(new Showmessage(ShowmsgPM1), msg);
            }
            else
            {
                richTextBox3.AppendText(string.Format("{0}:{1}", ip, port3));
                richTextBox3.AppendText(msg);
                richTextBox3.AppendText("\r\n");
            }


            // 입력된 텍스트에 맞게 스크롤을 내려준다.
            this.Activate();

            // 캐럿(커서)를 텍스트박스의 끝으로 내려준다.
            richTextBox3.SelectionStart = richTextBox3.Text.Length;
            richTextBox3.ScrollToCaret();   // 스크럴을 캐럿(커서)위치에 맞춰준다.
        }

        private void ShowmsgPM2(string msg)
        {
            // richTextBOX에서 개행이 정상적으로 작용되지 않으면
            // 아래처럼 따로따로          
            IPEndPoint ip_point = (IPEndPoint)clientSocket3.RemoteEndPoint;
            string ip = ip_point.Address.ToString();

            if (richTextBox4.InvokeRequired)
            {
                richTextBox4.Invoke(new Showmessage(ShowmsgPM2), msg);
            }
            else
            {
                richTextBox4.AppendText(string.Format("{0}:{1}", ip, port4));
                richTextBox4.AppendText(msg);
                richTextBox4.AppendText("\r\n");
            }

            // 입력된 텍스트에 맞게 스크롤을 내려준다.
            this.Activate();

            // 캐럿(커서)를 텍스트박스의 끝으로 내려준다.
            richTextBox4.SelectionStart = richTextBox4.Text.Length;
            richTextBox4.ScrollToCaret();   // 스크럴을 캐럿(커서)위치에 맞춰준다.
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            if (Button1.Text == "시작")
            {
                Button1.Text = "멈춤";
                Log("서버 시작됨");

                // Listen 쓰레드 처리
                listenThreadTMC = new Thread(new ThreadStart(ListenTMC));     // 실행되는 메소드
                listenThreadTMC.IsBackground = true;   // 스레드가 배경 스레드인지 나타내는 함수

                listenThreadEFEM = new Thread(new ThreadStart(ListenEFEM));
                listenThreadEFEM.IsBackground = true;

                listenThreadPM1 = new Thread(new ThreadStart(ListenPM1));
                listenThreadPM1.IsBackground = true;

                listenThreadPM2 = new Thread(new ThreadStart(ListenPM2));
                listenThreadPM2.IsBackground = true;

                // 운영체제에서 현재 인스턴스의 상태를 Run으로 만듬(시작)
                listenThreadTMC.Start();
                listenThreadEFEM.Start();
                listenThreadPM1.Start();
                listenThreadPM2.Start();
            }
            else
            {
                Button1.Text = "시작";
                Log("서버 멈춤");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] sendBuffer = Encoding.UTF8.GetBytes(TextBox1.Text.Trim());
            clientSocket.Send(sendBuffer);
            Log("메시지 전송됨");
            Showmsg("나]" + TextBox1.Text);
            TextBox1.Text = ""; // 초기화
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] sendBuffer = Encoding.UTF8.GetBytes(textBox2.Text.Trim());
            clientSocket1.Send(sendBuffer);
            Log("메시지 전송됨");
            ShowmsgEFEM("나]" + textBox2.Text);
            textBox2.Text = ""; // 초기화
        }

        private void button4_Click(object sender, EventArgs e)
        {
            byte[] sendBuffer = Encoding.UTF8.GetBytes(textBox3.Text.Trim());
            clientSocket2.Send(sendBuffer);
            Log("메시지 전송됨");
            ShowmsgPM1("나]" + textBox3.Text);
            textBox3.Text = ""; // 초기화
        }

        private void button5_Click(object sender, EventArgs e)
        {
            byte[] sendBuffer = Encoding.UTF8.GetBytes(textBox4.Text.Trim());
            clientSocket3.Send(sendBuffer);
            Log("메시지 전송됨");
            ShowmsgPM2("나]" + textBox3.Text);
            textBox4.Text = ""; // 초기화
        }
    }
}