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
using System.IO;
using System.Runtime.InteropServices;
using System.IO.Compression;

namespace JUSUNG_Client
{
    public partial class Form_EFEM : MetroFramework.Forms.MetroForm
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("EFEM");
        public struct NewEFEMOBJECT
        {
            public int[] nLPM_Curcnt;

            public int nATM_Curcnt;
            public int nATM_Compcnt;

            public int nATMspeedPick;
            public int nATMspeedPlace;
            public int nATMspeedRotate;
            public int nALG_Curcnt;
            public int nALGspeedProcess;

            public int nLL;
            public int nLLWafer;
            public int[] nLL_Curcnt;
            //추가
            public int[] nLL_Compcnt;
            //추가
            //0912
            // public int[] nLPM_Compcnt;
            public int nLPM_Compcnt;
            public int nLPM_nWafer;
            public int nLLVentTime;
            public int nLLVentStabTime;
            //0912
            public string[] sLL_State;
            public string sATM_State;
            public int speed;
            public int DoorTime;
            public int nTotal;

        }
        public NewEFEMOBJECT EFEMobjN;

        public List<int> list = new List<int>();
        public System.Object lockthis = new System.Object();
        private Socket socket;  // 소켓

        private Thread receiveThread;    // 대화 수신용

        public Thread atmThreadUP; // ATM용 스레드
        public Thread atmThreadDOWN; // ATM용 스레드

        public Thread RefillThread;

        public string ClientName;
        public string logPath;
        public string syslogPath;
        public string sendPath;

        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        ///////////////////////
        #region FileTransferProtocal
        enum State
        {
            STATE,
            FILESIZE,
            FILEDOWNLOAD
        }

        class FTPFile
        {
            // 상태
            protected State state = State.STATE;
            // 파일
            public byte[] Binary { get; set; }
        }
        class FTPCon : FTPFile
        {
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
            public FTPCon(Socket socket)
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
        class FTP : Socket
        {
            private int EFEMport = 10000;
            readonly Form_EFEM f1 = new Form_EFEM();
            // 생성자
            public FTP() : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                base.Bind(new IPEndPoint(IPAddress.Any, EFEMport));

                base.Listen(10);

                BeginAccept(Accept, this);
            }

            private void Accept(IAsyncResult result)
            {
                var client = new FTPCon(EndAccept(result));

                BeginAccept(Accept, this);
            }
        }
        #endregion
        ///////////////////////


        public Form_EFEM()
        {
            InitializeComponent();
            
        }
        private void Form1_Load_1(object sender, EventArgs e)
        {
            
            textBox1.Focus();
            Log("클라이언트 로드됨!!");
            StringBuilder iniPath = new StringBuilder();
            GetPrivateProfileString("VERSION", "EFEMver", "(NONE)", iniPath, 32, @"..\Debug\Log\Path.ini");
            tbVer.Text = "Ver." + iniPath.ToString();
            GetPrivateProfileString("PATH", "EFEMpath", "(NONE)", iniPath, 32, @"..\Debug\Log\Path.ini");
            sendPath = iniPath.ToString();
            logPath = iniPath.ToString() + "\\EFEM_LOG.txt";
            syslogPath = iniPath.ToString() + "\\EFEM_SYSLOG.txt";
            File.AppendAllText(@"" + syslogPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "SYSTEM Load\n");
            File.AppendAllText(@"" + syslogPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "Path.ini Load\n");


            LogSetting();
            
            new FTP();
        }

        //connect
        private void metroButton2_Click(object sender, EventArgs e)
        {
            // 서버 연결
            IPAddress ipaddress = IPAddress.Parse("192.168.1.6");

            int port = int.Parse(textBox2.Text);
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
                    // 연결된 클라이언트가 보낸 데이터 수신
                    byte[] receiveBuffer = new byte[40];

                    socket.Receive(receiveBuffer, 0, 4, SocketFlags.None);
                    int size = BitConverter.ToInt32(receiveBuffer, 0);

                    byte[] date = new byte[size];
                    socket.Receive(date, 0, size, SocketFlags.None);
                    string msg = Encoding.UTF8.GetString(date).Trim('\0');
                    if (richTextBox1.InvokeRequired)
                    { richTextBox1.Invoke(new Showmessage(ShowMsg), msg); }

                    string[] val = msg.Split(new char[] { '/' });
                    File.AppendAllText(@"" + logPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "< EFEM : RECEIVE TO( " + val[0] + ") > => " + val[1] + "\n");
                    if (val[0] == "SERVER")
                    {
                        if (val[1] == "SETDATA")
                        {
                            EFEMobjN.nLPM_Curcnt = new int[2];
                            EFEMobjN.nLPM_nWafer = Convert.ToInt32(val[2]);
                            EFEMobjN.nLPM_Curcnt[0] = Convert.ToInt32(val[2]);
                            EFEMobjN.nLPM_Curcnt[1] = Convert.ToInt32(val[2]);


                            EFEMobjN.nATMspeedPick = Convert.ToInt32(val[3]);
                            EFEMobjN.nATMspeedPlace = Convert.ToInt32(val[4]);
                            EFEMobjN.nATMspeedRotate = Convert.ToInt32(val[5]);
                            EFEMobjN.nALGspeedProcess = Convert.ToInt32(val[6]);
                            EFEMobjN.nLL = Convert.ToInt32(val[7]);
                            EFEMobjN.nLLWafer = Convert.ToInt32(val[8]);


                            EFEMobjN.nLL_Curcnt = new int[4];
                            EFEMobjN.nLL_Compcnt = new int[4];
                            //추가
                            EFEMobjN.sLL_State = new string[4] { "Nothing", "Nothing", "Nothing", "Nothing" };
                            //추가
                            EFEMobjN.sATM_State = "NotWork";
                            EFEMobjN.speed = Convert.ToInt32(val[9]);
                            EFEMobjN.DoorTime = Convert.ToInt32(val[10]);
                            EFEMobjN.nLLVentTime = Convert.ToInt32(val[11]);
                            EFEMobjN.nLLVentStabTime = Convert.ToInt32(val[12]);

                            SetText(label3, EFEMobjN.nLPM_Curcnt[0]);
                            SetText(label19, EFEMobjN.nLPM_Curcnt[1]);
                   
                            atmThreadUP = new Thread(new ThreadStart(NewATMworkUP));
                            atmThreadUP.Start();

                            atmThreadDOWN = new Thread(new ThreadStart(NewATMworkDOWN));
                            atmThreadDOWN.Start();
                            atmThreadDOWN.Suspend();

                            RefillThread = new Thread(new ThreadStart(Refillwork));
                            RefillThread.Start();
                        }

                        if (val[1] == "PlaceVACtoLL")
                        {
                            EFEMobjN.nLL_Compcnt[Convert.ToInt32(val[2])] += Convert.ToInt32(val[4]);
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
                            //
                            var sourcepath = Path.GetFullPath(@"" + sendPath);
                            // 서버에 접속한다.
                            var ipep = new IPEndPoint(IPAddress.Parse("192.168.1.6"), 9900);
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
                        if (val[1] == "RESULT")
                        {
                            NewSendtoServer("RESULT", EFEMobjN.nLPM_Compcnt, EFEMobjN.nTotal);
                        }
                    }
                    Log("메시지 수신함");
                }
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
                //NewSendtoServer("ERROR", -1, -1, -1, e.Message);
                log.Fatal(e.Message);
                File.AppendAllText(@"" + syslogPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + e.Message + "\n");
                throw;
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

        static int emptyLL = 0;


        private void NewATMworkUP()
        {
            try
            {

                
                //First Pick
                EFEMobjN.nLPM_Curcnt[0] -= 1;

                SetText(label3, EFEMobjN.nLPM_Curcnt[0]);
                EFEMobjN.nATM_Curcnt += 1;
                NewSendtoServer("FirstPickWafertoAlg");
                Delay(Convert.ToInt32(2500 / EFEMobjN.speed));
                Delay(Convert.ToInt32(((1000 * (EFEMobjN.nATMspeedRotate)) / EFEMobjN.speed)));
                Delay(Convert.ToInt32(((1000 * (EFEMobjN.nATMspeedPick)) / EFEMobjN.speed)));
                SetText(label4, EFEMobjN.nATM_Curcnt);
                Delay(Convert.ToInt32(((1000 * (EFEMobjN.nATMspeedRotate)) / EFEMobjN.speed)));
                Delay(Convert.ToInt32(2500 / EFEMobjN.speed));

                //First Place ALG
                NewSendtoServer("FirstPlaceWafer");
                Delay(Convert.ToInt32(((1000 * EFEMobjN.nATMspeedPlace) / EFEMobjN.speed)));
                EFEMobjN.nATM_Curcnt -= 1;
                EFEMobjN.nALG_Curcnt += 1;
                SetText(label4, EFEMobjN.nATM_Curcnt);
                SetText(label8, EFEMobjN.nALG_Curcnt);
                Delay(Convert.ToInt32(((1000 * (EFEMobjN.nATMspeedRotate)) / EFEMobjN.speed)));

                //First Move LPM1
                NewSendtoServer("FirstMoveLPM1");
                Delay(Convert.ToInt32(2500 / EFEMobjN.speed));


                while (true)
                {
                    int position = NewIsEmptyLPM();
                    if (EFEMobjN.sATM_State != "Work"
                        && SumLL() < (EFEMobjN.nLL * EFEMobjN.nLLWafer)
                        )
                    {

                        if (position == 0)
                        {
                            if (EFEMobjN.nLPM_Curcnt[0] == 1 && EFEMobjN.nLPM_Curcnt[1] == 0)
                            {
                                EFEMobjN.nLPM_Curcnt[1] = 25;
                            }
                            SetText(label3, EFEMobjN.nLPM_Curcnt[0]);
                        }
                        if (position == 1)
                        {
                            if (EFEMobjN.nLPM_Curcnt[0] == 0 && EFEMobjN.nLPM_Curcnt[1] == 1)
                            {
                                EFEMobjN.nLPM_Curcnt[0] = 25;
                            }
                            SetText(label19, EFEMobjN.nLPM_Curcnt[1]);
                        }

                        //Center -> LPM1 Move
                        if (EFEMobjN.nLPM_Curcnt[position] != 0)
                        {
                            switch (position)
                            {

                                case 0:
                                    //Pick
                                    SetText(label3, EFEMobjN.nLPM_Curcnt[0]);
                                    SetText(label19, EFEMobjN.nLPM_Curcnt[1]);

                                    NewSendtoServer("PickWaferLPM1");
                                    Delay(Convert.ToInt32(((1000 * (EFEMobjN.nATMspeedRotate)) / EFEMobjN.speed)));
                                    Delay(Convert.ToInt32(((1000 * EFEMobjN.nATMspeedPick) / EFEMobjN.speed)));
                                    EFEMobjN.nLPM_Curcnt[0] -= 1;
                                    EFEMobjN.nATM_Curcnt += 1;
                                    SetText(label4, EFEMobjN.nATM_Curcnt);

                                    Delay(Convert.ToInt32(((1000 * (EFEMobjN.nATMspeedRotate)) / EFEMobjN.speed)));

                                    //LPM1->ALG Move
                                    NewSendtoServer("MoveLPM1toAlg");
                                    Delay(Convert.ToInt32(2500 / EFEMobjN.speed));

                                    //Alg Pick and Place
                                    NewSendtoServer("PickAndPlace");
                                    Delay(Convert.ToInt32(((1000 * (EFEMobjN.nATMspeedRotate)) / EFEMobjN.speed)));
                                    Delay(Convert.ToInt32(((1000 * EFEMobjN.nATMspeedPick) / EFEMobjN.speed)));

                                    EFEMobjN.nATM_Curcnt += EFEMobjN.nALG_Curcnt;
                                    EFEMobjN.nALG_Curcnt -= 1;
                                    SetText(label4, EFEMobjN.nATM_Curcnt);
                                    SetText(label8, EFEMobjN.nALG_Curcnt);

                                    Delay(Convert.ToInt32(((1000 * EFEMobjN.nATMspeedPlace) / EFEMobjN.speed)));

                                    EFEMobjN.nATM_Curcnt -= 1;
                                    EFEMobjN.nALG_Curcnt += 1;
                                    SetText(label4, EFEMobjN.nATM_Curcnt);
                                    SetText(label8, EFEMobjN.nALG_Curcnt);



                                    //LLMove and Pick
                                    NewSendtoServer("LLRotate");
                                    Delay(Convert.ToInt32(((1000 * EFEMobjN.nATMspeedRotate) / EFEMobjN.speed)));
                                    Delay(Convert.ToInt32(2500 / EFEMobjN.speed));
                                    Delay(Convert.ToInt32(((1000 * EFEMobjN.nATMspeedPlace) / EFEMobjN.speed)));


                                    //placeUpwaferLL                      
                                    NewSendtoServer("PlaceUpWaferLL", EFEMobjN.nLL);

                                    Delay(Convert.ToInt32((1000 * (EFEMobjN.DoorTime + EFEMobjN.nATMspeedPlace)) / EFEMobjN.speed));
                                    if (EFEMobjN.nLL_Curcnt[emptyLL] == EFEMobjN.nLLWafer)
                                    {
                                        if (emptyLL == EFEMobjN.nLL - 1)
                                        {
                                            emptyLL = 0;
                                        }
                                        else
                                        {
                                            emptyLL += 1;
                                        }
                                    }

                                    switch (emptyLL)
                                    {
                                        case 0:
                                            EFEMobjN.nLL_Curcnt[0] += 1;
                                            break;
                                        case 1:
                                            EFEMobjN.nLL_Curcnt[1] += 1;
                                            break;
                                        case 2:
                                            EFEMobjN.nLL_Curcnt[2] += 1;
                                            break;
                                        case 3:
                                            EFEMobjN.nLL_Curcnt[3] += 1;
                                            break;
                                        case -1:
                                            break;
                                        default:
                                            break;
                                    }

                                    NewSendtoServer("PlaceATMtoLL", emptyLL, EFEMobjN.nATM_Curcnt, EFEMobjN.nLL);

                                    //수정

                                    EFEMobjN.nATM_Curcnt -= 1;
                                    SetText(label4, EFEMobjN.nATM_Curcnt);

                                    //Center->LPM1 Move
                                    if (EFEMobjN.nLPM_Curcnt[0] != 0)
                                    {
                                        NewSendtoServer("CenterMoveLPM1");
                                        Delay(Convert.ToInt32(2500 / EFEMobjN.speed));
                                    }
                                    break;
                                case 1:
                                    SetText(label3, EFEMobjN.nLPM_Curcnt[0]);
                                    SetText(label19, EFEMobjN.nLPM_Curcnt[1]);

                                    //LL -> LPM2 Move
                                    NewSendtoServer("MoveWaferLPM2");
                                    Delay(Convert.ToInt32(2500 / EFEMobjN.speed));

                                    //PickLPM2
                                    NewSendtoServer("PickWaferLPM2");
                                    Delay(Convert.ToInt32(((1000 * EFEMobjN.nATMspeedRotate) / EFEMobjN.speed)));
                                    Delay(Convert.ToInt32(((1000 * EFEMobjN.nATMspeedPick) / EFEMobjN.speed)));
                                    EFEMobjN.nLPM_Curcnt[1] -= 1;

                                    EFEMobjN.nATM_Curcnt += 1;
                                    SetText(label4, EFEMobjN.nATM_Curcnt);
                                    Delay(Convert.ToInt32(((1000 * (EFEMobjN.nATMspeedRotate)) / EFEMobjN.speed)));

                                    //LPM2 -> Alg Move
                                    NewSendtoServer("MoveLPM2toAlg");
                                    Delay(Convert.ToInt32(2500 / EFEMobjN.speed));

                                    //Alg Pick and Place
                                    NewSendtoServer("PickAndPlace");
                                    Delay(Convert.ToInt32(((1000 * (EFEMobjN.nATMspeedRotate)) / EFEMobjN.speed)));
                                    Delay(Convert.ToInt32(((1000 * EFEMobjN.nATMspeedPick) / EFEMobjN.speed)));

                                    EFEMobjN.nATM_Curcnt += EFEMobjN.nALG_Curcnt;
                                    EFEMobjN.nALG_Curcnt -= 1;
                                    SetText(label4, EFEMobjN.nATM_Curcnt);
                                    SetText(label8, EFEMobjN.nALG_Curcnt);

                                    Delay(Convert.ToInt32(((1000 * EFEMobjN.nATMspeedPlace) / EFEMobjN.speed)));

                                    EFEMobjN.nATM_Curcnt -= 1;
                                    EFEMobjN.nALG_Curcnt += 1;
                                    SetText(label4, EFEMobjN.nATM_Curcnt);
                                    SetText(label8, EFEMobjN.nALG_Curcnt);

                                    //LLMove and Pick
                                    NewSendtoServer("LLRotate");
                                    Delay(Convert.ToInt32(((1000 * EFEMobjN.nATMspeedRotate) / EFEMobjN.speed)));
                                    Delay(Convert.ToInt32(2500 / EFEMobjN.speed));
                                    Delay(Convert.ToInt32(((1000 * EFEMobjN.nATMspeedPlace) / EFEMobjN.speed)));

                                    //placeUpwaferLL
                                    NewSendtoServer("PlaceUpWaferLL", EFEMobjN.nLL);

                                    if (EFEMobjN.nLL_Curcnt[emptyLL] == EFEMobjN.nLLWafer)
                                    {
                                        if (emptyLL == EFEMobjN.nLL - 1)
                                        {
                                            emptyLL = 0;
                                        }
                                        else
                                        {
                                            emptyLL++;
                                        }
                                    }
                                    //
                                    switch (emptyLL)
                                    {
                                        case 0:
                                            EFEMobjN.nLL_Curcnt[0] += 1;
                                            break;
                                        case 1:
                                            EFEMobjN.nLL_Curcnt[1] += 1;
                                            break;
                                        case 2:
                                            EFEMobjN.nLL_Curcnt[2] += 1;
                                            break;
                                        case 3:
                                            EFEMobjN.nLL_Curcnt[3] += 1;
                                            break;
                                        case -1:
                                            break;
                                        default:
                                            break;
                                    }
                                    NewSendtoServer("PlaceATMtoLL", emptyLL, EFEMobjN.nATM_Curcnt, EFEMobjN.nLL);
                                    Delay(Convert.ToInt32((1000 * EFEMobjN.DoorTime) / EFEMobjN.speed));
                                    //수정

                                    EFEMobjN.nATM_Curcnt -= 1;
                                    SetText(label4, EFEMobjN.nATM_Curcnt);

                                    if (EFEMobjN.nLPM_Curcnt[0] == 25 && EFEMobjN.nLPM_Curcnt[1] == 0)
                                    {
                                        NewSendtoServer("LPM1Move");
                                        Delay(Convert.ToInt32(2500 / EFEMobjN.speed));
                                    }
                                    break;
                                default:
                                    break;
                            }
                            Delay(Convert.ToInt32(2500 / EFEMobjN.speed));
                        }
                    }

                    Thread.Sleep(15);
                    if (SumLL() == (EFEMobjN.nLL * EFEMobjN.nLLWafer))
                    {
                        atmThreadDOWN.Resume();
                        atmThreadUP.Suspend();
                    }
                }
            }
            catch (Exception e)
            {
                NewSendtoServer("ERROR", -1, -1, -1, e.Message);
                File.AppendAllText(@"" + syslogPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + e.Message + "\n");
                log.Fatal(e.Message);
                WriteLine(e.Message);
                //throw;
            }
        }

        private void NewATMworkDOWN()
        {
            try
            {
                int LLpos = -1;
                int a = 0;
                int b = 0;
                while (true)
                {
                    for (int i = b; i < EFEMobjN.nLL; i++)
                    {
                        if (a == 0)
                        {
                            if (EFEMobjN.nLL_Compcnt[i] == EFEMobjN.nLLWafer && EFEMobjN.nLL_Curcnt[i] == EFEMobjN.nLLWafer)
                            {
                                Delay(Convert.ToInt32(1000 * (EFEMobjN.nLLVentStabTime + EFEMobjN.nLLVentTime) / EFEMobjN.speed));
                                LLpos = i;
                                a = 1;
                                b = i;
                                break;
                            }
                            else
                            {
                                LLpos = -1;
                            }
                        }

                        if (a == 1)
                        {
                            if (EFEMobjN.nLL_Compcnt[i] == 0)
                            {
                                LLpos = -1;
                                if (b == i)
                                    a = 0;
                                if (b == (EFEMobjN.nLL - 1))
                                    b = 0;
                            }
                            else
                            {
                                LLpos = i;
                                break;
                            }
                        }
                        if (b == EFEMobjN.nLL)
                            b = 0;

                    }

                    if (LLpos != -1)
                    {
                        if (EFEMobjN.nLL_Compcnt[LLpos] >= 2)
                        {
                            NewSendtoServer("PickLLtoATM", LLpos, 2, EFEMobjN.nLL);
                            Delay(Convert.ToInt32((1000 * EFEMobjN.DoorTime) / EFEMobjN.speed));
                            Delay(Convert.ToInt32((1000 * EFEMobjN.nATMspeedPick) / EFEMobjN.speed));
                            EFEMobjN.nLL_Compcnt[LLpos] -= 2;
                            EFEMobjN.nATM_Curcnt += 2;
                            SetText(label15, EFEMobjN.nATM_Curcnt);

                            Delay(Convert.ToInt32((1000 * (EFEMobjN.nATMspeedPlace + EFEMobjN.nATMspeedRotate)) / EFEMobjN.speed));
                            EFEMobjN.nLPM_Compcnt += EFEMobjN.nATM_Curcnt;
                            EFEMobjN.nATM_Curcnt -= 2;
                            SetText(label15, EFEMobjN.nATM_Curcnt);
                            NewSendtoServer("Total", 2);
                        }

                        else
                        {
                            NewSendtoServer("PickLLtoATM", LLpos, 1, EFEMobjN.nLL);
                            Delay(Convert.ToInt32((1000 * EFEMobjN.DoorTime) / EFEMobjN.speed));

                            Delay(Convert.ToInt32((1000 * EFEMobjN.nATMspeedPick) / EFEMobjN.speed));
                            EFEMobjN.nLL_Compcnt[LLpos] -= 1;
                            EFEMobjN.nATM_Curcnt += 1;
                            SetText(label15, EFEMobjN.nATM_Curcnt);

                            Delay(Convert.ToInt32((1000 * (EFEMobjN.nATMspeedPlace + EFEMobjN.nATMspeedRotate)) / EFEMobjN.speed));
                            EFEMobjN.nLPM_Compcnt += EFEMobjN.nATM_Curcnt;
                            EFEMobjN.nATM_Curcnt -= 1;
                            SetText(label15, EFEMobjN.nATM_Curcnt);
                            NewSendtoServer("Total", 1);
                        }

                        if (EFEMobjN.nLPM_Compcnt > 25)
                        {
                            EFEMobjN.nTotal += 25;
                            EFEMobjN.nLPM_Compcnt -= 25;
                        }
                    }
                    SetText(label10, EFEMobjN.nLPM_Compcnt);
                    SetText(labelTotal, EFEMobjN.nTotal);

                    if (LLpos != -1 && EFEMobjN.nLL_Compcnt[LLpos] == 0)
                    {
                        EFEMobjN.nLL_Curcnt[LLpos] = 0;
                    }

                    //cpu점유율을 낮추기위한 0.15초
                    Thread.Sleep(15);
                    if (LLpos == -1)
                    {
                        if (emptyLL == EFEMobjN.nLL - 1 && EFEMobjN.nLL_Curcnt[emptyLL] == 0)
                        {
                            emptyLL = 0;
                        }
                        atmThreadUP.Resume();
                        atmThreadDOWN.Suspend();
                    }
                }
            }
            catch (Exception e)
            {
                NewSendtoServer("ERROR", -1, -1, -1, e.Message);
                File.AppendAllText(@"" + syslogPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + e.Message + "\n");
                log.Fatal(e.Message);
                WriteLine(e.Message);
            }
        }
        private void Refillwork()
        {
            try
            {
                while (true)
                {
                    SetText(labelTotal, EFEMobjN.nTotal);
                    Thread.Sleep(150);
                }
            }
            catch (Exception e)
            {
                NewSendtoServer("ERROR", -1, -1, -1, e.Message);
                File.AppendAllText(@"" + syslogPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + e.Message + "\n");
                log.Fatal(e.Message);
                Log(e.Message);
            }
        }
        private void NewSendtoServer(string msg, int things = 1, int things2 = 1, int things3 = 1, string errmsg = "null")
        {
            lock (lockthis)
            {
                File.AppendAllText(@"" + logPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "< EFEM : SEND > => " + msg + "\n");
                log.Info(msg);
            }

            byte[] sendBufferEFEM = Encoding.UTF8.GetBytes(
                         "EFEM" + "/" + //0
                         msg + "/" + //1
                                     //LLpos
                         things + "/" + //2
                                        //nWafer
                         things2 + "/" +//3
                         things3 + "/" +
                         errmsg + "/"
                         );
            int size = sendBufferEFEM.Length;
            byte[] data_size = BitConverter.GetBytes(size);
            socket.Send(data_size);
            socket.Send(sendBufferEFEM, 0, size, SocketFlags.None);
            Array.Clear(sendBufferEFEM, 0x0, sendBufferEFEM.Length);
        }

        private int NewIsEmptyLPM()
        {
            for (int i = 0; i < EFEMobjN.nLPM_Curcnt.Count(); i++)
            {
                if (EFEMobjN.nLPM_Curcnt[i] != 0) { return i; }
            }
            return -1;
        }
        private int SumLL()
        {
            int sum = 0;
            for (int i = 0; i < EFEMobjN.nLL; i++)
            {
                sum += EFEMobjN.nLL_Curcnt[i];
            }
            return sum;
        }

        private void SetText(Label label, int val)
        {
            if (label.InvokeRequired) { label.Invoke(new TextSafety(ShowText), label, val); }
            else { label.Text = "" + val; }
        }

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


        // 송수신 메시지들 대화창에 출력
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
        //UPDATE   
        private void metroButton1_Click(object sender, EventArgs e)
        {
            ShowMsg("=======UPDATE=======");
            StringBuilder iniPath = new StringBuilder();
            GetPrivateProfileString("VERSION", "EFEMver", "(NONE)", iniPath, 32, @"..\Debug\Log\Path.ini");
            tbVer.Text = "Ver." + iniPath.ToString();
            string str = String.Format("[{0}] VERSION -> {1}", DateTime.Now.ToString(), iniPath.ToString());
            ShowMsg(str);
            GetPrivateProfileString("PATH", "EFEMpath", "(NONE)", iniPath, 32, @"..\Debug\Log\Path.ini");
            str = String.Format("[{0}] PATH -> {1}", DateTime.Now.ToString(), iniPath.ToString());
            ShowMsg(str);
            sendPath = iniPath.ToString();
            logPath = iniPath.ToString() + "\\EFEM_LOG.txt";
            syslogPath = iniPath.ToString() + "\\EFEM_SYSLOG.txt";
            GetPrivateProfileString("PATCH", "EFEMmsg", "(NONE)", iniPath, 32, @"..\Debug\Log\Path.ini");
            str = String.Format("[{0}] PATCH -> {1}", DateTime.Now.ToString(), iniPath.ToString());
            ShowMsg(str);
            logPath = iniPath.ToString() + "\\EFEM_LOG.txt";
            ShowMsg("=======UPDATE=======");
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

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

            rollingAppender.Name = "EFEM";

            hierarchy.Root.Level = log4net.Core.Level.All;
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            log.Debug("1234");
        }
    }
}
