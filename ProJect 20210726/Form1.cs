using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        static int x = 500;
        static int y = 400;
        static int weght = 50;
        static int height = 50;

        Rectangle aa = new Rectangle(x, y, weght, height);
        Rectangle bb = new Rectangle(750, 330, weght, height);

        static int xx = 500;
        static int yy = 250;
        static int w = 10;
        static int h = 40;
        Rectangle[] rect = new Rectangle[12] ;


        public Form1()
        {
            InitializeComponent();
            //double buffuring
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();       
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        //private void Rotate(Graphics e)
        //{
        //    Pen rotatePen = new Pen(Color.Black, 5);


        //    e.DrawRectangle(rotatePen,aa);

        //    // Scale rotatePen by 2X in the x-direction.
        //    rotatePen.ScaleTransform(2, 1);

        //    // Rotate rotatePen 90 degrees clockwise.
        //    rotatePen.RotateTransform(90);


        //    e.DrawRectangle(rotatePen,aa);
        //}
        private void Move(Graphics g)
        {
            g.FillRectangle(Brushes.Red, aa);
        }

        private void Move2(Graphics g)
        {
            g.FillRectangle(Brushes.Blue, bb);
        }


        static int cnt = 0;

        private void Move3(Graphics g)
        {
            for (int i = 0; i < 12; i++)
            {
           
                rect[i].X = xx;
                rect[i].Y = yy;
                rect[i].Width = w;
                rect[i].Height = h;            
                g.FillRectangle(Brushes.Black, rect[i]);
                g.DrawRectangle(new Pen(Color.White), rect[i].X,rect[i].Y,10, rect[i].Height);
            }
        }



        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            Pen pen = new Pen(Color.Black, 2);
            Pen pen1 = new Pen(Color.Blue, 2);
            Pen pen2 = new Pen(Color.Red, 2);

            Rectangle EFEMrec = new Rectangle(500, 400, 100, 100);
            Rectangle EFEMrec1 = new Rectangle(600, 400, 100, 100);
            Rectangle EFEMrec2 = new Rectangle(700, 400, 100, 100);
            Rectangle LLrec1 = new Rectangle(500, 300, 300, 100);
            Rectangle PMrec = new Rectangle(500, 245, 300, 50);
            //EFEM
            graphics.DrawRectangle(pen, EFEMrec);
            graphics.DrawRectangle(pen, EFEMrec1);
            graphics.DrawRectangle(pen, EFEMrec2);

            //LL
            graphics.DrawRectangle(pen1, LLrec1);

            //PM
            graphics.DrawRectangle(pen2, PMrec);

            //Move         
            Move(e.Graphics);
            Move2(e.Graphics);
            Move3(e.Graphics);
            //Rotate(e.Graphics);
        }

        private void Start_Click_1(object sender, EventArgs e)
        {
            timer1.Start();
            timer1.Interval = 32;
            timer2.Start();
            timer2.Interval = 32;
            timer3.Interval = 45;

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            aa.Y -= 10;
            Invalidate();

            if (aa.Y == 330)
            {
                timer1.Stop();
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            Invalidate();
            if (aa.Y == 330)
            {
                aa.X += 10;
                if (aa.X == 750)
                {
                    aa.X = x;
                    aa.Y = y;
                    timer1.Start();
                    timer3.Start();
                }
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {            
            bb.X -= 10;
            bb.Y -= 3;

            if (bb.X == 580)
            {
                timer3.Stop();
                bb.X = 750;
                bb.Y = 330;
                
                rect[cnt].X = xx;                
                rect[cnt].Y = yy;                
                rect[cnt].Width = w;                
                rect[cnt].Height = h;
            
                w += 10;
                cnt++;
         
            }

        }  
    }
}
