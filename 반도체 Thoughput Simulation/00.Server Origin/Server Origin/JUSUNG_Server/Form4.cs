using MetroFramework;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Console;
using System.Data.OracleClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Diagnostics;
using Ionic.Zip;

namespace JUSUNG_Server
{
    public partial class Form4 : MetroFramework.Forms.MetroForm
    {
        Form1 f1 = new Form1();
        public Socket clientSocketTMC;
        public Socket clientSocketEFEM;
        public Socket clientSocketPM0;
        public Socket clientSocketPM1;
        public Socket clientSocketPM2;

        public string AssemCTCpath;
        public string ASSEMEFEMpath;
        public string ASSEMTMpath;
        public string ASSEMPM0path;
        public string ASSEMPM1path;
        public string ASSEMPM2path;

        public string dirPath;
        public string zipPath;
        public string filePath;

        public Form4()
        {
            InitializeComponent();
        }
        public Form4(Socket csEFEM, Socket csTM, Socket csPM0, Socket csPM1, Socket csPM2)
        {
            InitializeComponent();
            clientSocketEFEM = csEFEM;
            clientSocketTMC = csTM;
            clientSocketPM0 = csPM0;
            clientSocketPM1 = csPM1;
            clientSocketPM2 = csPM2;
        }

        //취합
        private void btn_assemble_Click(object sender, EventArgs e)
        {
            new FTP();
            new FTPTM();
            new FTPPM0();
            new FTPPM1();
            new FTPPM2();
            f1.NewSendtoServer(clientSocketEFEM, clientSocketTMC, clientSocketPM0, clientSocketPM1, clientSocketPM2);
        }

        #region FILE취합
        //취합
        private void btn_zip_Click(object sender, EventArgs e)
        {
            dirPath = @"..\Debug\Log";
            if (dirPath == "")
            {
                MetroMessageBox.Show(this, "Error", "경로를 설정하고 다시 실행해주세요.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                CompressZipByIonic(dirPath, (@zipPath + @"\" + DateTime.Now.ToString("MM-dd") + ".zip"));
                MetroMessageBox.Show(this, "File Merge Complete.", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        public static void CompressZipByIonic(string sourcePath, string zipPath)
        {
            var filelist = GetFileList(sourcePath, new List<String>());
            using (var zip = new Ionic.Zip.ZipFile())
            {
                foreach (string file in filelist)
                {
                    string path = file.Substring(sourcePath.Length + 1);
                    zip.AddEntry(path, File.ReadAllBytes(file));
                }
                zip.Save(zipPath);
            }
        }
        public static List<String> GetFileList(String rootPath, List<String> fileList)
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
            private string SaveEFEM = @"..\Debug\Log\1.EFEM";

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
                            ExtractZip(Binary, @"..\Debug\Log\1.EFEM");
                            //ExtractZip(Binary, SaveTMC);
                            // 접속을 끊는다.
                            this.socket.Disconnect(false);
                            this.socket.Close();
                            this.socket.Dispose();

                            //



                            //
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
            // 생성자
            public FTP() : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                base.Bind(new IPEndPoint(IPAddress.Any, 9900));

                base.Listen(10);

                BeginAccept(Accept, this);
            }

            private void Accept(IAsyncResult result)
            {
                var client = new FTPCon(EndAccept(result));

                BeginAccept(Accept, this);
            }
        }


        class FTPConTM : FTPFile
        {
            // 클라이언트 소켓
            private Socket socket;
            // 버퍼
            private byte[] buffer;
            // 파일 다운로드 위치
            private int seek = 0;
            // 다운로드 디렉토리
            private string SaveEFEM = @"..\Debug\Log\2.TM";

            // 생성자
            public FTPConTM(Socket socket)
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
                            ExtractZip(Binary, @"..\Debug\Log\2.TM");
                            //ExtractZip(Binary, SaveTMC);
                            // 접속을 끊는다.
                            this.socket.Disconnect(false);
                            this.socket.Close();
                            this.socket.Dispose();

                            //



                            //
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
        class FTPTM : Socket
        {
            // 생성자
            public FTPTM() : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                base.Bind(new IPEndPoint(IPAddress.Any, 9901));

                base.Listen(10);

                BeginAccept(Accept, this);
            }

            private void Accept(IAsyncResult result)
            {
                var client = new FTPConTM(EndAccept(result));

                BeginAccept(Accept, this);
            }
        }

        class FTPConPM0 : FTPFile
        {
            // 클라이언트 소켓
            private Socket socket;
            // 버퍼
            private byte[] buffer;
            // 파일 다운로드 위치
            private int seek = 0;
            // 다운로드 디렉토리
            private string SaveEFEM = @"..\Debug\Log\3.PM0";

            // 생성자
            public FTPConPM0(Socket socket)
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
                            ExtractZip(Binary, @"..\Debug\Log\3.PM0");
                            //ExtractZip(Binary, SaveTMC);
                            // 접속을 끊는다.
                            this.socket.Disconnect(false);
                            this.socket.Close();
                            this.socket.Dispose();

                            //



                            //
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
        class FTPPM0 : Socket
        {
            // 생성자
            public FTPPM0() : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                base.Bind(new IPEndPoint(IPAddress.Any, 9902));

                base.Listen(10);

                BeginAccept(Accept, this);
            }

            private void Accept(IAsyncResult result)
            {
                var client = new FTPConPM0(EndAccept(result));

                BeginAccept(Accept, this);
            }
        }

        class FTPConPM1 : FTPFile
        {
            // 클라이언트 소켓
            private Socket socket;
            // 버퍼
            private byte[] buffer;
            // 파일 다운로드 위치
            private int seek = 0;
            // 다운로드 디렉토리
            private string SaveEFEM = @"..\Debug\Log\4.PM1";

            // 생성자
            public FTPConPM1(Socket socket)
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
                            ExtractZip(Binary, @"..\Debug\Log\4.PM1");
                            //ExtractZip(Binary, SaveTMC);
                            // 접속을 끊는다.
                            this.socket.Disconnect(false);
                            this.socket.Close();
                            this.socket.Dispose();

                            //



                            //
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
        class FTPPM1 : Socket
        {
            // 생성자
            public FTPPM1() : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                base.Bind(new IPEndPoint(IPAddress.Any, 9903));

                base.Listen(10);

                BeginAccept(Accept, this);
            }

            private void Accept(IAsyncResult result)
            {
                var client = new FTPConPM1(EndAccept(result));

                BeginAccept(Accept, this);
            }
        }

        class FTPConPM2 : FTPFile
        {
            // 클라이언트 소켓
            private Socket socket;
            // 버퍼
            private byte[] buffer;
            // 파일 다운로드 위치
            private int seek = 0;
            // 다운로드 디렉토리
            private string SaveEFEM = @"..\Debug\Log\5.PM2";

            // 생성자
            public FTPConPM2(Socket socket)
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
                            ExtractZip(Binary, @"..\Debug\Log\5.PM2");
                            //ExtractZip(Binary, SaveTMC);
                            // 접속을 끊는다.
                            this.socket.Disconnect(false);
                            this.socket.Close();
                            this.socket.Dispose();

                            //



                            //
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
        class FTPPM2 : Socket
        {
            // 생성자
            public FTPPM2() : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                base.Bind(new IPEndPoint(IPAddress.Any, 9904));

                base.Listen(10);

                BeginAccept(Accept, this);
            }

            private void Accept(IAsyncResult result)
            {
                var client = new FTPConPM2(EndAccept(result));

                BeginAccept(Accept, this);
            }
        }
        #endregion

        #endregion

        #region LOG취합
        private void Form4_Load(object sender, EventArgs e)
        {
            listview.View = View.Details;
            listview.Columns.Add("Date", 200, HorizontalAlignment.Left);
            listview.Columns.Add("Module", 75, HorizontalAlignment.Left);
            listview.Columns.Add("Issue", 75, HorizontalAlignment.Left);
            listview.Columns.Add("Msg", 500, HorizontalAlignment.Left);

            string jsonFilePath = @"..\Debug\Log\0.CTC\CTC_LOG.json";
            using (StreamReader file = File.OpenText(jsonFilePath))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject json = (JObject)JToken.ReadFrom(reader);

                DataBase _db = new DataBase();

                var Date = json.SelectToken("DATE");
                var Module = json.SelectToken("MODULE");
                var Issue = json.SelectToken("ISSUE");
                var Msg = json.SelectToken("MSG");
                for (int i = 0; i < Date.Count(); i++)
                {
                    ListViewItem lvi = new ListViewItem(Date[i].ToString());
                    lvi.SubItems.Add(Module[i].ToString());
                    lvi.SubItems.Add(Issue[i].ToString());
                    lvi.SubItems.Add(Msg[i].ToString());

                    listview.Items.Add(lvi);
                }
            }
        }

        public class DataBase
        {
            public string Date = string.Empty;
            public string Module = string.Empty;
            public string Issue = string.Empty;
            public string msg = string.Empty;
        }


        #endregion

        private void btnSHOW_Click(object sender, EventArgs e)
        {
            listview.Items.Clear();
            string jsonFilePath = @"..\Debug\Log\0.CTC\CTC_LOG.json";
            if (cbTIME.Checked == true && cbISSUE.Checked == false && cbMODULE.Checked == false)
            {
                int Fromtime = Convert.ToInt32(tbFromHOUR.Text + tbFromMIN.Text);
                int Totime = Convert.ToInt32(tbToHOUR.Text + tbToMIN.Text);

                using (StreamReader file = File.OpenText(jsonFilePath))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject json = (JObject)JToken.ReadFrom(reader);

                    DataBase _db = new DataBase();

                    var Date = json.SelectToken("DATE");
                    var Module = json.SelectToken("MODULE");
                    var Issue = json.SelectToken("ISSUE");
                    var Msg = json.SelectToken("MSG");

                    for (int i = 0; i < Date.Count(); i++)
                    {
                        int compTime = Convert.ToInt32(Date[i].ToString().Substring(3, 2) + Date[i].ToString().Substring(6, 2));
                        if (compTime <= Totime && compTime >= Fromtime)
                        {
                            ListViewItem lvi = new ListViewItem(Date[i].ToString());
                            lvi.SubItems.Add(Module[i].ToString());
                            lvi.SubItems.Add(Issue[i].ToString());
                            lvi.SubItems.Add(Msg[i].ToString());
                            listview.Items.Add(lvi);
                        }
                    }
                }
            }

            if (cbISSUE.Checked == true && cbTIME.Checked == false && cbMODULE.Checked == false)
            {
                string compIssue = null;
                if (cbNORMAL.Checked == true)
                {
                    compIssue = "NORMAL";
                }
                if (cbERROR.Checked == true)
                {
                    compIssue = "ERROR";
                }
                using (StreamReader file = File.OpenText(jsonFilePath))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject json = (JObject)JToken.ReadFrom(reader);

                    DataBase _db = new DataBase();

                    var Date = json.SelectToken("DATE");
                    var Module = json.SelectToken("MODULE");
                    var Issue = json.SelectToken("ISSUE");
                    var Msg = json.SelectToken("MSG");

                    for (int i = 0; i < Date.Count(); i++)
                    {
                        if (Issue[i].ToString() == compIssue)
                        {
                            ListViewItem lvi = new ListViewItem(Date[i].ToString());
                            lvi.SubItems.Add(Module[i].ToString());
                            lvi.SubItems.Add(Issue[i].ToString());
                            lvi.SubItems.Add(Msg[i].ToString());
                            listview.Items.Add(lvi);
                        }
                    }
                }
            }

            if (cbMODULE.Checked == true && cbTIME.Checked == false && cbISSUE.Checked == false)
            {
                List<CheckBox> cb = new List<CheckBox>() { cbEFEM, cbTM, cbPM0, cbPM1, cbPM2 };
                List<string> compModule = new List<string>();

                foreach (var item in cb)
                {
                    if (item.Checked == true)
                    {
                        compModule.Add(item.Text.ToString());
                    }
                }

                using (StreamReader file = File.OpenText(jsonFilePath))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject json = (JObject)JToken.ReadFrom(reader);

                    DataBase _db = new DataBase();

                    var Date = json.SelectToken("DATE");
                    var Module = json.SelectToken("MODULE");
                    var Issue = json.SelectToken("ISSUE");
                    var Msg = json.SelectToken("MSG");

                    for (int i = 0; i < Date.Count(); i++)
                    {
                        foreach (var item in compModule)
                        {
                            if (item.ToString() == Module[i].ToString())
                            {
                                ListViewItem lvi = new ListViewItem(Date[i].ToString());
                                lvi.SubItems.Add(Module[i].ToString());
                                lvi.SubItems.Add(Issue[i].ToString());
                                lvi.SubItems.Add(Msg[i].ToString());
                                listview.Items.Add(lvi);
                                break;
                            }
                        }

                    }
                }
            }

            if (cbTIME.Checked == true && cbISSUE.Checked == true && cbMODULE.Checked == false)
            {
                int Fromtime = Convert.ToInt32(tbFromHOUR.Text + tbFromMIN.Text);
                int Totime = Convert.ToInt32(tbToHOUR.Text + tbToMIN.Text);
                string compIssue = null;
                if (cbNORMAL.Checked == true)
                {
                    compIssue = "NORMAL";
                }
                if (cbERROR.Checked == true)
                {
                    compIssue = "ERROR";
                }
                using (StreamReader file = File.OpenText(jsonFilePath))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject json = (JObject)JToken.ReadFrom(reader);

                    DataBase _db = new DataBase();

                    var Date = json.SelectToken("DATE");
                    var Module = json.SelectToken("MODULE");
                    var Issue = json.SelectToken("ISSUE");
                    var Msg = json.SelectToken("MSG");

                    for (int i = 0; i < Date.Count(); i++)
                    {
                        int compTime = Convert.ToInt32(Date[i].ToString().Substring(3, 2) + Date[i].ToString().Substring(6, 2));
                        if (compTime <= Totime && compTime >= Fromtime)
                        {
                            if (Issue[i].ToString() == compIssue)
                            {
                                ListViewItem lvi = new ListViewItem(Date[i].ToString());
                                lvi.SubItems.Add(Module[i].ToString());
                                lvi.SubItems.Add(Issue[i].ToString());
                                lvi.SubItems.Add(Msg[i].ToString());
                                listview.Items.Add(lvi);
                            }
                        }
                    }
                }
            }

            if (cbMODULE.Checked == true && cbISSUE.Checked == true && cbTIME.Checked == false)
            {
                List<CheckBox> cb = new List<CheckBox>() { cbEFEM, cbTM, cbPM0, cbPM1, cbPM2 };
                List<string> compModule = new List<string>();

                foreach (var item in cb)
                {
                    if (item.Checked == true)
                    {
                        compModule.Add(item.Text.ToString());
                    }
                }
                string compIssue = null;
                if (cbNORMAL.Checked == true)
                {
                    compIssue = "NORMAL";
                }
                if (cbERROR.Checked == true)
                {
                    compIssue = "ERROR";
                }

                using (StreamReader file = File.OpenText(jsonFilePath))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject json = (JObject)JToken.ReadFrom(reader);

                    DataBase _db = new DataBase();

                    var Date = json.SelectToken("DATE");
                    var Module = json.SelectToken("MODULE");
                    var Issue = json.SelectToken("ISSUE");
                    var Msg = json.SelectToken("MSG");

                    for (int i = 0; i < Date.Count(); i++)
                    {
                        foreach (var item in compModule)
                        {
                            if (item.ToString() == Module[i].ToString())
                            {
                                if (Issue[i].ToString() == compIssue)
                                {
                                    ListViewItem lvi = new ListViewItem(Date[i].ToString());
                                    lvi.SubItems.Add(Module[i].ToString());
                                    lvi.SubItems.Add(Issue[i].ToString());
                                    lvi.SubItems.Add(Msg[i].ToString());
                                    listview.Items.Add(lvi);
                                    break;
                                }
                            }
                        }

                    }
                }
            }

            if (cbMODULE.Checked == true && cbTIME.Checked == true && cbISSUE.Checked == false)
            {
                int Fromtime = Convert.ToInt32(tbFromHOUR.Text + tbFromMIN.Text);
                int Totime = Convert.ToInt32(tbToHOUR.Text + tbToMIN.Text);
                List<CheckBox> cb = new List<CheckBox>() { cbEFEM, cbTM, cbPM0, cbPM1, cbPM2 };
                List<string> compModule = new List<string>();

                foreach (var item in cb)
                {
                    if (item.Checked == true)
                    {
                        compModule.Add(item.Text.ToString());
                    }
                }

                using (StreamReader file = File.OpenText(jsonFilePath))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject json = (JObject)JToken.ReadFrom(reader);

                    DataBase _db = new DataBase();

                    var Date = json.SelectToken("DATE");
                    var Module = json.SelectToken("MODULE");
                    var Issue = json.SelectToken("ISSUE");
                    var Msg = json.SelectToken("MSG");

                    for (int i = 0; i < Date.Count(); i++)
                    {
                        int compTime = Convert.ToInt32(Date[i].ToString().Substring(3, 2) + Date[i].ToString().Substring(6, 2));
                        if (compTime <= Totime && compTime >= Fromtime)
                        {
                            foreach (var item in compModule)
                            {
                                if (item.ToString() == Module[i].ToString())
                                {
                                    ListViewItem lvi = new ListViewItem(Date[i].ToString());
                                    lvi.SubItems.Add(Module[i].ToString());
                                    lvi.SubItems.Add(Issue[i].ToString());
                                    lvi.SubItems.Add(Msg[i].ToString());
                                    listview.Items.Add(lvi);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (cbMODULE.Checked == true && cbTIME.Checked == true && cbISSUE.Checked == true)
            {
                int Fromtime = Convert.ToInt32(tbFromHOUR.Text + tbFromMIN.Text);
                int Totime = Convert.ToInt32(tbToHOUR.Text + tbToMIN.Text);
                string compIssue = null;
                if (cbNORMAL.Checked == true)
                {
                    compIssue = "NORMAL";
                }
                if (cbERROR.Checked == true)
                {
                    compIssue = "ERROR";
                }
                List<CheckBox> cb = new List<CheckBox>() { cbEFEM, cbTM, cbPM0, cbPM1, cbPM2 };
                List<string> compModule = new List<string>();

                foreach (var item in cb)
                {
                    if (item.Checked == true)
                    {
                        compModule.Add(item.Text.ToString());
                    }
                }

                using (StreamReader file = File.OpenText(jsonFilePath))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject json = (JObject)JToken.ReadFrom(reader);

                    DataBase _db = new DataBase();

                    var Date = json.SelectToken("DATE");
                    var Module = json.SelectToken("MODULE");
                    var Issue = json.SelectToken("ISSUE");
                    var Msg = json.SelectToken("MSG");

                    for (int i = 0; i < Date.Count(); i++)
                    {
                        int compTime = Convert.ToInt32(Date[i].ToString().Substring(3, 2) + Date[i].ToString().Substring(6, 2));
                        if (compTime <= Totime && compTime >= Fromtime)
                        {
                            if (Issue[i].ToString() == compIssue)
                            {
                                foreach (var item in compModule)
                                {
                                    if (item.ToString() == Module[i].ToString())
                                    {
                                        ListViewItem lvi = new ListViewItem(Date[i].ToString());
                                        lvi.SubItems.Add(Module[i].ToString());
                                        lvi.SubItems.Add(Issue[i].ToString());
                                        lvi.SubItems.Add(Msg[i].ToString());
                                        listview.Items.Add(lvi);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void btnUNDO_Click(object sender, EventArgs e)
        {
            listview.Items.Clear();
            string jsonFilePath = @"..\Debug\Log\0.CTC\test2.json";
            using (StreamReader file = File.OpenText(jsonFilePath))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject json = (JObject)JToken.ReadFrom(reader);

                DataBase _db = new DataBase();

                var Date = json.SelectToken("DATE");
                var Module = json.SelectToken("MODULE");
                var Issue = json.SelectToken("ISSUE");
                var Msg = json.SelectToken("MSG");
                for (int i = 0; i < Date.Count(); i++)
                {
                    ListViewItem lvi = new ListViewItem(Date[i].ToString());
                    lvi.SubItems.Add(Module[i].ToString());
                    lvi.SubItems.Add(Issue[i].ToString());
                    lvi.SubItems.Add(Msg[i].ToString());

                    listview.Items.Add(lvi);
                }
            }
        }

        private void btnPATH_Click(object sender, EventArgs e)
        {

        }
        private void btnSAVE_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listview.Items)
            {
                WriteLine(item.SubItems[0].ToString().Replace("ListViewSubItem: {", string.Empty).Replace("}", string.Empty));
                WriteLine(item.SubItems[1].ToString().Replace("ListViewSubItem: {", string.Empty).Replace("}", string.Empty));
                WriteLine(item.SubItems[2].ToString().Replace("ListViewSubItem: {", string.Empty).Replace("}", string.Empty));
                WriteLine(item.SubItems[3].ToString().Replace("ListViewSubItem: {", string.Empty).Replace("}", string.Empty));
                string strLog = String.Format("[{0}] ({1}) <{2}> => {3}\n",
                    item.SubItems[0].ToString().Replace("ListViewSubItem: {", string.Empty).Replace("}", string.Empty),
                    item.SubItems[1].ToString().Replace("ListViewSubItem: {", string.Empty).Replace("}", string.Empty),
                    item.SubItems[2].ToString().Replace("ListViewSubItem: {", string.Empty).Replace("}", string.Empty),
                    item.SubItems[3].ToString().Replace("ListViewSubItem: {", string.Empty).Replace("}", string.Empty)
                    );
                

                File.AppendAllText(@"..\Debug\Log\0.CTC\CTC_LOG.txt" , strLog);
            }
            string jsonFilePath = @"..\Debug\Log\0.CTC\CTC_LOG.json";
            using (StreamReader file = File.OpenText(jsonFilePath))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject json = (JObject)JToken.ReadFrom(reader);

                DataBase _db = new DataBase();

                var Date = json.SelectToken("DATE");
                var Module = json.SelectToken("MODULE");
                var Issue = json.SelectToken("ISSUE");
                var Msg = json.SelectToken("MSG");
                for (int i = 0; i < Date.Count(); i++)
                {
                    string strLog = String.Format("[{0}] ({1}) <{2}> => {3}\n",
                    Date[i].ToString(),
                    Module[i].ToString(),
                    Issue[i].ToString(),
                    Msg[i].ToString()
                    );
                    
                    File.AppendAllText(@"..\Debug\Log\0.CTC\CTC_TailLOG.txt", strLog);
                }
            }
        }

        private void cbTIME_CheckedChanged(object sender, EventArgs e)
        {
            if (cbTIME.Checked == false)
            {
                tbFromHOUR.Visible = false;
                tbFromMIN.Visible = false;
                tbToHOUR.Visible = false;
                tbToMIN.Visible = false;
                metroLabel2.Visible = false;
                metroLabel3.Visible = false;
                metroLabel4.Visible = false;
            }
            else
            {
                tbFromHOUR.Visible = true;
                tbFromMIN.Visible = true;
                tbToHOUR.Visible = true;
                tbToMIN.Visible = true;
                metroLabel2.Visible = true;
                metroLabel3.Visible = true;
                metroLabel4.Visible = true;
            }
        }

        private void cbISSUE_CheckedChanged(object sender, EventArgs e)
        {
            if (cbISSUE.Checked == false)
            {
                cbALL.Visible = false;
                cbNORMAL.Visible = false;
                cbERROR.Visible = false;
            }
            else
            {
                cbALL.Visible = true;
                cbNORMAL.Visible = true;
                cbERROR.Visible = true;
            }
        }

        private void cbMODULE_CheckedChanged(object sender, EventArgs e)
        {
            if (cbMODULE.Checked == false)
            {
                cbMALL.Visible = false;
                cbEFEM.Visible = false;
                cbTM.Visible = false;
                cbPM0.Visible = false;
                cbPM1.Visible = false;
                cbPM2.Visible = false;
            }
            else
            {
                cbMALL.Visible = true;
                cbEFEM.Visible = true;
                cbTM.Visible = true;
                cbPM0.Visible = true;
                cbPM1.Visible = true;
                cbPM2.Visible = true;
            }
        }


        Thread threadMain;          // Progress 진행 사항 표시
        private int mMaximum = 0;   // Progress 맥시멈 값

        private void btnSavePath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                tbSavePath.Text = dialog.SelectedPath;
            }
        }

        private void btnLoadFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                tbLoadFolderPath.Text = dialog.SelectedPath;
            }
        }



        private void btnZip_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbLoadFolderPath.Text))
            {
                MetroMessageBox.Show(this, "압축할 폴더를 선택하세요.", "Fail!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(tbSavePath.Text))
            {
                MetroMessageBox.Show(this, "저장경로를 선택하세요.", "Fail!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            threadMain = new Thread(new ThreadStart(zipFolder));
            threadMain.IsBackground = true;
            mMaximum = 0;
            threadMain.Start();
        }

        private void btnLoadZip_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                tbLoadZipPath.Text = dialog.FileName;
            }
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(tbLoadZipPath.Text))
            {
                MetroMessageBox.Show(this, "압축해제할 폴더를 선택하세요.", "Fail!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(tbSavePath.Text))
            {
                MetroMessageBox.Show(this, "저장경로를 선택하세요.", "Fail!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            threadMain = new Thread(new ThreadStart(unZipFile));
            threadMain.IsBackground = true;
            mMaximum = 0;
            threadMain.Start();
        }
        #region 압축, 압축해제 함수
        //압축
        private void zipFolder()
        {
            string sSourcePath = tbLoadFolderPath.Text;
            string sTargetPath = tbSavePath.Text;

            try
            {
                
                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
                {
                    DirectoryInfo di = new DirectoryInfo(sSourcePath);
                    //프로그레스바 최대 값 (디렉토리내 모든 파일 수)
                    mMaximum += di.GetFiles("*.*", System.IO.SearchOption.AllDirectories).Count();

                    //프로그레스바 이벤트 설정
                    zip.SaveProgress += Zip_SaveProgress;


                    FileInfo[] infos = di.GetFiles("*.*", SearchOption.AllDirectories);

                    string[] files = new string[infos.Length];

                    for (int i = 0; i < infos.Length; i++)
                    {
                        files[i] = infos[i].FullName;
                    }

                    byte[] b = null;
                    string fileName = string.Empty;

                    foreach (string file in files)
                    {
                        fileName = file.Replace(sSourcePath, "");

                        //기본 인코딩 타입으로 읽기
                        b = Encoding.Default.GetBytes(fileName);

                        // IBM437로 변환
                        fileName = Encoding.GetEncoding("IBM437").GetString(b);

                        zip.AddEntry(fileName, File.ReadAllBytes(file));
                    }

                    DirectoryInfo dir = new DirectoryInfo(sTargetPath);

                    if (!dir.Exists)
                    {
                        dir.Create();
                    }

                    string[] split = sSourcePath.Split('\\');

                    string sZipName = split[split.Length - 1];

                    zip.Save($"{sTargetPath}\\{sZipName}.zip");
                }
                Process.Start(sTargetPath);
            }
            catch (Exception ex)
            {
                MetroMessageBox.Show(this, $"{ex.Message}\n", "압축 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        //압축해제
        private void unZipFile()
        {
            string sSourcePath = tbLoadZipPath.Text;
            string sTargetPath = tbSavePath.Text;

            try
            {
                using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(sSourcePath))
                {
                    FileInfo fi = new FileInfo(sSourcePath);

                    zip.ExtractProgress += Extract_Progress;

                    //프로그레스바 맥시멈 값
                    mMaximum = zip.Entries.Count;

                    DirectoryInfo dir = new DirectoryInfo(sTargetPath);

                    if (!dir.Exists)
                    {
                        dir.Create();
                    }

                    string saveFolderPath = $"{sTargetPath}\\{Path.GetFileNameWithoutExtension(sSourcePath)}";

                    for (int i = 0; i < zip.Entries.Count; i++)
                    {
                        ZipEntry entry = zip[i];

                        //IBM437 인코딩
                        byte[] byteIbm437 = Encoding.GetEncoding("IBM437").GetBytes(zip[i].FileName);
                        //euckr 인코딩
                        string euckrFileName = Encoding.GetEncoding("euc-kr").GetString(byteIbm437);

                        zip[i].FileName = euckrFileName;

                        entry.Extract(saveFolderPath, ExtractExistingFileAction.OverwriteSilently);
                    }
                    Process.Start(saveFolderPath);
                }
            }
            catch (Exception ex)
            {
                MetroMessageBox.Show(this, $"{ex.Message}\n", "압축 해제 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void Zip_SaveProgress(object sender, SaveProgressEventArgs e)
        {
            if (e.EventType == ZipProgressEventType.Saving_BeforeWriteEntry)
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    byte[] ibm437 = Encoding.GetEncoding("IBM437").GetBytes(e.CurrentEntry.FileName);

                    string s = Encoding.GetEncoding("euc-kr").GetString(ibm437);

                    lbCurFileName.Text = s;

                    progressBar1.Maximum = mMaximum;
                    progressBar1.PerformStep();
                    progressBar1.Update();
                }));
            }
        }
        private void Extract_Progress(object sender, ExtractProgressEventArgs e)
        {
            if (e.EventType == ZipProgressEventType.Extracting_BeforeExtractEntry)
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    lbCurFileName.Text = e.CurrentEntry.FileName;

                    progressBar1.Maximum = mMaximum;
                    progressBar1.PerformStep();
                    progressBar1.Update();

                }));
            }
        }
        #endregion

        private void btnOneClick_Click(object sender, EventArgs e)
        {
            new FTP();
            new FTPTM();
            new FTPPM0();
            new FTPPM1();
            new FTPPM2();
            f1.NewSendtoServer(clientSocketEFEM, clientSocketTMC, clientSocketPM0, clientSocketPM1, clientSocketPM2);
        }
    }
}


