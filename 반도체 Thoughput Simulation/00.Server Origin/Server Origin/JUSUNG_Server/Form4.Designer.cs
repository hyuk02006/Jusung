
namespace JUSUNG_Server
{
    partial class Form4
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.metroTabControl1 = new MetroFramework.Controls.MetroTabControl();
            this.metroTabPage1 = new MetroFramework.Controls.MetroTabPage();
            this.btnOneClick = new MetroFramework.Controls.MetroButton();
            this.lbCurFileName = new MetroFramework.Controls.MetroLabel();
            this.progressBar1 = new MetroFramework.Controls.MetroProgressBar();
            this.tbLoadZipPath = new MetroFramework.Controls.MetroTextBox();
            this.tbLoadFolderPath = new MetroFramework.Controls.MetroTextBox();
            this.tbSavePath = new MetroFramework.Controls.MetroTextBox();
            this.btnLoadZip = new MetroFramework.Controls.MetroButton();
            this.btnExtract = new MetroFramework.Controls.MetroButton();
            this.btnZip = new MetroFramework.Controls.MetroButton();
            this.btnLoadFolder = new MetroFramework.Controls.MetroButton();
            this.btnSavePath = new MetroFramework.Controls.MetroButton();
            this.metroTabPage2 = new MetroFramework.Controls.MetroTabPage();
            this.listview = new MetroFramework.Controls.MetroListView();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.cbISSUE = new MetroFramework.Controls.MetroCheckBox();
            this.cbTIME = new MetroFramework.Controls.MetroCheckBox();
            this.cbMODULE = new MetroFramework.Controls.MetroCheckBox();
            this.cbMALL = new MetroFramework.Controls.MetroCheckBox();
            this.cbALL = new MetroFramework.Controls.MetroCheckBox();
            this.btnPATH = new MetroFramework.Controls.MetroButton();
            this.btnSAVE = new MetroFramework.Controls.MetroButton();
            this.cbERROR = new MetroFramework.Controls.MetroCheckBox();
            this.metroLabel6 = new MetroFramework.Controls.MetroLabel();
            this.cbNORMAL = new MetroFramework.Controls.MetroCheckBox();
            this.btnUNDO = new MetroFramework.Controls.MetroButton();
            this.btnSHOW = new MetroFramework.Controls.MetroButton();
            this.cbPM2 = new MetroFramework.Controls.MetroCheckBox();
            this.cbPM1 = new MetroFramework.Controls.MetroCheckBox();
            this.cbPM0 = new MetroFramework.Controls.MetroCheckBox();
            this.cbTM = new MetroFramework.Controls.MetroCheckBox();
            this.metroLabel5 = new MetroFramework.Controls.MetroLabel();
            this.cbEFEM = new MetroFramework.Controls.MetroCheckBox();
            this.metroLabel4 = new MetroFramework.Controls.MetroLabel();
            this.metroLabel3 = new MetroFramework.Controls.MetroLabel();
            this.tbToMIN = new MetroFramework.Controls.MetroTextBox();
            this.tbToHOUR = new MetroFramework.Controls.MetroTextBox();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.tbFromMIN = new MetroFramework.Controls.MetroTextBox();
            this.tbFromHOUR = new MetroFramework.Controls.MetroTextBox();
            this.metroTabControl1.SuspendLayout();
            this.metroTabPage1.SuspendLayout();
            this.metroTabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // metroTabControl1
            // 
            this.metroTabControl1.Controls.Add(this.metroTabPage1);
            this.metroTabControl1.Controls.Add(this.metroTabPage2);
            this.metroTabControl1.ItemSize = new System.Drawing.Size(110, 34);
            this.metroTabControl1.Location = new System.Drawing.Point(38, 63);
            this.metroTabControl1.Name = "metroTabControl1";
            this.metroTabControl1.SelectedIndex = 0;
            this.metroTabControl1.Size = new System.Drawing.Size(862, 502);
            this.metroTabControl1.Style = MetroFramework.MetroColorStyle.Lime;
            this.metroTabControl1.TabIndex = 0;
            this.metroTabControl1.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroTabControl1.UseSelectable = true;
            // 
            // metroTabPage1
            // 
            this.metroTabPage1.Controls.Add(this.btnOneClick);
            this.metroTabPage1.Controls.Add(this.lbCurFileName);
            this.metroTabPage1.Controls.Add(this.progressBar1);
            this.metroTabPage1.Controls.Add(this.tbLoadZipPath);
            this.metroTabPage1.Controls.Add(this.tbLoadFolderPath);
            this.metroTabPage1.Controls.Add(this.tbSavePath);
            this.metroTabPage1.Controls.Add(this.btnLoadZip);
            this.metroTabPage1.Controls.Add(this.btnExtract);
            this.metroTabPage1.Controls.Add(this.btnZip);
            this.metroTabPage1.Controls.Add(this.btnLoadFolder);
            this.metroTabPage1.Controls.Add(this.btnSavePath);
            this.metroTabPage1.HorizontalScrollbarBarColor = true;
            this.metroTabPage1.HorizontalScrollbarHighlightOnWheel = false;
            this.metroTabPage1.HorizontalScrollbarSize = 10;
            this.metroTabPage1.Location = new System.Drawing.Point(4, 38);
            this.metroTabPage1.Name = "metroTabPage1";
            this.metroTabPage1.Size = new System.Drawing.Size(854, 460);
            this.metroTabPage1.TabIndex = 0;
            this.metroTabPage1.Text = "FILE 취합              ";
            this.metroTabPage1.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroTabPage1.VerticalScrollbarBarColor = true;
            this.metroTabPage1.VerticalScrollbarHighlightOnWheel = false;
            this.metroTabPage1.VerticalScrollbarSize = 10;
            // 
            // btnOneClick
            // 
            this.btnOneClick.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.btnOneClick.Location = new System.Drawing.Point(131, 74);
            this.btnOneClick.Name = "btnOneClick";
            this.btnOneClick.Size = new System.Drawing.Size(119, 32);
            this.btnOneClick.TabIndex = 18;
            this.btnOneClick.Text = "원클릭파일취합";
            this.btnOneClick.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.btnOneClick.UseSelectable = true;
            this.btnOneClick.Click += new System.EventHandler(this.btnOneClick_Click);
            // 
            // lbCurFileName
            // 
            this.lbCurFileName.AutoSize = true;
            this.lbCurFileName.Location = new System.Drawing.Point(131, 316);
            this.lbCurFileName.Name = "lbCurFileName";
            this.lbCurFileName.Size = new System.Drawing.Size(51, 19);
            this.lbCurFileName.TabIndex = 17;
            this.lbCurFileName.Text = "파일명";
            this.lbCurFileName.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(131, 283);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(592, 30);
            this.progressBar1.Style = MetroFramework.MetroColorStyle.Lime;
            this.progressBar1.TabIndex = 16;
            this.progressBar1.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // tbLoadZipPath
            // 
            // 
            // 
            // 
            this.tbLoadZipPath.CustomButton.Image = null;
            this.tbLoadZipPath.CustomButton.Location = new System.Drawing.Point(320, 1);
            this.tbLoadZipPath.CustomButton.Name = "";
            this.tbLoadZipPath.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.tbLoadZipPath.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.tbLoadZipPath.CustomButton.TabIndex = 1;
            this.tbLoadZipPath.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.tbLoadZipPath.CustomButton.UseSelectable = true;
            this.tbLoadZipPath.CustomButton.Visible = false;
            this.tbLoadZipPath.Lines = new string[0];
            this.tbLoadZipPath.Location = new System.Drawing.Point(131, 226);
            this.tbLoadZipPath.MaxLength = 32767;
            this.tbLoadZipPath.Name = "tbLoadZipPath";
            this.tbLoadZipPath.PasswordChar = '\0';
            this.tbLoadZipPath.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbLoadZipPath.SelectedText = "";
            this.tbLoadZipPath.SelectionLength = 0;
            this.tbLoadZipPath.SelectionStart = 0;
            this.tbLoadZipPath.ShortcutsEnabled = true;
            this.tbLoadZipPath.Size = new System.Drawing.Size(342, 23);
            this.tbLoadZipPath.TabIndex = 15;
            this.tbLoadZipPath.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tbLoadZipPath.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.tbLoadZipPath.UseSelectable = true;
            this.tbLoadZipPath.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.tbLoadZipPath.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // tbLoadFolderPath
            // 
            // 
            // 
            // 
            this.tbLoadFolderPath.CustomButton.Image = null;
            this.tbLoadFolderPath.CustomButton.Location = new System.Drawing.Point(320, 1);
            this.tbLoadFolderPath.CustomButton.Name = "";
            this.tbLoadFolderPath.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.tbLoadFolderPath.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.tbLoadFolderPath.CustomButton.TabIndex = 1;
            this.tbLoadFolderPath.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.tbLoadFolderPath.CustomButton.UseSelectable = true;
            this.tbLoadFolderPath.CustomButton.Visible = false;
            this.tbLoadFolderPath.Lines = new string[0];
            this.tbLoadFolderPath.Location = new System.Drawing.Point(131, 178);
            this.tbLoadFolderPath.MaxLength = 32767;
            this.tbLoadFolderPath.Name = "tbLoadFolderPath";
            this.tbLoadFolderPath.PasswordChar = '\0';
            this.tbLoadFolderPath.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbLoadFolderPath.SelectedText = "";
            this.tbLoadFolderPath.SelectionLength = 0;
            this.tbLoadFolderPath.SelectionStart = 0;
            this.tbLoadFolderPath.ShortcutsEnabled = true;
            this.tbLoadFolderPath.Size = new System.Drawing.Size(342, 23);
            this.tbLoadFolderPath.TabIndex = 14;
            this.tbLoadFolderPath.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tbLoadFolderPath.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.tbLoadFolderPath.UseSelectable = true;
            this.tbLoadFolderPath.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.tbLoadFolderPath.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // tbSavePath
            // 
            // 
            // 
            // 
            this.tbSavePath.CustomButton.Image = null;
            this.tbSavePath.CustomButton.Location = new System.Drawing.Point(320, 1);
            this.tbSavePath.CustomButton.Name = "";
            this.tbSavePath.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.tbSavePath.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.tbSavePath.CustomButton.TabIndex = 1;
            this.tbSavePath.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.tbSavePath.CustomButton.UseSelectable = true;
            this.tbSavePath.CustomButton.Visible = false;
            this.tbSavePath.Lines = new string[0];
            this.tbSavePath.Location = new System.Drawing.Point(131, 129);
            this.tbSavePath.MaxLength = 32767;
            this.tbSavePath.Name = "tbSavePath";
            this.tbSavePath.PasswordChar = '\0';
            this.tbSavePath.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbSavePath.SelectedText = "";
            this.tbSavePath.SelectionLength = 0;
            this.tbSavePath.SelectionStart = 0;
            this.tbSavePath.ShortcutsEnabled = true;
            this.tbSavePath.Size = new System.Drawing.Size(342, 23);
            this.tbSavePath.TabIndex = 13;
            this.tbSavePath.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tbSavePath.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.tbSavePath.UseSelectable = true;
            this.tbSavePath.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.tbSavePath.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // btnLoadZip
            // 
            this.btnLoadZip.Location = new System.Drawing.Point(494, 220);
            this.btnLoadZip.Name = "btnLoadZip";
            this.btnLoadZip.Size = new System.Drawing.Size(119, 32);
            this.btnLoadZip.TabIndex = 8;
            this.btnLoadZip.Text = "압축파일 불러오기";
            this.btnLoadZip.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.btnLoadZip.UseSelectable = true;
            this.btnLoadZip.Click += new System.EventHandler(this.btnLoadZip_Click);
            // 
            // btnExtract
            // 
            this.btnExtract.Location = new System.Drawing.Point(635, 220);
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.Size = new System.Drawing.Size(88, 32);
            this.btnExtract.TabIndex = 9;
            this.btnExtract.Text = "압축 해제";
            this.btnExtract.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.btnExtract.UseSelectable = true;
            this.btnExtract.Click += new System.EventHandler(this.btnExtract_Click);
            // 
            // btnZip
            // 
            this.btnZip.Location = new System.Drawing.Point(635, 173);
            this.btnZip.Name = "btnZip";
            this.btnZip.Size = new System.Drawing.Size(88, 32);
            this.btnZip.TabIndex = 10;
            this.btnZip.Text = "압축";
            this.btnZip.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.btnZip.UseSelectable = true;
            this.btnZip.Click += new System.EventHandler(this.btnZip_Click);
            // 
            // btnLoadFolder
            // 
            this.btnLoadFolder.Location = new System.Drawing.Point(494, 173);
            this.btnLoadFolder.Name = "btnLoadFolder";
            this.btnLoadFolder.Size = new System.Drawing.Size(119, 32);
            this.btnLoadFolder.TabIndex = 11;
            this.btnLoadFolder.Text = "압축할 폴더 불러오기";
            this.btnLoadFolder.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.btnLoadFolder.UseSelectable = true;
            this.btnLoadFolder.Click += new System.EventHandler(this.btnLoadFolder_Click);
            // 
            // btnSavePath
            // 
            this.btnSavePath.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.btnSavePath.Location = new System.Drawing.Point(494, 125);
            this.btnSavePath.Name = "btnSavePath";
            this.btnSavePath.Size = new System.Drawing.Size(119, 32);
            this.btnSavePath.TabIndex = 12;
            this.btnSavePath.Text = "저장할 경로";
            this.btnSavePath.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.btnSavePath.UseSelectable = true;
            this.btnSavePath.Click += new System.EventHandler(this.btnSavePath_Click);
            // 
            // metroTabPage2
            // 
            this.metroTabPage2.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.metroTabPage2.Controls.Add(this.listview);
            this.metroTabPage2.Controls.Add(this.metroLabel1);
            this.metroTabPage2.Controls.Add(this.cbISSUE);
            this.metroTabPage2.Controls.Add(this.cbTIME);
            this.metroTabPage2.Controls.Add(this.cbMODULE);
            this.metroTabPage2.Controls.Add(this.cbMALL);
            this.metroTabPage2.Controls.Add(this.cbALL);
            this.metroTabPage2.Controls.Add(this.btnPATH);
            this.metroTabPage2.Controls.Add(this.btnSAVE);
            this.metroTabPage2.Controls.Add(this.cbERROR);
            this.metroTabPage2.Controls.Add(this.metroLabel6);
            this.metroTabPage2.Controls.Add(this.cbNORMAL);
            this.metroTabPage2.Controls.Add(this.btnUNDO);
            this.metroTabPage2.Controls.Add(this.btnSHOW);
            this.metroTabPage2.Controls.Add(this.cbPM2);
            this.metroTabPage2.Controls.Add(this.cbPM1);
            this.metroTabPage2.Controls.Add(this.cbPM0);
            this.metroTabPage2.Controls.Add(this.cbTM);
            this.metroTabPage2.Controls.Add(this.metroLabel5);
            this.metroTabPage2.Controls.Add(this.cbEFEM);
            this.metroTabPage2.Controls.Add(this.metroLabel4);
            this.metroTabPage2.Controls.Add(this.metroLabel3);
            this.metroTabPage2.Controls.Add(this.tbToMIN);
            this.metroTabPage2.Controls.Add(this.tbToHOUR);
            this.metroTabPage2.Controls.Add(this.metroLabel2);
            this.metroTabPage2.Controls.Add(this.tbFromMIN);
            this.metroTabPage2.Controls.Add(this.tbFromHOUR);
            this.metroTabPage2.HorizontalScrollbarBarColor = true;
            this.metroTabPage2.HorizontalScrollbarHighlightOnWheel = false;
            this.metroTabPage2.HorizontalScrollbarSize = 10;
            this.metroTabPage2.Location = new System.Drawing.Point(4, 38);
            this.metroTabPage2.Name = "metroTabPage2";
            this.metroTabPage2.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.metroTabPage2.Size = new System.Drawing.Size(854, 460);
            this.metroTabPage2.Style = MetroFramework.MetroColorStyle.Lime;
            this.metroTabPage2.TabIndex = 1;
            this.metroTabPage2.Text = "LOG 취합              ";
            this.metroTabPage2.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroTabPage2.VerticalScrollbarBarColor = true;
            this.metroTabPage2.VerticalScrollbarHighlightOnWheel = false;
            this.metroTabPage2.VerticalScrollbarSize = 10;
            // 
            // listview
            // 
            this.listview.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.listview.FullRowSelect = true;
            this.listview.GridLines = true;
            this.listview.Location = new System.Drawing.Point(0, 151);
            this.listview.Name = "listview";
            this.listview.OwnerDraw = true;
            this.listview.Size = new System.Drawing.Size(850, 310);
            this.listview.Style = MetroFramework.MetroColorStyle.Lime;
            this.listview.TabIndex = 25;
            this.listview.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.listview.UseCompatibleStateImageBehavior = false;
            this.listview.UseSelectable = true;
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.metroLabel1.Location = new System.Drawing.Point(4, 10);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(38, 19);
            this.metroLabel1.TabIndex = 3;
            this.metroLabel1.Text = "TIME";
            this.metroLabel1.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // cbISSUE
            // 
            this.cbISSUE.AutoSize = true;
            this.cbISSUE.Location = new System.Drawing.Point(501, 16);
            this.cbISSUE.Name = "cbISSUE";
            this.cbISSUE.Size = new System.Drawing.Size(26, 15);
            this.cbISSUE.Style = MetroFramework.MetroColorStyle.Lime;
            this.cbISSUE.TabIndex = 29;
            this.cbISSUE.Text = " ";
            this.cbISSUE.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.cbISSUE.UseSelectable = true;
            this.cbISSUE.CheckedChanged += new System.EventHandler(this.cbISSUE_CheckedChanged);
            // 
            // cbTIME
            // 
            this.cbTIME.AutoSize = true;
            this.cbTIME.CausesValidation = false;
            this.cbTIME.Location = new System.Drawing.Point(43, 13);
            this.cbTIME.Name = "cbTIME";
            this.cbTIME.Size = new System.Drawing.Size(26, 15);
            this.cbTIME.Style = MetroFramework.MetroColorStyle.Lime;
            this.cbTIME.TabIndex = 28;
            this.cbTIME.Text = " ";
            this.cbTIME.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.cbTIME.UseSelectable = true;
            this.cbTIME.CheckedChanged += new System.EventHandler(this.cbTIME_CheckedChanged);
            // 
            // cbMODULE
            // 
            this.cbMODULE.AutoSize = true;
            this.cbMODULE.Location = new System.Drawing.Point(71, 83);
            this.cbMODULE.Name = "cbMODULE";
            this.cbMODULE.Size = new System.Drawing.Size(26, 15);
            this.cbMODULE.Style = MetroFramework.MetroColorStyle.Lime;
            this.cbMODULE.TabIndex = 27;
            this.cbMODULE.Text = " ";
            this.cbMODULE.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.cbMODULE.UseSelectable = true;
            this.cbMODULE.CheckedChanged += new System.EventHandler(this.cbMODULE_CheckedChanged);
            // 
            // cbMALL
            // 
            this.cbMALL.AutoSize = true;
            this.cbMALL.Checked = true;
            this.cbMALL.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbMALL.ForeColor = System.Drawing.SystemColors.Window;
            this.cbMALL.Location = new System.Drawing.Point(4, 113);
            this.cbMALL.Name = "cbMALL";
            this.cbMALL.Size = new System.Drawing.Size(43, 15);
            this.cbMALL.Style = MetroFramework.MetroColorStyle.Lime;
            this.cbMALL.TabIndex = 26;
            this.cbMALL.Text = "ALL";
            this.cbMALL.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.cbMALL.UseSelectable = true;
            this.cbMALL.Visible = false;
            // 
            // cbALL
            // 
            this.cbALL.AutoSize = true;
            this.cbALL.Checked = true;
            this.cbALL.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbALL.ForeColor = System.Drawing.SystemColors.Window;
            this.cbALL.Location = new System.Drawing.Point(453, 47);
            this.cbALL.Name = "cbALL";
            this.cbALL.Size = new System.Drawing.Size(43, 15);
            this.cbALL.Style = MetroFramework.MetroColorStyle.Lime;
            this.cbALL.TabIndex = 24;
            this.cbALL.Text = "ALL";
            this.cbALL.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.cbALL.UseSelectable = true;
            this.cbALL.Visible = false;
            // 
            // btnPATH
            // 
            this.btnPATH.ForeColor = System.Drawing.SystemColors.Window;
            this.btnPATH.Location = new System.Drawing.Point(695, 105);
            this.btnPATH.Name = "btnPATH";
            this.btnPATH.Size = new System.Drawing.Size(75, 30);
            this.btnPATH.TabIndex = 23;
            this.btnPATH.Text = "PATH";
            this.btnPATH.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.btnPATH.UseSelectable = true;
            this.btnPATH.Click += new System.EventHandler(this.btnPATH_Click);
            // 
            // btnSAVE
            // 
            this.btnSAVE.ForeColor = System.Drawing.SystemColors.Window;
            this.btnSAVE.Location = new System.Drawing.Point(776, 105);
            this.btnSAVE.Name = "btnSAVE";
            this.btnSAVE.Size = new System.Drawing.Size(75, 30);
            this.btnSAVE.TabIndex = 22;
            this.btnSAVE.Text = "SAVE";
            this.btnSAVE.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.btnSAVE.UseSelectable = true;
            this.btnSAVE.Click += new System.EventHandler(this.btnSAVE_Click);
            // 
            // cbERROR
            // 
            this.cbERROR.AutoSize = true;
            this.cbERROR.ForeColor = System.Drawing.SystemColors.Window;
            this.cbERROR.Location = new System.Drawing.Point(581, 47);
            this.cbERROR.Name = "cbERROR";
            this.cbERROR.Size = new System.Drawing.Size(59, 15);
            this.cbERROR.Style = MetroFramework.MetroColorStyle.Lime;
            this.cbERROR.TabIndex = 21;
            this.cbERROR.Text = "ERROR";
            this.cbERROR.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.cbERROR.UseSelectable = true;
            this.cbERROR.Visible = false;
            // 
            // metroLabel6
            // 
            this.metroLabel6.AutoSize = true;
            this.metroLabel6.Location = new System.Drawing.Point(453, 13);
            this.metroLabel6.Name = "metroLabel6";
            this.metroLabel6.Size = new System.Drawing.Size(42, 19);
            this.metroLabel6.TabIndex = 20;
            this.metroLabel6.Text = "ISSUE";
            this.metroLabel6.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // cbNORMAL
            // 
            this.cbNORMAL.AutoSize = true;
            this.cbNORMAL.ForeColor = System.Drawing.SystemColors.Window;
            this.cbNORMAL.Location = new System.Drawing.Point(502, 47);
            this.cbNORMAL.Name = "cbNORMAL";
            this.cbNORMAL.Size = new System.Drawing.Size(73, 15);
            this.cbNORMAL.Style = MetroFramework.MetroColorStyle.Lime;
            this.cbNORMAL.TabIndex = 19;
            this.cbNORMAL.Text = "NORMAL";
            this.cbNORMAL.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.cbNORMAL.UseSelectable = true;
            this.cbNORMAL.Visible = false;
            // 
            // btnUNDO
            // 
            this.btnUNDO.ForeColor = System.Drawing.SystemColors.Window;
            this.btnUNDO.Location = new System.Drawing.Point(696, 69);
            this.btnUNDO.Name = "btnUNDO";
            this.btnUNDO.Size = new System.Drawing.Size(75, 30);
            this.btnUNDO.TabIndex = 18;
            this.btnUNDO.Text = "UNDO";
            this.btnUNDO.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.btnUNDO.UseSelectable = true;
            this.btnUNDO.Click += new System.EventHandler(this.btnUNDO_Click);
            // 
            // btnSHOW
            // 
            this.btnSHOW.ForeColor = System.Drawing.SystemColors.Window;
            this.btnSHOW.Location = new System.Drawing.Point(776, 69);
            this.btnSHOW.Name = "btnSHOW";
            this.btnSHOW.Size = new System.Drawing.Size(75, 30);
            this.btnSHOW.TabIndex = 17;
            this.btnSHOW.Text = "SHOW";
            this.btnSHOW.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.btnSHOW.UseSelectable = true;
            this.btnSHOW.Click += new System.EventHandler(this.btnSHOW_Click);
            // 
            // cbPM2
            // 
            this.cbPM2.AutoSize = true;
            this.cbPM2.ForeColor = System.Drawing.SystemColors.Window;
            this.cbPM2.Location = new System.Drawing.Point(358, 113);
            this.cbPM2.Name = "cbPM2";
            this.cbPM2.Size = new System.Drawing.Size(47, 15);
            this.cbPM2.Style = MetroFramework.MetroColorStyle.Lime;
            this.cbPM2.TabIndex = 15;
            this.cbPM2.Text = "PM2";
            this.cbPM2.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.cbPM2.UseSelectable = true;
            this.cbPM2.Visible = false;
            // 
            // cbPM1
            // 
            this.cbPM1.AutoSize = true;
            this.cbPM1.ForeColor = System.Drawing.SystemColors.Window;
            this.cbPM1.Location = new System.Drawing.Point(286, 113);
            this.cbPM1.Name = "cbPM1";
            this.cbPM1.Size = new System.Drawing.Size(47, 15);
            this.cbPM1.Style = MetroFramework.MetroColorStyle.Lime;
            this.cbPM1.TabIndex = 14;
            this.cbPM1.Text = "PM1";
            this.cbPM1.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.cbPM1.UseSelectable = true;
            this.cbPM1.Visible = false;
            // 
            // cbPM0
            // 
            this.cbPM0.AutoSize = true;
            this.cbPM0.ForeColor = System.Drawing.SystemColors.Window;
            this.cbPM0.Location = new System.Drawing.Point(214, 113);
            this.cbPM0.Name = "cbPM0";
            this.cbPM0.Size = new System.Drawing.Size(47, 15);
            this.cbPM0.Style = MetroFramework.MetroColorStyle.Lime;
            this.cbPM0.TabIndex = 13;
            this.cbPM0.Text = "PM0";
            this.cbPM0.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.cbPM0.UseSelectable = true;
            this.cbPM0.Visible = false;
            // 
            // cbTM
            // 
            this.cbTM.AutoSize = true;
            this.cbTM.ForeColor = System.Drawing.SystemColors.Window;
            this.cbTM.Location = new System.Drawing.Point(149, 113);
            this.cbTM.Name = "cbTM";
            this.cbTM.Size = new System.Drawing.Size(40, 15);
            this.cbTM.Style = MetroFramework.MetroColorStyle.Lime;
            this.cbTM.TabIndex = 12;
            this.cbTM.Text = "TM";
            this.cbTM.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.cbTM.UseSelectable = true;
            this.cbTM.Visible = false;
            // 
            // metroLabel5
            // 
            this.metroLabel5.AutoSize = true;
            this.metroLabel5.Location = new System.Drawing.Point(3, 80);
            this.metroLabel5.Name = "metroLabel5";
            this.metroLabel5.Size = new System.Drawing.Size(63, 19);
            this.metroLabel5.TabIndex = 11;
            this.metroLabel5.Text = "MODULE";
            this.metroLabel5.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // cbEFEM
            // 
            this.cbEFEM.AutoSize = true;
            this.cbEFEM.ForeColor = System.Drawing.SystemColors.Window;
            this.cbEFEM.Location = new System.Drawing.Point(72, 113);
            this.cbEFEM.Name = "cbEFEM";
            this.cbEFEM.Size = new System.Drawing.Size(52, 15);
            this.cbEFEM.Style = MetroFramework.MetroColorStyle.Lime;
            this.cbEFEM.TabIndex = 10;
            this.cbEFEM.Text = "EFEM";
            this.cbEFEM.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.cbEFEM.UseSelectable = true;
            this.cbEFEM.Visible = false;
            // 
            // metroLabel4
            // 
            this.metroLabel4.AutoSize = true;
            this.metroLabel4.ForeColor = System.Drawing.SystemColors.Window;
            this.metroLabel4.Location = new System.Drawing.Point(194, 44);
            this.metroLabel4.Name = "metroLabel4";
            this.metroLabel4.Size = new System.Drawing.Size(18, 19);
            this.metroLabel4.TabIndex = 9;
            this.metroLabel4.Text = "~";
            this.metroLabel4.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroLabel4.Visible = false;
            // 
            // metroLabel3
            // 
            this.metroLabel3.AutoSize = true;
            this.metroLabel3.ForeColor = System.Drawing.SystemColors.Window;
            this.metroLabel3.Location = new System.Drawing.Point(311, 44);
            this.metroLabel3.Name = "metroLabel3";
            this.metroLabel3.Size = new System.Drawing.Size(12, 19);
            this.metroLabel3.TabIndex = 8;
            this.metroLabel3.Text = ":";
            this.metroLabel3.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroLabel3.Visible = false;
            // 
            // tbToMIN
            // 
            // 
            // 
            // 
            this.tbToMIN.CustomButton.Image = null;
            this.tbToMIN.CustomButton.Location = new System.Drawing.Point(53, 1);
            this.tbToMIN.CustomButton.Name = "";
            this.tbToMIN.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.tbToMIN.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.tbToMIN.CustomButton.TabIndex = 1;
            this.tbToMIN.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.tbToMIN.CustomButton.UseSelectable = true;
            this.tbToMIN.CustomButton.Visible = false;
            this.tbToMIN.Lines = new string[0];
            this.tbToMIN.Location = new System.Drawing.Point(331, 44);
            this.tbToMIN.MaxLength = 32767;
            this.tbToMIN.Name = "tbToMIN";
            this.tbToMIN.PasswordChar = '\0';
            this.tbToMIN.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbToMIN.SelectedText = "";
            this.tbToMIN.SelectionLength = 0;
            this.tbToMIN.SelectionStart = 0;
            this.tbToMIN.ShortcutsEnabled = true;
            this.tbToMIN.Size = new System.Drawing.Size(75, 23);
            this.tbToMIN.Style = MetroFramework.MetroColorStyle.Lime;
            this.tbToMIN.TabIndex = 7;
            this.tbToMIN.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.tbToMIN.UseSelectable = true;
            this.tbToMIN.Visible = false;
            this.tbToMIN.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.tbToMIN.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // tbToHOUR
            // 
            // 
            // 
            // 
            this.tbToHOUR.CustomButton.Image = null;
            this.tbToHOUR.CustomButton.Location = new System.Drawing.Point(53, 1);
            this.tbToHOUR.CustomButton.Name = "";
            this.tbToHOUR.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.tbToHOUR.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.tbToHOUR.CustomButton.TabIndex = 1;
            this.tbToHOUR.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.tbToHOUR.CustomButton.UseSelectable = true;
            this.tbToHOUR.CustomButton.Visible = false;
            this.tbToHOUR.Lines = new string[0];
            this.tbToHOUR.Location = new System.Drawing.Point(230, 44);
            this.tbToHOUR.MaxLength = 32767;
            this.tbToHOUR.Name = "tbToHOUR";
            this.tbToHOUR.PasswordChar = '\0';
            this.tbToHOUR.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbToHOUR.SelectedText = "";
            this.tbToHOUR.SelectionLength = 0;
            this.tbToHOUR.SelectionStart = 0;
            this.tbToHOUR.ShortcutsEnabled = true;
            this.tbToHOUR.Size = new System.Drawing.Size(75, 23);
            this.tbToHOUR.Style = MetroFramework.MetroColorStyle.Lime;
            this.tbToHOUR.TabIndex = 6;
            this.tbToHOUR.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.tbToHOUR.UseSelectable = true;
            this.tbToHOUR.Visible = false;
            this.tbToHOUR.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.tbToHOUR.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // metroLabel2
            // 
            this.metroLabel2.AutoSize = true;
            this.metroLabel2.ForeColor = System.Drawing.SystemColors.Window;
            this.metroLabel2.Location = new System.Drawing.Point(85, 43);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(12, 19);
            this.metroLabel2.TabIndex = 5;
            this.metroLabel2.Text = ":";
            this.metroLabel2.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroLabel2.Visible = false;
            // 
            // tbFromMIN
            // 
            // 
            // 
            // 
            this.tbFromMIN.CustomButton.Image = null;
            this.tbFromMIN.CustomButton.Location = new System.Drawing.Point(53, 1);
            this.tbFromMIN.CustomButton.Name = "";
            this.tbFromMIN.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.tbFromMIN.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.tbFromMIN.CustomButton.TabIndex = 1;
            this.tbFromMIN.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.tbFromMIN.CustomButton.UseSelectable = true;
            this.tbFromMIN.CustomButton.Visible = false;
            this.tbFromMIN.Lines = new string[0];
            this.tbFromMIN.Location = new System.Drawing.Point(105, 43);
            this.tbFromMIN.MaxLength = 32767;
            this.tbFromMIN.Name = "tbFromMIN";
            this.tbFromMIN.PasswordChar = '\0';
            this.tbFromMIN.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbFromMIN.SelectedText = "";
            this.tbFromMIN.SelectionLength = 0;
            this.tbFromMIN.SelectionStart = 0;
            this.tbFromMIN.ShortcutsEnabled = true;
            this.tbFromMIN.Size = new System.Drawing.Size(75, 23);
            this.tbFromMIN.Style = MetroFramework.MetroColorStyle.Lime;
            this.tbFromMIN.TabIndex = 4;
            this.tbFromMIN.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.tbFromMIN.UseSelectable = true;
            this.tbFromMIN.Visible = false;
            this.tbFromMIN.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.tbFromMIN.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // tbFromHOUR
            // 
            // 
            // 
            // 
            this.tbFromHOUR.CustomButton.Image = null;
            this.tbFromHOUR.CustomButton.Location = new System.Drawing.Point(53, 1);
            this.tbFromHOUR.CustomButton.Name = "";
            this.tbFromHOUR.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.tbFromHOUR.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.tbFromHOUR.CustomButton.TabIndex = 1;
            this.tbFromHOUR.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.tbFromHOUR.CustomButton.UseSelectable = true;
            this.tbFromHOUR.CustomButton.Visible = false;
            this.tbFromHOUR.Lines = new string[0];
            this.tbFromHOUR.Location = new System.Drawing.Point(4, 43);
            this.tbFromHOUR.MaxLength = 32767;
            this.tbFromHOUR.Name = "tbFromHOUR";
            this.tbFromHOUR.PasswordChar = '\0';
            this.tbFromHOUR.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbFromHOUR.SelectedText = "";
            this.tbFromHOUR.SelectionLength = 0;
            this.tbFromHOUR.SelectionStart = 0;
            this.tbFromHOUR.ShortcutsEnabled = true;
            this.tbFromHOUR.Size = new System.Drawing.Size(75, 23);
            this.tbFromHOUR.Style = MetroFramework.MetroColorStyle.Lime;
            this.tbFromHOUR.TabIndex = 2;
            this.tbFromHOUR.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.tbFromHOUR.UseSelectable = true;
            this.tbFromHOUR.Visible = false;
            this.tbFromHOUR.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.tbFromHOUR.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // Form4
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(939, 617);
            this.Controls.Add(this.metroTabControl1);
            this.Name = "Form4";
            this.Style = MetroFramework.MetroColorStyle.Silver;
            this.Text = "파일/로그 취합 Tool";
            this.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.Load += new System.EventHandler(this.Form4_Load);
            this.metroTabControl1.ResumeLayout(false);
            this.metroTabPage1.ResumeLayout(false);
            this.metroTabPage1.PerformLayout();
            this.metroTabPage2.ResumeLayout(false);
            this.metroTabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroTabControl metroTabControl1;
        private MetroFramework.Controls.MetroTabPage metroTabPage1;
        private MetroFramework.Controls.MetroTabPage metroTabPage2;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private MetroFramework.Controls.MetroTextBox tbFromMIN;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroTextBox tbFromHOUR;
        private MetroFramework.Controls.MetroCheckBox cbEFEM;
        private MetroFramework.Controls.MetroLabel metroLabel4;
        private MetroFramework.Controls.MetroLabel metroLabel3;
        private MetroFramework.Controls.MetroTextBox tbToMIN;
        private MetroFramework.Controls.MetroTextBox tbToHOUR;
        private MetroFramework.Controls.MetroLabel metroLabel5;
        private MetroFramework.Controls.MetroCheckBox cbPM2;
        private MetroFramework.Controls.MetroCheckBox cbPM1;
        private MetroFramework.Controls.MetroCheckBox cbPM0;
        private MetroFramework.Controls.MetroCheckBox cbTM;
        private MetroFramework.Controls.MetroCheckBox cbALL;
        private MetroFramework.Controls.MetroButton btnPATH;
        private MetroFramework.Controls.MetroButton btnSAVE;
        private MetroFramework.Controls.MetroCheckBox cbERROR;
        private MetroFramework.Controls.MetroLabel metroLabel6;
        private MetroFramework.Controls.MetroCheckBox cbNORMAL;
        private MetroFramework.Controls.MetroButton btnUNDO;
        private MetroFramework.Controls.MetroButton btnSHOW;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private MetroFramework.Controls.MetroListView listview;
        private MetroFramework.Controls.MetroCheckBox cbISSUE;
        private MetroFramework.Controls.MetroCheckBox cbTIME;
        private MetroFramework.Controls.MetroCheckBox cbMODULE;
        private MetroFramework.Controls.MetroCheckBox cbMALL;
        private MetroFramework.Controls.MetroLabel lbCurFileName;
        private MetroFramework.Controls.MetroProgressBar progressBar1;
        private MetroFramework.Controls.MetroTextBox tbLoadZipPath;
        private MetroFramework.Controls.MetroTextBox tbLoadFolderPath;
        private MetroFramework.Controls.MetroTextBox tbSavePath;
        private MetroFramework.Controls.MetroButton btnLoadZip;
        private MetroFramework.Controls.MetroButton btnExtract;
        private MetroFramework.Controls.MetroButton btnZip;
        private MetroFramework.Controls.MetroButton btnLoadFolder;
        private MetroFramework.Controls.MetroButton btnSavePath;
        private MetroFramework.Controls.MetroButton btnOneClick;
    }
}