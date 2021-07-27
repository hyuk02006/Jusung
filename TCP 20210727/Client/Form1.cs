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

namespace JUSUNG_Client
{
    public partial class Form1 : Form
    {
        private Socket socket;  // 소켓
        private Thread receiveThread;    // 대화 수신용
        public Form1()
        {
            InitializeComponent();
           
        }
        private void Form1_Load_1(object sender, EventArgs e)
        {
            this.AcceptButton = button2;
        }

        private void Log(string msg)
        {
            listBox1.Items.Add(string.Format("[{0}]{1}", DateTime.Now.ToString(), msg));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Focus();
            Log("클라이언트 로드됨!!");
        }

        private void Receive()
        {
            while (true)
            {
                // 연결된 클라이언트가 보낸 데이터 수신
                byte[] receiveBuffer = new byte[512];
                int length = socket.Receive(receiveBuffer, receiveBuffer.Length, SocketFlags.None);
                string msg = Encoding.UTF8.GetString(receiveBuffer, 0, length);
                ShowMsg(": "+msg);
            }
        }

        private void TextBox3_KeyDown(object sender, KeyEventArgs e)
        {
            // 메시지 전송하기(공백이 아니고, Enter 눌렀을때)
            if (textBox3.Text.Trim() != "" && e.KeyCode == Keys.Enter)
            {
                byte[] sendBuffer = Encoding.UTF8.GetBytes(textBox3.Text.Trim());
                socket.Send(sendBuffer);
                Log("메시지 전송됨");
                ShowMsg("나]" + textBox3.Text);
                textBox3.Text = ""; // 초기화
            }
        }
        // 송수신 메시지들 대화창에 출력
        private void ShowMsg(string msg)
        {
            // richTextBox에서 개행이 정상적으로 작용되지 않으면
            // 아래처럼 따로따로
            richTextBox1.AppendText(msg);
            richTextBox1.AppendText("\r\n");

            // 입력된 텍스트에 맞게 스크롤을 내려준다.
            this.Activate();
       

            // 캐럿(커서)를 텍스트박스의 끝으로 내려준다.
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();   // 스크럴을 캐럿(커서)위치에 맞춰준다.
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            // 서버 연결
            IPAddress ipaddress = IPAddress.Parse(textBox1.Text);
            IPEndPoint endPoint = new IPEndPoint(ipaddress, int.Parse(textBox2.Text));

            socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
                );

            // 연결하기
            Log("서버에 연결 시도중...");
            socket.Connect(endPoint);
     
            Log("서버에 접속됨");

            // Receive 스레드 처리(서버 <--> 클라이언트)
            receiveThread = new Thread(new ThreadStart(Receive));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] sendBuffer = Encoding.UTF8.GetBytes(textBox3.Text.Trim());
            socket.Send(sendBuffer);
            Log("메시지 전송됨");
            ShowMsg("나]" + textBox3.Text);
            textBox3.Text = ""; // 초기화
        }
    }
}