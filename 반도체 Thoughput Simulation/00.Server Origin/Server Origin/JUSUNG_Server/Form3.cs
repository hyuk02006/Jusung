using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace JUSUNG_Server
{
    public partial class Form3 : MetroFramework.Forms.MetroForm
    {
        Form f3;
        public Form3()
        {
            InitializeComponent();
        }
        public Form3(Form f1)
        {
            InitializeComponent();
            f3 = f1;
            
        }
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        private void Form3_Load(object sender, EventArgs e)
        {

            StringBuilder iniPath = new StringBuilder();
            GetPrivateProfileString("PATH", "path", "(NONE)", iniPath, 32, @"..\Debug\Log\0.CTC\Path\Path.ini");
            tbCTCPath.Text = iniPath.ToString();
            GetPrivateProfileString("PATH", "EFEMpath", "(NONE)", iniPath, 32, @"..\Debug\Log\0.CTC\Path\Path.ini");
            tbEFEMPath.Text = iniPath.ToString();
            GetPrivateProfileString("PATH", "TMpath", "(NONE)", iniPath, 32, @"..\Debug\Log\0.CTC\Path\Path.ini");
            tbTMPath.Text = iniPath.ToString();
            GetPrivateProfileString("PATH", "PM0path", "(NONE)", iniPath, 32, @"..\Debug\Log\0.CTC\Path\Path.ini");
            tbPM0Path.Text = iniPath.ToString();
            GetPrivateProfileString("PATH", "PM1path", "(NONE)", iniPath, 32, @"..\Debug\Log\0.CTC\Path\Path.ini");
            tbPM1Path.Text = iniPath.ToString();
            GetPrivateProfileString("PATH", "PM2path", "(NONE)", iniPath, 32, @"..\Debug\Log\0.CTC\Path\Path.ini");
            tbPM2Path.Text = iniPath.ToString();

            GetPrivateProfileString("VERSION", "CTCver", "(NONE)", iniPath, 32, @"..\Debug\Log\0.CTC\Path\Path.ini");
            tbCTCVer.Text = iniPath.ToString();
            GetPrivateProfileString("VERSION", "EFEMver", "(NONE)", iniPath, 32, @"..\Debug\Log\0.CTC\Path\Path.ini");
            tbEFEMVer.Text = iniPath.ToString();
            GetPrivateProfileString("VERSION", "TMver", "(NONE)", iniPath, 32, @"..\Debug\Log\0.CTC\Path\Path.ini");
            tbTMVer.Text = iniPath.ToString();
            GetPrivateProfileString("VERSION", "PM0", "(NONE)", iniPath, 32, @"..\Debug\Log\0.CTC\Path\Path.ini");
            tbPM0Ver.Text = iniPath.ToString();
            GetPrivateProfileString("VERSION", "PM1", "(NONE)", iniPath, 32, @"..\Debug\Log\0.CTC\Path\Path.ini");
            tbPM1Ver.Text = iniPath.ToString();
            GetPrivateProfileString("VERSION", "PM2", "(NONE)", iniPath, 32, @"..\Debug\Log\0.CTC\Path\Path.ini");
            tbPM2Ver.Text = iniPath.ToString();
        }
        //UPDATE

        private void UPDATE_Click(object sender, EventArgs e)
        {
            WritePrivateProfileString("PATH", "CTCpath", tbCTCPath.Text, @"..\Debug\Log\0.CTC\Path\Path.ini");
            WritePrivateProfileString("PATH", "EFEMpath", tbEFEMPath.Text, @"..\Debug\Log\0.CTC\Path\Path.ini");
            WritePrivateProfileString("PATH", "TMpath", tbTMPath.Text, @"..\Debug\Log\0.CTC\Path\Path.ini");
            WritePrivateProfileString("PATH", "PM0path", tbPM0Path.Text, @"..\Debug\Log\0.CTC\Path\Path.ini");
            WritePrivateProfileString("PATH", "PM1Path", tbPM1Path.Text, @"..\Debug\Log\0.CTC\Path\Path.ini");
            WritePrivateProfileString("PATH", "PM2path", tbPM2Path.Text, @"..\Debug\Log\0.CTC\Path\Path.ini");

            WritePrivateProfileString("VERSION", "CTCver", tbCTCVer.Text, @"..\Debug\Log\0.CTC\Path\Path.ini");
            WritePrivateProfileString("VERSION", "EFEMver", tbEFEMVer.Text, @"..\Debug\Log\0.CTC\Path\Path.ini");
            WritePrivateProfileString("VERSION", "TMver", tbTMVer.Text, @"..\Debug\Log\0.CTC\Path\Path.ini");
            WritePrivateProfileString("VERSION", "PM0", tbPM0Ver.Text, @"..\Debug\Log\0.CTC\Path\Path.ini");
            WritePrivateProfileString("VERSION", "PM1", tbPM1Ver.Text, @"..\Debug\Log\0.CTC\Path\Path.ini");
            WritePrivateProfileString("VERSION", "PM2", tbPM2Ver.Text, @"..\Debug\Log\0.CTC\Path\Path.ini");

            WritePrivateProfileString("PATCH", "EFEMmsg", tbEFEMmsg.Text, @"..\Debug\Log\0.CTC\Path\Path.ini");
            WritePrivateProfileString("PATCH", "TMmsg", tbTMmsg.Text, @"..\Debug\Log\0.CTC\Path\Path.ini");
            WritePrivateProfileString("PATCH", "PM0msg", tbPM0msg.Text, @"..\Debug\Log\0.CTC\Path\Path.ini");
            WritePrivateProfileString("PATCH", "PM1msg", tbPM1msg.Text, @"..\Debug\Log\0.CTC\Path\Path.ini");
            WritePrivateProfileString("PATCH", "PM2msg", tbPM2msg.Text, @"..\Debug\Log\0.CTC\Path\Path.ini");

            this.Close();
        }
    }
}
