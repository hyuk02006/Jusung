using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace JUSUNG_Client
{
    // 상태 열거형
    enum State
    {
        STATE,
        FILENAMESIZE,
        FILENAME,
        FILESIZE,
        FILEDOWNLOAD
    }
    class FileTransfer
    {
        // 상태
        protected State state = State.STATE;
        // 파일 이름
        public byte[] FileName { get; set; }
        // 파일
        public byte[] Binary { get; set; }
    }

    class Client : FileTransfer
    {
        private Socket socket;
        // 버퍼
        private byte[] buffer;
        // 파일 다운로드 위치
        private int seek = 0;
        // 다운로드 디렉토리
        private string SaveDirectory = @"..\Debug\Log";
        // 생성자
        public Client(Socket socket)
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
            Console.WriteLine($"Client:(From:{remoteAddr.Address.ToString()}:{remoteAddr.Port},Connection time:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")})");
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
                        // 0이면 파일 이름 크기
                        case 0:
                            state = State.FILENAMESIZE;
                            // 크기는 int형으로 받는다.
                            buffer = new byte[4];
                            break;
                        // 1이면 파일 이름
                        case 1:
                            state = State.FILENAME;
                            // 파일 이름 버퍼 설정
                            buffer = new byte[FileName.Length];
                            // 다운로드 위치 0
                            seek = 0;
                            break;
                        // 2이면 파일 크기
                        case 2:
                            state = State.FILESIZE;
                            // 크기는 int형으로 받는다.
                            buffer = new byte[4];
                            break;
                        // 3이면 파일
                        case 3:
                            state = State.FILEDOWNLOAD;
                            // 파일 버퍼 설정
                            buffer = new byte[Binary.Length];
                            seek = 0;
                            break;
                    }
                }
                // 파일 이름 사이즈
                else if (state == State.FILENAMESIZE)
                {
                    // 데이터를 int형으로 변환(Bigendian)해서 FileName 변수를 할당한다.
                    FileName = new byte[BitConverter.ToInt32(buffer, 0)];
                    // 상태를 초기화
                    buffer = new byte[1];
                    state = State.STATE;
                }
                // 파일 이름
                else if (state == State.FILENAME)
                {
                    // 다운 받은 데이터를 FileName 변수로 옮긴다.
                    Array.Copy(buffer, 0, FileName, seek, size);
                    // 받은 만큼 위치 옮긴다.
                    seek += size;
                    // 위치와 파일 이름 크기가 같으면 종료
                    if (seek >= FileName.Length)
                    {
                        // 상태를 초기화
                        buffer = new byte[1];
                        state = State.STATE;
                    }
                }
                // 파일 크기
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
                        // IO를 이용해서 binary를 파일로 저장한다.
                        using (var stream = new FileStream(SaveDirectory + Encoding.UTF8.GetString(FileName), FileMode.Create, FileAccess.Write))
                        {
                            // 파일 쓰기
                            stream.Write(Binary, 0, Binary.Length);
                        }
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

        public class Program : Socket
        {
            public Program() : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                // 포트는 10000을 Listen한다.
                base.Bind(new IPEndPoint(IPAddress.Any, 10000));
                // 서버 대기
                base.Listen(10);
                // 비동기 소켓으로 Accept 클래스로 대기한다.
                BeginAccept(Accept, this);
            }
            // 클라이언트가 접속하면 함수가 호출된다.
            private void Accept(IAsyncResult result)
            {
                // EndAccept로 접속 Client Socket을 받는다. EndAccept는 대기를 끝나는 것이다.
                // Client 클래스를 생성한다.
                var client = new Client(EndAccept(result));
                // 비동기 소켓으로 Accept 클래스로 대기한다.
                BeginAccept(Accept, this);
            }
            public static void Run()
            {
                new Program();
                // q키를 누르면 서버는 종료한다.
                Console.WriteLine("Press the q key to exit.");
                while (true)
                {
                    string k = Console.ReadLine();
                    if ("q".Equals(k, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }
            }
        }
    }
}
