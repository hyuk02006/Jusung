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
    public partial class Form_TM : MetroFramework.Forms.MetroForm
    {
        //수정
        public struct TMOBJECT
        {
            public int nVac;

            public int nLL;
            public int nLLWafer;
            public int[] nLL_Curcnt;
            //추가
            public int[] nLL_Compcnt;
            //추가
            public string[] sLL_State;

            public string[] sPM_State;
            public int nPM;
            public int nPMWafer;
            public int[] nPM_Curcnt;

            public int nLLPumpTime;
            public int nLLPumpStabTime;
            public int nLLVentTime;
            public int nLLVentStabTime;

            public int nVAC_Curcnt;
            public int nVACspeedPick;
            public int nVACspeedPlace;
            public int nVACspeedRotate;

            public bool[] DoorValve;
            public bool[] SlotValve;

            public int speed;
            public int DoorTime;
        }
        TMOBJECT TMobj;

        public struct COMPOBJ
        {
            public int PMpos;
            public int LLpos;
            public int nWafer;
        }
        //수정
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("TM");
        COMPOBJ Compobj;
        public List<COMPOBJ> TodoList = new List<COMPOBJ>();
        public List<bool> PMcompList = new List<bool>();
        public System.Object lockthis = new System.Object();
        private Socket socket;  // 소켓

        public Thread receiveThread;    // 대화 수신용

        public Thread LoadLockThread0;
        public Thread LoadLockThread1;
        public Thread LoadLockThread2;
        public Thread LoadLockThread3;

        public Thread vacThreadUP;
        public Thread vacThreadDown;


        public string ClientName;
        public string logPath;
        public string syslogPath;
        public string sendPath;

        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// /// /////////////////

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
            readonly Form_TM f1;
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
                WriteLine($"TMClient:(From:{remoteAddr.Address.ToString()}:{remoteAddr.Port},Connection time:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")})");
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
            private int EFEMport = 10001;
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

        /// /// /////////////////
        public Form_TM()
        {
            InitializeComponent();
        }
        private void Form1_Load_1(object sender, EventArgs e)
        {
            textBox1.Focus();
            Log("클라이언트 로드됨!!");
            StringBuilder iniPath = new StringBuilder();
            GetPrivateProfileString("VERSION", "TMver", "(NONE)", iniPath, 32, @"..\Debug\Log\Path.ini");
            tbVer.Text = "Ver." + iniPath.ToString();
            GetPrivateProfileString("PATH", "TMpath", "(NONE)", iniPath, 32, @"..\Debug\Log\Path.ini");
            sendPath = iniPath.ToString();
            logPath = iniPath.ToString() + "\\TM_LOG.txt";
            syslogPath = iniPath.ToString() + "\\TM_SYSLOG.txt";
            File.AppendAllText(@"" + syslogPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "SYSTEM Load\n");
            File.AppendAllText(@"" + syslogPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "Path.ini Load\n");
            LogSetting();
            new EFEM();
        }
        private void NewVACworkDOWN()
        {
            try
            {
                WriteLine("Down Thread 시작됨");
                WriteLine("----------------------------------------");
                foreach (var item in TodoList)
                {
                    WriteLine("[{0}]번 PM에서 [{1}] LL 으로 가져다놓기", item.PMpos, item.LLpos);
                }
                WriteLine("----------------------------------------");
                while (true)
                {
                    //PM에 가득 채워졌을때
                    if (TodoList.Count > 0)
                    {
                        for (int i = 0; i < TodoList.Count; i++)
                        {
                            if (TMobj.nPM_Curcnt[TodoList[i].PMpos] < TMobj.nVac)
                            {
                                NewSendtoServer("PickPMtoVAC", TodoList[i].LLpos, TodoList[i].PMpos, TMobj.nPM_Curcnt[TodoList[i].PMpos], TMobj.nLL, TMobj.nVac);
                                TMobj.nVAC_Curcnt += TMobj.nPM_Curcnt[TodoList[i].PMpos];
                                TMobj.nPM_Curcnt[TodoList[i].PMpos] = 0;
                                Delay(Convert.ToInt32((1000 * TMobj.DoorTime) / TMobj.speed));
                                Delay(Convert.ToInt32((1000 * (TMobj.nVACspeedPick + TMobj.nVACspeedRotate)) / TMobj.speed));

                                switch (TodoList[i].PMpos)
                                {
                                    case 0:
                                        SetText(PM0_nWafer, TMobj.nPM_Curcnt[TodoList[i].PMpos]);
                                        break;
                                    case 1:
                                        SetText(PM1_nWafer, TMobj.nPM_Curcnt[TodoList[i].PMpos]);
                                        break;
                                    case 2:
                                        SetText(PM2_nWafer, TMobj.nPM_Curcnt[TodoList[i].PMpos]);
                                        break;
                                    default:
                                        break;
                                } //settext switch
                                if (TMobj.nVAC_Curcnt == 2)
                                {
                                    SetText(vacUP, TMobj.nVAC_Curcnt / 2);
                                    SetText(vacDOWN, TMobj.nVAC_Curcnt / 2);
                                }
                                else
                                {
                                    SetText(vacUP, 1);
                                    SetText(vacDOWN, 0);
                                }

                                NewSendtoServer("PlaceVACtoLL", TodoList[i].LLpos, TodoList[i].PMpos, TMobj.nVAC_Curcnt, TMobj.nLL, TMobj.nVac);
                                TMobj.nLL_Compcnt[TodoList[i].LLpos] += TMobj.nVAC_Curcnt;
                                TMobj.nVAC_Curcnt = 0;
                                SetText(vacUP, TMobj.nVAC_Curcnt / 2);
                                SetText(vacDOWN, TMobj.nVAC_Curcnt / 2);

                                Delay(Convert.ToInt32((1000 * (TMobj.nVACspeedPlace + TMobj.nVACspeedRotate)) / TMobj.speed));
                                Delay(Convert.ToInt32((1000 * TMobj.DoorTime) / TMobj.speed));

                                switch (TodoList[i].LLpos)
                                {
                                    case 0:
                                        SetText(labelLLcomp0, TMobj.nLL_Compcnt[TodoList[i].LLpos]);
                                        break;
                                    case 1:
                                        SetText(labelLLcomp1, TMobj.nLL_Compcnt[TodoList[i].LLpos]);
                                        break;
                                    case 2:
                                        SetText(labelLLcomp2, TMobj.nLL_Compcnt[TodoList[i].LLpos]);
                                        break;
                                    case 3:
                                        SetText(labelLLcomp3, TMobj.nLL_Compcnt[TodoList[i].LLpos]);
                                        break;
                                    default:
                                        break;
                                } //settext switch
                            }
                            else
                            {
                                //pick
                                NewSendtoServer("PickPMtoVAC", TodoList[i].LLpos, TodoList[i].PMpos, TMobj.nVac, TMobj.nLL, TMobj.nVac);
                                TMobj.nPM_Curcnt[TodoList[i].PMpos] -= TMobj.nVac;
                                Delay(Convert.ToInt32((1000 * TMobj.DoorTime) / TMobj.speed));
                                Delay(Convert.ToInt32((1000 * (TMobj.nVACspeedPick + TMobj.nVACspeedRotate)) / TMobj.speed));
                                switch (TodoList[i].PMpos)
                                {
                                    case 0:
                                        SetText(PM0_nWafer, TMobj.nPM_Curcnt[TodoList[i].PMpos]);
                                        break;
                                    case 1:
                                        SetText(PM1_nWafer, TMobj.nPM_Curcnt[TodoList[i].PMpos]);
                                        break;
                                    case 2:
                                        SetText(PM2_nWafer, TMobj.nPM_Curcnt[TodoList[i].PMpos]);
                                        break;
                                    default:
                                        break;
                                } //settext switch
                                TMobj.nVAC_Curcnt += TMobj.nVac;
                                SetText(vacUP, TMobj.nVAC_Curcnt / 2);
                                SetText(vacDOWN, TMobj.nVAC_Curcnt / 2);

                                NewSendtoServer("PlaceVACtoLL", TodoList[i].LLpos, TodoList[i].PMpos, TMobj.nVAC_Curcnt, TMobj.nLL, TMobj.nVac);
                                TMobj.nLL_Compcnt[TodoList[i].LLpos] += TMobj.nVAC_Curcnt;
                                TMobj.nVAC_Curcnt -= TMobj.nVac;
                                Delay(Convert.ToInt32((1000 * TMobj.DoorTime) / TMobj.speed));
                                Delay(Convert.ToInt32((1000 * (TMobj.nVACspeedPlace + TMobj.nVACspeedRotate)) / TMobj.speed));
                                SetText(vacUP, TMobj.nVAC_Curcnt / 2);
                                SetText(vacDOWN, TMobj.nVAC_Curcnt / 2);

                                switch (TodoList[i].LLpos)
                                {
                                    case 0:
                                        SetText(labelLLcomp0, TMobj.nLL_Compcnt[TodoList[i].LLpos]);
                                        break;
                                    case 1:
                                        SetText(labelLLcomp1, TMobj.nLL_Compcnt[TodoList[i].LLpos]);
                                        break;
                                    case 2:
                                        SetText(labelLLcomp2, TMobj.nLL_Compcnt[TodoList[i].LLpos]);
                                        break;
                                    case 3:
                                        SetText(labelLLcomp3, TMobj.nLL_Compcnt[TodoList[i].LLpos]);
                                        break;
                                    default:
                                        break;
                                } //settext switch
                            }

                        }
                        TodoList.Clear();
                        PMcompList.Clear();
                    }

                    //cpu점유율을 낮추기위한 0.15초
                    Thread.Sleep(15);
                    if (TodoList.Count == 0)
                    {
                        vacThreadUP.Resume();
                        vacThreadDown.Suspend();
                    }
                }
            }
            catch (Exception e)
            {
                NewSendtoServer("ERROR", -1, -1, -1, -1, -1, e.Message);
                File.AppendAllText(@"" + syslogPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + e.Message + "\n");
                log.Fatal(e.Message);
                Log(e.Message);
                WriteLine(e.Message);
            }
        }
        private void NewVACworkUP()
        {
            try
            {
                WriteLine("Up Thread 시작됨");
                //0924추가
                //반복횟수 결정
                int cnt = 0;
                if (TMobj.nPMWafer % TMobj.nVac == 0)
                {
                    cnt = TMobj.nPMWafer / TMobj.nVac;
                }
                else
                {
                    cnt = TMobj.nPMWafer / TMobj.nVac + 1;
                }
                //0924추가

                int sumPM = 0;
                while (true)
                {
                    int LLpos = -1;
                    int PMpos = -1;
                    //꽉 채워진 LL 찾기
                    for (int i = 0; i < TMobj.nLL; i++)
                    {
                        if (TMobj.nLL_Curcnt[i] == TMobj.nLLWafer)
                        {
                            LLpos = i;
                            break;
                        }
                    }
                    //빈 PM 찾기
                    for (int i = 0; i < TMobj.nPM; i++)
                    {
                        if (TMobj.nPM_Curcnt[i] < TMobj.nPMWafer)
                        {
                            PMpos = i;
                            break;
                        }
                    }


                    if (PMpos != -1 && LLpos != -1 && TMobj.nPM_Curcnt[PMpos] < TMobj.nPMWafer)
                    {
                        switch (PMpos)
                        {
                            case 0:
                                SetTextString(pv0, "Open");
                                break;
                            case 1:
                                SetTextString(pv1, "Open");
                                break;
                            case 2:
                                SetTextString(pv2, "Open");
                                break;
                            default:
                                break;
                        } //settext PM valve switch Open
                        for (int i = 0; i < cnt; i++)
                        {
                            Delay(Convert.ToInt32((1000 * TMobj.nVACspeedPick) / TMobj.speed));

                            //pick - 121 / 242
                            if (TMobj.nVac > TMobj.nLLWafer)
                            {
                                NewSendtoServer("VacPick", LLpos, PMpos, TMobj.nLL_Curcnt[LLpos], TMobj.nLL, TMobj.nVac);
                                Delay(Convert.ToInt32((1000 * TMobj.DoorTime) / TMobj.speed));
                                TMobj.nVAC_Curcnt += TMobj.nLL_Curcnt[LLpos];
                                TMobj.nLL_Curcnt[LLpos] = 0;
                                Delay(Convert.ToInt32((1000 * (TMobj.nVACspeedPick + TMobj.nVACspeedRotate)) / TMobj.speed));

                                switch (LLpos)
                                {
                                    case 0:
                                        SetText(LL0_nWafer, TMobj.nLL_Curcnt[LLpos]);
                                        break;
                                    case 1:
                                        SetText(LL1_nWafer, TMobj.nLL_Curcnt[LLpos]);
                                        break;
                                    case 2:
                                        SetText(LL2_nWafer, TMobj.nLL_Curcnt[LLpos]);
                                        break;
                                    case 3:
                                        SetText(LL3_nWafer, TMobj.nLL_Curcnt[LLpos]);
                                        break;
                                    default:
                                        break;
                                } //settext switch

                                //vac count+
                                if (TMobj.nLLWafer == 1)
                                {
                                    SetText(vacUP, TMobj.nVAC_Curcnt);
                                    SetText(vacDOWN, 0);
                                }
                                else
                                {
                                    SetText(vacUP, TMobj.nVAC_Curcnt / 2);
                                    SetText(vacDOWN, TMobj.nVAC_Curcnt / 2);
                                }
                                //place
                                NewSendtoServer("VacPlace", LLpos, PMpos, TMobj.nVAC_Curcnt, TMobj.nLL, TMobj.nVac);
                                Delay(Convert.ToInt32((1000 * TMobj.DoorTime) / TMobj.speed));
                                Delay(Convert.ToInt32((1000 * (TMobj.nVACspeedPlace + TMobj.nVACspeedRotate)) / TMobj.speed));
                                NewSendtoServer("FillChamber", PMpos, TMobj.nVAC_Curcnt, LLpos);
                                TMobj.nPM_Curcnt[PMpos] += TMobj.nVAC_Curcnt;
                                Compobj.nWafer = TMobj.nVAC_Curcnt;
                                TMobj.nVAC_Curcnt = 0;

                                switch (PMpos)
                                {
                                    case 0:
                                        SetText(PM0_nWafer, TMobj.nPM_Curcnt[PMpos]);
                                        break;
                                    case 1:
                                        SetText(PM1_nWafer, TMobj.nPM_Curcnt[PMpos]);
                                        break;
                                    case 2:
                                        SetText(PM2_nWafer, TMobj.nPM_Curcnt[PMpos]);
                                        break;
                                    default:
                                        break;
                                } //settext switch
                                  //vac count-
                                SetText(vacUP, TMobj.nVAC_Curcnt / 2);
                                SetText(vacDOWN, TMobj.nVAC_Curcnt / 2);

                                Compobj.LLpos = LLpos;
                                Compobj.PMpos = PMpos;

                                TodoList.Add(Compobj);
                            }
                            //pick - 222 / 323 / 424 / 525 / 626
                            //pick - 242 / 444 / 646
                            else
                            {
                                if (TMobj.nVac > TMobj.nLL_Curcnt[LLpos])
                                {
                                    NewSendtoServer("VacPick", LLpos, PMpos, TMobj.nLL_Curcnt[LLpos], TMobj.nLL, TMobj.nVac);
                                    Delay(Convert.ToInt32((1000 * TMobj.DoorTime) / TMobj.speed));
                                    TMobj.nVAC_Curcnt += TMobj.nLL_Curcnt[LLpos];
                                    TMobj.nLL_Curcnt[LLpos] -= TMobj.nVAC_Curcnt;
                                    Delay(Convert.ToInt32((1000 * (TMobj.nVACspeedPick + TMobj.nVACspeedRotate)) / TMobj.speed));
                                    switch (LLpos)
                                    {
                                        case 0:
                                            SetText(LL0_nWafer, TMobj.nLL_Curcnt[LLpos]);
                                            break;
                                        case 1:
                                            SetText(LL1_nWafer, TMobj.nLL_Curcnt[LLpos]);
                                            break;
                                        case 2:
                                            SetText(LL2_nWafer, TMobj.nLL_Curcnt[LLpos]);
                                            break;
                                        case 3:
                                            SetText(LL3_nWafer, TMobj.nLL_Curcnt[LLpos]);
                                            break;
                                        default:
                                            break;
                                    } //settext switch
                                    //vac count+
                                    if (TMobj.nVAC_Curcnt == 1)
                                    {
                                        SetText(vacUP, 1);
                                        SetText(vacDOWN, 0);
                                    }
                                    else
                                    {
                                        SetText(vacUP, TMobj.nVAC_Curcnt / 2);
                                        SetText(vacDOWN, TMobj.nVAC_Curcnt / 2);
                                    }
                                    //place
                                    NewSendtoServer("VacPlace", LLpos, PMpos, TMobj.nVAC_Curcnt, TMobj.nLL, TMobj.nVac);
                                    Delay(Convert.ToInt32((1000 * TMobj.DoorTime) / TMobj.speed));
                                    Delay(Convert.ToInt32((1000 * (TMobj.nVACspeedPlace + TMobj.nVACspeedRotate)) / TMobj.speed));
                                    TMobj.nPM_Curcnt[PMpos] += TMobj.nVAC_Curcnt;
                                    NewSendtoServer("FillChamber", PMpos, TMobj.nVAC_Curcnt, LLpos);
                                    TMobj.nVAC_Curcnt -= TMobj.nVAC_Curcnt;
                                    switch (PMpos)
                                    {
                                        case 0:
                                            SetText(PM0_nWafer, TMobj.nPM_Curcnt[PMpos]);
                                            break;
                                        case 1:
                                            SetText(PM1_nWafer, TMobj.nPM_Curcnt[PMpos]);
                                            break;
                                        case 2:
                                            SetText(PM2_nWafer, TMobj.nPM_Curcnt[PMpos]);
                                            break;
                                        default:
                                            break;
                                    } //settext switch
                                      //vac count-
                                    SetText(vacUP, TMobj.nVAC_Curcnt / 2);
                                    SetText(vacDOWN, TMobj.nVAC_Curcnt / 2);
                                    Compobj.LLpos = LLpos;
                                    Compobj.PMpos = PMpos;
                                    TodoList.Add(Compobj);
                                }
                                else
                                {
                                    NewSendtoServer("VacPick", LLpos, PMpos, TMobj.nLL_Curcnt[LLpos], TMobj.nLL, TMobj.nVac);
                                    Delay(Convert.ToInt32((1000 * TMobj.DoorTime) / TMobj.speed));
                                    TMobj.nLL_Curcnt[LLpos] -= TMobj.nVac;
                                    Delay(Convert.ToInt32((1000 * (TMobj.nVACspeedPick + TMobj.nVACspeedRotate)) / TMobj.speed));

                                    switch (LLpos)
                                    {
                                        case 0:
                                            SetText(LL0_nWafer, TMobj.nLL_Curcnt[LLpos]);
                                            break;
                                        case 1:
                                            SetText(LL1_nWafer, TMobj.nLL_Curcnt[LLpos]);
                                            break;
                                        case 2:
                                            SetText(LL2_nWafer, TMobj.nLL_Curcnt[LLpos]);
                                            break;
                                        case 3:
                                            SetText(LL3_nWafer, TMobj.nLL_Curcnt[LLpos]);
                                            break;
                                        default:
                                            break;
                                    } //settext switch
                                    TMobj.nVAC_Curcnt += TMobj.nVac;

                                    //vac count+
                                    SetText(vacUP, TMobj.nVAC_Curcnt / 2);
                                    SetText(vacDOWN, TMobj.nVAC_Curcnt / 2);

                                    //place
                                    NewSendtoServer("VacPlace", LLpos, PMpos, TMobj.nVAC_Curcnt, TMobj.nLL, TMobj.nVac);
                                    Delay(Convert.ToInt32((1000 * TMobj.DoorTime) / TMobj.speed));
                                    Delay(Convert.ToInt32((1000 * (TMobj.nVACspeedPlace + TMobj.nVACspeedRotate)) / TMobj.speed));
                                    TMobj.nVAC_Curcnt -= TMobj.nVac;
                                    NewSendtoServer("FillChamber", PMpos, TMobj.nVac, LLpos);
                                    TMobj.nPM_Curcnt[PMpos] += TMobj.nVac;
                                    switch (PMpos)
                                    {
                                        case 0:
                                            SetText(PM0_nWafer, TMobj.nPM_Curcnt[PMpos]);
                                            break;
                                        case 1:
                                            SetText(PM1_nWafer, TMobj.nPM_Curcnt[PMpos]);
                                            break;
                                        case 2:
                                            SetText(PM2_nWafer, TMobj.nPM_Curcnt[PMpos]);
                                            break;
                                        default:
                                            break;
                                    } //settext switch
                                      //vac count-
                                    SetText(vacUP, TMobj.nVAC_Curcnt / 2);
                                    SetText(vacDOWN, TMobj.nVAC_Curcnt / 2);
                                    Compobj.LLpos = LLpos;
                                    Compobj.PMpos = PMpos;
                                    TodoList.Add(Compobj);
                                }
                            }

                        }

                        switch (PMpos)
                        {
                            case 0:
                                SetTextString(pv0, "Close");
                                break;
                            case 1:
                                SetTextString(pv1, "Close");
                                break;
                            case 2:
                                SetTextString(pv2, "Close");
                                break;
                            default:
                                break;
                        } //settext PM valve switch Close

                        WriteLine(TMobj.nPM_Curcnt[PMpos]);
                        sumPM += TMobj.nPM_Curcnt[PMpos];
                        WriteLine("sumPM = " + sumPM);

                    }

                    Thread.Sleep(15);
                    if (sumPM == TMobj.nPM * TMobj.nPMWafer
                        && PMcompList.Count == TMobj.nPM)
                    {
                        sumPM = 0;
                        vacThreadDown.Resume();
                        vacThreadUP.Suspend();
                    }
                }
            }
            catch (Exception e)
            {
                NewSendtoServer("ERROR", -1, -1, -1, -1, -1, e.Message);
                File.AppendAllText(@"" + syslogPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + e.Message + "\n");
                log.Fatal(e.Message);
                Log(e.Message);
                WriteLine(e.Message);
            }
        }
        private void NewLoadLockWork(
            Label labelDV, Label labelSV, Label labelLL_Curcnt, Label labelLL_CompCnt, Label labelState,
            int LLpos, int nWafer,
            Thread LLthread
            )
        {
            try
            {
                WriteLine("Thread" + LLpos + " 실행됨");

                while (true)
                {
                    SetText(labelLL_Curcnt, TMobj.nLL_Curcnt[LLpos]);

                    if (TMobj.nLL_Curcnt[LLpos] == TMobj.nLLWafer
                        && TMobj.sLL_State[LLpos] == "Nothing")
                    {
                        SetTextString(labelDV, "Close");

                        NewSendtoServer("Pump", LLpos, TMobj.nLL);
                        TMobj.sLL_State[LLpos] = "Pump";

                        SetTextString(labelState, "Pumping");
                        Delay(Convert.ToInt32((1000 * TMobj.nLLPumpTime) / TMobj.speed));

                        SetTextString(labelState, "PumpStab");
                        Delay(Convert.ToInt32((1000 * TMobj.nLLPumpStabTime) / TMobj.speed));

                        SetTextString(labelState, "PumpEND");

                        SetTextString(labelSV, "Open");
                        TMobj.sLL_State[LLpos] = "PumpEnd";
                    }

                    if (TMobj.nLL_Compcnt[LLpos] == TMobj.nLLWafer
                        && TMobj.sLL_State[LLpos] == "PumpEnd")
                    {
                        SetTextString(labelSV, "Close");

                        NewSendtoServer("Vent", LLpos, TMobj.nLL);
                        TMobj.sLL_State[LLpos] = "Vent";

                        SetTextString(labelState, "Venting");
                        Delay(Convert.ToInt32((1000 * TMobj.nLLVentTime) / TMobj.speed));

                        SetTextString(labelState, "VentStab");
                        Delay(Convert.ToInt32((1000 * TMobj.nLLVentStabTime) / TMobj.speed));

                        SetTextString(labelState, "VentEND");
                        SetTextString(labelDV, "Open");
                        TMobj.sLL_State[LLpos] = "Nothing";
                    }
                    //cpu점유율을 낮추기위한 0.15초
                    Thread.Sleep(15);
                }
            }
            catch (Exception e)
            {
                NewSendtoServer("ERROR", -1, -1, -1, -1, -1, e.Message);
                File.AppendAllText(@"" + syslogPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + e.Message + "\n");
                log.Fatal(e.Message);
                Log(e.Message);                
            }

        }

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
                Label labelDV = null;
                Label labelSV = null;
                Label labelLL_Curcnt = null;
                Label labelLL_CompCnt = null;
                Label labelState = null;
                byte[] receiveBuffer = null;
                int size = 0;
                byte[] date = null;
                string msg = null;
                string[] val = null;
                while (true)
                {
                    receiveBuffer = new byte[40];

                    socket.Receive(receiveBuffer, 0, 4, SocketFlags.None);
                    size = BitConverter.ToInt32(receiveBuffer, 0);

                    date = new byte[size];
                    socket.Receive(date, 0, size, SocketFlags.None);
                    msg = Encoding.UTF8.GetString(date).Trim('\0');
                    if (richTextBox1.InvokeRequired) { richTextBox1.Invoke(new Showmessage(ShowMsg), msg); }

                    //Decoding
                    val = msg.Split(new char[] { '/' });
                    File.AppendAllText(@"" + logPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "< TM : RECEIVE TO( " + val[0] + ") > => " + val[1] + "\n");
                    if (val[0] == "SERVER")
                    {
                        if (val[1] == "RESET")
                        {
                            Application.Restart();
                        }
                        if (val[1] == "EXIT")
                        {
                            Application.Exit();
                        }
                        if (val[1] == "SETDATA")
                        {
                            TMobj.nVac = Convert.ToInt32(val[2]);
                            TMobj.nLL = Convert.ToInt32(val[3]);
                            TMobj.nLLWafer = Convert.ToInt32(val[4]);
                            TMobj.nPM = Convert.ToInt32(val[5]);
                            TMobj.nPMWafer = Convert.ToInt32(val[6]);
                            TMobj.nLLPumpTime = Convert.ToInt32(val[7]);
                            TMobj.nLLPumpStabTime = Convert.ToInt32(val[8]);
                            TMobj.nLLVentTime = Convert.ToInt32(val[9]);
                            TMobj.nLLVentStabTime = Convert.ToInt32(val[10]);
                            TMobj.nVACspeedPick = Convert.ToInt32(val[11]);
                            TMobj.nVACspeedPlace = Convert.ToInt32(val[12]);
                            TMobj.nVACspeedRotate = Convert.ToInt32(val[13]);
                            TMobj.speed = Convert.ToInt32(val[14]);
                            TMobj.DoorTime = Convert.ToInt32(val[15]);

                            TMobj.sLL_State = new string[4] { "Nothing", "Nothing", "Nothing", "Nothing" };
                            TMobj.sPM_State = new string[3] { "Nothing", "Nothing", "Nothing" };

                            TMobj.nLL_Curcnt = new int[4];
                            //추가
                            TMobj.nLL_Compcnt = new int[4];
                            //추가
                            TMobj.nPM_Curcnt = new int[3];

                            vacThreadUP = new Thread(new ThreadStart(NewVACworkUP));
                            vacThreadUP.Start();

                            vacThreadDown = new Thread(new ThreadStart(NewVACworkDOWN));
                            vacThreadDown.Start();
                            vacThreadDown.Suspend();
                        }
                        if (val[1] == "PlaceATMtoLL")
                        {                          
                            TMobj.nLL_Curcnt[Convert.ToInt32(val[2])] += Convert.ToInt32(val[3]);

                            switch (Convert.ToInt32(val[2]))
                            {
                                case 0:
                                    if (LoadLockThread0 == null)
                                    {
                                        labelDV = dv0;
                                        labelSV = sv0;
                                        labelLL_Curcnt = LL0_nWafer;
                                        labelLL_CompCnt = labelLLcomp0;
                                        labelState = lls0;
                                        LoadLockThread0 = new Thread(() => NewLoadLockWork(labelDV, labelSV, labelLL_Curcnt, labelLL_CompCnt, labelState, Convert.ToInt32(val[2]), Convert.ToInt32(val[3]), LoadLockThread0));
                                        LoadLockThread0.Start();
                                    }
                                    break;
                                case 1:
                                    if (LoadLockThread1 == null)
                                    {
                                        labelDV = dv1;
                                        labelSV = sv1;
                                        labelLL_Curcnt = LL1_nWafer;
                                        labelLL_CompCnt = labelLLcomp1;
                                        labelState = lls1;
                                        LoadLockThread1 = new Thread(() => NewLoadLockWork(labelDV, labelSV, labelLL_Curcnt, labelLL_CompCnt, labelState, Convert.ToInt32(val[2]), Convert.ToInt32(val[3]), LoadLockThread1));
                                        LoadLockThread1.Start();
                                    }
                                    break;
                                case 2:
                                    if (LoadLockThread2 == null)
                                    {
                                        labelDV = dv2;
                                        labelSV = sv2;
                                        labelLL_Curcnt = LL2_nWafer;
                                        labelLL_CompCnt = labelLLcomp2;
                                        labelState = lls2;
                                        LoadLockThread2 = new Thread(() => NewLoadLockWork(labelDV, labelSV, labelLL_Curcnt, labelLL_CompCnt, labelState, Convert.ToInt32(val[2]), Convert.ToInt32(val[3]), LoadLockThread2));
                                        LoadLockThread2.Start();
                                    }
                                    break;
                                case 3:
                                    if (LoadLockThread3 == null)
                                    {
                                        labelDV = dv3;
                                        labelSV = sv3;
                                        labelLL_Curcnt = LL3_nWafer;
                                        labelLL_CompCnt = labelLLcomp3;
                                        labelState = lls3;
                                        LoadLockThread3 = new Thread(() => NewLoadLockWork(labelDV, labelSV, labelLL_Curcnt, labelLL_CompCnt, labelState, Convert.ToInt32(val[2]), Convert.ToInt32(val[3]), LoadLockThread3));
                                        LoadLockThread3.Start();
                                    }
                                    break;
                                default:
                                    break;
                            }

                            SetTextString(labelDV, "Open");
                        }

                        if (val[1] == "ProcessEND")
                        {
                            PMcompList.Add(true);
                        }

                        if (val[1] == "PickLLtoATM")
                        {
                            TMobj.nLL_Compcnt[Convert.ToInt32(val[2])] -= Convert.ToInt32(val[3]);
                            switch (Convert.ToInt32(val[2]))
                            {
                                case 0:
                                    SetText(labelLLcomp0, TMobj.nLL_Compcnt[0]);
                                    break;
                                case 1:
                                    SetText(labelLLcomp1, TMobj.nLL_Compcnt[1]);
                                    break;
                                case 2:
                                    SetText(labelLLcomp2, TMobj.nLL_Compcnt[2]);
                                    break;
                                case 3:
                                    SetText(labelLLcomp3, TMobj.nLL_Compcnt[3]);
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (val[1] == "Assemble")
                        {
                            WriteLine("Assemble");
                            var sourcepath = Path.GetFullPath(@"" + sendPath);
                            // 서버에 접속한다.
                            var ipep = new IPEndPoint(IPAddress.Parse("192.168.1.6"), 9901);
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

                    Log("메시지 수신함");
                }
            }
            catch (Exception e)
            {
                NewSendtoServer("ERROR", -1, -1, -1, -1, -1, e.Message);
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

        private void NewSendtoServer(string msg, int pos, int things = -1, int things2 = -1, int things3 = -1, int things4 = -1, string errmsg = "null")
        {
            lock (lockthis)
            {
                File.AppendAllText(@"" + logPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "< TM : SEND > => " + msg + "\n");
                log.Info(msg);
            }
            byte[] sendBufferTM = Encoding.UTF8.GetBytes(
                         "TM" + "/" +
                         msg + "/" +
                         pos + "/" +
                         things + "/" +
                         things2 + "/" +
                         things3 + "/" +
                         things4 + "/" +
                         errmsg + "/"
                         );
            int size = sendBufferTM.Length;
            byte[] data_size = BitConverter.GetBytes(size);
            socket.Send(data_size);
            socket.Send(sendBufferTM, 0, size, SocketFlags.None);
            Array.Clear(sendBufferTM, 0x0, sendBufferTM.Length);
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
        // 송수신 메시지들 대화창에 출력
        delegate void TextSafety(Label label, int Wafercount);
        public void SetText(Label label, object val)
        {
            if (label.InvokeRequired) { label.Invoke(new TextSafety(ShowText), label, val); }
            else { label.Text = "" + val; }
        }
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

        delegate void TextSafetyString(Label label, string sWaferState);
        private void SetTextString(Label label, string val)
        {
            if (label.InvokeRequired) { label.Invoke(new TextSafetyString(ShowTextString), label, val); }
            else { label.Text = "" + val; }
        }
        private void ShowTextString(Label label, string sWaferState)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new TextSafetyString(ShowTextString), label, sWaferState);
            }

            else
            {
                label.Text = "" + sWaferState;
            }
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


        delegate void LogSafety(string msg);
        private void Log(string msg)
        {
            //Cross Thread
            if (listBox1.InvokeRequired)
                listBox1.Invoke(new LogSafety(Log), msg);
            else
                listBox1.Items.Add(string.Format("[{0}]{1}", DateTime.Now.ToString(), msg));
        }

        private void button2_Click(object sender, EventArgs e)
        {
          
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            // 서버 연결
            IPAddress ipaddress = IPAddress.Parse("192.168.1.6");
            //int port = int.Parse(textBox2.Text);
            int port = 9000;
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

            TMobj.nLL_Curcnt = new int[4];
            TMobj.sLL_State = new string[4];
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
            GetPrivateProfileString("VERSION", "TMver", "(NONE)", iniPath, 32, @"..\Debug\Log\Path.ini");
            tbVer.Text = "Ver." + iniPath.ToString();
            string str = String.Format("[{0}] VERSION -> {1}", DateTime.Now.ToString(), iniPath.ToString());
            ShowMsg(str);
            GetPrivateProfileString("PATH", "TMpath", "(NONE)", iniPath, 32, @"..\Debug\Log\Path.ini");
            str = String.Format("[{0}] PATH -> {1}", DateTime.Now.ToString(), iniPath.ToString());
            ShowMsg(str);
            sendPath = iniPath.ToString();
            logPath = iniPath.ToString() + "\\TM_LOG.txt";
            syslogPath = iniPath.ToString() + "\\TM_SYSLOG.txt";
            GetPrivateProfileString("PATCH", "TMmsg", "(NONE)", iniPath, 32, @"..\Debug\Log\Path.ini");
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

            rollingAppender.Name = "TM";

            hierarchy.Root.Level = log4net.Core.Level.All;
        }

    }
}

