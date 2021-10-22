using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;

namespace ExcelExam
{
    public partial class Form1 : Form
    {
        public class FileData
        {
            public string level;
            public string Name;
            public string Count;
            public string Number;
        }
        public Form1()
        {
            InitializeComponent();
        }
        List<FileData> fileDatas1 = new List<FileData>();
        List<FileData> fileDatas2 = new List<FileData>();
        static int a = 0;
        static int b = 0;
        static int c = 0;
    private void button2_Click(object sender, EventArgs e)
        {
            // XLSX, OleDB
            // x64에서 실행
            string path = @"C:\Users\BIT\source\repos\ExcelExam\ExcelExam\bin\Debug\PLM BOM_비교 1.xls";
            string path1 = @"C:\Users\BIT\source\repos\ExcelExam\ExcelExam\bin\Debug\PLM BOM_비교 2.xls";
            string strConn = "provider=Microsoft.ACE.OLEDB.12.0;data source=" + path +
                ";Extended Properties='Excel 12.0 XML;HDR=No;IMEX=1';";
            string strConn1 = "provider=Microsoft.ACE.OLEDB.12.0;data source=" + path1 +
                ";Extended Properties='Excel 12.0 XML;HDR=No;IMEX=1';";

            string sql = "select * from [목록$A7:E] " ;
            string sql1 = "select * from [목록$A7:E] " ;
        


            
            OleDbConnection conn = new OleDbConnection(strConn);
            OleDbCommand cmd = new OleDbCommand(sql, conn);
            conn.Open();

            OleDbConnection conn1 = new OleDbConnection(strConn1);
            OleDbCommand cmd1 = new OleDbCommand(sql1, conn1);
            conn1.Open();


            OleDbDataReader dr = cmd.ExecuteReader();
            OleDbDataReader dr1 = cmd1.ExecuteReader();

            while(dr1.Read())
            {
                List<string> arr1 = new List<string>();

                if (dr1[0].ToString() == "3" || dr1[0].ToString() == "4")
                {              
                    arr1.Add(dr1[0].ToString());
                    arr1.Add(dr1[1].ToString());
                    arr1.Add(dr1[2].ToString());
                    arr1.Add(dr1[4].ToString());
                    b++;
                    arr1.Add(b.ToString());

                    ListViewItem lvi1 = new ListViewItem(arr1.ToArray());
                    listView2.Items.Add(lvi1);

                    FileData Filearr1 = new FileData();
                    Filearr1.level = dr1[0].ToString();
                    Filearr1.Number = dr1[1].ToString();
                    Filearr1.Name = dr1[2].ToString();
                    Filearr1.Count = dr1[4].ToString();

                    fileDatas2.Add(Filearr1);
                }
                else
                {
                    continue;
                }

            }

            while (dr.Read())
            {
                List<string> arr = new List<string>();
                

                if (dr[0].ToString() == "3" || dr[0].ToString() == "4")
                {

                    arr.Add(dr[0].ToString());
                    arr.Add(dr[1].ToString());
                    arr.Add(dr[2].ToString());
                    arr.Add(dr[3].ToString());
                    a++;
                    arr.Add(a.ToString());             
                }
                else
                {
                    continue;
                }


                ListViewItem lvi = new ListViewItem(arr.ToArray());
                listView1.Items.Add(lvi);

                FileData Filearr = new FileData();
                Filearr.level = dr[0].ToString();
                Filearr.Number = dr[1].ToString();
                Filearr.Name = dr[2].ToString();
                Filearr.Count = dr[3].ToString();
       
                fileDatas1.Add(Filearr);                  

            }
            for (int i = 0; i < fileDatas1.Count; i++)
            {
                List<string> arr2 = new List<string>();
                arr2.Add(fileDatas1[i].level.ToString());
                arr2.Add(fileDatas1[i].Number.ToString());
                arr2.Add(fileDatas1[i].Name.ToString());
                arr2.Add(fileDatas1[i].Count.ToString());       
                for (int j = 0; j < fileDatas2.Count; j++)
                {
                    if (fileDatas1[i].Number.ToString() == fileDatas2[j].Number.ToString() &&fileDatas1[i].level.ToString() == fileDatas2[j].level.ToString())
                    {
                        arr2.Add(fileDatas2[j].Count.ToString());
                        if (fileDatas1[i].Count.ToString() == fileDatas2[j].Count.ToString() && fileDatas1[i].Name.ToString() == fileDatas2[j].Name.ToString())
                        {                                                 
                            break;
                        }
                        else
                        {                        
                            ListViewItem lvi2 = new ListViewItem(arr2.ToArray());
                            listView3.Items.Add(lvi2);
                            break;
                        }
                    }
                    if (j == fileDatas2.Count-1)
                    {
                        arr2.Add("File2에 존재 하지 않음");

                        ListViewItem lvi2 = new ListViewItem(arr2.ToArray());
                        listView3.Items.Add(lvi2);

                    }
                    else
                        continue;
                }
            }
            dr.Close();
            conn.Close();
            dr1.Close();
            conn1.Close();

        }
    }
}

