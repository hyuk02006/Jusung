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
using static System.Console;
using System.IO;
using System.Runtime.InteropServices;
using System.IO.Compression;

namespace JUSUNG_Client
{
    public partial class Form_PM1 : MetroFramework.Forms.MetroForm
    {
        public struct PMOBJECT
        {
            public int nPM;
            public int nPMWafer;
            public int nPM_Curcnt;
            public int nPM_Accumul;
            public int nProcessTime;
            public int nVac;
            public int speed;
        }
        public PMOBJECT PMobj;

        private Socket socket;  // 소켓

        private Thread receiveThread;    // 대화 수신용
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("PM1");
        public System.Object lockthis = new System.Object();

        public string ClientName;
        public string logPath;
        public string syslogPath;
        public string sendPath;
        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        #region FileTransferProtocal
        enum State
        {
            STATE,
            FILESIZE,
            FILEDOWNLOAD
        }

        class EFEMFile
        {
            // 상태
            protected State state = State.STATE;
            // 파일
            public byte[] Binary { get; set; }
        }
        class EFEMTest : EFEMFile
        {
            readonly Form_PM1 f1;
            // 클라이언트 소켓
            private Socket socket;
            // 버퍼
            private byte[] buffer;
            // 파일 다운로드 위치
            private int seek = 0;
            // 다운로드 디렉토리
            private string SaveEFEM = @"..\Debug\Log";
            //private string SaveTMC = @"C:\0914Test\TMC";

            // 생성자
            public EFEMTest(Socket socket)
            {
                // 소켓 받기
                this.socket = socket;
                // 버퍼
                buffer = new byte[1];
                // buffer로 메시지를 받고 Receive함수로 메시지가 올 때까지 대기한다.
                this.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Receive, this);
                // 접속 환영 메시지
                var remoteAddr = (IPEndPoint)socket.RemoteEndPoint;
                // 콘솔 출력
                WriteLine($"EFEMClient:(From:{remoteAddr.Address.ToString()}:{remoteAddr.Port},Connection time:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")})");
            }
            // 데이터 수신
            private void Receive(IAsyncResult result)
            {
                // 접속이 연결되어 있으면...
                if (socket.Connected)
                {
                    // EndReceive를 호출하여 데이터 사이즈를 받는다.
                    // EndReceive는 대기를 끝내는 것이다.
                    int size = this.socket.EndReceive(result);
                    // 상태
                    if (state == State.STATE)
                    {
                        switch (buffer[0])
                        {
                            // 0이면 파일 크기
                            case 0:
                                state = State.FILESIZE;
                                // 크기는 int형으로 받는다.
                                buffer = new byte[4];
                                break;
                            // 1이면 파일
                            case 1:
                                state = State.FILEDOWNLOAD;
                                // 파일 버퍼 설정
                                buffer = new byte[Binary.Length];
                                // 다운로드 위치 0
                                seek = 0;
                                break;
                        }
                    }
                    else if (state == State.FILESIZE)
                    {
                        // 데이터를 int형으로 변환(Bigendian)해서 파일 Binary를 할당한다.
                        Binary = new byte[BitConverter.ToInt32(buffer, 0)];
                        // 상태를 초기화
                        buffer = new byte[1];
                        state = State.STATE;
                    }
                    // 파일 다운로드
                    else if (state == State.FILEDOWNLOAD)
                    {
                        // 다운 받은 데이터를 FileName 변수로 옮긴다.
                        Array.Copy(buffer, 0, Binary, seek, size);
                        // 받은 만큼 위치 옮긴다.
                        seek += size;
                        // 위치와 파일 크기가 같으면 종료
                        if (seek >= Binary.Length)
                        {
                            // binary의 압축 풀고 디렉토리에 저장한다.
                            ExtractZip(Binary, SaveEFEM);
                            //ExtractZip(Binary, SaveTMC);
                            // 접속을 끊는다.
                            this.socket.Disconnect(false);
                            this.socket.Close();
                            this.socket.Dispose();
                            return;
                        }
                    }
                    // buffer로 메시지를 받고 Receive함수로 메시지가 올 때까지 대기한다.
                    this.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Receive, this);
                }
            }
            // 압축 풀기 소스
            public void ExtractZip(byte[] binary, string destinationPath)
            {
                // 디렉토리가 있는지 확인.
                if (!Directory.Exists(destinationPath))
                {
                    // 없으면 생성
                    Directory.CreateDirectory(destinationPath);
                }
                // byte형식을 Stream 형식으로 변환
                using (var stream = new MemoryStream(binary))
                {
                    // 압축 클래스에 넣는다.
                    using (ZipArchive zip = new ZipArchive(stream))
                    {
                        // 압축 아이템 별로
                        foreach (ZipArchiveEntry entry in zip.Entries)
                        {
                            // 압축 파일의 상대 주소를 디스크의 물리 주소로 바꾼다.
                            var filepath = Path.Combine(destinationPath, entry.FullName);
                            // 파일의 디렉토리를 확인한다.
                            var subDir = Path.GetDirectoryName(filepath);
                            // 디렉토리가 없으면
                            if (!Directory.Exists(subDir))
                            {
                                // 디렉토리 생성
                                Directory.CreateDirectory(subDir);
                            }
                            // 압축 풀기(파일이 존재하면 덮어쓰기)
                            entry.ExtractToFile(filepath, true);
                        }
                    }
                }
            }
        }
        class EFEM : Socket
        {
            private int EFEMport = 10003;
            Form_PM1 f1 = new Form_PM1();
            // 생성자
            public EFEM() : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                base.Bind(new IPEndPoint(IPAddress.Any, EFEMport));

                base.Listen(10);

                BeginAccept(Accept, this);
            }

            private void Accept(IAsyncResult result)
            {
                var client = new EFEMTest(EndAccept(result));

                BeginAccept(Accept, this);
            }
        }
        #endregion
        public Form_PM1()
        {
            InitializeComponent();

        }
        private void Form1_Load_1(object sender, EventArgs e)
        {
            textBox1.Focus();
            Log("클라이언트 로드됨!!");
            StringBuilder iniPath = new StringBuilder();
            GetPrivateProfileString("VERSION", "PM1", "(NONE)", iniPath, 32, @"..\Debug\Log\Path.ini");
            tbVer.Text = "Ver." + iniPath.ToString();
            GetPrivateProfileString("PATH", "PM1path", "(NONE)", iniPath, 32, @"..\Debug\Log\Path.ini");
            sendPath = iniPath.ToString();
            logPath = iniPath.ToString() + "\\PM1_LOG.txt";
            syslogPath = iniPath.ToString() + "\\PM1_SYSLOG.txt";
            File.AppendAllText(@"" + syslogPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "SYSTEM Load\n");
            File.AppendAllText(@"" + syslogPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "Path.ini Load\n");
            LogSetting();
            new EFEM();
        }

        //connect
        private void Button_Connect(object sender, EventArgs e)
        {
           

        }
        //disconnect
        private void Button_DisConnect(object sender, EventArgs e)
        {
            byte[] sendBuffer = Encoding.UTF8.GetBytes("exit");
            socket.Send(sendBuffer);
            socket.Close();
        }
        //receive
        private void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] receiveBuffer = new byte[40];

                    socket.Receive(receiveBuffer, 0, 4, SocketFlags.None);
                    int size = BitConverter.ToInt32(receiveBuffer, 0);

                    byte[] date = new byte[size];
                    socket.Receive(date, 0, size, SocketFlags.None);
                    string msg = Encoding.UTF8.GetString(date).Trim('\0');
                    Log(msg);


                    string[] val = msg.Split(new char[] { '/' });
                    File.AppendAllText(@"" + logPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "< PM1 : RECEIVE TO( " + val[0] + ") > => " + val[1] + "\n");
                    if (val[0] == "SERVER")
                    {
                        if (val[1] == "SETDATA")
                        {
                            PMobj.nPM = Convert.ToInt32(val[2]);
                            PMobj.nPMWafer = Convert.ToInt32(val[3]);
                            PMobj.nProcessTime = Convert.ToInt32(val[4]);
                            PMobj.nVac = Convert.ToInt32(val[5]);
                            PMobj.speed = Convert.ToInt32(val[6]);
                        }

                        if (val[1] == "FillChamber")
                        {
                            PMobj.nPM_Curcnt += Convert.ToInt32(val[3]);
                            //추가
                            PMobj.nPM_Accumul += Convert.ToInt32(val[3]);
                            SetText(labelPM_Acc, PMobj.nPM_Accumul);
                            SetText(labelPMcnt, PMobj.nPM_Curcnt);

                            if (PMobj.nPM_Curcnt == PMobj.nPMWafer)
                            {
                                NewSendtoServer("ProcessStart", //1
                                                              //PMpos
                                    Convert.ToInt32(val[2]), //2 
                                                             //nWafer
                                    Convert.ToInt32(val[3]), //3
                                                             //LLpos
                                    Convert.ToInt32(val[4])); //4
                                Delay((1000 * PMobj.nProcessTime) / PMobj.speed);
                                Log("PMpos = " + Convert.ToInt32(val[2]));
                                Log("LLpos = " + Convert.ToInt32(val[4]));
                                NewSendtoServer("ProcessEND", //1
                                                              //PMpos
                                    Convert.ToInt32(val[2]), //2 
                                                             //nWafer
                                    Convert.ToInt32(val[3]), //3
                                                             //LLpos
                                    Convert.ToInt32(val[4])); //4
                            }
                        }
                        if (val[1] == "PickPMtoVAC")
                        {
                            PMobj.nPM_Curcnt -= Convert.ToInt32(val[4]);
                            SetText(labelPMcnt, PMobj.nPM_Curcnt);
                        }
                        if (val[1] == "RESET")
                        {
                            Application.Restart();
                        }
                        if (val[1] == "EXIT")
                        {
                            Application.Exit();
                        }
                        if (val[1] == "Assemble")
                        {
                            WriteLine("Assemble");
                            //var sourcepath = @"C:\Users\BIT\Desktop\1001_Solution\03.Client Origin_PM\JUSUNG_Client1\bin\Debug\Log\PM1_LOG";
                            var sourcepath = Path.GetFullPath(@"" + sendPath);
                            // 서버에 접속한다.
                            var ipep = new IPEndPoint(IPAddress.Parse("192.168.1.6"), 9903);
                            // 디렉토리가 존재하는지
                            if (Directory.Exists(sourcepath))
                            {
                                // 바이너리 버퍼
                                var binary = CommpressDirectory(sourcepath);
                                // 소켓 생성
                                using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                                {
                                    // 접속
                                    client.Connect(ipep);
                                    // 상태 0 - 파일 크기를 보낸다.
                                    client.Send(new byte[] { 0 });
                                    // 송신 - 파일 이름 크기 Bigendian
                                    client.Send(BitConverter.GetBytes(binary.Length));
                                    // 상태 1 - 파일를 보낸다.
                                    client.Send(new byte[] { 1 });
                                    // 송신 - 파일
                                    client.Send(binary);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                NewSendtoServer("ERROR", -1, -1, -1, e.Message);
                File.AppendAllText(@"" + syslogPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + e.Message + "\n");
                Log(e.Message);
            }
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
        private void SetText(Label label, int val)
        {
            if (label.InvokeRequired) { label.Invoke(new TextSafety(ShowText), label, val); }
            else { label.Text = "" + val; }
        }

        private void NewSendtoServer(string msg, int pos, int things = 2, int things2 = 2, string errmsg = "null")
        {
            lock (lockthis)
            {
                File.AppendAllText(@"" + logPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "< PM1 : SEND > => " + msg + "\n");
                log.Info(msg);
            }
            byte[] sendBufferPM0 = Encoding.UTF8.GetBytes(
                         "PM0" + "/" +
                         msg + "/" +
                         pos + "/" +
                         things + "/" +
                         things2 + "/" +
                         errmsg + "/"
                         );

            int size = sendBufferPM0.Length;
            byte[] data_size = BitConverter.GetBytes(size);
            socket.Send(data_size);
            socket.Send(sendBufferPM0, 0, size, SocketFlags.None);
            Array.Clear(sendBufferPM0, 0x0, sendBufferPM0.Length);
        }
        private static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }

            return DateTime.Now;
        }

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

        delegate void LogSafety(string msg);
        private void Log(string msg)
        {
            //Cross Thread
            if (listBox1.InvokeRequired)
                listBox1.Invoke(new LogSafety(Log), msg);
            else
                listBox1.Items.Add(string.Format("[{0}]{1}", DateTime.Now.ToString(), msg));
        }
        delegate void Showmessage(string msg);
        private void ShowMsg(string msg)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke(new Showmessage(ShowMsg), msg);
            }
            else
            {
                richTextBox1.AppendText(msg);
                richTextBox1.AppendText("\r\n");
            }
            this.Activate();

            // 캐럿(커서)를 텍스트박스의 끝으로 내려준다.
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();   // 스크럴을 캐럿(커서)위치에 맞춰준다.
        }

       

        private void metroButton1_Click(object sender, EventArgs e)
        {
            // 서버 연결
            IPAddress ipaddress = IPAddress.Parse("192.168.1.6");
            //int port = int.Parse(textBox2.Text);
            int port = 9003;
            IPEndPoint endPoint = new IPEndPoint(ipaddress, port);

            //select client name
            switch (port)
            {
                case 9000:
                    ClientName = "TMC";
                    break;
                case 9001:
                    ClientName = "EFEM";
                    break;
                case 9002:
                    ClientName = "PM1";
                    break;
                case 9003:
                    ClientName = "PM2";
                    break;
                case 9004:
                    ClientName = "PM3";
                    break;
                default:
                    break;
            }
            //==================

            socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
                );

            // 연결하기
            Log("서버에 연결 시도중...");
            socket.Connect(endPoint);

            Log("서버에 접속됨");
            File.AppendAllText(@"" + syslogPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + ipaddress.ToString() + " CONNECT\n");
            // Receive 스레드 처리(서버 <--> 클라이언트)
            receiveThread = new Thread(new ThreadStart(Receive));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            ShowMsg("=======UPDATE=======");
            StringBuilder iniPath = new StringBuilder();
            GetPrivateProfileString("VERSION", "PM1", "(NONE)", iniPath, 32, @"..\Debug\Log\Path.ini");
            tbVer.Text = "Ver." + iniPath.ToString();
            string str = String.Format("[{0}] VERSION -> {1}", DateTime.Now.ToString(), iniPath.ToString());
            ShowMsg(str);
            GetPrivateProfileString("PATH", "PM1path", "(NONE)", iniPath, 32, @"..\Debug\Log\Path.ini");
            str = String.Format("[{0}] PATH -> {1}", DateTime.Now.ToString(), iniPath.ToString());
            ShowMsg(str);
            sendPath = iniPath.ToString();
            logPath = iniPath.ToString() + "\\PM1_LOG.txt";
            syslogPath = iniPath.ToString() + "\\PM1_SYSLOG.txt";
            GetPrivateProfileString("PATCH", "PM1msg", "(NONE)", iniPath, 32, @"..\Debug\Log\Path.ini");
            str = String.Format("[{0}] PATCH -> {1}", DateTime.Now.ToString(), iniPath.ToString());
            ShowMsg(str);
            ShowMsg("=======UPDATE=======");
        }
        public void LogSetting()
        {
            log4net.Repository.Hierarchy.Hierarchy hierarchy = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
            hierarchy.Configured = true;

            log4net.Appender.RollingFileAppender rollingAppender = new log4net.Appender.RollingFileAppender();


            rollingAppender.File = (@"" + logPath) + string.Format("_{0}_", DateTime.Now.ToString("yyyyMMddHH")); //2021092811
                                                                                                                  //2021092812

            //rollingAppender.AppendToFile = true;
            //rollingAppender.RollingStyle = log4net.Appender.RollingFileAppender.RollingMode.Date;

            rollingAppender.LockingModel = new log4net.Appender.FileAppender.MinimalLock();

            rollingAppender.DatePattern = "mm\".log\""; // 시간이 지나간 경우 이전 로그에 붙을 이름 구성
            //rollingAppender.PreserveLogFileNameExtension = true;
            rollingAppender.StaticLogFileName = false;
            rollingAppender.AppendToFile = true;
            rollingAppender.RollingStyle = log4net.Appender.RollingFileAppender.RollingMode.Composite;

            //log4net.Layout.PatternLayout layout = new log4net.Layout.PatternLayout("%date [%property{buildversion}] %-5level %logger - %message%newline");
            log4net.Layout.PatternLayout layout = new log4net.Layout.PatternLayout("%date [%-5level] %logger - %message%newline");
            rollingAppender.Layout = layout;

            hierarchy.Root.AddAppender(rollingAppender);
            rollingAppender.ActivateOptions();

            rollingAppender.Name = "PM1";

            hierarchy.Root.Level = log4net.Core.Level.All;
        }
    }
}