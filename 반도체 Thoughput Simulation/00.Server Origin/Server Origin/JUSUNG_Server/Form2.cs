using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Console;
using System.Net.Sockets;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JUSUNG_Server
{
    public partial class Form2 : MetroFramework.Forms.MetroForm
    {
        Form1 f1;
        string strPath = @"..\Debug\ParameterSets";

        List<string> FileName = new List<string>();
        List<string> FileCreDate = new List<string>();
        List<string> FilePath = new List<string>();
        string fasPath;
        public Form2(Form1 form)
        {
            InitializeComponent();
            f1 = form;
        }
        public Form2(Form1 form ,string fasPath)
        {
            InitializeComponent();
            f1 = form;
            this.fasPath = fasPath;
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            listView1.View = View.Details;
            listView1.GridLines = true;
            listView1.CheckBoxes = true;
            
            listView1.Columns.Add("파일명", 100, HorizontalAlignment.Center);
            listView1.Columns.Add("생성일", 70, HorizontalAlignment.Center);
            listView1.Columns.Add("파일경로", 350, HorizontalAlignment.Center);

            listView1.BackColor = Color.White;

            lbPath.Text = strPath;
            string PathFind = @"..\Debug\ParameterSets\ParamsPath.json";

            JObject dbSpec = new JObject(
            new JProperty("EFEM-nLPM", 25),
            new JProperty("EFEM-nAspeedPick", 4),
            new JProperty("EFEM-nAspeedPlace", 4),
            new JProperty("EFEM-nAspeedRotate", 1),
            new JProperty("EFEM-nAlgTime", 8),

            new JProperty("TM-nVac", 2),
            new JProperty("TM-nLL", 4),
            new JProperty("TM-nLLWafer", 6),
            new JProperty("TM-nLLPumpTime", 15),
            new JProperty("TM-nLLPumpStabTime", 5),
            new JProperty("TM-nLLVentTime", 15),
            new JProperty("TM-nLLVentStabTime", 5),
            new JProperty("TM-nVspeedPick", 4),
            new JProperty("TM-nVspeedPlace", 4),
            new JProperty("TM-nVspeedRotate", 1),

            new JProperty("PM-nPM", 3),
            new JProperty("PM-nPMWafer", 6),
            new JProperty("PM-nProcessTime", 1500),

            new JProperty("Speed", 1)
            );

            File.WriteAllText(@"..\Debug\ParameterSets\ParamSet-DefaultValue.json", dbSpec.ToString());
            ListViewItem lvi = new ListViewItem("DefaultValue");
            lvi.SubItems.Add(DateTime.Now.ToString("yy-MM-dd"));
            lvi.SubItems.Add(@"..\Debug\ParameterSets\ParamSet-DefaultValue.json");
            listView1.Items.Add(lvi);

            using (StreamReader file = File.OpenText(PathFind))
            using (JsonTextReader reader = new JsonTextReader(file))
            {

                JObject json = (JObject)JToken.ReadFrom(reader);
                var FileNameV = json.SelectToken("FileName");
                var FileCreDateV = json.SelectToken("FileCreDate");
                var FilePathV = json.SelectToken("FilePath");
                for (int idx = 0; idx < FileNameV.Count(); idx++)
                {

                    var fName = FileNameV[idx].ToString();
                    var fDate = FileCreDateV[idx].ToString();
                    var fPath = FilePathV[idx].ToString();
                    ListViewItem lvi1 = new ListViewItem(fName);
                    lvi1.SubItems.Add(fDate);
                    lvi1.SubItems.Add(fPath);
                    listView1.Items.Add(lvi1);

                    FileName.Add(fName);
                    FileCreDate.Add(fDate);
                    FilePath.Add(fPath);
                }
                File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "GENERATE Default ParameterSet Sucess!!\n");

            }
        }

        private void btnOpen_Click_1(object sender, EventArgs e)
        {
            string targetFileName = null;
            foreach (ListViewItem item in listView1.CheckedItems)
            {
                targetFileName = item.Text;
                string PathFind = listView1.Items[item.Index].SubItems[2].Text;
                using (StreamReader file = File.OpenText(PathFind))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject json = (JObject)JToken.ReadFrom(reader);

                    textBox_EFEM_nWafer.Text = json.SelectToken("EFEM-nLPM").ToString();
                    textBox_EFEM_dAspeedPick.Text = json.SelectToken("EFEM-nAspeedPick").ToString();
                    textBox_EFEM_dAspeedPlace.Text = json.SelectToken("EFEM-nAspeedPlace").ToString();
                    textBox_EFEM_dAspeedRotate.Text = json.SelectToken("EFEM-nAspeedRotate").ToString();
                    textBox_EFEM_dAlgTime.Text = json.SelectToken("EFEM-nAlgTime").ToString();

                    textBoxVac.Text = json.SelectToken("TM-nVac").ToString();
                    textBox_nLL.Text = json.SelectToken("TM-nLL").ToString();
                    textBoxnLLWafer.Text = json.SelectToken("TM-nLLWafer").ToString();
                    textBox_TM_dLLPumpTime.Text = json.SelectToken("TM-nLLPumpTime").ToString();
                    textBox_TM_dLLPumpStabTime.Text = json.SelectToken("TM-nLLPumpStabTime").ToString();
                    textBox_TM_dLLVentTime.Text = json.SelectToken("TM-nLLVentTime").ToString();
                    textBox_TM_dLLVentStabTime.Text = json.SelectToken("TM-nLLVentStabTime").ToString();
                    textBox_TM_fVspeedPick.Text = json.SelectToken("TM-nVspeedPick").ToString();
                    textBox_TM_fVspeedPlace.Text = json.SelectToken("TM-nVspeedPlace").ToString();
                    textBox_TM_fVspeedRotate.Text = json.SelectToken("TM-nVspeedRotate").ToString();

                    textBox_TM_nPM.Text = json.SelectToken("PM-nPM").ToString();
                    textboxnPMWafer.Text = json.SelectToken("PM-nPMWafer").ToString();
                    textbox_PM_ProcessTime.Text = json.SelectToken("PM-nProcessTime").ToString();

                    textBox_Multi.Text = json.SelectToken("Speed").ToString();
                }
            }
            File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "OPEN " + targetFileName + " ParameterSet Success!!\n");
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                strPath = fbd.SelectedPath;
                lbPath.Text = strPath;
                File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "UPDATE New ParameterSet Path Success!!\n");
            }
        }

        private void btnSave_Click_1(object sender, EventArgs e)
        {
            FileName.Add(tbFilename.Text);
            FileCreDate.Add(DateTime.Now.ToString("yy-MM-dd"));
            FilePath.Add(strPath + "\\ParamSet-" + tbFilename.Text + ".json");

            //데이터를 저장할 제이슨

            //File.Create(strPath + "\\ParamSet-" + tbFilename.Text + ".json");
            JObject dbSpec = new JObject(
                new JProperty("EFEM-nLPM", Convert.ToInt32(textBox_EFEM_nWafer.Text)),
                new JProperty("EFEM-nAspeedPick", Convert.ToInt32(textBox_EFEM_dAspeedPick.Text)),
                new JProperty("EFEM-nAspeedPlace", Convert.ToInt32(textBox_EFEM_dAspeedPlace.Text)),
                new JProperty("EFEM-nAspeedRotate", Convert.ToInt32(textBox_EFEM_dAspeedRotate.Text)),
                new JProperty("EFEM-nAlgTime", Convert.ToInt32(textBox_EFEM_dAlgTime.Text)),

                new JProperty("TM-nVac", Convert.ToInt32(textBoxVac.Text)),
                new JProperty("TM-nLL", Convert.ToInt32(textBox_nLL.Text)),
                new JProperty("TM-nLLWafer", Convert.ToInt32(textBoxnLLWafer.Text)),
                new JProperty("TM-nLLPumpTime", Convert.ToInt32(textBox_TM_dLLPumpTime.Text)),
                new JProperty("TM-nLLPumpStabTime", Convert.ToInt32(textBox_TM_dLLPumpStabTime.Text)),
                new JProperty("TM-nLLVentTime", Convert.ToInt32(textBox_TM_dLLVentTime.Text)),
                new JProperty("TM-nLLVentStabTime", Convert.ToInt32(textBox_TM_dLLVentStabTime.Text)),
                new JProperty("TM-nVspeedPick", Convert.ToInt32(textBox_TM_fVspeedPick.Text)),
                new JProperty("TM-nVspeedPlace", Convert.ToInt32(textBox_TM_fVspeedPlace.Text)),
                new JProperty("TM-nVspeedRotate", Convert.ToInt32(textBox_TM_fVspeedRotate.Text)),

                new JProperty("PM-nPM", Convert.ToInt32(textBox_TM_nPM.Text)),
                new JProperty("PM-nPMWafer", Convert.ToInt32(textboxnPMWafer.Text)),
                new JProperty("PM-nProcessTime", Convert.ToInt32(textbox_PM_ProcessTime.Text)),

                new JProperty("Speed", Convert.ToInt32(textBox_Multi.Text))
                );

            File.WriteAllText(strPath + "\\ParamSet-" + tbFilename.Text + ".json", dbSpec.ToString());


            //ListView에 해당 제이슨 정보 삽입
            ListViewItem lvi = new ListViewItem(tbFilename.Text);
            lvi.SubItems.Add(DateTime.Now.ToString("yy-MM-dd"));
            lvi.SubItems.Add(strPath + "\\ParamSet-" + tbFilename.Text + ".json");
            listView1.Items.Add(lvi);

            File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "SAVE " + tbFilename.Text + " ParameterSet Success!!\n");
        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            string targetFileName = null;
            foreach (ListViewItem item in listView1.CheckedItems)
            {
                targetFileName = item.Text;
                WriteLine(item.Index);
                string tmp = listView1.Items[item.Index].SubItems[2].Text;
                WriteLine(tmp);
                File.Delete(tmp);
                FileName.RemoveAt(item.Index - 1);
                FileCreDate.RemoveAt(item.Index - 1);
                FilePath.RemoveAt(item.Index - 1);
                listView1.Items.Remove(item);
            }
            File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "DELETE " + targetFileName + " ParameterSet Success!!\n");
        }

        private void btnListUpdate_Click_1(object sender, EventArgs e)
        {
            File.Delete(@"..\Debug\ParameterSets\ParamsPath.json");

            //데이터를 저장한 제이슨의 이름,생성일자,경로를 저장하는 제이슨
            JObject dbSpec = new JObject();
            dbSpec.Add("FileName", JArray.FromObject(FileName));
            dbSpec.Add("FileCreDate", JArray.FromObject(FileCreDate));
            dbSpec.Add("FilePath", JArray.FromObject(FilePath));


            File.WriteAllText(@"..\Debug\ParameterSets\ParamsPath.json", dbSpec.ToString());
            File.AppendAllText(@"" + fasPath, DateTime.Now.ToString("[yy-MM-dd-ddd] (hh:mm:ss) => ") + "UPDATE ParamsPath.json Success!!\n");
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            byte[] sendBufferEFEM = Encoding.UTF8.GetBytes(
                       "SERVER" + "/" + //0
                       "SETDATA" + "/" + //1
                       Convert.ToInt32(textBox_EFEM_nWafer.Text) + "/" + //2
                       Convert.ToInt32(textBox_EFEM_dAspeedPick.Text) + "/" + //3
                       Convert.ToInt32(textBox_EFEM_dAspeedPlace.Text) + "/" + //4
                       Convert.ToInt32(textBox_EFEM_dAspeedRotate.Text) + "/" + //5
                       Convert.ToInt32(textBox_EFEM_dAlgTime.Text) + "/" + //6
                       Convert.ToInt32(textBox_nLL.Text) + "/" + //7
                       Convert.ToInt32(textBoxnLLWafer.Text) + "/" + //8
                       Convert.ToInt32(textBox_Multi.Text) + "/" +//9
                       Convert.ToInt32(textBox_DT.Text) + "/" +//10
                       Convert.ToInt32(textBox_TM_dLLVentTime.Text) + "/" +  //11
                       Convert.ToInt32(textBox_TM_dLLVentStabTime.Text) + "/" //12
                       );
            int size = sendBufferEFEM.Length;
            byte[] data_size = BitConverter.GetBytes(size);

            f1.Obj.ckTime = Convert.ToInt32(tb_CheckTime.Text);

            f1.clientSocketEFEM.Send(data_size);
            f1.clientSocketEFEM.Send(sendBufferEFEM, 0, size, SocketFlags.None);
            Array.Clear(sendBufferEFEM, 0x0, sendBufferEFEM.Length);

            f1.Obj.EFEMobj.nAspeedPick = Convert.ToInt32(textBox_EFEM_dAspeedPick.Text);
            f1.Obj.EFEMobj.nAspeedPlace = Convert.ToInt32(textBox_EFEM_dAspeedPlace.Text);
            f1.Obj.EFEMobj.nAspeedRotate = Convert.ToInt32(textBox_EFEM_dAspeedRotate.Text);
            f1.Obj.EFEMobj.nLLVentTime = Convert.ToInt32(textBox_TM_dLLVentTime.Text);
            f1.Obj.EFEMobj.nLLVentStabTime = Convert.ToInt32(textBox_TM_dLLVentStabTime.Text);
            f1.Obj.speed = Convert.ToInt32(textBox_Multi.Text);
            f1.Obj.EFEMobj.LLwafer = Convert.ToInt32(textBoxnLLWafer.Text);
            f1.Obj.doorSpeed = Convert.ToInt32(textBox_DT.Text);

            byte[] sendBufferTM = Encoding.UTF8.GetBytes(
                        "SERVER" + "/" + //0
                        "SETDATA" + "/" + //1
                        Convert.ToInt32(textBoxVac.Text) + "/" + //2
                        Convert.ToInt32(textBox_nLL.Text) + "/" + //3
                        Convert.ToInt32(textBoxnLLWafer.Text) + "/" + //4
                        Convert.ToInt32(textBox_TM_nPM.Text) + "/" + //5
                        Convert.ToInt32(textboxnPMWafer.Text) + "/" + //6
                        Convert.ToInt32(textBox_TM_dLLPumpTime.Text) + "/" + //7
                        Convert.ToInt32(textBox_TM_dLLPumpStabTime.Text) + "/" + //8
                        Convert.ToInt32(textBox_TM_dLLVentTime.Text) + "/" + //9
                        Convert.ToInt32(textBox_TM_dLLVentStabTime.Text) + "/" + //10
                        Convert.ToInt32(textBox_TM_fVspeedPick.Text) + "/" + //11
                        Convert.ToInt32(textBox_TM_fVspeedPlace.Text) + "/" + //12
                        Convert.ToInt32(textBox_TM_fVspeedRotate.Text) + "/" + //13
                        Convert.ToInt32(textBox_Multi.Text) + "/" +//14
                        Convert.ToInt32(textBox_DT.Text) + "/" //15                                        
                        );

            int sizeTM = sendBufferTM.Length;
            byte[] data_sizeTM = BitConverter.GetBytes(sizeTM);
            f1.clientSocketTMC.Send(data_sizeTM);
            f1.clientSocketTMC.Send(sendBufferTM, 0, sizeTM, SocketFlags.None);
            Array.Clear(sendBufferTM, 0x0, sendBufferTM.Length);

            f1.Obj.TMobj.nVspeedPick = Convert.ToInt32(textBox_TM_fVspeedPick.Text);
            f1.Obj.TMobj.nVspeedPlace = Convert.ToInt32(textBox_TM_fVspeedPlace.Text);
            f1.Obj.TMobj.nVspeedRotate = Convert.ToInt32(textBox_TM_fVspeedRotate.Text);

            f1.Obj.TMobj.nLLPumpTime = Convert.ToInt32(textBox_TM_dLLPumpTime.Text);
            f1.Obj.TMobj.nLLPumpStabTime = Convert.ToInt32(textBox_TM_dLLPumpStabTime.Text);
            f1.Obj.TMobj.nLLVentTime = Convert.ToInt32(textBox_TM_dLLVentTime.Text);
            f1.Obj.TMobj.nLLVentStabTime = Convert.ToInt32(textBox_TM_dLLVentStabTime.Text);
            f1.Obj.speed = Convert.ToInt32(textBox_Multi.Text);
            f1.Obj.doorSpeed = Convert.ToInt32(textBox_DT.Text);
            f1.Obj.TMobj.nVac = Convert.ToInt32(textBoxVac.Text);

            byte[] sendBufferPM = Encoding.UTF8.GetBytes(
                       "SERVER" + "/" + //0
                       "SETDATA" + "/" + //1
                       Convert.ToInt32(textBox_TM_nPM.Text) + "/" + //2
                       Convert.ToInt32(textboxnPMWafer.Text) + "/" + //3
                       Convert.ToInt32(textbox_PM_ProcessTime.Text) + "/" + //4
                       Convert.ToInt32(textBoxVac.Text) + "/" + //5
                       Convert.ToInt32(textBox_Multi.Text) + "/" //6
                       );
            size = sendBufferPM.Length;
            data_size = BitConverter.GetBytes(size);
            f1.Obj.PMobj.nProcessTime = Convert.ToInt32(textbox_PM_ProcessTime.Text);
            f1.clientSocketPM0.Send(data_size);
            f1.clientSocketPM0.Send(sendBufferPM, 0, size, SocketFlags.None);

            f1.clientSocketPM1.Send(data_size);
            f1.clientSocketPM1.Send(sendBufferPM, 0, size, SocketFlags.None);

            f1.clientSocketPM2.Send(data_size);
            f1.clientSocketPM2.Send(sendBufferPM, 0, size, SocketFlags.None);

            Array.Clear(sendBufferPM, 0x0, sendBufferPM.Length);
            Close();
        }
    }
}
