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
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using static System.Console;
using System.Drawing.Drawing2D;
using OpenCvSharp;
using System.IO;
using System.Runtime.InteropServices;
using System.IO.Compression;
using System.Data.OracleClient;
using MetroFramework;

namespace JUSUNG_Server
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {

        #region IP,PORT..
        private const string ip = "192.168.1.6";

        //port
        private int port = 9000;
        private int port2 = 9001;
        private int port3 = 9002;
        private int port4 = 9003;
        private int port5 = 9004;

        //listen
        private Thread listenThreadTMC;
        private Thread listenThreadEFEM;
        private Thread listenThreadPM0;
        private Thread listenThreadPM1;
        private Thread listenThreadPM2;

        //receive
        private Thread receiveThreadTMC;
        private Thread receiveThreadEFEM;
        private Thread receiveThreadPM0;
        private Thread receiveThreadPM1;
        private Thread receiveThreadPM2;

        private Thread TimerThread;
        //socket
        public Socket clientSocketTMC;
        public Socket clientSocketEFEM;
        public Socket clientSocketPM0;
        public Socket clientSocketPM1;
        public Socket clientSocketPM2;

        #endregion

        #region  Form

        Mat src, alignersrc, src1, VACsrc;
        Mat[] LLsrc = new Mat[4];
        Mat[] LPMsrc = new Mat[2];
        Mat[] PMsrc = new Mat[3];
        Mat LPMcomSrc;
        string logPath;
        string fasPath;
        List<string> jsDATE = new List<string>();
        List<string> jsMODULE = new List<string>();
        List<string> jsISSUE = new List<string>();
        List<string> jsMSG = new List<string>();
        
        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        public Form1()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }

        public Form1(Socket csEFEM , Socket csTM , Socket csPM0 , Socket csPM1 , Socket csPM2)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            StringBuilder CTCversion = new StringBuilder();
            StringBuilder iniPath = new StringBuilder();
            
            GetPrivateProfileString("PATH", "path", "(NONE)", iniPath, 32, @"..\Debug\Log\0.CTC\Path\Path.ini");
            GetPrivateProfileString("VERSION", "CTCver", "(NONE)", CTCversion, 32, @"..\Debug\Log\0.CTC\Path\Path.ini");
            this.Text = "Ver." + CTCversion.ToString();
            logPath = iniPath.ToString() + "\\CTC_LOG.txt";
            fasPath = iniPath.ToString() + "\\CTC_FAS.txt";

            WriteLine(fasPath);
            src = new Mat(@"..\Debug\robot.png", ImreadModes.Unchanged);

            pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(src);
            alignersrc = new Mat(@"..\Debug\Aligner.png", ImreadModes.Unchanged);
            for (int i = 0; i < 4; i++)
                LLsrc[i] = new Mat(@"..\Debug\LL.png", ImreadModes.Unchanged);
            pictureBox2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(alignersrc);
            pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);
            pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
            pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
            pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
            VACsrc = new Mat(@"..\Debug\Vacrobot1.png", ImreadModes.Unchanged);
            pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

            for (int i = 0; i < 2; i++)
                LPMsrc[i] = new Mat(@"..\Debug\LPMimage\LPM25.png", ImreadModes.Unchanged);
            pictureBoxLPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LPMsrc[0]);
            pictureBoxLPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LPMsrc[1]);
            LPMcomSrc = new Mat(@"..\Debug\LPMimage\LPM0.png", ImreadModes.Unchanged);
            pictureBoxLPM3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LPMcomSrc);

            PMsrc[0] = new Mat(@"..\Debug\PM10.png", ImreadModes.Unchanged);
            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
            PMsrc[1] = new Mat(@"..\Debug\PM20.png", ImreadModes.Unchanged);
            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
            PMsrc[2] = new Mat(@"..\Debug\PM30.png", ImreadModes.Unchanged);
            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);

            File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "SYSTEM Load\n");
            File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "Path.ini Load\n");
        }
        public class class_DoubleBufferPanel : Panel
        {
            public class_DoubleBufferPanel()
            {
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
                this.UpdateStyles();
            }
        }

        #endregion

        #region 서버
        public NewObject Obj = new NewObject();
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
                    clientSocketTMC = listenSocket.Accept(); //Accept를 반복해야지 1:N 통신가능

                    Log("TMC클라이언트 접속됨 ");
                    File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "TMC Client Connect\n");
                    // Receive 스레드 호출
                    receiveThreadTMC = new Thread(new ThreadStart(ReceiveTMC));
                    receiveThreadTMC.IsBackground = true;
                    receiveThreadTMC.Start();      // Receive() 호출              

                }
            }
            catch (SocketException e)
            {
                Log(e.Message);
                File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + " <TMC ERROR> " + e.Message +"\n");
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
                    clientSocketEFEM = listenSocket1.Accept(); //Accept를 반복해야지 1:N 통신가능

                    Log("EFEM클라이언트 접속됨 ");
                    File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "EFEM Client Connect\n");
                    //button3.Enabled = true;
                    // Receive 스레드 호출
                    receiveThreadEFEM = new Thread(new ThreadStart(ReceiveEFEM));
                    receiveThreadEFEM.IsBackground = true;
                    receiveThreadEFEM.Start();      // Receive() 호출
                }
            }
            catch (SocketException e)
            {
                Log(e.Message);
                File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + " <EFEM ERROR> " + e.Message + "\n");
            }
        }

        private void ListenPM0()
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
                    clientSocketPM0 = listenSocket2.Accept(); //Accept를 반복해야지 1:N 통신가능

                    Log("PM1클라이언트 접속됨 ");
                    File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "PM0 Client Connect\n");
                    // Receive 스레드 호출
                    receiveThreadPM0 = new Thread(new ThreadStart(ReceivePM0));
                    receiveThreadPM0.IsBackground = true;
                    receiveThreadPM0.Start();      // Receive() 호출
                    break;
                }
            }
            catch (SocketException e)
            {
                Log(e.Message);
                File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + " <PM0 ERROR> " + e.Message + "\n");
            }
        }

        private void ListenPM1()
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
                    clientSocketPM1 = listenSocket3.Accept(); //Accept를 반복해야지 1:N 통신가능

                    Log("PM2클라이언트 접속됨");
                    File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "PM1 Client Connect\n");
                    // Receive 스레드 호출
                    receiveThreadPM1 = new Thread(new ThreadStart(ReceivePM1));
                    receiveThreadPM1.IsBackground = true;
                    receiveThreadPM1.Start();      // Receive() 호출
                }
            }
            catch (SocketException e)
            {
                Log(e.Message);
                File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + " <PM1 ERROR> " + e.Message + "\n");
            }
        }

        private void ListenPM2()
        {
            // IP 주소 문자열을 IPAddress 인스턴스로 변환
            IPAddress ipaddress = IPAddress.Parse(ip);

            // 네트워크 끝점(종단점)을 IP주소 및 포트번호로 나타냄
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port5);

            // 소켓 생성
            try
            {
                Socket listenSocket4 = new Socket(
                    // Socket 클래스의 인스턴스가 사용할 수 있는 주소지정 체계 지정
                    AddressFamily.InterNetwork,
                    SocketType.Stream,              // 소켓 유형 지정
                    ProtocolType.Tcp                // 프로토콜 지정
                    );

                // Socket을 endPoint와 연결(IP주소, 포트번호 할당)
                listenSocket4.Bind(endPoint);

                // Socket을 수신 상태로 둔다.(연결 가능한 상태)
                // 클라이언트에 의한 연결 요청이 수신될때까지 기다린다.
                listenSocket4.Listen(5);

                Log("PM3클라이언트 요청 대기중...");

                // 연결 요청에 대한 수락
                while (true)
                {
                    clientSocketPM2 = listenSocket4.Accept(); //Accept를 반복해야지 1:N 통신가능

                    Log("PM3클라이언트 접속됨");
                    File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "PM2 Client Connect\n");
                    // Receive 스레드 호출
                    receiveThreadPM2 = new Thread(new ThreadStart(ReceivePM2));
                    receiveThreadPM2.IsBackground = true;
                    receiveThreadPM2.Start();      // Receive() 호출
                }
            }
            catch (SocketException e)
            {
                Log(e.Message);
                File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + e.Message + "\n");
            }
        }
        // 수신 처리...
        private void ReceiveTMC()
        {
            try
            {
                while (true)
                {
                    // 연결된 클라이언트가 보낸 데이터 수신
                    byte[] receiveBuffer = new byte[50];

                    clientSocketTMC.Receive(receiveBuffer, 0, 4, SocketFlags.None);
                    int size = BitConverter.ToInt32(receiveBuffer, 0);

                    byte[] date = new byte[size];
                    clientSocketTMC.Receive(date, 0, size, SocketFlags.None);
                    string msg = Encoding.UTF8.GetString(date).Trim('\0');
                    if (richTextBox1.InvokeRequired)
                    { richTextBox1.Invoke(new Showmsgsafety(Showmsg), clientSocketTMC, richTextBox1, msg); }

                    DecodingTM(msg);
                    Log("TM으로부터 메시지 수신함");
                }
            }
            catch (SocketException e)
            {
                WriteLine(e.Message);
                File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + e.Message + "\n");
            }
        }

        private void ReceiveEFEM()
        {
            try
            {
                while (true)
                {
                    // 연결된 클라이언트가 보낸 데이터 수신
                    byte[] receiveBuffer = new byte[40];

                    clientSocketEFEM.Receive(receiveBuffer, 0, 4, SocketFlags.None);
                    int size = BitConverter.ToInt32(receiveBuffer, 0);

                    byte[] date = new byte[size];
                    clientSocketEFEM.Receive(date, 0, size, SocketFlags.None);
                    string msg = Encoding.UTF8.GetString(date).Trim('\0');
                    if (richTextBox2.InvokeRequired)
                    { richTextBox2.Invoke(new Showmsgsafety(Showmsg), clientSocketEFEM, richTextBox2, msg); }

                    if (msg != null)
                        DecodingEFEM(msg);

                    Log("메시지 수신함");
                }
            }
            catch (SocketException e)
            {
                Log(e.Message);
                File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + e.Message + "\n");
            }
        }
        // 수신 처리...
        private void ReceivePM0()
        {
            try
            {
                while (true)
                {
                    byte[] receiveBuffer = new byte[40];

                    clientSocketPM0.Receive(receiveBuffer, 0, 4, SocketFlags.None);
                    int size = BitConverter.ToInt32(receiveBuffer, 0);

                    byte[] date = new byte[size];
                    clientSocketPM0.Receive(date, 0, size, SocketFlags.None);
                    string msg = Encoding.UTF8.GetString(date).Trim('\0');
                    if (richTextBox7.InvokeRequired)
                    { richTextBox7.Invoke(new Showmsgsafety(Showmsg), clientSocketPM0, richTextBox7, msg); }


                    DecodingPM(msg);

                    Log("메시지 수신함");
                }
            }
            catch (SocketException e)
            {
                File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + e.Message + "\n");
            }
            clientSocketPM0.Close();
        }
        // 수신 처리...
        private void ReceivePM1()
        {
            try
            {
                while (true)
                {
                    byte[] receiveBuffer = new byte[40];

                    clientSocketPM1.Receive(receiveBuffer, 0, 4, SocketFlags.None);
                    int size = BitConverter.ToInt32(receiveBuffer, 0);

                    byte[] date = new byte[size];
                    clientSocketPM1.Receive(date, 0, size, SocketFlags.None);
                    string msg = Encoding.UTF8.GetString(date).Trim('\0');
                    if (richTextBox8.InvokeRequired)
                    { richTextBox8.Invoke(new Showmsgsafety(Showmsg), clientSocketPM1, richTextBox8, msg); }


                    DecodingPM1(msg);

                    Log("메시지 수신함");
                }
            }
            catch (SocketException e)
            {
                File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + e.Message + "\n");
            }
            clientSocketPM1.Close();
        }

        private void ReceivePM2()
        {
            try
            {
                while (true)
                {
                    byte[] receiveBuffer = new byte[40];

                    clientSocketPM2.Receive(receiveBuffer, 0, 4, SocketFlags.None);
                    int size = BitConverter.ToInt32(receiveBuffer, 0);

                    byte[] date = new byte[size];
                    clientSocketPM2.Receive(date, 0, size, SocketFlags.None);
                    string msg = Encoding.UTF8.GetString(date).Trim('\0');
                    if (richTextBox9.InvokeRequired)
                    { richTextBox9.Invoke(new Showmsgsafety(Showmsg), clientSocketPM2, richTextBox9, msg); }


                    DecodingPM2(msg);

                    Log("메시지 수신함");
                }
            }
            catch (SocketException e)
            {
                File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + " <PM2 ERROR> " + e.Message + "\n");
            }
            clientSocketPM2.Close();
        }
        #endregion sever
        static int ckTime = 0;
        private void TimerTask(int speed)
        {
            int cntSec = 0;
            int cntMin = 0;
            int cntHour = 0;
            while (true)
            {
                //Thread.Sleep(1000 / speed);
                Delay(1000 / speed);
                cntSec++;
                tbSec.Text = cntSec.ToString();
                if (cntSec == 60)
                {
                    tbSec.Text = "00";
                    cntSec = 0;
                    cntMin++;
                    tbMin.Text = cntMin.ToString();

                    if (cntMin == 60)
                    {
                        tbMin.Text = "00";
                        cntMin = 0;
                        ckTime += 1;
                        cntHour++;
                        tbHour.Text = cntHour.ToString();
                        if (ckTime == cntHour)
                        {
                            byte[] sendBufferSERVER = Encoding.UTF8.GetBytes(
                            "SERVER" + "/" +
                             "RESULT" + "/"
                             );

                            int size = sendBufferSERVER.Length;
                            byte[] data_size = BitConverter.GetBytes(size);
                            clientSocketEFEM.Send(data_size);
                            clientSocketEFEM.Send(sendBufferSERVER, 0, size, SocketFlags.None);
                            Array.Clear(sendBufferSERVER, 0x0, sendBufferSERVER.Length);
                        }
                    }
                }
            }
        }

        #region 메세지
        delegate void LogSafety(string msg);

        public void Log(string msg)
        {
            //Cross Thread
            if (listBox1.InvokeRequired)
                listBox1.Invoke(new LogSafety(Log), msg);
            else
                listBox1.Items.Add(string.Format("[{0}]{1}", DateTime.Now.ToString(), msg));
        }
        delegate void Showmsgsafety(Socket socket, RichTextBox richTextBox, string msg);

        public void Showmsg(Socket socket, RichTextBox richTextBox, string msg)
        {
            IPEndPoint ip_point = (IPEndPoint)socket.RemoteEndPoint;
            string ip = ip_point.Address.ToString();

            if (richTextBox.InvokeRequired)
            {
                richTextBox.Invoke(new Showmsgsafety(Showmsg), socket, richTextBox, msg);
            }
            else
            {
                richTextBox.AppendText(string.Format("{0}:{1}", DateTime.Now.ToString(), port));
                richTextBox.AppendText(" : " + msg);
                richTextBox.AppendText("\r\n");
                // 입력된 텍스트에 맞게 스크롤을 내려준다.
            }
            this.Activate();


            // 캐럿(커서)를 텍스트박스의 끝으로 내려준다.
            richTextBox.SelectionStart = richTextBox.Text.Length;
            richTextBox.ScrollToCaret();   // 스크럴을 캐럿(커서)위치에 맞춰준다.
        }

        //Text 수정
        delegate void TextSafety(Label label, int Wafercount);
        private void ShowText(Label label, int Wafercount)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new TextSafety(ShowText), label, Wafercount);
            }

            else
            {
                label.Text = "" + Wafercount;
            }
        }

        private void SetText1(Label label, int val)
        {
            if (label.InvokeRequired) { label.Invoke(new TextSafety(ShowText), label, val); }
            else { label.Text = "" + val; }
        }
        #endregion


        #region Decoding
        //TM Decoding
        private void DecodingTM(string msg)
        {
            //================TM============================
            string[] val = msg.Split(new char[] { '/' });
            jsDATE.Add(DateTime.Now.ToString("dd-HH-mm"));
            jsMODULE.Add(val[0]);
            if (val[1] == "ERROR")
            {
                jsISSUE.Add("ERROR");
                jsMSG.Add(val[7]);
            }
            if (val[0] == "TM")
            {
                jsISSUE.Add("NORMAL");
                jsMSG.Add(val[1]);
                #region NORMAL

                //LL1
                if (val[1] == "Pump" && val[2] == 0.ToString() && val[3] == 1.ToString())
                    LLPump1();

                //LL2
                if (val[1] == "Pump" && val[2] == 0.ToString() && val[3] == 2.ToString())
                    LLPump1();
                else if (val[1] == "Pump" && val[2] == 1.ToString() && val[3] == 2.ToString())
                    LLPump2();

                //LL3
                if (val[1] == "Pump" && val[2] == 0.ToString() && val[3] == 3.ToString())
                    LLPump1();
                else if (val[1] == "Pump" && val[2] == 1.ToString() && val[3] == 3.ToString())
                    LLPump2();
                else if (val[1] == "Pump" && val[2] == 2.ToString() && val[3] == 3.ToString())
                    LLPump3();

                //LL4
                if (val[1] == "Pump" && val[2] == 0.ToString() && val[3] == 4.ToString())
                    LLPump1();

                else if (val[1] == "Pump" && val[2] == 1.ToString() && val[3] == 4.ToString())
                    LLPump2();

                else if (val[1] == "Pump" && val[2] == 2.ToString() && val[3] == 4.ToString())
                    LLPump3();

                else if (val[1] == "Pump" && val[2] == 3.ToString() && val[3] == 4.ToString())
                    LLPump4();

                /////////////////////////////                             

                //쿼드암
                /////////////////////////////////////////////////////////////////////
                //LL이 1개 일때
                if (val[1] == "VacPick" && val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 1.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM1();

                //LL이 2개 일때
                if (val[1] == "VacPick" && val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 2.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM1();
                else if (val[1] == "VacPick" && val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 2.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM2();
                else if (val[1] == "VacPick" && val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 2.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM10();


                //LL이 3개 일때
                if (val[1] == "VacPick" && val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM1();
                else if (val[1] == "VacPick" && val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM2();
                else if (val[1] == "VacPick" && val[2] == 2.ToString() && val[3] == 2.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM3();
                else if (val[1] == "VacPick" && val[2] == 2.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM7();
                else if (val[1] == "VacPick" && val[2] == 2.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM11();
                else if (val[1] == "VacPick" && val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM10();
                else if (val[1] == "VacPick" && val[2] == 0.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM5();

                //LL이 4개일때 
                if (val[1] == "VacPick" && val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM1();

                else if (val[1] == "VacPick" && val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM2();

                else if (val[1] == "VacPick" && val[2] == 2.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM3();

                else if (val[1] == "VacPick" && val[2] == 3.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM4();

                else if (val[1] == "VacPick" && val[2] == 0.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM5();

                else if (val[1] == "VacPick" && val[2] == 1.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM6();

                else if (val[1] == "VacPick" && val[2] == 2.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM7();

                else if (val[1] == "VacPick" && val[2] == 3.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM8();

                else if (val[1] == "VacPick" && val[2] == 0.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM9();

                else if (val[1] == "VacPick" && val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM10();

                else if (val[1] == "VacPick" && val[2] == 2.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM11();

                else if (val[1] == "VacPick" && val[2] == 3.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPickQuadPM12();



                //LL이 1개 일때
                if (val[1] == "VacPlace" && val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 1.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad1();

                //LL이 2개 일때
                if (val[1] == "VacPlace" && val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 2.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad1();
                else if (val[1] == "VacPlace" && val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 2.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad2();
                else if (val[1] == "VacPlace" && val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 2.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad10();

                //LL이 3개 일때
                if (val[1] == "VacPlace" && val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad1();
                else if (val[1] == "VacPlace" && val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad2();
                else if (val[1] == "VacPlace" && val[2] == 2.ToString() && val[3] == 2.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad3();
                else if (val[1] == "VacPlace" && val[2] == 2.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad7();
                else if (val[1] == "VacPlace" && val[2] == 2.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad11();
                else if (val[1] == "VacPlace" && val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad10();
                else if (val[1] == "VacPlace" && val[2] == 0.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad5();

                //LL이 4개일때 
                if (val[1] == "VacPlace" && val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad1();
                else if (val[1] == "VacPlace" && val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad2();
                else if (val[1] == "VacPlace" && val[2] == 2.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad3();
                else if (val[1] == "VacPlace" && val[2] == 3.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad4();
                else if (val[1] == "VacPlace" && val[2] == 0.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad5();
                else if (val[1] == "VacPlace" && val[2] == 1.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad6();
                else if (val[1] == "VacPlace" && val[2] == 2.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad7();
                else if (val[1] == "VacPlace" && val[2] == 3.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad8();
                else if (val[1] == "VacPlace" && val[2] == 0.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad9();
                else if (val[1] == "VacPlace" && val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad10();
                else if (val[1] == "VacPlace" && val[2] == 2.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad11();
                else if (val[1] == "VacPlace" && val[2] == 3.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                    VacPlaceQuad12();



                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////


                //LL이 1개 일때
                if (val[1] == "VacPick" && val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 1.ToString() && val[6] == 2.ToString())
                    VacPickPM1();

                //LL이 2개 일때
                if (val[1] == "VacPick" && val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 2.ToString() && val[6] == 2.ToString())
                    VacPickPM1();
                else if (val[1] == "VacPick" && val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 2.ToString() && val[6] == 2.ToString())
                    VacPickPM2();
                else if (val[1] == "VacPick" && val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 2.ToString() && val[6] == 2.ToString())
                    VacPickPM10();


                //LL이 3개 일때
                if (val[1] == "VacPick" && val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                    VacPickPM1();
                else if (val[1] == "VacPick" && val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                    VacPickPM2();
                else if (val[1] == "VacPick" && val[2] == 2.ToString() && val[3] == 2.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                    VacPickPM3();
                else if (val[1] == "VacPick" && val[2] == 2.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                    VacPickPM7();
                else if (val[1] == "VacPick" && val[2] == 2.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                    VacPickPM11();
                else if (val[1] == "VacPick" && val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                    VacPickPM10();
                else if (val[1] == "VacPick" && val[2] == 0.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                    VacPickPM5();

                //LL이 4개일때 
                if (val[1] == "VacPick" && val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPickPM1();

                else if (val[1] == "VacPick" && val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPickPM2();

                else if (val[1] == "VacPick" && val[2] == 2.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPickPM3();

                else if (val[1] == "VacPick" && val[2] == 3.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPickPM4();

                else if (val[1] == "VacPick" && val[2] == 0.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPickPM5();

                else if (val[1] == "VacPick" && val[2] == 1.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPickPM6();

                else if (val[1] == "VacPick" && val[2] == 2.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPickPM7();

                else if (val[1] == "VacPick" && val[2] == 3.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPickPM8();

                else if (val[1] == "VacPick" && val[2] == 0.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPickPM9();

                else if (val[1] == "VacPick" && val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPickPM10();

                else if (val[1] == "VacPick" && val[2] == 2.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPickPM11();

                else if (val[1] == "VacPick" && val[2] == 3.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPickPM12();

                /////////////////////////////         
                //LL이 1개 일때
                if (val[1] == "VacPlace" && val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 1.ToString() && val[6] == 2.ToString())
                    VacPlace1();

                //LL이 2개 일때
                if (val[1] == "VacPlace" && val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 2.ToString() && val[6] == 2.ToString())
                    VacPlace1();
                else if (val[1] == "VacPlace" && val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 2.ToString() && val[6] == 2.ToString())
                    VacPlace2();
                else if (val[1] == "VacPlace" && val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 2.ToString() && val[6] == 2.ToString())
                    VacPlace10();

                //LL이 3개 일때
                //LL이 3개 일때
                if (val[1] == "VacPlace" && val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                    VacPlace1();
                else if (val[1] == "VacPlace" && val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                    VacPlace2();
                else if (val[1] == "VacPlace" && val[2] == 2.ToString() && val[3] == 2.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                    VacPlace3();
                else if (val[1] == "VacPlace" && val[2] == 2.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                    VacPlace7();
                else if (val[1] == "VacPlace" && val[2] == 2.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                    VacPlace11();
                else if (val[1] == "VacPlace" && val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                    VacPlace10();
                else if (val[1] == "VacPlace" && val[2] == 0.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                    VacPlace5();

                //LL이 4개일때 
                if (val[1] == "VacPlace" && val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPlace1();
                else if (val[1] == "VacPlace" && val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPlace2();
                else if (val[1] == "VacPlace" && val[2] == 2.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPlace3();
                else if (val[1] == "VacPlace" && val[2] == 3.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPlace4();
                else if (val[1] == "VacPlace" && val[2] == 0.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPlace5();
                else if (val[1] == "VacPlace" && val[2] == 1.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPlace6();
                else if (val[1] == "VacPlace" && val[2] == 2.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPlace7();
                else if (val[1] == "VacPlace" && val[2] == 3.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPlace8();
                else if (val[1] == "VacPlace" && val[2] == 0.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPlace9();
                else if (val[1] == "VacPlace" && val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPlace10();
                else if (val[1] == "VacPlace" && val[2] == 2.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPlace11();
                else if (val[1] == "VacPlace" && val[2] == 3.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                    VacPlace12();

                //Vent
                //LL1
                if (val[1] == "Vent" && val[2] == 0.ToString() && val[3] == 1.ToString())
                    LLVent1();
                //LL2
                if (val[1] == "Vent" && val[2] == 0.ToString() && val[3] == 2.ToString())
                    LLVent1();
                else if (val[1] == "Vent" && val[2] == 1.ToString() && val[3] == 2.ToString())
                    LLVent2();
                //LL3
                if (val[1] == "Vent" && val[2] == 0.ToString() && val[3] == 3.ToString())
                    LLVent1();
                else if (val[1] == "Vent" && val[2] == 1.ToString() && val[3] == 3.ToString())
                    LLVent2();
                else if (val[1] == "Vent" && val[2] == 2.ToString() && val[3] == 3.ToString())
                    LLVent3();

                //LL4
                if (val[1] == "Vent" && val[2] == 0.ToString() && val[3] == 4.ToString())
                    LLVent1();

                else if (val[1] == "Vent" && val[2] == 1.ToString() && val[3] == 4.ToString())
                    LLVent2();

                else if (val[1] == "Vent" && val[2] == 2.ToString() && val[3] == 4.ToString())
                    LLVent3();

                else if (val[1] == "Vent" && val[2] == 3.ToString() && val[3] == 4.ToString())
                    LLVent4();

                if (val[1] == "LLFill")
                {
                    byte[] sendBufferEFEM = Encoding.UTF8.GetBytes(
                        "SERVER" + "/" +
                        val[1] + "/" +
                        val[2] + "/" +
                        val[3] + "/"
                        );
                    clientSocketEFEM.Send(sendBufferEFEM);
                    Log("TM에게 메시지 전송됨");
                    Array.Clear(sendBufferEFEM, 0x0, sendBufferEFEM.Length);
                }
                if (val[1] == "FillChamber")
                {
                    Socket socketPM = null;
                    byte[] sendBufferPM = Encoding.UTF8.GetBytes(
                        "SERVER" + "/" +
                        val[1] + "/" +
                        //PMpos
                        val[2] + "/" +
                        //nWafer
                        val[3] + "/" +
                        //LLpos 돌아가야할 위치를 알고있어야함
                        val[4] + "/"
                        );
                    switch (Convert.ToInt32(val[2]))
                    {
                        case 0:
                            socketPM = clientSocketPM0;
                            break;
                        case 1:
                            socketPM = clientSocketPM1;
                            break;
                        case 2:
                            socketPM = clientSocketPM2;
                            break;
                        default:
                            break;
                    }
                    int size = sendBufferPM.Length;
                    byte[] data_size = BitConverter.GetBytes(size);
                    socketPM.Send(data_size);
                    socketPM.Send(sendBufferPM, 0, size, SocketFlags.None);
                    Log("TM에게 메시지 전송됨");
                    Array.Clear(sendBufferPM, 0x0, sendBufferPM.Length);
                }
                if (val[1] == "VentEnd")
                {
                    byte[] sendBufferEFEM = Encoding.UTF8.GetBytes(
                        "SERVER" + "/" +
                        //msg
                        val[1] + "/" +
                        //LLpos
                        val[2] + "/"
                        );
                    int size = sendBufferEFEM.Length;
                    byte[] data_size = BitConverter.GetBytes(size);
                    clientSocketEFEM.Send(data_size);
                    clientSocketEFEM.Send(sendBufferEFEM, 0, size, SocketFlags.None);
                    Array.Clear(sendBufferEFEM, 0x0, sendBufferEFEM.Length);
                }
                if (val[1] == "PickPMtoVAC")
                {
                    byte[] sendBufferPM = Encoding.UTF8.GetBytes(
                       "SERVER" + "/" +
                       //msg
                       val[1] + "/" +
                       //LLpos
                       val[2] + "/" +
                       //PMpos
                       val[3] + "/" +
                       //nWafer
                       val[4] + "/"
                       );
                    Socket socketPM = null;
                    switch (Convert.ToInt32(val[3]))
                    {
                        case 0:
                            socketPM = clientSocketPM0;
                            break;
                        case 1:
                            socketPM = clientSocketPM1;
                            break;
                        case 2:
                            socketPM = clientSocketPM2;
                            break;
                        default:
                            break;
                    }

                    //쿼드암
                    //LL 1개일때
                    if (val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 1.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM0();

                    //LL2개일떄
                    //LL이 2개 일때

                    if (val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 2.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM0();
                    else if (val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 2.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM1();
                    else if (val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 2.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM9();

                    //LL3개일떄
                    if (val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM0();
                    else if (val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM1();
                    else if (val[2] == 2.ToString() && val[3] == 2.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM2();
                    else if (val[2] == 0.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM4();
                    else if (val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM9();
                    else if (val[2] == 2.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM10();
                    else if (val[2] == 2.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM6();

                    //LL4개일때
                    if (val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM0();
                    else if (val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM1();
                    else if (val[2] == 2.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM2();
                    else if (val[2] == 3.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM3();
                    else if (val[2] == 0.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM4();
                    else if (val[2] == 1.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM5();
                    else if (val[2] == 2.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM6();
                    else if (val[2] == 3.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM7();
                    else if (val[2] == 0.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM8();
                    else if (val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM9();
                    else if (val[2] == 2.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM10();
                    else if (val[2] == 3.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        VacPickQuadEndPM11();

                    ///////////////////////////////////////////////////////////////////////////////
                    //LL 1개일때
                    if (val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 1.ToString() && val[6] == 2.ToString())
                        VacPickEndPM0();

                    //LL2개일떄
                    //LL이 2개 일때

                    if (val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 2.ToString() && val[6] == 2.ToString())
                        VacPickEndPM0();
                    else if (val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 2.ToString() && val[6] == 2.ToString())
                        VacPickEndPM1();
                    else if (val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 2.ToString() && val[6] == 2.ToString())
                        VacPickEndPM9();

                    //LL3개일떄
                    if (val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                        VacPickEndPM0();
                    else if (val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                        VacPickEndPM1();
                    else if (val[2] == 2.ToString() && val[3] == 2.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                        VacPickEndPM2();
                    else if (val[2] == 0.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                        VacPickEndPM4();
                    else if (val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                        VacPickEndPM9();
                    else if (val[2] == 2.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                        VacPickEndPM10();
                    else if (val[2] == 2.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                        VacPickEndPM6();

                    //LL4개일때
                    if (val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        VacPickEndPM0();
                    else if (val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        VacPickEndPM1();
                    else if (val[2] == 2.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        VacPickEndPM2();
                    else if (val[2] == 3.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        VacPickEndPM3();
                    else if (val[2] == 0.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        VacPickEndPM4();
                    else if (val[2] == 1.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        VacPickEndPM5();
                    else if (val[2] == 2.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        VacPickEndPM6();
                    else if (val[2] == 3.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        VacPickEndPM7();
                    else if (val[2] == 0.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        VacPickEndPM8();
                    else if (val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        VacPickEndPM9();
                    else if (val[2] == 2.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        VacPickEndPM10();
                    else if (val[2] == 3.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        VacPickEndPM11();

                    int size = sendBufferPM.Length;
                    byte[] data_size = BitConverter.GetBytes(size);
                    socketPM.Send(data_size);
                    socketPM.Send(sendBufferPM, 0, size, SocketFlags.None);
                    Array.Clear(sendBufferPM, 0x0, sendBufferPM.Length);
                }

                if (val[1] == "PlaceVACtoLL")
                {
                    byte[] sendBufferEFEM = Encoding.UTF8.GetBytes(
                        "SERVER" + "/" +
                        //msg
                        val[1] + "/" +
                        //LLpos
                        val[2] + "/" +
                        //PMpos
                        val[3] + "/" +
                        //nWafer
                        val[4] + "/" +
                        //LL수
                        val[5] + "/" +
                        //Vac수
                        val[6] + "/"
                        );
                    int size = sendBufferEFEM.Length;
                    byte[] data_size = BitConverter.GetBytes(size);
                    clientSocketEFEM.Send(data_size);
                    clientSocketEFEM.Send(sendBufferEFEM, 0, size, SocketFlags.None);
                    Array.Clear(sendBufferEFEM, 0x0, sendBufferEFEM.Length);

                    //쿼드암
                    //LL1
                    if (val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 1.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL1();

                    //LL2
                    if (val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 2.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL1();
                    else if (val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 2.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL2();
                    else if (val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 2.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL10();

                    //LL3
                    if (val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL1();
                    else if (val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL2();
                    else if (val[2] == 2.ToString() && val[3] == 2.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL3();
                    else if (val[2] == 0.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL5();
                    else if (val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL10();
                    else if (val[2] == 2.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL11();
                    else if (val[2] == 2.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL7();

                    //LL4
                    if (val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL1();
                    else if (val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL2();
                    else if (val[2] == 2.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL3();
                    else if (val[2] == 3.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL4();
                    else if (val[2] == 0.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL5();
                    else if (val[2] == 1.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL6();
                    else if (val[2] == 2.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL7();
                    else if (val[2] == 3.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL8();
                    else if (val[2] == 0.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL9();
                    else if (val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL10();
                    else if (val[2] == 2.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL11();
                    else if (val[2] == 3.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 4.ToString())
                        PlaceQuadVACtoLL12();
                    //////////////////////////////////////////////////////////////////////////////////
                    //LL1
                    if (val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 1.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL1();

                    //LL2
                    if (val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 2.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL1();
                    else if (val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 2.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL2();
                    else if (val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 2.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL10();

                    //LL3
                    if (val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL1();
                    else if (val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL2();
                    else if (val[2] == 2.ToString() && val[3] == 2.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL3();
                    else if (val[2] == 0.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL5();
                    else if (val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL10();
                    else if (val[2] == 2.ToString() && val[3] == 1.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL11();
                    else if (val[2] == 2.ToString() && val[3] == 0.ToString() && val[5] == 3.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL7();


                    //LL4
                    if (val[2] == 0.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL1();
                    else if (val[2] == 1.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL2();
                    else if (val[2] == 2.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL3();
                    else if (val[2] == 3.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL4();
                    else if (val[2] == 0.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL5();
                    else if (val[2] == 1.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL6();
                    else if (val[2] == 2.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL7();
                    else if (val[2] == 3.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL8();
                    else if (val[2] == 0.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL9();
                    else if (val[2] == 1.ToString() && val[3] == 0.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL10();
                    else if (val[2] == 2.ToString() && val[3] == 1.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL11();
                    else if (val[2] == 3.ToString() && val[3] == 2.ToString() && val[5] == 4.ToString() && val[6] == 2.ToString())
                        PlaceVACtoLL12();
                }
                #endregion
            }
            //================TM============================       
        }
        //EFEM Decoding
        private void DecodingEFEM(string msg)
        {

            string[] val = msg.Split(new char[] { '/' });
            jsDATE.Add(DateTime.Now.ToString("dd-HH-mm"));
            jsMODULE.Add(val[0]);
            if (val[1] == "RESULT")
            {
                tbCompWafer.Text = val[2].ToString() + " 개";
                tbCompFoup.Text = (Convert.ToInt32(val[3]) / 25).ToString() + " 개";
            }
            if (val[1] == "ERROR")
            {
                jsISSUE.Add("ERROR");
                jsMSG.Add(val[5]);
            }
            if (val[0] == "EFEM")
            {
                jsISSUE.Add("NORMAL");
                jsMSG.Add(val[1]);
                //================공용=========================
                if (val[1] == "PickAndPlace")
                    PickAndPlace();

                if (val[1] == "LLMoveandPick")
                    LLMoveandPick();


                if (val[1] == "LLRotate")
                    LLRotate();

                //================공용=========================

                //================LPM1=========================
                if (val[1] == "FirstPickWafertoAlg")
                {
                    TimerThread = new Thread(() => TimerTask(Obj.speed));
                    TimerThread.IsBackground = true;
                    TimerThread.Start();
                    FirstPickWafertoAlg();
                }

                if (val[1] == "FirstPlaceWafer")
                    FirstPlaceWafer();

                if (val[1] == "FirstMoveLPM1")
                    FirstMoveLPM1();


                if (val[1] == "PickWaferLPM1")
                    PickWaferLPM1();

                if (val[1] == "MoveLPM1toAlg")
                    MoveLPM1toAlg();

                if (val[1] == "CenterMoveLPM1")
                    CenterMoveLPM1();

                //================LPM1=========================

                //================LPM2=========================


                if (val[1] == "PickWaferLPM2")
                    PickWaferLPM2();

                if (val[1] == "MoveWaferLPM2")
                    MoveWaferLPM2();

                if (val[1] == "MoveLPM2toAlg")
                    MoveLPM2toAlg();

                if (val[1] == "LPM1Move")
                    LPM1Move();

                //================LPM2=========================           

                //================LL============================
                if (val[1] == "PlaceATMtoLL")
                {
                    byte[] sendBufferTM = Encoding.UTF8.GetBytes(
                        "SERVER" + "/" +
                        //msg
                        val[1] + "/" +
                        //LLpos
                        val[2] + "/" +
                        //nWafer
                        val[3] + "/"
                        );

                    int size = sendBufferTM.Length;
                    byte[] data_size = BitConverter.GetBytes(size);
                    clientSocketTMC.Send(data_size);
                    clientSocketTMC.Send(sendBufferTM, 0, size, SocketFlags.None);             
                    Array.Clear(sendBufferTM, 0x0, sendBufferTM.Length);
                }

                //LL 채우기
                if (val[1] == "PlaceUpWaferLL" &&val[2] == 1.ToString())
                    PlaceUpWaferLL1();
                else if (val[1] == "PlaceUpWaferLL" && val[2] == 2.ToString())
                    PlaceUpWaferLL2();
                else if (val[1] == "PlaceUpWaferLL" && val[2] == 3.ToString())
                    PlaceUpWaferLL3();
                else if (val[1] == "PlaceUpWaferLL" && val[2] == 4.ToString())
                    PlaceUpWaferLL4();

                //0911
                if (val[1] == "PickLLtoATM")
                {
                    byte[] sendBufferTM = Encoding.UTF8.GetBytes(
                        "SERVER" + "/" +
                        //msg
                        val[1] + "/" +
                        //LLpos
                        val[2] + "/" +
                        //LPMpos
                        val[3] + "/" +
                        //nWafer
                        val[4] + "/"
                        );                
                    int size = sendBufferTM.Length;
                    byte[] data_size = BitConverter.GetBytes(size);
                    clientSocketTMC.Send(data_size);
                    clientSocketTMC.Send(sendBufferTM, 0, size, SocketFlags.None);

                    //LL1개
                    if (val[2] == 0.ToString() && val[4] == 1.ToString())
                        PickLLtoATM1();
                    //LL2개
                    if (val[2] == 0.ToString() && val[4] == 2.ToString())
                        PickLLtoATM1();

                    if (val[2] == 1.ToString() && val[4] == 2.ToString())
                        PickLLtoATM2();

                    //LL3개
                    if (val[2] == 0.ToString() && val[4] == 3.ToString())
                        PickLLtoATM1();

                    if (val[2] == 1.ToString() && val[4] == 3.ToString())
                        PickLLtoATM2();

                    if (val[2] == 2.ToString() && val[4] == 3.ToString())
                        PickLLtoATM3();

                    //LL4개일때
                    if (val[2] ==0.ToString() &&val[4] == 4.ToString())
                        PickLLtoATM1();

                    if (val[2] == 1.ToString() && val[4] == 4.ToString())
                        PickLLtoATM2();

                    if (val[2] == 2.ToString() && val[4] == 4.ToString())
                        PickLLtoATM3();

                    if (val[2] == 3.ToString() && val[4] == 4.ToString())
                        PickLLtoATM4();
                    
                    Array.Clear(sendBufferTM, 0x0, sendBufferTM.Length);
                }

                //0911
                if (val[1] == "End")
                {
                    byte[] sendBufferTM = Encoding.UTF8.GetBytes(
                           "SERVER" + "/" +
                           //msg
                           val[1] + "/" +
                           //LLpos
                           val[2] + "/" +
                           //nWafer
                           val[3] + "/"
                           );
                    int size = sendBufferTM.Length;
                    byte[] data_size = BitConverter.GetBytes(size);
                    clientSocketTMC.Send(data_size);
                    clientSocketTMC.Send(sendBufferTM, 0, size, SocketFlags.None);
                    Array.Clear(sendBufferTM, 0x0, sendBufferTM.Length);
                }
            }
        }
        //PM Decoding
        private void DecodingPM(string msg)
        {
            string[] val = msg.Split(new char[] { '/' });
            jsDATE.Add(DateTime.Now.ToString("dd-HH-mm"));
            jsMODULE.Add(val[0]);
            if (val[1] == "ERROR")
            {
                jsISSUE.Add("ERROR");
                jsMSG.Add(val[5]);
            }
            else
            {
                if (val[1] == "ProcessStart")
                {
                    //프로세스 진행 그림 표현
                    jsISSUE.Add("NORMAL");
                    jsMSG.Add(val[1]);
                    for (int i = Convert.ToInt32(Obj.PMobj.nProcessTime); i >= 0; i--)
                    {
                        Delay(1000/Obj.speed);
                        label17.Text = i.ToString();

                    }
                }
                if (val[1] == "ProcessEND")
                {
                    jsISSUE.Add("NOMAL");
                    jsMSG.Add(val[1]);
                    byte[] sendBufferTM = Encoding.UTF8.GetBytes(
                        "SERVER" + "/" +
                        val[1] + "/" +
                        //PMpos
                        val[2] + "/" +
                        //nWafer
                        val[3] + "/" +
                        //LLpos
                        val[4] + "/"
                        );

                    int size = sendBufferTM.Length;
                    byte[] data_size = BitConverter.GetBytes(size);
                    clientSocketTMC.Send(data_size);
                    clientSocketTMC.Send(sendBufferTM, 0, size, SocketFlags.None);
                    ProcessEndPM0();
                    Log("TM에게 메시지 전송됨");
                    Array.Clear(sendBufferTM, 0x0, sendBufferTM.Length);
                }
            }
        }
        //PM1 Decoding
        private void DecodingPM1(string msg)
        {
            string[] val = msg.Split(new char[] { '/' });
            jsDATE.Add(DateTime.Now.ToString("dd-HH-mm"));
            jsMODULE.Add(val[0]);
            if (val[1] == "ERROR")
            {
                jsISSUE.Add("ERROR");
                jsMSG.Add(val[5]);
            }
            else
            {
                if (val[1] == "ProcessStart")
                {
                    //프로세스 진행 그림 표현
                    jsISSUE.Add("NORMAL");
                    jsMSG.Add(val[1]);
                    for (int i = Convert.ToInt32(Obj.PMobj.nProcessTime); i >= 0; i--)
                    {
                        Delay(1000 / Obj.speed);
                        label18.Text = i.ToString();

                    }
                }
                if (val[1] == "ProcessEND")
                {
                    jsISSUE.Add("NORMAL");
                    jsMSG.Add(val[1]);
                    byte[] sendBufferTM = Encoding.UTF8.GetBytes(
                        "SERVER" + "/" +
                        val[1] + "/" +
                        //PMpos
                        val[2] + "/" +
                        //nWafer
                        val[3] + "/" +
                        //LLpos
                        val[4] + "/"
                        );

                    int size = sendBufferTM.Length;
                    byte[] data_size = BitConverter.GetBytes(size);
                    clientSocketTMC.Send(data_size);
                    clientSocketTMC.Send(sendBufferTM, 0, size, SocketFlags.None);
                    ProcessEndPM1();
                    Log("TM에게 메시지 전송됨");
                    Array.Clear(sendBufferTM, 0x0, sendBufferTM.Length);
                }

            }
        }
        //PM2 Decoding
        private void DecodingPM2(string msg)
        {
            string[] val = msg.Split(new char[] { '/' });
            jsDATE.Add(DateTime.Now.ToString("dd-HH-mm"));
            jsMODULE.Add(val[0]);
            if (val[1] == "ERROR")
            {
                jsISSUE.Add("ERROR");
                jsMSG.Add(val[5]);
            }
            else
            {
                if (val[1] == "ProcessStart")
                {
                    //프로세스 진행 그림 표현
                    jsISSUE.Add("NORMAL");
                    jsMSG.Add(val[1]);
                    for (int i = Convert.ToInt32(Obj.PMobj.nProcessTime); i >= 0; i--)
                    {
                        Delay(1000 / Obj.speed);
                        label19.Text = i.ToString();

                    }
                }
                if (val[1] == "ProcessEND")
                {
                    jsISSUE.Add("NORMAL");
                    jsMSG.Add(val[1]);
                    byte[] sendBufferTM = Encoding.UTF8.GetBytes(
                        "SERVER" + "/" +
                        val[1] + "/" +
                        //PMpos
                        val[2] + "/" +
                        //nWafer
                        val[3] + "/" +
                        //LLpos
                        val[4] + "/"
                        );

                    int size = sendBufferTM.Length;
                    byte[] data_size = BitConverter.GetBytes(size);
                    clientSocketTMC.Send(data_size);
                    clientSocketTMC.Send(sendBufferTM, 0, size, SocketFlags.None);
                    ProcessEndPM2();
                    Log("TM에게 메시지 전송됨");
                    Array.Clear(sendBufferTM, 0x0, sendBufferTM.Length);
                }
            }
        }
        //추가
        #endregion


        #region 버튼 컨트롤
        //server open
        private void Start_Click(object sender, EventArgs e)
        {
            if (Start.Text == "START")
            {
                Start.Text = "STOP";
                Log("서버 시작됨");

                // Listen 쓰레드 처리
                listenThreadTMC = new Thread(new ThreadStart(ListenTMC));     // 실행되는 메소드
                listenThreadTMC.IsBackground = true;   // 스레드가 배경 스레드인지 나타내는 함수

                listenThreadEFEM = new Thread(new ThreadStart(ListenEFEM));
                listenThreadEFEM.IsBackground = true;

                listenThreadPM0 = new Thread(new ThreadStart(ListenPM0));
                listenThreadPM0.IsBackground = true;

                listenThreadPM1 = new Thread(new ThreadStart(ListenPM1));
                listenThreadPM1.IsBackground = true;

                listenThreadPM2 = new Thread(new ThreadStart(ListenPM2));
                listenThreadPM2.IsBackground = true;

                // 운영체제에서 현재 인스턴스의 상태를 Run으로 만듬(시작)
                listenThreadTMC.Start();
                listenThreadEFEM.Start();
                listenThreadPM0.Start();
                listenThreadPM1.Start();
                listenThreadPM2.Start();

                File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "SERVER open\n");


            }
            else
            {
                Start.Text = "START";
                Log("서버 멈춤");
            }
        }
        private void metroButton1_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2(this, fasPath);
            f2.Show();
            File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "SETTING FORM OPEN\n");
        }
        private void metroButton3_Click(object sender, EventArgs e)
        {
            Form3 f3 = new Form3(this);
            File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "PATCH SETTING FORM OPEN\n");
            f3.Show();
        }
        //patch

        private void metroButton2_Click(object sender, EventArgs e)
        {
            MetroMessageBox.Show(this, "PATCH START", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "PATCH\n");
            //var sourcepath = @"C:\Users\BIT\Desktop\1001_Solution\00.Server Origin\Server Origin\JUSUNG_Server\bin\Debug\Log\0.CTC\Path";
            var sourcepath =Path.GetFullPath(@"..\Debug\Log\0.CTC\Path");
            // 서버에 접속한다.
            var ipep = new IPEndPoint(IPAddress.Parse("192.168.1.6"), 10000);

            IPEndPoint[] ipeparr = {
            new IPEndPoint(IPAddress.Parse("192.168.1.6"), 10000),
            new IPEndPoint(IPAddress.Parse("192.168.1.6"), 10001),
            new IPEndPoint(IPAddress.Parse("192.168.1.6"), 10002),
            new IPEndPoint(IPAddress.Parse("192.168.1.6"), 10003),
            new IPEndPoint(IPAddress.Parse("192.168.1.6"), 10005)
            };

            // 디렉토리가 존재하는지
            if (Directory.Exists(sourcepath))
            {
                // 바이너리 버퍼
                var binary = CommpressDirectory(sourcepath);
                // 소켓 생성
                for (int i = 0; i < ipeparr.Count(); i++)
                {
                    using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        // 접속
                        //client.Connect(ipep);
                        client.Connect(ipeparr[i]);
                        // 상태 0 - 파일 크기를 보낸다.
                        client.Send(new byte[] { 0 });
                        // 송신 - 파일 이름 크기 Bigendian
                        client.Send(BitConverter.GetBytes(binary.Length));
                        // 상태 1 - 파일를 보낸다.
                        client.Send(new byte[] { 1 });
                        // 송신 - 파일
                        client.Send(binary);

                    }
                    WriteLine("전송선공");
                }
            }
            else
            {
                // 콘솔 출력
                Console.WriteLine("The directory is not exists.");
            }
        }

        private void metroButton4_Click(object sender, EventArgs e)
        {
            JObject dbSpec = new JObject();
            dbSpec.Add("DATE", JArray.FromObject(jsDATE));
            dbSpec.Add("ISSUE", JArray.FromObject(jsISSUE));
            dbSpec.Add("MODULE", JArray.FromObject(jsMODULE));
            dbSpec.Add("MSG", JArray.FromObject(jsMSG));

            File.WriteAllText(@"..\Debug\Log\0.CTC\CTC_LOG.json", dbSpec.ToString());

            Form4 f4 = new Form4(clientSocketEFEM, clientSocketTMC, clientSocketPM0, clientSocketPM1, clientSocketPM2);
            f4.Show();
        }
        #endregion


        #region 그리기
        private static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            ThisMoment.ToLocalTime();
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }
            return DateTime.Now;
        }

        // drawing function
        delegate void PictureBoxSafety();

        public Mat Rotate(Mat src, int angle)
        {
            Mat rotate = new Mat(src.Size(), MatType.CV_8UC3);
            Mat matrix = Cv2.GetRotationMatrix2D(new Point2f(src.Width / 2, src.Height / 2), angle, 1);
            Cv2.WarpAffine(src, rotate, matrix, src.Size(), InterpolationFlags.Linear);
            return rotate;
        }

        static int PM1cnt = 0;
        static int PM2cnt = 0;
        static int PM3cnt = 0;
        static int PM4cnt = 0;
        static int PM5cnt = 0;
        static int PM6cnt = 0;
        static int PM7cnt = 0;
        static int PM8cnt = 0;
        static int PM9cnt = 0;
        static int PM10cnt = 0;
        static int PM11cnt = 0;
        static int PM12cnt = 0;

        static int PMLL1cnt = 0;
        static int PMLL2cnt = 0;
        static int PMLL3cnt = 0;
        static int PMLL4cnt = 0;

        // 마지막 체크하기 위해
        static int last = 0;

        // LPM3에 집어넣기
        void ATMinLPM3()
        {
            Mat ATMsrc = new Mat(@"..\Debug\VACrobotC\VACrobot10.png", ImreadModes.Unchanged);
            if (Obj.EFEMobj.LLwafer % 2 == 1)
            {
                if (PMLL1cnt == -1)
                {
                    PMLL1cnt = 0;
                    ATMsrc = new Mat(@"..\Debug\VACC1\robot10.png", ImreadModes.Unchanged);
                    last = 1;
                }
                else if (PMLL2cnt == -1)
                {
                    PMLL2cnt = 0;
                    ATMsrc = new Mat(@"..\Debug\VACC1\robot10.png", ImreadModes.Unchanged);
                    last = 1;
                }
                else if (PMLL3cnt == -1)
                {
                    PMLL3cnt = 0;
                    ATMsrc = new Mat(@"..\Debug\VACC1\robot10.png", ImreadModes.Unchanged);
                    last = 1;
                }
                else if (PMLL4cnt == -1)
                {
                    PMLL4cnt = 0;
                    ATMsrc = new Mat(@"..\Debug\VACC1\robot10.png", ImreadModes.Unchanged);
                    last = 1;
                }
            }
            Mat dst = new Mat(ATMsrc.Size(), MatType.CV_8UC3);
            dst = Rotate(ATMsrc, 180);

            for (int i = 0; i < 16; i++)
            {
                pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                pictureBox1.Left += 10;
                Delay(Convert.ToInt32(40 / Obj.speed));
            }

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(ATMsrc, 180 + (10 * i));
                pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((55 * Obj.EFEMobj.nAspeedRotate) / Obj.speed));
            }

            if (last == 0)
            {
                if (LPMcnt3 != 24)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        ATMsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(ATMsrc);
                        Delay(Convert.ToInt32((100 * Obj.EFEMobj.nAspeedPick) / Obj.speed));

                        if (i == 6)
                        {
                            LPMcnt3 += 2;
                            LPMcomSrc = new Mat(@"..\Debug\LPMimagePM\LPM" + LPMcnt3 + ".png", ImreadModes.Unchanged);
                            pictureBoxLPM3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LPMcomSrc);
                            if (LPMcnt3 == 25)
                            {
                                LPMcnt3 = 0;
                            }
                        }
                    }
                }

                else if (LPMcnt3 == 24)
                {
                    for (int i = 1; i <= 20; i++)
                    {
                        ATMsrc = new Mat(@"..\Debug\VACrobotC\robot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(ATMsrc);
                        Delay(Convert.ToInt32((100 * Obj.EFEMobj.nAspeedPick) / Obj.speed));

                        if (i == 6)
                        {
                            LPMcnt3++;
                            LPMcomSrc = new Mat(@"..\Debug\LPMimagePM\LPM" + LPMcnt3 + ".png", ImreadModes.Unchanged);
                            pictureBoxLPM3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LPMcomSrc);
                        }
                        else if (i == 10)
                        {
                            LPMcnt3 = 0;
                            Delay(Convert.ToInt32(1000 / Obj.speed));
                            LPMcomSrc = new Mat(@"..\Debug\LPMimagePM\LPM" + LPMcnt3 + ".png", ImreadModes.Unchanged);
                            pictureBoxLPM3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LPMcomSrc);
                            Delay(Convert.ToInt32(1000 / Obj.speed));
                        }
                        else if (i == 15)
                        {
                            LPMcnt3++;
                            LPMcomSrc = new Mat(@"..\Debug\LPMimagePM\LPM" + LPMcnt3 + ".png", ImreadModes.Unchanged);
                            pictureBoxLPM3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LPMcomSrc);
                        }
                    }
                }
            }
            else if (last == 1)
            {
                for (int i = 10; i >= 1; i--)
                {
                    ATMsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(ATMsrc);
                    Delay(Convert.ToInt32((100 * Obj.EFEMobj.nAspeedPick) / Obj.speed));

                    if (i == 6)
                    {
                        LPMcnt3 += 1;
                        LPMcomSrc = new Mat(@"..\Debug\LPMimagePM\LPM" + LPMcnt3 + ".png", ImreadModes.Unchanged);
                        pictureBoxLPM3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LPMcomSrc);
                        if (LPMcnt3 == 25)
                        {
                            LPMcnt3 = 0;
                        }
                    }
                }
                Delay(Convert.ToInt32(1000 / Obj.speed));
                LPMcomSrc = new Mat(@"..\Debug\LPMimagePM\LPM" + LPMcnt3 + ".png", ImreadModes.Unchanged);
                pictureBoxLPM3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LPMcomSrc);
                last = 0;
            }

            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(ATMsrc, (10 * i));
                pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((55 * Obj.EFEMobj.nAspeedRotate) / Obj.speed));
            }

            for (int i = 0; i < 16; i++)
            {
                pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                pictureBox1.Left -= 10;
                Delay(Convert.ToInt32(40 / (Obj.speed)));
            }

            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(ATMsrc, 90 + (10 * i));
                pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((55 * Obj.EFEMobj.nAspeedRotate) / Obj.speed));
            }
        }

        private void PickLLtoATM1()
        {

            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new PictureBoxSafety(PickLLtoATM1));
            }
            else
            {
                Delay(300);
                if (PMLL1cnt == 0)
                    PMLL1cnt = Obj.EFEMobj.LLwafer;

                Mat ATMsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
                Mat dst = new Mat(ATMsrc.Size(), MatType.CV_8UC3);

                if (pictureBox1.Left != 252)
                {
                    while (pictureBox1.Left != 252)
                    {
                        if (pictureBox1.Left < 252)
                        {
                            pictureBox1.Left += 10;
                            Delay(Convert.ToInt32(40 / (Obj.speed)));
                        }
                        else if (pictureBox1.Left > 252)
                        {
                            pictureBox1.Left -= 10;
                            Delay(Convert.ToInt32(40 / (Obj.speed)));
                        }
                    }
                }
                if (PMLL1cnt > 1)
                {

                    PMLL1cnt -= 2;

                    for (int i = 1; i <= 10; i++)
                    {
                        ATMsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(ATMsrc, 180);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[0] = new Mat(@"..\Debug\LLpm\LL" + PMLL1cnt + ".png", ImreadModes.Unchanged);
                            pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);
                        }
                    }
                }
                else
                {
                    PMLL1cnt -= 1;

                    for (int i = 1; i <= 10; i++)
                    {
                        ATMsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(ATMsrc, 180);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[0] = new Mat(@"..\Debug\LLpm\LL" + PMLL1cnt + ".png", ImreadModes.Unchanged);
                            pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                        }
                    }
                }
                if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    if (PMLL1cnt == 0)
                        PMLL1cnt = -1;
                }
                ATMinLPM3();
            }
        }

        private void PickLLtoATM2()
        {
            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new PictureBoxSafety(PickLLtoATM2));
            }
            else
            {
                Delay(300);
                if (PMLL2cnt == 0)
                    PMLL2cnt = Obj.EFEMobj.LLwafer;

                Mat ATMsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
                Mat dst = new Mat(ATMsrc.Size(), MatType.CV_8UC3);
                if (pictureBox1.Left != 252)
                {
                    while (pictureBox1.Left != 252)
                    {
                        if (pictureBox1.Left < 252)
                        {
                            pictureBox1.Left += 10;
                            Delay(Convert.ToInt32(40 / (Obj.speed)));
                        }
                        else if (pictureBox1.Left > 252)
                        {
                            pictureBox1.Left -= 10;
                            Delay(Convert.ToInt32(40 / (Obj.speed)));
                        }
                    }
                }
                if (PMLL2cnt > 1)
                {
                    PMLL2cnt -= 2;

                    for (int i = 1; i <= 10; i++)
                    {
                        ATMsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(ATMsrc, 180);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[1] = new Mat(@"..\Debug\LLpm\LL" + PMLL2cnt + ".png", ImreadModes.Unchanged);
                            pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                        }
                    }
                }
                else
                {
                    PMLL2cnt -= 1;

                    for (int i = 1; i <= 10; i++)
                    {
                        ATMsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(ATMsrc, 180);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[1] = new Mat(@"..\Debug\LLpm\LL" + PMLL2cnt + ".png", ImreadModes.Unchanged);
                            pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                        }
                    }
                }
                if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    if (PMLL2cnt == 0)
                        PMLL2cnt = -1;
                }
                ATMinLPM3();
            }
        }

        private void PickLLtoATM3()
        {
            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new PictureBoxSafety(PickLLtoATM3));
            }
            else
            {
                Delay(300);
                if (PMLL3cnt == 0)
                    PMLL3cnt = Obj.EFEMobj.LLwafer;

                Mat ATMsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
                Mat dst = new Mat(ATMsrc.Size(), MatType.CV_8UC3);

                if (pictureBox1.Left != 252)
                {
                    while (pictureBox1.Left != 252)
                    {
                        if (pictureBox1.Left < 252)
                        {
                            pictureBox1.Left += 10;
                            Delay(Convert.ToInt32(40 / (Obj.speed)));
                        }
                        else if (pictureBox1.Left > 252)
                        {
                            pictureBox1.Left -= 10;
                            Delay(Convert.ToInt32(40 / (Obj.speed)));
                        }
                    }
                }
                if (PMLL3cnt > 1)
                {
                    PMLL3cnt -= 2;
                    for (int i = 1; i <= 10; i++)
                    {
                        ATMsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(ATMsrc, 180);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[2] = new Mat(@"..\Debug\LLpm\LL" + PMLL3cnt + ".png", ImreadModes.Unchanged);
                            pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                        }
                    }
                }
                else
                {
                    PMLL3cnt -= 1;
                    for (int i = 1; i <= 10; i++)
                    {
                        ATMsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(ATMsrc, 180);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[2] = new Mat(@"..\Debug\LLpm\LL" + PMLL3cnt + ".png", ImreadModes.Unchanged);
                            pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                        }
                    }
                }
                if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    if (PMLL3cnt == 0)
                        PMLL3cnt = -1;
                }
                ATMinLPM3();
            }
        }

        private void PickLLtoATM4()
        {
            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new PictureBoxSafety(PickLLtoATM4));
            }
            else
            {
                Delay(300);
                if (PMLL4cnt == 0)
                    PMLL4cnt = Obj.EFEMobj.LLwafer;

                Mat ATMsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
                Mat dst = new Mat(ATMsrc.Size(), MatType.CV_8UC3);
                if (pictureBox1.Left != 252)
                {
                    while (pictureBox1.Left != 252)
                    {
                        if (pictureBox1.Left < 252)
                        {
                            pictureBox1.Left += 10;
                            Delay(Convert.ToInt32(40 / (Obj.speed)));
                        }
                        else if (pictureBox1.Left > 252)
                        {
                            pictureBox1.Left -= 10;
                            Delay(Convert.ToInt32(40 / (Obj.speed)));
                        }
                    }
                }
                if (PMLL4cnt > 1)
                {
                    PMLL4cnt -= 2;
                    for (int i = 1; i <= 10; i++)
                    {
                        ATMsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(ATMsrc, 180);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[3] = new Mat(@"..\Debug\LLpm\LL" + PMLL4cnt + ".png", ImreadModes.Unchanged);
                            pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                        }
                    }
                }
                else
                {
                    PMLL4cnt -= 1;
                    for (int i = 1; i <= 10; i++)
                    {
                        ATMsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(ATMsrc, 180);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[3] = new Mat(@"..\Debug\LLpm\LL" + PMLL4cnt + ".png", ImreadModes.Unchanged);
                            pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);

                        }
                    }
                }
                if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    if (PMLL4cnt == 0)
                        PMLL4cnt = -1;
                }
                ATMinLPM3();
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////

        //쿼드암
        private void VacPickQuadEndPM0()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            if (PM1cnt == Obj.EFEMobj.LLwafer)
            {
                PM1cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 360 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
            if ((Obj.EFEMobj.LLwafer - PM1cnt) > 2)
            {
                PM1cnt += 2;
                if (PM1cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }

                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM1cnt == 2)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM1cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 15 * j);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }

                    }
                }
                PM1cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM1cnt == 4)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM1cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 15 * j);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM1cnt) == 2 && Obj.EFEMobj.LLwafer == 6)
            {
                PM1cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM1cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        Delay(10);
                    }
                }

            }

            if (PM1cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }

            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 270 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }

        private void VacPickQuadEndPM1()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            if (PM2cnt == Obj.EFEMobj.LLwafer)
            {
                PM2cnt = 0;
            }

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if ((Obj.EFEMobj.LLwafer - PM2cnt) > 2)
            {
                PM2cnt += 2;
                if (PM2cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM2cnt == 2)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM2cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 15 * j);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
                PM2cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM2cnt == 4)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM2cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 15 * j);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM2cnt) == 2 && Obj.EFEMobj.LLwafer == 6)
            {
                PM2cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM2cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        Delay(10);
                    }
                }
            }
            if (PM2cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 180 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }

        private void VacPickQuadEndPM2()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            if (PM3cnt == Obj.EFEMobj.LLwafer)
            {
                PM3cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if ((Obj.EFEMobj.LLwafer - PM3cnt) > 2)
            {
                PM3cnt += 2;
                if (PM3cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM3cnt == 2)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM3cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 15 * j);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
                PM3cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM3cnt == 4)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM3cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 15 * j);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM3cnt) == 2 && Obj.EFEMobj.LLwafer == 6)
            {
                PM3cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM3cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        Delay(10);
                    }
                }
            }
            if (PM3cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 90 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));

            }
        }

        private void VacPickQuadEndPM3()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            if (PM4cnt == Obj.EFEMobj.LLwafer)
            {
                PM4cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 360 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
            if ((Obj.EFEMobj.LLwafer - PM4cnt) > 2)
            {
                PM4cnt += 2;
                if (PM4cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }

                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM4cnt == 2)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM4cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 15 * j);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }

                    }
                }
                PM4cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM4cnt == 4)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM4cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 15 * j);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM4cnt) == 2 && Obj.EFEMobj.LLwafer == 6)
            {
                PM4cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM4cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        Delay(10);
                    }
                }

            }

            if (PM5cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }

            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 270 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }

        private void VacPickQuadEndPM4()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;


            if (PM5cnt == Obj.EFEMobj.LLwafer)
            {
                PM5cnt = 0;
            }

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if ((Obj.EFEMobj.LLwafer - PM5cnt) > 2)
            {
                PM5cnt += 2;
                if (PM5cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM5cnt == 2)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM5cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 15 * j);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
                PM5cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM5cnt == 4)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM5cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 15 * j);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM5cnt) == 2 && Obj.EFEMobj.LLwafer == 6)
            {
                PM5cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM5cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        Delay(10);
                    }
                }
            }
            if (PM5cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 180 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }

        private void VacPickQuadEndPM5()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            if (PM6cnt == Obj.EFEMobj.LLwafer)
            {
                PM6cnt = 0;
            }


            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if ((Obj.EFEMobj.LLwafer - PM6cnt) > 2)
            {
                PM6cnt += 2;
                if (PM6cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM6cnt == 2)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM6cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 15 * j);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
                PM6cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM6cnt == 4)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM6cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 15 * j);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM6cnt) == 2 && Obj.EFEMobj.LLwafer == 6)
            {
                PM6cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM6cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        Delay(10);
                    }
                }
            }
            if (PM6cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 90 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));

            }
        }

        private void VacPickQuadEndPM6()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;


            if (PM7cnt == Obj.EFEMobj.LLwafer)
            {
                PM7cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 360 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
            if ((Obj.EFEMobj.LLwafer - PM7cnt) > 2)
            {
                PM7cnt += 2;
                if (PM7cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }

                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM7cnt == 2)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM7cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 15 * j);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }

                    }
                }
                PM7cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM7cnt == 4)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM7cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 15 * j);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM7cnt) == 2 && Obj.EFEMobj.LLwafer == 6)
            {
                PM7cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM7cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        Delay(10);
                    }
                }

            }

            if (PM7cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }

            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 270 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }

        private void VacPickQuadEndPM7()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            if (PM8cnt == Obj.EFEMobj.LLwafer)
            {
                PM8cnt = 0;
            }

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if ((Obj.EFEMobj.LLwafer - PM8cnt) > 2)
            {
                PM8cnt += 2;
                if (PM8cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM8cnt == 2)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM8cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 15 * j);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
                PM8cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM8cnt == 4)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM8cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 15 * j);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM8cnt) == 2 && Obj.EFEMobj.LLwafer == 6)
            {
                PM8cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM8cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        Delay(10);
                    }
                }
            }
            if (PM8cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 180 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }

        private void VacPickQuadEndPM8()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            if (PM9cnt == Obj.EFEMobj.LLwafer)
            {
                PM9cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if ((Obj.EFEMobj.LLwafer - PM9cnt) > 2)
            {
                PM9cnt += 2;
                if (PM9cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM9cnt == 2)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM9cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 15 * j);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
                PM9cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM9cnt == 4)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM9cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 15 * j);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM9cnt) == 2 && Obj.EFEMobj.LLwafer == 6)
            {
                PM9cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM9cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        Delay(10);
                    }
                }
            }
            if (PM9cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 90 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));

            }
        }

        private void VacPickQuadEndPM9()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            if (PM10cnt == Obj.EFEMobj.LLwafer)
            {
                PM10cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 360 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
            if ((Obj.EFEMobj.LLwafer - PM10cnt) > 2)
            {
                PM10cnt += 2;
                if (PM10cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }

                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM10cnt == 2)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM10cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 15 * j);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }

                    }
                }
                PM10cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM10cnt == 4)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM10cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 15 * j);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM10cnt) == 2 && Obj.EFEMobj.LLwafer == 6)
            {
                PM10cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM10cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        Delay(10);
                    }
                }

            }

            if (PM10cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }

            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 270 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }

        private void VacPickQuadEndPM10()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            if (PM11cnt == Obj.EFEMobj.LLwafer)
            {
                PM11cnt = 0;
            }

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if ((Obj.EFEMobj.LLwafer - PM11cnt) > 2)
            {
                PM11cnt += 2;
                if (PM11cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM11cnt == 2)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM11cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 15 * j);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
                PM11cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM11cnt == 4)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM11cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 15 * j);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM11cnt) == 2 && Obj.EFEMobj.LLwafer == 6)
            {
                PM11cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM11cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        Delay(10);
                    }
                }
            }
            if (PM11cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 180 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }
        private void VacPickQuadEndPM11()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            if (PM12cnt == Obj.EFEMobj.LLwafer)
            {
                PM12cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if ((Obj.EFEMobj.LLwafer - PM12cnt) > 2)
            {
                PM12cnt += 2;
                if (PM12cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM12cnt == 2)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM12cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 15 * j);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
                PM12cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM12cnt == 4)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM12cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 15 * j);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM12cnt) == 2 && Obj.EFEMobj.LLwafer == 6)
            {
                PM12cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM12cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        Delay(10);
                    }
                }
            }
            if (PM12cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 90 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));

            }
        }


        ///////////////////////////////////////////////////////////////////////////////////
        private void ProcessEndPM0()
        {
            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PM1" + Obj.EFEMobj.LLwafer + ".png", ImreadModes.Unchanged);
            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                PMsrc[0] = Rotate(PMsrc[0], 120);
            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                PMsrc[0] = Rotate(PMsrc[0], 240);
            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
            Delay(10);
            if (Obj.EFEMobj.LLwafer == 3)
            {
                for (int i = 0; i < 4; i++)
                {
                    PMsrc[0] = Rotate(PMsrc[0], 360 - (i * 30));
                    if (i == 3)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PMimageC\PM1" + Obj.EFEMobj.LLwafer + ".png", ImreadModes.Unchanged);
                    }
                    pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                    Delay(20);
                }
            }
            else if (Obj.EFEMobj.LLwafer == 5)
            {
                for (int i = 0; i < 8; i++)
                {
                    PMsrc[0] = Rotate(PMsrc[0], 360 - (i * 30));
                    if (i == 7)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PMimageC\PM1" + Obj.EFEMobj.LLwafer + ".png", ImreadModes.Unchanged);
                    }
                    pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                    Delay(20);
                }
            }
            label17.Text = "0";
        }
        private void VacPickEndPM0()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            if (PM1cnt == Obj.EFEMobj.LLwafer)
            {
                PM1cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 360 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
            if ((Obj.EFEMobj.LLwafer - PM1cnt) > 1)
            {
                PM1cnt += 2;
                if (PM1cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM1cnt == 2)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM1cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 15 * j);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM1cnt == 4)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM1cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 15 * j);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM1cnt == 6)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM1cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                            Delay(10);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM1cnt) == 1)
            {
                PM1cnt += 1;

                if (PM1cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM1cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        Delay(10);
                    }
                }

            }
            if (PM1cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }

            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 270 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }

        //PM1
        private void ProcessEndPM1()
        {
            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PM2" + Obj.EFEMobj.LLwafer + ".png", ImreadModes.Unchanged);
            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                PMsrc[1] = Rotate(PMsrc[1], 120);
            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                PMsrc[1] = Rotate(PMsrc[1], 240);
            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
            Delay(10);
            if (Obj.EFEMobj.LLwafer == 3)
            {
                for (int i = 0; i < 4; i++)
                {
                    PMsrc[1] = Rotate(PMsrc[1], 360 - (i * 30));
                    if (i == 3)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PMimageC\PM2" + Obj.EFEMobj.LLwafer + ".png", ImreadModes.Unchanged);
                    }
                    pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                    Delay(20);
                }
            }
            else if (Obj.EFEMobj.LLwafer == 5)
            {
                for (int i = 0; i < 8; i++)
                {
                    PMsrc[1] = Rotate(PMsrc[1], 360 - (i * 30));
                    if (i == 7)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PMimageC\PM2" + Obj.EFEMobj.LLwafer + ".png", ImreadModes.Unchanged);
                    }
                    pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                    Delay(20);
                }
            }
            label18.Text = "0";
        }

        private void VacPickEndPM1()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            if (PM2cnt == Obj.EFEMobj.LLwafer)
            {
                PM2cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if ((Obj.EFEMobj.LLwafer - PM2cnt) > 1)
            {
                PM2cnt += 2;
                if (PM2cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM2cnt == 2)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM2cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 15 * j);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM2cnt == 4)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM2cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 15 * j);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM2cnt == 6)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM2cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                            Delay(10);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM2cnt) == 1)
            {
                PM2cnt += 1;
                if (PM2cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM2cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        Delay(10);
                    }
                }
            }
            if (PM2cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 180 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }


        ///////////////////////////////////////////////////////////////////////////////////


        //PM2
        private void ProcessEndPM2()
        {
            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PM3" + Obj.EFEMobj.LLwafer + ".png", ImreadModes.Unchanged);
            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                PMsrc[2] = Rotate(PMsrc[2], 120);
            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                PMsrc[2] = Rotate(PMsrc[2], 240);
            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
            Delay(10);

            if (Obj.EFEMobj.LLwafer == 3)
            {
                for (int i = 0; i < 4; i++)
                {
                    PMsrc[2] = Rotate(PMsrc[2], 360 - (i * 30));
                    if (i == 3)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PMimageC\PM3" + Obj.EFEMobj.LLwafer + ".png", ImreadModes.Unchanged);
                    }
                    pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                    Delay(20);
                }
            }
            else if (Obj.EFEMobj.LLwafer == 5)
            {
                for (int i = 0; i < 8; i++)
                {
                    PMsrc[2] = Rotate(PMsrc[2], 360 - (i * 30));
                    if (i == 7)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PMimageC\PM3" + Obj.EFEMobj.LLwafer + ".png", ImreadModes.Unchanged);
                    }
                    pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                    Delay(20);
                }
            }
            label19.Text = "0";
        }

        private void VacPickEndPM2()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            if (PM3cnt == Obj.EFEMobj.LLwafer)
            {
                PM3cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if ((Obj.EFEMobj.LLwafer - PM3cnt) > 1)
            {
                PM3cnt += 2;
                if (PM3cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM3cnt == 2)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM3cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 15 * j);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM3cnt == 4)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM3cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 15 * j);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM3cnt == 6)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM3cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                            Delay(10);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM3cnt) == 1)
            {
                PM3cnt += 1;
                if (PM3cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM3cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        Delay(10);
                    }
                }
            }
            if (PM3cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 90 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));

            }
        }

        private void VacPickEndPM3()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            if (PM4cnt == Obj.EFEMobj.LLwafer)
            {
                PM4cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 360 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
            if ((Obj.EFEMobj.LLwafer - PM4cnt) > 1)
            {
                PM4cnt += 2;
                if (PM4cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM4cnt == 2)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM4cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 15 * j);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM4cnt == 4)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM4cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 15 * j);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM4cnt == 6)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM4cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                            Delay(10);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM4cnt) == 1)
            {
                PM4cnt += 1;
                if (PM4cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM4cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        Delay(10);
                    }
                }
            }
            if (PM4cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 270 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }

        private void VacPickEndPM4()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            if (PM5cnt == Obj.EFEMobj.LLwafer)
            {
                PM5cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if ((Obj.EFEMobj.LLwafer - PM5cnt) > 1)
            {
                PM5cnt += 2;
                if (PM5cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }

                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM5cnt == 2)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM5cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 15 * j);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM5cnt == 4)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM5cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 15 * j);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM5cnt == 6)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM5cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                            Delay(10);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM5cnt) == 1)
            {
                PM5cnt += 1;
                if (PM5cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM5cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        Delay(10);
                    }
                }
            }
            if (PM5cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 180 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }

        private void VacPickEndPM5()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            if (PM6cnt == Obj.EFEMobj.LLwafer)
            {
                PM6cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if ((Obj.EFEMobj.LLwafer - PM6cnt) > 1)
            {
                PM6cnt += 2;
                if (PM6cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM6cnt == 2)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM6cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 15 * j);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM6cnt == 4)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM6cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 15 * j);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM6cnt == 6)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM6cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                            Delay(10);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM6cnt) == 1)
            {
                PM6cnt += 1;
                if (PM6cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM6cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        Delay(10);
                    }
                }
            }
            if (PM6cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 90 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));

            }
        }

        private void VacPickEndPM6()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;


            if (PM7cnt == Obj.EFEMobj.LLwafer)
            {
                PM7cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 360 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
            if ((Obj.EFEMobj.LLwafer - PM7cnt) > 1)
            {
                PM7cnt += 2;
                if (PM7cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM7cnt == 2)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM7cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 15 * j);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM7cnt == 4)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM7cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 15 * j);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM7cnt == 6)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM7cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                            Delay(10);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM7cnt) == 1)
            {
                PM7cnt += 1;
                if (PM7cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM7cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        Delay(10);
                    }
                }
            }
            if (PM7cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 270 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }

        private void VacPickEndPM7()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            if (PM8cnt == Obj.EFEMobj.LLwafer)
            {
                PM8cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if ((Obj.EFEMobj.LLwafer - PM8cnt) > 1)
            {
                PM8cnt += 2;
                if (PM8cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM8cnt == 2)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM8cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 15 * j);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM8cnt == 4)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM8cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 15 * j);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM8cnt == 6)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM8cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                            Delay(10);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM8cnt) == 1)
            {
                PM8cnt += 1;
                if (PM8cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM8cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        Delay(10);
                    }
                }
            }
            if (PM8cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 180 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }

        private void VacPickEndPM8()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            if (PM9cnt == Obj.EFEMobj.LLwafer)
            {
                PM9cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if ((Obj.EFEMobj.LLwafer - PM9cnt) > 1)
            {
                PM9cnt += 2;
                if (PM9cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM9cnt == 2)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM9cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 15 * j);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM9cnt == 4)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM9cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 15 * j);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM9cnt == 6)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM9cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                            Delay(10);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM9cnt) == 1)
            {
                PM9cnt += 1;
                if (PM9cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM9cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        Delay(10);
                    }
                }
            }
            if (PM9cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 90 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));

            }
        }

        private void VacPickEndPM9()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            if (PM10cnt == Obj.EFEMobj.LLwafer)
            {
                PM10cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 360 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
            if ((Obj.EFEMobj.LLwafer - PM10cnt) > 1)
            {
                PM10cnt += 2;
                if (PM10cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM10cnt == 2)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM10cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 15 * j);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM10cnt == 4)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM10cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 15 * j);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM10cnt == 6)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM10cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                            Delay(10);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM10cnt) == 1)
            {
                PM10cnt += 1;
                if (PM10cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PMimageC\PMC1" + (Obj.EFEMobj.LLwafer - PM10cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        Delay(10);
                    }
                }
            }
            if (PM10cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }

            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 270 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }

        private void VacPickEndPM10()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            if (PM11cnt == Obj.EFEMobj.LLwafer)
            {
                PM11cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if ((Obj.EFEMobj.LLwafer - PM11cnt) > 1)
            {
                PM11cnt += 2;
                if (PM11cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM11cnt == 2)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM11cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 15 * j);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM11cnt == 4)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM11cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 15 * j);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM11cnt == 6)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM11cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                            Delay(10);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM11cnt) == 1)
            {
                PM11cnt += 1;
                if (PM11cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PMimageC\PMC2" + (Obj.EFEMobj.LLwafer - PM11cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        Delay(10);
                    }
                }
            }
            if (PM11cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 180 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }

        private void VacPickEndPM11()
        {
            VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot1.png", ImreadModes.Unchanged);
            Mat dst = new Mat(VACsrc.Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            if (PM12cnt == Obj.EFEMobj.LLwafer)
            {
                PM12cnt = 0;
            }

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if ((Obj.EFEMobj.LLwafer - PM12cnt) > 1)
            {
                PM12cnt += 2;
                if (PM12cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        Mat dst1 = new Mat(src.Size(), MatType.CV_8UC3);
                        if (PM12cnt == 2)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM12cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);

                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 15 * j);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM12cnt == 4)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM12cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                            for (int j = 1; j <= 8; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 15 * j);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                        if (PM12cnt == 6)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM12cnt) + ".png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                            Delay(10);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM12cnt) == 1)
            {
                PM12cnt += 1;
                if (PM12cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PMimageC\PMC3" + (Obj.EFEMobj.LLwafer - PM12cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        Delay(10);
                    }
                }
            }
            if (PM12cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 90 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));

            }
        }
        
        //쿼드암

        private void PlaceQuadVACtoLL1()
        {
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM1cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLpm\LL2.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                    }
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLpm\LL4.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                    }
                }
            }
            if (Obj.EFEMobj.LLwafer - PM1cnt == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLpm\LL6.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                    }
                }
            }
            if (PM1cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceQuadVACtoLL2()
        {
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM2cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLpm\LL2.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);

                    }
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLpm\LL4.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);

                    }
                }
            }
            if (Obj.EFEMobj.LLwafer - PM2cnt == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLpm\LL6.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                    }
                }
            }
            if (PM2cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceQuadVACtoLL3()
        {
            Mat SlotSrc;
            Mat SlotSrc1;

            if (PM3cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLpm\LL2.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);

                    }
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLpm\LL4.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);

                    }
                }
            }
            if (Obj.EFEMobj.LLwafer - PM3cnt == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLpm\LL6.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                    }
                }
            }
            if (PM3cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceQuadVACtoLL4()
        {
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM4cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLpm\LL2.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);

                    }
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLpm\LL4.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);

                    }
                }
            }

            if (Obj.EFEMobj.LLwafer - PM4cnt == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLpm\LL6.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                    }
                }
            }
            if (PM4cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceQuadVACtoLL5()
        {
            Mat SlotSrc;
            Mat SlotSrc1;

            if (PM5cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLpm\LL2.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                    }
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLpm\LL4.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                    }
                }
            }
            if (Obj.EFEMobj.LLwafer - PM5cnt == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLpm\LL6.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                    }
                }
            }
            if (PM5cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceQuadVACtoLL6()
        {
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM6cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLpm\LL2.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);

                    }
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLpm\LL4.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);

                    }
                }
            }
            if (Obj.EFEMobj.LLwafer - PM6cnt == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLpm\LL6.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                    }
                }
            }
            if (PM6cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceQuadVACtoLL7()
        {
            Mat SlotSrc;
            Mat SlotSrc1;

            if (PM7cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLpm\LL2.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);

                    }
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLpm\LL4.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);

                    }
                }
            }
            if (Obj.EFEMobj.LLwafer - PM7cnt == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLpm\LL6.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                    }
                }
            }
            if (PM7cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceQuadVACtoLL8()
        {
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM8cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLpm\LL2.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);

                    }
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLpm\LL4.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);

                    }
                }
            }
            if (Obj.EFEMobj.LLwafer - PM8cnt == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLpm\LL6.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                    }
                }
            }
            if (PM8cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceQuadVACtoLL9()
        {
            Mat SlotSrc;
            Mat SlotSrc1;

            if (PM9cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLpm\LL2.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                    }
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLpm\LL4.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                    }
                }
            }
            if (Obj.EFEMobj.LLwafer - PM9cnt == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLpm\LL6.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                    }
                }
            }
            if (PM9cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceQuadVACtoLL10()
        {
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM10cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLpm\LL2.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);

                    }
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLpm\LL4.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);

                    }
                }
            }
            if (Obj.EFEMobj.LLwafer - PM10cnt == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLpm\LL6.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                    }
                }
            }
            if (PM10cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceQuadVACtoLL11()
        {
            Mat SlotSrc;
            Mat SlotSrc1;

            if (PM11cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLpm\LL2.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);

                    }
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLpm\LL4.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);

                    }
                }
            }
            if (Obj.EFEMobj.LLwafer - PM11cnt == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLpm\LL6.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                    }
                }
            }
            if (PM11cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceQuadVACtoLL12()
        {
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM12cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLpm\LL2.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);

                    }
                }
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLpm\LL4.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);

                    }
                }
            }
            if (Obj.EFEMobj.LLwafer - PM12cnt == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLpm\LL6.png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                    }
                }
            }
            if (PM12cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////
        #region LL4개 PlaceVACtoLL
        private void PlaceVACtoLL1()
        {
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM1cnt == 1 || PM1cnt == 2)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM1cnt) > 1)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLpm\LL" + PM1cnt + ".png", ImreadModes.Unchanged);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                    }
                }
            }
            else
            {
                if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[0] = new Mat(@"..\Debug\LLpm\LL" + PM1cnt + ".png", ImreadModes.Unchanged);
                            pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        if (i == 5)
                        {
                            LLsrc[0] = new Mat(@"..\Debug\LLpm\LL" + PM1cnt + ".png", ImreadModes.Unchanged);
                            pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                        }
                    }
                }
            }
            if (PM1cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }

        private void PlaceVACtoLL2()
        {
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM2cnt == 1 || PM2cnt == 2)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM2cnt) > 1)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLpm\LL" + PM2cnt + ".png", ImreadModes.Unchanged);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);

                    }
                }
            }
            else
            {
                if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[1] = new Mat(@"..\Debug\LLpm\LL" + PM2cnt + ".png", ImreadModes.Unchanged);
                            pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);

                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        if (i == 5)
                        {
                            LLsrc[1] = new Mat(@"..\Debug\LLpm\LL" + PM2cnt + ".png", ImreadModes.Unchanged);
                            pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);

                        }
                    }
                }
            }
            if (PM2cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceVACtoLL3()
        {
            Mat SlotSrc;
            Mat SlotSrc1;

            if (PM3cnt == 1 || PM3cnt == 2)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM3cnt) > 1)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLpm\LL" + PM3cnt + ".png", ImreadModes.Unchanged);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);

                    }
                }
            }
            else
            {
                if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[2] = new Mat(@"..\Debug\LLpm\LL" + PM3cnt + ".png", ImreadModes.Unchanged);
                            pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);

                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        if (i == 5)
                        {
                            LLsrc[2] = new Mat(@"..\Debug\LLpm\LL" + PM3cnt + ".png", ImreadModes.Unchanged);
                            pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                        }
                    }
                }
            }
            if (PM3cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceVACtoLL4()
        {
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM4cnt == 1 || PM4cnt == 2)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM4cnt) > 1)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLpm\LL" + PM4cnt + ".png", ImreadModes.Unchanged);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);

                    }
                }
            }
            else
            {
                if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[3] = new Mat(@"..\Debug\LLpm\LL" + PM4cnt + ".png", ImreadModes.Unchanged);
                            pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);

                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        if (i == 5)
                        {
                            LLsrc[3] = new Mat(@"..\Debug\LLpm\LL" + PM4cnt + ".png", ImreadModes.Unchanged);
                            pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);

                        }
                    }
                }
            }
            if (PM4cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceVACtoLL5()
        {
            Mat SlotSrc;
            Mat SlotSrc1;

            if (PM5cnt == 1 || PM5cnt == 2)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM5cnt) > 1)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLpm\LL" + PM5cnt + ".png", ImreadModes.Unchanged);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                    }
                }
            }
            else
            {
                if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[0] = new Mat(@"..\Debug\LLpm\LL" + PM5cnt + ".png", ImreadModes.Unchanged);
                            pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        if (i == 5)
                        {
                            LLsrc[0] = new Mat(@"..\Debug\LLpm\LL" + PM5cnt + ".png", ImreadModes.Unchanged);
                            pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                        }
                    }
                }
            }
            if (PM5cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceVACtoLL6()
        {
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM6cnt == 1 || PM6cnt == 2)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM6cnt) > 1)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLpm\LL" + PM6cnt + ".png", ImreadModes.Unchanged);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);

                    }
                }
            }
            else
            {
                if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[1] = new Mat(@"..\Debug\LLpm\LL" + PM6cnt + ".png", ImreadModes.Unchanged);
                            pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);

                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        if (i == 5)
                        {
                            LLsrc[1] = new Mat(@"..\Debug\LLpm\LL" + PM6cnt + ".png", ImreadModes.Unchanged);
                            pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);

                        }
                    }
                }
            }
            if (PM6cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceVACtoLL7()
        {
            Mat SlotSrc;
            Mat SlotSrc1;

            if (PM7cnt == 1 || PM7cnt == 2)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM7cnt) > 1)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLpm\LL" + PM7cnt + ".png", ImreadModes.Unchanged);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);

                    }
                }
            }
            else
            {
                if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[2] = new Mat(@"..\Debug\LLpm\LL" + PM7cnt + ".png", ImreadModes.Unchanged);
                            pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);

                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        if (i == 5)
                        {
                            LLsrc[2] = new Mat(@"..\Debug\LLpm\LL" + PM7cnt + ".png", ImreadModes.Unchanged);
                            pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                        }
                    }
                }
            }
            if (PM7cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceVACtoLL8()
        {
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM8cnt == 1 || PM8cnt == 2)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM8cnt) > 1)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLpm\LL" + PM8cnt + ".png", ImreadModes.Unchanged);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);

                    }
                }
            }
            else
            {
                if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[3] = new Mat(@"..\Debug\LLpm\LL" + PM8cnt + ".png", ImreadModes.Unchanged);
                            pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);

                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        if (i == 5)
                        {
                            LLsrc[3] = new Mat(@"..\Debug\LLpm\LL" + PM8cnt + ".png", ImreadModes.Unchanged);
                            pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);

                        }
                    }
                }
            }
            if (PM8cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }

        private void PlaceVACtoLL9()
        {
            Mat SlotSrc;
            Mat SlotSrc1;

            if (PM9cnt == 1 || PM9cnt == 2)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM9cnt) > 1)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLpm\LL" + PM9cnt + ".png", ImreadModes.Unchanged);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                    }
                }
            }
            else
            {
                if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[0] = new Mat(@"..\Debug\LLpm\LL" + PM9cnt + ".png", ImreadModes.Unchanged);
                            pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        if (i == 5)
                        {
                            LLsrc[0] = new Mat(@"..\Debug\LLpm\LL" + PM9cnt + ".png", ImreadModes.Unchanged);
                            pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                        }
                    }
                }
            }

            if (PM9cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }

        private void PlaceVACtoLL10()
        {
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM10cnt == 1 || PM10cnt == 2)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM10cnt) > 1)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLpm\LL" + PM10cnt + ".png", ImreadModes.Unchanged);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);

                    }
                }
            }
            else
            {
                if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[1] = new Mat(@"..\Debug\LLpm\LL" + PM10cnt + ".png", ImreadModes.Unchanged);
                            pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);

                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        if (i == 5)
                        {
                            LLsrc[1] = new Mat(@"..\Debug\LLpm\LL" + PM10cnt + ".png", ImreadModes.Unchanged);
                            pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);

                        }
                    }
                }
            }
            if (PM10cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceVACtoLL11()
        {
            Mat SlotSrc;
            Mat SlotSrc1;

            if (PM11cnt == 1 || PM11cnt == 2)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM11cnt) > 1)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLpm\LL" + PM11cnt + ".png", ImreadModes.Unchanged);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);

                    }
                }
            }
            else
            {
                if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[2] = new Mat(@"..\Debug\LLpm\LL" + PM11cnt + ".png", ImreadModes.Unchanged);
                            pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);

                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        if (i == 5)
                        {
                            LLsrc[2] = new Mat(@"..\Debug\LLpm\LL" + PM11cnt + ".png", ImreadModes.Unchanged);
                            pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                        }
                    }
                }
            }
            if (PM11cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        private void PlaceVACtoLL12()
        {
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM12cnt == 1 || PM12cnt == 2)
            {
                for (int i = 1; i <= 4; i++)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM12cnt) > 1)
            {
                for (int i = 10; i >= 1; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLpm\LL" + PM12cnt + ".png", ImreadModes.Unchanged);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);

                    }
                }
            }
            else
            {
                if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobotC\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        // LL 2개 뽑음
                        if (i == 5)
                        {
                            LLsrc[3] = new Mat(@"..\Debug\LLpm\LL" + PM12cnt + ".png", ImreadModes.Unchanged);
                            pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);

                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    for (int i = 10; i >= 1; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACC1\robot" + i + ".png", ImreadModes.Unchanged);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                        if (i == 5)
                        {
                            LLsrc[3] = new Mat(@"..\Debug\LLpm\LL" + PM12cnt + ".png", ImreadModes.Unchanged);
                            pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);

                        }
                    }
                }
            }
            if (PM12cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }
        #endregion
        ///////////////////////////////////////////////////////////////////////////////////        
        private void LLVent1()
        {
            if (LL1cnt == 0)
            {
                LLsrc[0] = new Mat(@"..\Debug\LLPm\LL" + Obj.EFEMobj.LLwafer + ".png", ImreadModes.Unchanged);
                for (int i = (Obj.EFEMobj.nLLVentStabTime + Obj.EFEMobj.nLLVentStabTime); i >= 0; i--)
                {
                    Delay(1000 / Obj.speed);
                    label20.Text = i.ToString();
                }
                pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);
            }
        }

        private void LLVent2()
        {
            if (LL2cnt == 0)
            {
                LLsrc[1] = new Mat(@"..\Debug\LLPm\LL" + Obj.EFEMobj.LLwafer + ".png", ImreadModes.Unchanged);
                for (int i = (Obj.EFEMobj.nLLVentStabTime + Obj.EFEMobj.nLLVentStabTime); i >= 0; i--)
                {
                    Delay(1000 / Obj.speed);
                    label21.Text = i.ToString();

                }
                pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
            }
        }

        private void LLVent3()
        {
            if (LL3cnt ==0)
            {
                LLsrc[2] = new Mat(@"..\Debug\LLPm\LL" + Obj.EFEMobj.LLwafer + ".png", ImreadModes.Unchanged);
                for (int i = (Obj.EFEMobj.nLLVentStabTime + Obj.EFEMobj.nLLVentStabTime); i >= 0; i--)
                {
                    Delay(1000 / Obj.speed);
                    label22.Text = i.ToString();

                }
                pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
            }

        }

        private void LLVent4()
        {


            if (LL4cnt == 0)
            {
                LLsrc[3] = new Mat(@"..\Debug\LLPm\LL" + Obj.EFEMobj.LLwafer + ".png", ImreadModes.Unchanged);
                for (int i = (Obj.EFEMobj.nLLVentStabTime + Obj.EFEMobj.nLLVentStabTime); i >= 0; i--)
                {
                    Delay(1000 / Obj.speed);
                    label23.Text = i.ToString();

                }
                pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
            }

        }


        #region 쿼드암
        private void VacPickQuadPM1()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM1cnt >= Obj.EFEMobj.LLwafer)
            {
                PM1cnt = 0;
            }

            if (Obj.EFEMobj.LLwafer - PM1cnt > 2)
            {
                PM1cnt += 2;
                if (PM1cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }

                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM1cnt) + ".png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);
                    }
                }
                PM1cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM1cnt) + ".png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);
                    }
                }
            }
            else if (Obj.EFEMobj.LLwafer - PM1cnt == 2)
            {
                PM1cnt += 2;

                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM1cnt) + ".png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);
                    }
                }
            }
            if (PM1cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }

        private void VacPlaceQuad1()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[0].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 360 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            // PM1 PICK
            if (PM1cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PM12.png", ImreadModes.Unchanged);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                    }

                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[0], 10 * j);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PM14.png", ImreadModes.Unchanged);
                        PMsrc[0] = Rotate(PMsrc[0], 120);
                        Delay(10);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                    }
                    if (i == 1)
                    {
                        if (Obj.EFEMobj.LLwafer == 6)
                        {
                            for (int j = 1; j <= 12; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 10 * j);
                                Delay(10);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                            }
                        }
                    }
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM1cnt) == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PM16.png", ImreadModes.Unchanged);
                        PMsrc[0] = Rotate(PMsrc[0], 240);
                        Delay(10);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                    }
                }
            }

            if (PM1cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 270 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));

            }
        }

        private void VacPickQuadPM2()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM2cnt >= Obj.EFEMobj.LLwafer)
            {
                PM2cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM2cnt) > 2)
            {
                PM2cnt += 2;
                if (PM2cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM2cnt) + ".png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                    }
                }
                PM2cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM2cnt) + ".png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                    }
                }
            }
            else if (Obj.EFEMobj.LLwafer - PM2cnt == 2)
            {
                PM2cnt += 2;

                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM2cnt) + ".png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                    }
                }
            }
            if (PM2cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }

        private void VacPlaceQuad2()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[1].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            // 회전 180도

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
            if (PM2cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PM22.png", ImreadModes.Unchanged);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[1], 10 * j);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PM24.png", ImreadModes.Unchanged);
                        PMsrc[1] = Rotate(PMsrc[1], 120);
                        Delay(10);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                    }
                    if (i == 1)
                    {
                        if (Obj.EFEMobj.LLwafer == 6)
                        {
                            for (int j = 1; j <= 12; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 10 * j);
                                Delay(10);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                            }
                        }
                    }
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM2cnt) == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PM26.png", ImreadModes.Unchanged);
                        PMsrc[1] = Rotate(PMsrc[1], 240);
                        Delay(10);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                    }
                }
            }

            if (PM2cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }


            // 회전 180도
            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 180 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }

        private void VacPickQuadPM3()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM3cnt >= Obj.EFEMobj.LLwafer)
            {
                PM3cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM3cnt) > 2)
            {
                PM3cnt += 2;
                if (PM3cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM3cnt) + ".png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                    }
                }
                PM3cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM3cnt) + ".png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM3cnt) == 2)
            {
                PM3cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 1개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM3cnt) + ".png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                    }
                }
            }
            if (PM3cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }

        private void VacPlaceQuad3()
        {

            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[2].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if (PM3cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PM32.png", ImreadModes.Unchanged);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            Delay(10);
                            dst1 = Rotate(PMsrc[2], 10 * j);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PM34.png", ImreadModes.Unchanged);
                        PMsrc[2] = Rotate(PMsrc[2], 120);
                        Delay(10);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                    }
                    if (i == 1)
                    {
                        if (Obj.EFEMobj.LLwafer == 6)
                        {
                            for (int j = 1; j <= 12; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 10 * j);
                                Delay(10);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                            }
                        }
                    }
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM3cnt) == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PM36.png", ImreadModes.Unchanged);
                        PMsrc[2] = Rotate(PMsrc[2], 240);
                        Delay(10);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                    }
                }
            }

            if (PM3cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 90 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));

            }
        }

        private void VacPickQuadPM4()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM4cnt >= Obj.EFEMobj.LLwafer)
            {
                PM4cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM4cnt) > 2)
            {
                PM4cnt += 2;
                if (PM4cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM4cnt) + ".png", ImreadModes.Unchanged);
                        Delay(5);

                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                    }
                }
                PM4cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM4cnt) + ".png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM4cnt) == 2)
            {
                PM4cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM4cnt) + ".png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                    }
                }
            }
            if (PM4cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }

        private void VacPlaceQuad4()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[0].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 360 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            // PM1 PICK
            if (PM4cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PM12.png", ImreadModes.Unchanged);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[0], 10 * j);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PM14.png", ImreadModes.Unchanged);
                        PMsrc[0] = Rotate(PMsrc[0], 120);
                        Delay(10);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                    }
                    if (i == 1)
                    {
                        if (Obj.EFEMobj.LLwafer == 6)
                        {
                            for (int j = 1; j <= 12; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 10 * j);
                                Delay(10);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                            }
                        }
                    }
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM4cnt) == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PM16.png", ImreadModes.Unchanged);
                        Delay(10);
                        PMsrc[0] = Rotate(PMsrc[0], 240);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                    }
                }
            }
            if (PM4cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 270 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));

            }
        }

        private void VacPickQuadPM5()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM5cnt >= Obj.EFEMobj.LLwafer)
            {
                PM5cnt = 0;
            }

            if (Obj.EFEMobj.LLwafer - PM5cnt > 2)
            {
                PM5cnt += 2;
                if (PM5cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }

                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM5cnt) + ".png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);
                    }
                }
                PM5cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM5cnt) + ".png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);
                    }
                }
            }
            else if (Obj.EFEMobj.LLwafer - PM5cnt == 2)
            {
                PM5cnt += 2;

                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM5cnt) + ".png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);
                    }
                }
            }
            if (PM5cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }

        private void VacPlaceQuad5()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[1].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            // 회전 180도

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
            if (PM5cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PM22.png", ImreadModes.Unchanged);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[1], 10 * j);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PM24.png", ImreadModes.Unchanged);
                        PMsrc[1] = Rotate(PMsrc[1], 120);
                        Delay(10);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                    }
                    if (i == 1)
                    {
                        if (Obj.EFEMobj.LLwafer == 6)
                        {
                            for (int j = 1; j <= 12; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 10 * j);
                                Delay(10);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                            }
                        }
                    }
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM5cnt) == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PM26.png", ImreadModes.Unchanged);
                        PMsrc[1] = Rotate(PMsrc[1], 240);
                        Delay(10);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                    }
                }
            }

            if (PM5cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }


            // 회전 180도
            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 180 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }

        private void VacPickQuadPM6()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM6cnt >= Obj.EFEMobj.LLwafer)
            {
                PM6cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM6cnt) > 2)
            {
                PM6cnt += 2;
                if (PM6cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM6cnt) + ".png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                    }
                }
                PM6cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM6cnt) + ".png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                    }
                }
            }
            else if (Obj.EFEMobj.LLwafer - PM6cnt == 2)
            {
                PM6cnt += 2;

                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM6cnt) + ".png", ImreadModes.Unchanged);
                        Delay(5);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                    }
                }
            }
            if (PM6cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }

        private void VacPlaceQuad6()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[2].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if (PM6cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PM32.png", ImreadModes.Unchanged);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[2], 10 * j);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PM34.png", ImreadModes.Unchanged);
                        PMsrc[2] = Rotate(PMsrc[2], 120);
                        Delay(10);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                    }
                    if (i == 1)
                    {
                        if (Obj.EFEMobj.LLwafer == 6)
                        {
                            for (int j = 1; j <= 12; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 10 * j);
                                Delay(10);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                            }
                        }
                    }
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM6cnt) == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PM36.png", ImreadModes.Unchanged);
                        PMsrc[2] = Rotate(PMsrc[2], 240);
                        Delay(10);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                    }
                }
            }

            if (PM6cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 90 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));

            }
        }

        private void VacPickQuadPM7()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM7cnt >= Obj.EFEMobj.LLwafer)
            {
                PM7cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM7cnt) > 2)
            {
                PM7cnt += 2;
                if (PM7cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM7cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                    }
                }
                PM7cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM7cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM7cnt) == 2)
            {
                PM7cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 1개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM7cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                    }
                }
            }
            if (PM7cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }

        private void VacPlaceQuad7()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[0].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 360 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            // PM1 PICK
            if (PM7cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PM12.png", ImreadModes.Unchanged);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                    }

                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[0], 10 * j);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PM14.png", ImreadModes.Unchanged);
                        PMsrc[0] = Rotate(PMsrc[0], 120);
                        Delay(10);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                    }
                    if (i == 1)
                    {
                        if (Obj.EFEMobj.LLwafer == 6)
                        {
                            for (int j = 1; j <= 12; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 10 * j);
                                Delay(10);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                            }
                        }
                    }
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM7cnt) == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PM16.png", ImreadModes.Unchanged);
                        PMsrc[0] = Rotate(PMsrc[0], 240);
                        Delay(10);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                    }
                }
            }

            if (PM7cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 270 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));

            }
        }

        private void VacPickQuadPM8()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM8cnt >= Obj.EFEMobj.LLwafer)
            {
                PM8cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM8cnt) > 2)
            {
                PM8cnt += 2;
                if (PM8cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM8cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                    }
                }
                PM8cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM8cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM8cnt) == 2)
            {
                PM8cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM8cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                    }
                }
            }
            if (PM8cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }

        private void VacPlaceQuad8()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[1].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            // 회전 180도

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
            if (PM8cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PM22.png", ImreadModes.Unchanged);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[1], 10 * j);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PM24.png", ImreadModes.Unchanged);
                        PMsrc[1] = Rotate(PMsrc[1], 120);
                        Delay(10);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                    }
                    if (i == 1)
                    {
                        if (Obj.EFEMobj.LLwafer == 6)
                        {
                            for (int j = 1; j <= 12; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 10 * j);
                                Delay(10);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                            }
                        }
                    }
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM8cnt) == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PM26.png", ImreadModes.Unchanged);
                        PMsrc[1] = Rotate(PMsrc[1], 240);
                        Delay(10);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                    }
                }
            }

            if (PM8cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }


            // 회전 180도
            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 180 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }

        private void VacPickQuadPM9()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM9cnt >= Obj.EFEMobj.LLwafer)
            {
                PM9cnt = 0;
            }

            if (Obj.EFEMobj.LLwafer - PM9cnt > 2)
            {
                PM9cnt += 2;
                if (PM9cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }

                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM9cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);
                    }
                }
                PM9cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM9cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);
                    }
                }
            }
            else if (Obj.EFEMobj.LLwafer - PM9cnt == 2)
            {
                PM9cnt += 2;

                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM9cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);
                    }
                }
            }
            if (PM9cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }

        private void VacPlaceQuad9()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[2].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if (PM9cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PM32.png", ImreadModes.Unchanged);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[2], 10 * j);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PM34.png", ImreadModes.Unchanged);
                        PMsrc[2] = Rotate(PMsrc[2], 120);
                        Delay(10);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                    }
                    if (i == 1)
                    {
                        if (Obj.EFEMobj.LLwafer == 6)
                        {
                            for (int j = 1; j <= 12; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 10 * j);
                                Delay(10);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                            }
                        }
                    }
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM9cnt) == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PM36.png", ImreadModes.Unchanged);
                        PMsrc[2] = Rotate(PMsrc[2], 240);
                        Delay(10);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                    }
                }
            }

            if (PM9cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 90 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));

            }
        }

        private void VacPickQuadPM10()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM10cnt >= Obj.EFEMobj.LLwafer)
            {
                PM10cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM10cnt) > 2)
            {
                PM10cnt += 2;
                if (PM10cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM10cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                    }
                }
                PM10cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM10cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                    }
                }
            }
            else if (Obj.EFEMobj.LLwafer - PM10cnt == 2)
            {
                PM10cnt += 2;

                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM10cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                    }
                }
            }
            if (PM10cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }

        private void VacPlaceQuad10()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[0].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 360 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            // PM1 PICK
            if (PM10cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PM12.png", ImreadModes.Unchanged);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                    }

                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[0], 10 * j);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PM14.png", ImreadModes.Unchanged);
                        PMsrc[0] = Rotate(PMsrc[0], 120);
                        Delay(10);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                    }
                    if (i == 1)
                    {
                        if (Obj.EFEMobj.LLwafer == 6)
                        {
                            for (int j = 1; j <= 12; j++)
                            {
                                dst1 = Rotate(PMsrc[0], 10 * j);
                                pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                                Delay(10);
                            }
                        }
                    }
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM10cnt) == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[0] = new Mat(@"..\Debug\PM16.png", ImreadModes.Unchanged);
                        PMsrc[0] = Rotate(PMsrc[0], 240);
                        Delay(10);
                        pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                    }
                }
            }

            if (PM10cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 270 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));

            }
        }

        private void VacPickQuadPM11()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM11cnt >= Obj.EFEMobj.LLwafer)
            {
                PM11cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM11cnt) > 2)
            {
                PM11cnt += 2;
                if (PM11cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM11cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                    }
                }
                PM11cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM11cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM11cnt) == 2)
            {
                PM11cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 1개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM11cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                    }
                }
            }
            if (PM11cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }

        private void VacPlaceQuad11()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[1].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            // 회전 180도

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
            if (PM11cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PM22.png", ImreadModes.Unchanged);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[1], 10 * j);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PM24.png", ImreadModes.Unchanged);
                        PMsrc[1] = Rotate(PMsrc[1], 120);
                        Delay(10);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                    }
                    if (i == 1)
                    {
                        if (Obj.EFEMobj.LLwafer == 6)
                        {
                            for (int j = 1; j <= 12; j++)
                            {
                                dst1 = Rotate(PMsrc[1], 10 * j);
                                Delay(10);
                                pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                            }
                        }
                    }
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM11cnt) == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[1] = new Mat(@"..\Debug\PM26.png", ImreadModes.Unchanged);
                        PMsrc[1] = Rotate(PMsrc[1], 240);
                        pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                    }
                }
            }

            if (PM11cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }


            // 회전 180도
            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 180 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
        }

        private void VacPickQuadPM12()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM12cnt >= Obj.EFEMobj.LLwafer)
            {
                PM12cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM12cnt) > 2)
            {
                PM12cnt += 2;
                if (PM12cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM12cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                    }
                }
                PM12cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM12cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM12cnt) == 2)
            {
                PM12cnt += 2;
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM12cnt) + ".png", ImreadModes.Unchanged);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                    }
                }
            }
            if (PM12cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
        }

        private void VacPlaceQuad12()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[2].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            if (PM12cnt == 4)
            {
                for (int i = 1; i <= 4; i++)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PM32.png", ImreadModes.Unchanged);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[2], 10 * j);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                            Delay(10);
                        }
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));

                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PM34.png", ImreadModes.Unchanged);
                        PMsrc[2] = Rotate(PMsrc[2], 120);
                        Delay(10);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                    }
                    if (i == 1)
                    {
                        if (Obj.EFEMobj.LLwafer == 6)
                        {
                            for (int j = 1; j <= 12; j++)
                            {
                                dst1 = Rotate(PMsrc[2], 10 * j);
                                Delay(10);
                                pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                            }
                        }
                    }
                }
            }
            if ((Obj.EFEMobj.LLwafer - PM12cnt) == 0 && Obj.EFEMobj.LLwafer == 6)
            {
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        PMsrc[2] = new Mat(@"..\Debug\PM36.png", ImreadModes.Unchanged);
                        PMsrc[2] = Rotate(PMsrc[2], 240);
                        Delay(10);
                        pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                    }
                }
            }

            if (PM12cnt == Obj.EFEMobj.LLwafer)
            {
                for (int i = 4; i > 0; i--)
                {
                    PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                    pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 90 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));

            }
        }

        #endregion

        #region LL4개 VacPick or Place
        private void VacPickPM1()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM1cnt >= Obj.EFEMobj.LLwafer)
            {
                PM1cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM1cnt) > 1)
            {
                PM1cnt += 2;
                if (PM1cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM1cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);
                    }
                }
                if (PM1cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM1cnt) == 1)
            {
                PM1cnt += 1;
                if (PM1cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 1개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM1cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);
                    }
                }
                if (PM1cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
            }
        }


        private void VacPlace1()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[0].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 360 - (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }

            // PM1 PICK
            if ((Obj.EFEMobj.LLwafer - PM1cnt) > 0)
            {
                if (PM1cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    if (i == 5)
                    {
                        if (PM1cnt == 1)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM11.png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM1cnt == 2)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM12.png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM1cnt == 3)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM13.png", ImreadModes.Unchanged);
                            PMsrc[0] = Rotate(PMsrc[0], 120);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM1cnt == 4)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM14.png", ImreadModes.Unchanged);
                            PMsrc[0] = Rotate(PMsrc[0], 120);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM1cnt == 5)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM15.png", ImreadModes.Unchanged);
                            PMsrc[0] = Rotate(PMsrc[0], 240);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM1cnt == 6)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM16.png", ImreadModes.Unchanged);
                            PMsrc[0] = Rotate(PMsrc[0], 240);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[0], 10 * j);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }

            }

            else if ((Obj.EFEMobj.LLwafer - PM1cnt) == 0)
            {
                if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    if (PM1cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 270);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        if (i == 5)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM1" + PM1cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[0] = Rotate(PMsrc[0], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[0] = Rotate(PMsrc[0], 240);
                                Delay(10);
                            }
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    if (PM1cnt == 2)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 270);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        if (i == 5)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM1" + PM1cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[0] = Rotate(PMsrc[0], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[0] = Rotate(PMsrc[0], 240);
                                Delay(10);
                            }
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                    }
                }
                if (PM1cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 270 + (10 * i));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));

            }
        }

        private void VacPickPM2()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM2cnt >= Obj.EFEMobj.LLwafer)
            {
                PM2cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM2cnt) > 1)
            {
                PM2cnt += 2;
                if (PM2cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM2cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                    }
                }
                if (PM2cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM2cnt) == 1)
            {
                PM2cnt += 1;
                if (PM2cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 1개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM2cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                    }
                }
                if (PM2cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
            }
        }

        private void VacPlace2()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[1].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;
            // 회전 180도

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
            }
            if ((Obj.EFEMobj.LLwafer - PM2cnt) > 0)
            {
                if (PM2cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);

                    if (i == 5)
                    {
                        if (PM2cnt == 1)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM21.png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                        else if (PM2cnt == 2)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM22.png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                        else if (PM2cnt == 3)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM23.png", ImreadModes.Unchanged);
                            PMsrc[1] = Rotate(PMsrc[1], 120);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                        else if (PM2cnt == 4)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM24.png", ImreadModes.Unchanged);
                            PMsrc[1] = Rotate(PMsrc[1], 120);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                        else if (PM2cnt == 5)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM25.png", ImreadModes.Unchanged);
                            PMsrc[1] = Rotate(PMsrc[1], 240);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                        else if (PM2cnt == 6)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM26.png", ImreadModes.Unchanged);
                            PMsrc[1] = Rotate(PMsrc[1], 240);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[1], 10 * j);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM1cnt) == 0)
            {
                if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    if (PM2cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 180);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        if (i == 5)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM2" + PM2cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[1] = Rotate(PMsrc[1], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[1] = Rotate(PMsrc[1], 240);
                                Delay(10);
                            }
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    if (PM2cnt == 2)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 180);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM2" + PM2cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[1] = Rotate(PMsrc[1], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[1] = Rotate(PMsrc[1], 240);
                                Delay(10);
                            }
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                    }
                }
                if (PM2cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    }
                }
            }

            // 회전 180도
            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 180 + (10 * i));
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }
        }

        private void VacPickPM3()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM3cnt >= Obj.EFEMobj.LLwafer)
            {
                PM3cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM3cnt) > 1)
            {
                PM3cnt += 2;
                if (PM3cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM3cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                    }
                }
                if (PM3cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM3cnt) == 1)
            {
                PM3cnt += 1;
                if (PM3cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));

                    // LL 1개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM3cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                    }
                }
                if (PM3cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
            }
        }

        private void VacPlace3()
        {

            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[2].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, (10 * i));
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }


            if ((Obj.EFEMobj.LLwafer - PM3cnt) > 0)
            {
                if (PM3cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);

                    if (i == 5)
                    {
                        if (PM3cnt == 1)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM31.png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM3cnt == 2)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM32.png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM3cnt == 3)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM33.png", ImreadModes.Unchanged);
                            PMsrc[2] = Rotate(PMsrc[2], 120);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);

                        }
                        else if (PM3cnt == 4)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM34.png", ImreadModes.Unchanged);
                            PMsrc[2] = Rotate(PMsrc[2], 120);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM3cnt == 5)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM35.png", ImreadModes.Unchanged);
                            PMsrc[2] = Rotate(PMsrc[2], 240);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM3cnt == 6)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM36.png", ImreadModes.Unchanged);
                            PMsrc[2] = Rotate(PMsrc[2], 240);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[2], 10 * j);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM3cnt) == 0)
            {
                if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    if (PM3cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 90);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM3" + PM3cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[2] = Rotate(PMsrc[2], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[2] = Rotate(PMsrc[2], 240);
                                Delay(10);
                            }
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    if (PM3cnt == 2)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 90);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM3" + PM3cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[2] = Rotate(PMsrc[2], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[2] = Rotate(PMsrc[2], 240);
                                Delay(10);
                            }
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                    }
                }
                if (PM3cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    }
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 90 - (10 * i));
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);

            }
        }

        private void VacPickPM4()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM4cnt >= Obj.EFEMobj.LLwafer)
            {
                PM4cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM4cnt) > 1)
            {
                PM4cnt += 2;
                if (PM4cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM4cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                    }
                }
                if (PM4cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM4cnt) == 1)
            {
                PM4cnt += 1;
                if (PM4cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 1개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM4cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                    }
                }
                if (PM4cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
            }
        }

        private void VacPlace4()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[0].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 360 - (10 * i));
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }

            // PM1 PICK
            if ((Obj.EFEMobj.LLwafer - PM4cnt) > 0)
            {
                if (PM4cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    if (i == 5)
                    {
                        if (PM4cnt == 1)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM11.png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM4cnt == 2)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM12.png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM4cnt == 3)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM13.png", ImreadModes.Unchanged);
                            PMsrc[0] = Rotate(PMsrc[0], 120);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM4cnt == 4)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM14.png", ImreadModes.Unchanged);
                            PMsrc[0] = Rotate(PMsrc[0], 120);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM4cnt == 5)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM15.png", ImreadModes.Unchanged);
                            PMsrc[0] = Rotate(PMsrc[0], 240);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM4cnt == 6)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM16.png", ImreadModes.Unchanged);
                            PMsrc[0] = Rotate(PMsrc[0], 240);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[0], 10 * j);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM4cnt) == 0)
            {
                if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    if (PM4cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 270);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM1" + PM4cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[0] = Rotate(PMsrc[0], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[0] = Rotate(PMsrc[0], 240);
                                Delay(10);
                            }
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    if (PM4cnt == 2)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 270);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM1" + PM4cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[0] = Rotate(PMsrc[0], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[0] = Rotate(PMsrc[0], 240);
                                Delay(10);
                            }
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                    }
                }
                if (PM4cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    }
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 270 + (10 * i));
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);

            }
        }

        private void VacPickPM5()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;

            if (PM5cnt >= Obj.EFEMobj.LLwafer)
            {
                PM5cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM5cnt) > 1)
            {
                PM5cnt += 2;

                if (PM5cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM5cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);
                    }
                }
                if (PM5cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM5cnt) == 1)
            {
                PM5cnt += 1;
                if (PM5cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 1개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM5cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);
                    }
                }
                if (PM5cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
            }
        }


        private void VacPlace5()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[1].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            // 회전 180도

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }
            if ((Obj.EFEMobj.LLwafer - PM5cnt) > 0)
            {
                if (PM5cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    }
                }

                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);

                    if (i == 5)
                    {
                        if (PM5cnt == 1)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM21.png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                        else if (PM5cnt == 2)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM22.png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                        else if (PM5cnt == 3)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM23.png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                            PMsrc[1] = Rotate(PMsrc[1], 120);
                        }
                        else if (PM5cnt == 4)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM24.png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                            PMsrc[1] = Rotate(PMsrc[1], 120);
                        }
                        else if (PM5cnt == 5)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM25.png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                            PMsrc[1] = Rotate(PMsrc[1], 240);
                        }
                        else if (PM5cnt == 6)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM26.png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                            PMsrc[1] = Rotate(PMsrc[1], 240);
                        }
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[1], 10 * j);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM5cnt) == 0)
            {
                if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    if (PM5cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 180);
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        if (i == 5)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM2" + PM5cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[1] = Rotate(PMsrc[1], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[1] = Rotate(PMsrc[1], 240);
                                Delay(10);
                            }
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    if (PM5cnt == 2)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 180);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM2" + PM5cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[1] = Rotate(PMsrc[1], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[1] = Rotate(PMsrc[1], 240);
                                Delay(10);
                            }
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                    }
                }

                if (PM5cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    }
                }
            }

            // 회전 180도
            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 180 + (10 * i));
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }
        }

        private void VacPickPM6()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM6cnt >= Obj.EFEMobj.LLwafer)
            {
                PM6cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM6cnt) > 1)
            {
                PM6cnt += 2;
                if (PM6cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM6cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                    }
                }
                if (PM6cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM6cnt) == 1)
            {
                PM6cnt += 1;
                if (PM6cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 1개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM6cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                    }
                }
                if (PM6cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
            }
        }

        private void VacPlace6()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[2].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            // 회전 180도

            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, (10 * i));
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }


            if ((Obj.EFEMobj.LLwafer - PM6cnt) > 0)
            {
                if (PM6cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);

                    if (i == 5)
                    {
                        if (PM6cnt == 1)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM31.png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM6cnt == 2)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM32.png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM6cnt == 3)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM33.png", ImreadModes.Unchanged);
                            PMsrc[2] = Rotate(PMsrc[2], 120);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM6cnt == 4)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM34.png", ImreadModes.Unchanged);
                            PMsrc[2] = Rotate(PMsrc[2], 120);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM6cnt == 5)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM35.png", ImreadModes.Unchanged);
                            PMsrc[2] = Rotate(PMsrc[2], 240);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM6cnt == 6)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM36.png", ImreadModes.Unchanged);
                            PMsrc[2] = Rotate(PMsrc[2], 240);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[2], 10 * j);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM6cnt) == 0)
            {
                if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    if (PM6cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 90);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM3" + PM6cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[2] = Rotate(PMsrc[2], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[2] = Rotate(PMsrc[2], 240);
                                Delay(10);
                            }
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    if (PM6cnt == 2)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 90);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM3" + PM6cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[2] = Rotate(PMsrc[2], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[2] = Rotate(PMsrc[2], 240);
                                Delay(10);
                            }
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                    }
                }

                if (PM6cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    }
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 90 - (10 * i));
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }
        }

        private void VacPickPM7()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM7cnt >= Obj.EFEMobj.LLwafer)
            {
                PM7cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM7cnt) > 1)
            {
                PM7cnt += 2;
                if (PM7cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM7cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                    }
                }

                if (PM7cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM7cnt) == 1)
            {
                PM7cnt += 1;
                if (PM7cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 1개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM7cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                    }
                }
                if (PM7cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
            }
        }

        private void VacPlace7()

        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[0].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 360 - (10 * i));
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }

            // PM1 PICK
            if ((Obj.EFEMobj.LLwafer - PM7cnt) > 0)
            {
                if (PM7cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    if (i == 5)
                    {
                        if (PM7cnt == 1)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM11.png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM7cnt == 2)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM12.png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM7cnt == 3)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM13.png", ImreadModes.Unchanged);
                            PMsrc[0] = Rotate(PMsrc[0], 120);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM7cnt == 4)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM14.png", ImreadModes.Unchanged);
                            PMsrc[0] = Rotate(PMsrc[0], 120);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM7cnt == 5)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM15.png", ImreadModes.Unchanged);
                            PMsrc[0] = Rotate(PMsrc[0], 240);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM7cnt == 6)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM16.png", ImreadModes.Unchanged);
                            PMsrc[0] = Rotate(PMsrc[0], 240);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[0], 10 * j);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM7cnt) == 0)
            {
                if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    if (PM7cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 270);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM1" + PM7cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[0] = Rotate(PMsrc[0], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[0] = Rotate(PMsrc[0], 240);
                                Delay(10);
                            }
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    if (PM7cnt == 2)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 270);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM1" + PM7cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[0] = Rotate(PMsrc[0], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[0] = Rotate(PMsrc[0], 240);
                                Delay(10);
                            }
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                    }
                }

                if (PM7cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    }
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 270 + (10 * i));
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }
        }

        private void VacPickPM8()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM8cnt >= Obj.EFEMobj.LLwafer)
            {
                PM8cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM8cnt) > 1)
            {
                PM8cnt += 2;
                if (PM8cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM8cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                    }
                }
                if (PM8cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM8cnt) == 1)
            {
                PM8cnt += 1;
                if (PM8cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 1개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM8cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                    }
                }
                if (PM8cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
            }
        }

        private void VacPlace8()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[1].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            // 회전 180도

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }
            if ((Obj.EFEMobj.LLwafer - PM8cnt) > 0)
            {
                if (PM8cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    }
                }

                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);

                    if (i == 5)
                    {
                        if (PM8cnt == 1)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM21.png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                        else if (PM8cnt == 2)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM22.png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                        else if (PM8cnt == 3)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM23.png", ImreadModes.Unchanged);
                            PMsrc[1] = Rotate(PMsrc[1], 120);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                        else if (PM8cnt == 4)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM24.png", ImreadModes.Unchanged);
                            PMsrc[1] = Rotate(PMsrc[1], 120);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                        else if (PM8cnt == 5)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM25.png", ImreadModes.Unchanged);
                            PMsrc[1] = Rotate(PMsrc[1], 240);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                        else if (PM8cnt == 6)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM26.png", ImreadModes.Unchanged);
                            PMsrc[1] = Rotate(PMsrc[1], 240);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[1], 10 * j);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM8cnt) == 0)
            {
                if (PM8cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    }
                }
                if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 180);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM2" + PM8cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[1] = Rotate(PMsrc[1], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[1] = Rotate(PMsrc[1], 240);
                                Delay(10);
                            }
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    if (PM8cnt == 2)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 180);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM2" + PM8cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[1] = Rotate(PMsrc[1], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[1] = Rotate(PMsrc[1], 240);
                                Delay(10);
                            }
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                    }
                }
                if (PM8cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    }
                }
            }

            // 회전 180도
            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 180 + (10 * i));
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }
        }

        private void VacPickPM9()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM9cnt >= Obj.EFEMobj.LLwafer)
            {
                PM9cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM9cnt) > 1)
            {
                PM9cnt += 2;

                if (PM9cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM9cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);
                    }
                }
                if (PM9cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM9cnt) == 1)
            {
                PM9cnt += 1;
                if (PM9cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 1개 뽑음
                    if (i == 5)
                    {
                        LLsrc[0] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM9cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);
                    }
                }
                if (PM9cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
            }
        }

        private void VacPlace9()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[2].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            // 회전 180도

            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, (10 * i));
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }

            if ((Obj.EFEMobj.LLwafer - PM9cnt) > 0)
            {
                if (PM9cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);

                    if (i == 5)
                    {
                        if (PM9cnt == 1)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM31.png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM9cnt == 2)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM32.png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM9cnt == 3)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM33.png", ImreadModes.Unchanged);
                            PMsrc[2] = Rotate(PMsrc[2], 120);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM9cnt == 4)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM34.png", ImreadModes.Unchanged);
                            PMsrc[2] = Rotate(PMsrc[2], 120);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM9cnt == 5)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM35.png", ImreadModes.Unchanged);
                            PMsrc[2] = Rotate(PMsrc[2], 240);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM9cnt == 6)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM36.png", ImreadModes.Unchanged);
                            PMsrc[2] = Rotate(PMsrc[2], 240);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[2], 10 * j);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM9cnt) == 0)
            {
                if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    if (PM9cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 90);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM3" + PM9cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[2] = Rotate(PMsrc[2], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[2] = Rotate(PMsrc[2], 240);
                                Delay(10);
                            }
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    if (PM9cnt == 2)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 90);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM3" + PM9cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[2] = Rotate(PMsrc[2], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[2] = Rotate(PMsrc[2], 240);
                                Delay(10);
                            }
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                    }
                }
                if (PM9cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    }
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 90 - (10 * i));
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }
        }

        private void VacPickPM10()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM10cnt >= Obj.EFEMobj.LLwafer)
            {
                PM10cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM10cnt) > 1)
            {
                PM10cnt += 2;
                if (PM10cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM10cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                    }
                }
                if (PM10cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM10cnt) == 1)
            {
                PM10cnt += 1;
                if (PM10cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 1개 뽑음
                    if (i == 5)
                    {
                        LLsrc[1] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM10cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
                    }
                }
                if (PM10cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
            }
        }

        private void VacPlace10()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[0].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 360 - (10 * i));
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }

            // PM1 PICK
            if ((Obj.EFEMobj.LLwafer - PM10cnt) > 0)
            {
                if (PM10cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 270);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    if (i == 5)
                    {
                        if (PM10cnt == 1)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM11.png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM10cnt == 2)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM12.png", ImreadModes.Unchanged);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM10cnt == 3)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM13.png", ImreadModes.Unchanged);
                            PMsrc[0] = Rotate(PMsrc[0], 120);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM10cnt == 4)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM14.png", ImreadModes.Unchanged);
                            PMsrc[0] = Rotate(PMsrc[0], 120);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM10cnt == 5)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM15.png", ImreadModes.Unchanged);
                            PMsrc[0] = Rotate(PMsrc[0], 240);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                        else if (PM10cnt == 6)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM16.png", ImreadModes.Unchanged);
                            PMsrc[0] = Rotate(PMsrc[0], 240);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[0], 10 * j);
                            Delay(10);
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM10cnt) == 0)
            {
                if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    if (PM10cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 270);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM1" + PM10cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[0] = Rotate(PMsrc[0], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[0] = Rotate(PMsrc[0], 240);
                                Delay(10);
                            }
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    if (PM10cnt == 2)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 270);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[0] = new Mat(@"..\Debug\PM1" + PM10cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[0] = Rotate(PMsrc[0], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[0] = Rotate(PMsrc[0], 240);
                                Delay(10);
                            }
                            pictureBox8.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[0]);
                        }
                    }
                }
                if (PM10cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PMDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM0.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    }
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 270 + (10 * i));
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);

            }
        }

        private void VacPickPM11()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM11cnt >= Obj.EFEMobj.LLwafer)
            {
                PM11cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM11cnt) > 1)
            {
                PM11cnt += 2;
                if (PM11cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM11cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                    }
                }

                if (PM11cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM11cnt) == 1)
            {
                PM11cnt += 1;
                if (PM11cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 1개 뽑음
                    if (i == 5)
                    {
                        LLsrc[2] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM11cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
                    }
                }
                if (PM11cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc = new Mat(@"..\Debug\door\Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc);
                    }
                }
            }
        }

        private void VacPlace11()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[1].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            // 회전 180도

            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 10 * i);
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }
            if ((Obj.EFEMobj.LLwafer - PM11cnt) > 0)
            {
                if (PM11cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 180);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);

                    if (i == 5)
                    {
                        if (PM11cnt == 1)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM21.png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                        else if (PM11cnt == 2)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM22.png", ImreadModes.Unchanged);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                        else if (PM11cnt == 3)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM23.png", ImreadModes.Unchanged);
                            PMsrc[1] = Rotate(PMsrc[1], 120);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                        else if (PM11cnt == 4)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM24.png", ImreadModes.Unchanged);
                            PMsrc[1] = Rotate(PMsrc[1], 120);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                        else if (PM11cnt == 5)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM25.png", ImreadModes.Unchanged);
                            PMsrc[1] = Rotate(PMsrc[1], 240);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                        else if (PM11cnt == 6)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM26.png", ImreadModes.Unchanged);
                            PMsrc[1] = Rotate(PMsrc[1], 240);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[1], 10 * j);
                            Delay(10);
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM11cnt) == 0)
            {
                if (PM11cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    }
                }
                if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 180);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM2" + PM11cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[1] = Rotate(PMsrc[1], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[1] = Rotate(PMsrc[1], 240);
                                Delay(10);
                            }
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    if (PM11cnt == 2)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 180);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[1] = new Mat(@"..\Debug\PM2" + PM11cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[1] = Rotate(PMsrc[1], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[1] = Rotate(PMsrc[1], 240);
                                Delay(10);
                            }
                            pictureBox9.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[1]);
                        }
                    }
                }
                if (PM11cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        PMDoorSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc1);
                    }
                }
            }

            // 회전 180도
            for (int i = 1; i <= 18; i++)
            {
                dst = Rotate(VACsrc, 180 + (10 * i));
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }
        }

        private void VacPickPM12()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat SlotSrc;
            Mat SlotSrc1;
            if (PM12cnt >= Obj.EFEMobj.LLwafer)
            {
                PM12cnt = 0;
            }
            if ((Obj.EFEMobj.LLwafer - PM12cnt) > 1)
            {
                PM12cnt += 2;
                if (PM12cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 2개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM12cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                    }
                }
                if (PM12cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
            }
            else if ((Obj.EFEMobj.LLwafer - PM12cnt) == 1)
            {
                PM12cnt += 1;
                if (PM12cnt == 1)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
                for (int i = 1; i <= 10; i++)
                {
                    VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPick) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(VACsrc);

                    // LL 1개 뽑음
                    if (i == 5)
                    {
                        LLsrc[3] = new Mat(@"..\Debug\LLPump" + (Obj.EFEMobj.LLwafer - PM12cnt) + ".png", ImreadModes.Unchanged);
                        Delay(10);
                        pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
                    }
                }
                if (PM12cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        SlotSrc1 = new Mat(@"..\Debug\door\BDoor" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxSlot4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SlotSrc1);
                    }
                }
            }
        }

        private void VacPlace12()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat dst1 = new Mat(PMsrc[2].Size(), MatType.CV_8UC3);
            Mat PMDoorSrc;
            Mat PMDoorSrc1;

            // 회전 180도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, (10 * i));
                Delay(Convert.ToInt32((55 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }

            if ((Obj.EFEMobj.LLwafer - PM12cnt) > 0)
            {
                if (PM12cnt == 2)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    }
                }
                for (int i = 10; i > 0; i--)
                {
                    VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(VACsrc, 90);
                    Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                    pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);

                    if (i == 5)
                    {
                        if (PM12cnt == 1)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM31.png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM12cnt == 2)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM32.png", ImreadModes.Unchanged);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM12cnt == 3)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM33.png", ImreadModes.Unchanged);
                            PMsrc[2] = Rotate(PMsrc[2], 120);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM12cnt == 4)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM34.png", ImreadModes.Unchanged);
                            PMsrc[2] = Rotate(PMsrc[2], 120);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM12cnt == 5)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM35.png", ImreadModes.Unchanged);
                            PMsrc[2] = Rotate(PMsrc[2], 240);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                        else if (PM12cnt == 6)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM36.png", ImreadModes.Unchanged);
                            PMsrc[2] = Rotate(PMsrc[2], 240);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                    }
                    if (i == 1)
                    {
                        for (int j = 1; j <= 12; j++)
                        {
                            dst1 = Rotate(PMsrc[2], 10 * j);
                            Delay(10);
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst1);
                        }
                    }
                }
            }

            else if ((Obj.EFEMobj.LLwafer - PM12cnt) == 0)
            {
                if (Obj.EFEMobj.LLwafer % 2 == 1)
                {
                    if (PM12cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VAC1\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 90);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM3" + PM12cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[2] = Rotate(PMsrc[2], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[2] = Rotate(PMsrc[2], 240);
                                Delay(10);
                            }
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                    }
                }
                else if (Obj.EFEMobj.LLwafer % 2 == 0)
                {
                    if (PM12cnt == 2)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                            pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                        }
                    }
                    for (int i = 10; i > 0; i--)
                    {
                        VACsrc = new Mat(@"..\Debug\VACrobot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(VACsrc, 90);
                        Delay(Convert.ToInt32((100 * Obj.TMobj.nVspeedPlace) / Obj.speed));
                        pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        if (i == 5)
                        {
                            PMsrc[2] = new Mat(@"..\Debug\PM3" + PM12cnt + ".png", ImreadModes.Unchanged);
                            if ((Obj.EFEMobj.LLwafer - 2) < 3 && (Obj.EFEMobj.LLwafer - 2) > 0)
                            {
                                PMsrc[2] = Rotate(PMsrc[2], 120);
                                Delay(10);
                            }
                            else if ((Obj.EFEMobj.LLwafer - 2) < 5 && (Obj.EFEMobj.LLwafer - 2) > 2)
                            {
                                PMsrc[2] = Rotate(PMsrc[2], 240);
                                Delay(10);
                            }
                            pictureBox10.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMsrc[2]);
                        }
                    }
                }
                if (PM12cnt == Obj.EFEMobj.LLwafer)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        PMDoorSrc = new Mat(@"..\Debug\door\PM3Door" + i + ".png", ImreadModes.Unchanged);
                        Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        pictureBoxPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(PMDoorSrc);
                    }
                }
            }
            // 회전 90도
            for (int i = 1; i <= 9; i++)
            {
                dst = Rotate(VACsrc, 90 - (10 * i));
                Delay(Convert.ToInt32((110 * Obj.TMobj.nVspeedRotate) / Obj.speed));
                pictureBox7.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }
        }
        #endregion

        //LLPump
        private void LLPump1()
        {
            if (LL1cnt == Obj.EFEMobj.LLwafer)
            {
                LLsrc[0] = new Mat(@"..\Debug\LLPump" + Obj.EFEMobj.LLwafer + ".png", ImreadModes.Unchanged);
                for (int i = (Obj.EFEMobj.nLLVentStabTime + Obj.EFEMobj.nLLVentStabTime); i >= 0; i--)
                {
                    Delay(1000  / Obj.speed);
                    label20.Text = i.ToString();

                }
                pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

            }

        }
        private void LLPump2()
        {
            if (LL2cnt == Obj.EFEMobj.LLwafer)
            {
                LLsrc[1] = new Mat(@"..\Debug\LLPump" + Obj.EFEMobj.LLwafer + ".png", ImreadModes.Unchanged);
                for (int i = (Obj.EFEMobj.nLLVentStabTime + Obj.EFEMobj.nLLVentStabTime); i >= 0; i--)
                {
                    Delay(1000 / Obj.speed);
                    label21.Text = i.ToString();

                }
                pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);
            }

        }

        private void LLPump3()
        {

            if (LL3cnt == Obj.EFEMobj.LLwafer)
            {
                LLsrc[2] = new Mat(@"..\Debug\LLPump" + Obj.EFEMobj.LLwafer + ".png", ImreadModes.Unchanged);
                for (int i = (Obj.EFEMobj.nLLVentStabTime + Obj.EFEMobj.nLLVentStabTime); i >= 0; i--)
                {
                    Delay(1000 / Obj.speed);
                    label22.Text = i.ToString();

                }
                pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);
            }

        }

        private void LLPump4()
        {

            if (LL4cnt == Obj.EFEMobj.LLwafer)
            {
                LLsrc[3] = new Mat(@"..\Debug\LLPump" + Obj.EFEMobj.LLwafer + ".png", ImreadModes.Unchanged);
                for (int i = (Obj.EFEMobj.nLLVentStabTime + Obj.EFEMobj.nLLVentStabTime); i >= 0; i--)
                {
                    Delay(1000 / Obj.speed);
                    label23.Text = i.ToString();

                }
                pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);
            }
        }


        //LL
        static int LL1cnt = 0;
        static int LL2cnt = 0;
        static int LL3cnt = 0;
        static int LL4cnt = 0;

        //LL 1개
        private void PlaceUpWaferLL1()
        {
            Mat Doorsrc;
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            if (pictureBox3.InvokeRequired)
            {
                pictureBox3.Invoke(new PictureBoxSafety(PlaceUpWaferLL1));
            }
            else
            {
                if (LL1cnt == Obj.EFEMobj.LLwafer)
                {
                    LL1cnt = 0;
                }
                LL1cnt += 1;
                if (LL1cnt <= Obj.EFEMobj.LLwafer)
                {
                    // Door Open
                    if (LL1cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            Doorsrc = new Mat(@"..\Debug\Door\BDoor" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                    for (int i = 20; i >= 11; i--)
                    {
                        src = new Mat(@"..\Debug\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(src, 180);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.EFEMobj.nAspeedPlace + Obj.EFEMobj.nAspeedPick) / Obj.speed));
                    }
                    LLsrc[0] = new Mat(@"..\Debug\LL" + LL1cnt + ".png", ImreadModes.Unchanged);                    
                    pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                    // Door Close
                    if (LL1cnt == Obj.EFEMobj.LLwafer)
                    {
                        for (int i = 4; i > 0; i--)
                        {
                            Doorsrc = new Mat(@"..\Debug\Door\BDoor" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                }
            }
        }

        //LL2개
        private void PlaceUpWaferLL2()
        {
            Mat Doorsrc;
            Mat Doorsrc1;
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            if (pictureBox3.InvokeRequired)
            {
                pictureBox3.Invoke(new PictureBoxSafety(PlaceUpWaferLL2));
            }
            else
            {
                if (LL1cnt == Obj.EFEMobj.LLwafer * 2)
                {
                    LL1cnt = 0;
                    LL2cnt = 0;
                }
                LL1cnt += 1;
                if (LL1cnt <= Obj.EFEMobj.LLwafer)
                {
                    // Door Open
                    if (LL1cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            Doorsrc = new Mat(@"..\Debug\Door\BDoor" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                    for (int i = 20; i >= 11; i--)
                    {
                        src = new Mat(@"..\Debug\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(src, 180);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.EFEMobj.nAspeedPlace + Obj.EFEMobj.nAspeedPick) / Obj.speed));
                    }
                    LLsrc[0] = new Mat(@"..\Debug\LL" + LL1cnt + ".png", ImreadModes.Unchanged);
                    pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                    // Door Close
                    if (LL1cnt == Obj.EFEMobj.LLwafer)
                    {
                        for (int i = 4; i > 0; i--)
                        {
                            Doorsrc = new Mat(@"..\Debug\Door\BDoor" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                }
                else if (LL1cnt <= Obj.EFEMobj.LLwafer * 2)
                {
                    LL2cnt += 1;
                    // Door Open
                    if (LL2cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            Doorsrc1 = new Mat(@"..\Debug\Door\Door" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc1);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                    for (int i = 20; i >= 11; i--)
                    {
                        src = new Mat(@"..\Debug\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(src, 180);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.EFEMobj.nAspeedPlace + Obj.EFEMobj.nAspeedPick) / Obj.speed));
                    }
                    LLsrc[1] = new Mat(@"..\Debug\LLa" + LL2cnt + ".png", ImreadModes.Unchanged);
                    pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);

                    // Door Close
                    if (LL2cnt == Obj.EFEMobj.LLwafer)
                    {
                        for (int i = 4; i > 0; i--)
                        {
                            Doorsrc1 = new Mat(@"..\Debug\Door\Door" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc1);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }

                }
            }
        }
        //LL2개
        private void PlaceUpWaferLL3()
        {
            Mat Doorsrc;
            Mat Doorsrc1;
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            if (pictureBox3.InvokeRequired)
            {
                pictureBox3.Invoke(new PictureBoxSafety(PlaceUpWaferLL3));
            }
            else
            {
                if (LL1cnt == Obj.EFEMobj.LLwafer * 3)
                {
                    LL1cnt = 0;
                    LL2cnt = 0;
                    LL3cnt = 0;
                }
                LL1cnt += 1;
                if (LL1cnt <= Obj.EFEMobj.LLwafer)
                {
                    // Door Open
                    if (LL1cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            Doorsrc = new Mat(@"..\Debug\Door\BDoor" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                    for (int i = 20; i >= 11; i--)
                    {
                        src = new Mat(@"..\Debug\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(src, 180);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.EFEMobj.nAspeedPlace + Obj.EFEMobj.nAspeedPick) / Obj.speed));
                    }
                    LLsrc[0] = new Mat(@"..\Debug\LL" + LL1cnt + ".png", ImreadModes.Unchanged);
                    pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                    // Door Close
                    if (LL1cnt == Obj.EFEMobj.LLwafer)
                    {
                        for (int i = 4; i > 0; i--)
                        {
                            Doorsrc = new Mat(@"..\Debug\Door\BDoor" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                }
                else if (LL1cnt <= Obj.EFEMobj.LLwafer * 2)
                {
                    LL2cnt += 1;
                    // Door Open
                    if (LL2cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            Doorsrc1 = new Mat(@"..\Debug\Door\Door" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc1);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                    for (int i = 20; i >= 11; i--)
                    {
                        src = new Mat(@"..\Debug\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(src, 180);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.EFEMobj.nAspeedPlace + Obj.EFEMobj.nAspeedPick) / Obj.speed));
                    }
                    LLsrc[1] = new Mat(@"..\Debug\LLa" + LL2cnt + ".png", ImreadModes.Unchanged);
                    pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);

                    // Door Close
                    if (LL2cnt == Obj.EFEMobj.LLwafer)
                    {
                        for (int i = 4; i > 0; i--)
                        {
                            Doorsrc1 = new Mat(@"..\Debug\Door\Door" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc1);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }

                }
                else if (LL1cnt <= Obj.EFEMobj.LLwafer * 3)
                {
                    LL3cnt += 1;
                    // Door Open
                    if (LL3cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            Doorsrc = new Mat(@"..\Debug\Door\BDoor" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                    for (int i = 20; i >= 11; i--)
                    {
                        src = new Mat(@"..\Debug\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(src, 180);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.EFEMobj.nAspeedPlace + Obj.EFEMobj.nAspeedPick) / Obj.speed));
                    }
                    LLsrc[2] = new Mat(@"..\Debug\LLb" + LL3cnt + ".png", ImreadModes.Unchanged);
                    pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);

                    // Door Close
                    if (LL3cnt == Obj.EFEMobj.LLwafer)
                    {
                        for (int i = 4; i > 0; i--)
                        {
                            Doorsrc = new Mat(@"..\Debug\Door\BDoor" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                }
            }
        }

        //LL이 4개
        private void PlaceUpWaferLL4()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            Mat Doorsrc;
            Mat Doorsrc1;
            if (pictureBox3.InvokeRequired)
            {
                pictureBox3.Invoke(new PictureBoxSafety(PlaceUpWaferLL4));
            }
            else
            {
                if (LL1cnt == Obj.EFEMobj.LLwafer * 4)
                {
                    LL1cnt = 0;
                    LL2cnt = 0;
                    LL3cnt = 0;
                    LL4cnt = 0;
                }
                LL1cnt += 1;
                if (LL1cnt <= Obj.EFEMobj.LLwafer)
                {
                    // Door Open
                    if (LL1cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            Doorsrc = new Mat(@"..\Debug\Door\BDoor" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                    for (int i = 20; i >= 11; i--)
                    {
                        src = new Mat(@"..\Debug\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(src, 180);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.EFEMobj.nAspeedPlace + Obj.EFEMobj.nAspeedPick) / Obj.speed));
                    }
                    LLsrc[0] = new Mat(@"..\Debug\LL" + LL1cnt + ".png", ImreadModes.Unchanged);
                    pictureBox3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[0]);

                    // Door Close
                    if (LL1cnt == Obj.EFEMobj.LLwafer)
                    {
                        for (int i = 4; i > 0; i--)
                        {
                            Doorsrc = new Mat(@"..\Debug\Door\BDoor" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                }
                else if (LL1cnt <= Obj.EFEMobj.LLwafer * 2)
                {
                    LL2cnt += 1;
                    // Door Open
                    if (LL2cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            Doorsrc1 = new Mat(@"..\Debug\Door\Door" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc1);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                    for (int i = 20; i >= 11; i--)
                    {
                        src = new Mat(@"..\Debug\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(src, 180);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.EFEMobj.nAspeedPlace + Obj.EFEMobj.nAspeedPick) / Obj.speed));
                    }
                    LLsrc[1] = new Mat(@"..\Debug\LLa" + LL2cnt + ".png", ImreadModes.Unchanged);
                    pictureBox4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[1]);

                    // Door Close
                    if (LL2cnt == Obj.EFEMobj.LLwafer)
                    {
                        for (int i = 4; i > 0; i--)
                        {
                            Doorsrc1 = new Mat(@"..\Debug\Door\Door" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc1);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }

                }
                else if (LL1cnt <= Obj.EFEMobj.LLwafer * 3)
                {
                    LL3cnt += 1;
                    // Door Open
                    if (LL3cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            Doorsrc = new Mat(@"..\Debug\Door\BDoor" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                    for (int i = 20; i >= 11; i--)
                    {
                        src = new Mat(@"..\Debug\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(src, 180);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.EFEMobj.nAspeedPlace + Obj.EFEMobj.nAspeedPick) / Obj.speed));
                    }
                    LLsrc[2] = new Mat(@"..\Debug\LLb" + LL3cnt + ".png", ImreadModes.Unchanged);
                    pictureBox5.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[2]);

                    // Door Close
                    if (LL3cnt == Obj.EFEMobj.LLwafer)
                    {
                        for (int i = 4; i > 0; i--)
                        {
                            Doorsrc = new Mat(@"..\Debug\Door\BDoor" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen3.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                }
                else if (LL1cnt <= Obj.EFEMobj.LLwafer * 4)
                {
                    LL4cnt += 1;
                    if (LL4cnt == 1)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            Doorsrc1 = new Mat(@"..\Debug\Door\Door" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc1);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                    for (int i = 20; i >= 11; i--)
                    {
                        src = new Mat(@"..\Debug\robot" + i + ".png", ImreadModes.Unchanged);
                        dst = Rotate(src, 180);
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                        Delay(Convert.ToInt32((100 * Obj.EFEMobj.nAspeedPlace + Obj.EFEMobj.nAspeedPick) / Obj.speed));
                    }
                    LLsrc[3] = new Mat(@"..\Debug\LLc" + LL4cnt + ".png", ImreadModes.Unchanged);
                    pictureBox6.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LLsrc[3]);

                    // Door Close
                    if (LL4cnt == Obj.EFEMobj.LLwafer)
                    {
                        for (int i = 4; i > 0; i--)
                        {
                            Doorsrc1 = new Mat(@"..\Debug\Door\Door" + i + ".png", ImreadModes.Unchanged);
                            pictureBoxOpen4.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(Doorsrc1);
                            Delay(Convert.ToInt32((250 * Obj.doorSpeed) / Obj.speed));
                        }
                    }
                }
            }
        }

        //공용
        private void PickAndPlace()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);

            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new PictureBoxSafety(PickAndPlace));
            }
            else
            {
                // align 왼손 꺼냄
                // 20까지갈 예정
                for (int i = 1; i <= 10; i++)
                {
                    src = new Mat(@"..\Debug\robotc" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(src, 90);
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((50 * Obj.EFEMobj.nAspeedPlace) / Obj.speed));

                    // align 불 꺼짐
                    if (i == 5)
                    {
                        alignersrc = new Mat(@"..\Debug\Aligner.png", ImreadModes.Unchanged);
                        pictureBox2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(alignersrc);
                    }
                    else if (i == 15)
                    {
                        // align 불 켜짐
                        alignersrc = new Mat(@"..\Debug\Alignerin.png", ImreadModes.Unchanged);
                        pictureBox2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(alignersrc);
                    }
                }
                for (int i = 11; i <= 20; i++)
                {
                    src = new Mat(@"..\Debug\robotc" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(src, 90);
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((50 * Obj.EFEMobj.nAspeedPick) / Obj.speed));

                    // align 불 꺼짐
                    if (i == 5)
                    {
                        alignersrc = new Mat(@"..\Debug\Aligner.png", ImreadModes.Unchanged);
                        pictureBox2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(alignersrc);
                    }
                    else if (i == 15)
                    {
                        // align 불 켜짐
                        alignersrc = new Mat(@"..\Debug\Alignerin.png", ImreadModes.Unchanged);
                        pictureBox2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(alignersrc);
                    }
                }
                //Delay(Convert.ToInt32((1000 *Obj.EFEMobj.nAlgTime) / Obj.EFEMobj.speed));                          
            }
        }

        private void LLMoveandPick()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            src = new Mat(@"..\Debug\robot1.png", ImreadModes.Unchanged);

            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new PictureBoxSafety(LLMoveandPick));
            }
            else
            {
                for (int i = 1; i <= 9; i++)
                {
                    dst = Rotate(src, 90 + (10 * i));
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((110 * Obj.EFEMobj.nAspeedRotate) / Obj.speed));
                }

                for (int i = 0; i < 18; i++)
                {
                    pictureBox1.Left -= 10;
                    Delay(Convert.ToInt32(40 / Obj.speed));
                }

                for (int i = 20; i >= 11; i--)
                {
                    src = new Mat(@"..\Debug\robot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(src, 180);
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((100 * Obj.EFEMobj.nAspeedPlace + Obj.EFEMobj.nAspeedPick) / Obj.speed));
                }
            }
        }

        private void LLRotate()
        {

            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            src = new Mat(@"..\Debug\robot20.png", ImreadModes.Unchanged);
            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new PictureBoxSafety(LLRotate));
            }
            else
            {
                for (int i = 1; i <= 9; i++)
                {
                    dst = Rotate(src, 90 + (10 * i));
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((110 * Obj.EFEMobj.nAspeedRotate) / Obj.speed));
                }

                for (int i = 0; i < 18; i++)
                {
                    pictureBox1.Left -= 10;
                    Delay(Convert.ToInt32(40 / Obj.speed));
                }


            }
        }

        static int LPMcnt1 = 25;
        static int LPMcnt2 = 25;
        static int LPMcnt3 = 0;

        //LPM1

        private void LPM1Move()
        {
            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new PictureBoxSafety(LPM1Move));
            }
            else
            {
                for (int i = 0; i < 7; i++)
                {
                    pictureBox1.Left -= 10;
                    Delay(Convert.ToInt32(40 / (Obj.speed)));
                }
            }
        }

        private void PickWaferLPM1()
        {
            Mat src = new Mat(@"..\Debug\robot.png", ImreadModes.Unchanged);
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new PictureBoxSafety(PickWaferLPM1));
            }
            else
            {
                if (pictureBox1.Left != 182)
                {
                    while (pictureBox1.Left != 182)
                    {
                        pictureBox1.Left -= 10;
                        Delay(Convert.ToInt32(40 / (Obj.speed)));
                    }
                }

                for (int i = 1; i <= 18; i++)
                {
                    dst = Rotate(src, 10 * i);
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((55 * Obj.EFEMobj.nAspeedRotate) / Obj.speed));
                }

                for (int i = 1; i <= 10; i++)
                {
                    src = new Mat(@"..\Debug\robot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(src);
                    Delay(Convert.ToInt32((100 * Obj.EFEMobj.nAspeedPick) / Obj.speed));
                    if (i == 5)
                    {
                        LPMcnt1 -= 1;
                        LPMsrc[0] = new Mat(@"..\Debug\LPMimage\LPM" + LPMcnt1 + ".png", ImreadModes.Unchanged);
                        pictureBoxLPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LPMsrc[0]);

                    }
                }

                for (int i = 1; i <= 9; i++)
                {
                    dst = Rotate(src1, 10 * i);
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((110 * Obj.EFEMobj.nAspeedRotate) / Obj.speed));
                }
                if (LPMcnt1 == 10)
                {
                    LPMcnt2 = 25;
                    LPMsrc[1] = new Mat(@"..\Debug\LPMimage\LPM25.png", ImreadModes.Unchanged);
                    pictureBoxLPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LPMsrc[1]);
                }

            }
        }

        private void FirstPickWafertoAlg()
        {
            src = new Mat(@"..\Debug\robot.png", ImreadModes.Unchanged);
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);

            src1 = new Mat(@"..\Debug\robot10.png", ImreadModes.Unchanged);
            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new PictureBoxSafety(FirstPickWafertoAlg));
            }

            else
            {
                for (int i = 0; i < 7; i++)
                {
                    pictureBox1.Left -= 10;
                    Delay(Convert.ToInt32(120 / Obj.speed));
                }

                for (int i = 1; i <= 18; i++)
                {
                    dst = Rotate(src, 10 * i);
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((55 * Obj.EFEMobj.nAspeedRotate) / Obj.speed));
                }

                for (int i = 1; i <= 10; i++)
                {
                    src = new Mat(@"..\Debug\robot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(src);
                    Delay(Convert.ToInt32((100 * Obj.EFEMobj.nAspeedPick) / Obj.speed));
                    if (i == 6)
                    {
                        LPMcnt1 -= 1;
                        LPMsrc[0] = new Mat(@"..\Debug\LPMimage\LPM" + LPMcnt1 + ".png", ImreadModes.Unchanged);
                        pictureBoxLPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LPMsrc[0]);
                    }

                }

                for (int i = 1; i <= 9; i++)
                {
                    dst = Rotate(src1, 10 * i);
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((110 * Obj.EFEMobj.nAspeedRotate) / Obj.speed));
                }

                for (int i = 0; i < 25; i++)
                {
                    pictureBox1.Left += 10;
                    Delay(Convert.ToInt32(40 / Obj.speed));
                }
            }
        }

        private void FirstPlaceWafer()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);

            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new PictureBoxSafety(FirstPlaceWafer));
            }

            else
            {
                // align 오른손
                for (int i = 10; i >= 1; i--)
                {
                    src = new Mat(@"..\Debug\robot" + i + ".png", ImreadModes.Unchanged);
                    dst = Rotate(src, 90);
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((50 * Obj.EFEMobj.nAspeedPlace) / Obj.speed));

                    // align 불 켜짐
                    if (i == 5)
                    {
                        alignersrc = new Mat(@"..\Debug\Alignerin.png", ImreadModes.Unchanged);
                        pictureBox2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(alignersrc);
                    }
                }
            }
        }

        private void FirstMoveLPM1()
        {
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);
            src = new Mat(@"..\Debug\robot1.png", ImreadModes.Unchanged);
            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new PictureBoxSafety(FirstMoveLPM1));
            }

            else
            {
                for (int i = 1; i <= 9; i++)
                {
                    dst = Rotate(src, 90 + (10 * i));
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((110 * Obj.EFEMobj.nAspeedRotate) / Obj.speed));
                }

                for (int i = 0; i < 25; i++)
                {
                    pictureBox1.Left -= 10;
                    Delay(Convert.ToInt32(40 / Obj.speed));
                }
            }
        }

        private void MoveLPM1toAlg()
        {
            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new PictureBoxSafety(MoveLPM1toAlg));
            }
            else
            {
                for (int i = 0; i < 25; i++)
                {
                    pictureBox1.Left += 10;
                    Delay(Convert.ToInt32(40 / Obj.speed));
                }
            }
        }

        private void CenterMoveLPM1()
        {
            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new PictureBoxSafety(CenterMoveLPM1));
            }
            else
            {
                for (int i = 0; i < 7; i++)
                {
                    pictureBox1.Left -= 10;
                    Delay(Convert.ToInt32(40 / (Obj.speed)));
                }
            }
        }

        private void PickWaferLPM2()
        {
            src = new Mat(@"..\Debug\robot.png", ImreadModes.Unchanged);
            Mat dst = new Mat(src.Size(), MatType.CV_8UC3);

            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new PictureBoxSafety(PickWaferLPM2));
            }
            else
            {
                if (pictureBox1.Left != 292)
                {
                    while (pictureBox1.Left != 292)
                    {
                        if (pictureBox1.Left < 292)
                        {
                            pictureBox1.Left += 10;
                            Delay(Convert.ToInt32(40 / (Obj.speed)));
                        }
                        else if (pictureBox1.Left > 292)
                        {
                            pictureBox1.Left -= 10;
                            Delay(Convert.ToInt32(40 / (Obj.speed)));
                        }
                    }
                }
                for (int i = 1; i <= 18; i++)
                {
                    dst = Rotate(src, 10 * i);
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((55 * Obj.EFEMobj.nAspeedRotate) / Obj.speed));
                }

                for (int i = 1; i <= 10; i++)
                {
                    src = new Mat(@"..\Debug\robot" + i + ".png", ImreadModes.Unchanged);
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(src);
                    Delay(Convert.ToInt32((100 * Obj.EFEMobj.nAspeedPick) / Obj.speed));
                    if (i == 6)
                    {
                        LPMcnt2 -= 1;
                        LPMsrc[1] = new Mat(@"..\Debug\LPMimage\LPM" + LPMcnt2 + ".png", ImreadModes.Unchanged);
                        pictureBoxLPM2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LPMsrc[1]);

                    }
                }
                for (int i = 1; i <= 9; i++)
                {
                    dst = Rotate(src1, 10 * i);
                    pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    Delay(Convert.ToInt32((110 * Obj.EFEMobj.nAspeedRotate) / Obj.speed));
                }
                if (LPMcnt2 == 10)
                {
                    LPMcnt1 = 25;
                    LPMsrc[0] = new Mat(@"..\Debug\LPMimage\LPM25.png", ImreadModes.Unchanged);
                    pictureBoxLPM1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(LPMsrc[0]);
                }

            }
        }

        private void MoveWaferLPM2()
        {
            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new PictureBoxSafety(MoveWaferLPM2));
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    pictureBox1.Left += 10;

                    Delay(Convert.ToInt32(40 / (Obj.speed)));
                }
            }
        }

        private void MoveLPM2toAlg()
        {

            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new PictureBoxSafety(MoveLPM2toAlg));
            }
            else
            {
                for (int i = 0; i < 14; i++)
                {
                    pictureBox1.Left += 10;
                    Delay(Convert.ToInt32(40 / Obj.speed));
                }
            }
        }














        #endregion


        public void NewSendtoServer(Socket csEFEM, Socket csTM, Socket csPM0, Socket csPM1, Socket csPM2)
        {           
            byte[] sendBufferSERVER = Encoding.UTF8.GetBytes(
                         "SERVER" + "/" +
                         "Assemble" + "/"
                         );

            int size = sendBufferSERVER.Length;
            byte[] data_size = BitConverter.GetBytes(size);
            csEFEM.Send(data_size);
            csEFEM.Send(sendBufferSERVER, 0, size, SocketFlags.None);

            csTM.Send(data_size);
            csTM.Send(sendBufferSERVER, 0, size, SocketFlags.None);

            csPM0.Send(data_size);
            csPM0.Send(sendBufferSERVER, 0, size, SocketFlags.None);

            csPM1.Send(data_size);
            csPM1.Send(sendBufferSERVER, 0, size, SocketFlags.None);

            csPM2.Send(data_size);
            csPM2.Send(sendBufferSERVER, 0, size, SocketFlags.None);

            Array.Clear(sendBufferSERVER, 0x0, sendBufferSERVER.Length);
        }

        static List<String> GetFileList(String rootPath, List<String> fileList)
        {
            if (fileList == null)
            {
                return null;
            }
            var attr = File.GetAttributes(rootPath);
            // 해당 path가 디렉토리이면
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var dirInfo = new DirectoryInfo(rootPath);
                // 하위 모든 디렉토리는
                foreach (var dir in dirInfo.GetDirectories())
                {
                    // 재귀로 통하여 list를 취득한다.
                    GetFileList(dir.FullName, fileList);
                }
                // 하위 모든 파일은
                foreach (var file in dirInfo.GetFiles())
                {
                    // 재귀를 통하여 list를 취득한다.
                    GetFileList(file.FullName, fileList);
                }
            }
            // 해당 path가 파일이면 (재귀를 통해 들어온 경로)
            else
            {
                var fileInfo = new FileInfo(rootPath);
                // 리스트에 full path를 저장한다.
                fileList.Add(fileInfo.FullName);
            }
            return fileList;
        }

        static byte[] CommpressDirectory(string sourcePath)
        {
            // 재귀 식으로 파일 리스트를 가져온다.
            var filelist = GetFileList(sourcePath, new List<String>());
            // 메모리 스트림으로 바이너리를 만든다.
            using (MemoryStream stream = new MemoryStream())
            {
                // 압축 클래스 생성
                using (ZipArchive zipArchive = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    // 파일 리스트 별로 압축 아이템을 만든다.
                    foreach (string file in filelist)
                    {
                        // 실제 경로로 부터 압축 경로로 바꾼다.
                        string path = file.Substring(sourcePath.Length + 1);
                        // 파일로 부터 압축
                        zipArchive.CreateEntryFromFile(file, path);
                    }
                }
                // 압축 바이너리를 byte형식으로 리턴
                return stream.GetBuffer();
            }
        }
    }
}