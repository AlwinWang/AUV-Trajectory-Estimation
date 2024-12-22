using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using System.Net;

using CCWin;
using CCWin.SkinClass;
using CCWin.SkinControl;
using Microsoft.Win32;

namespace vvlaistick
{
    public partial class playform : Skin_VS
    {
        string filepath = null; int openhandle = -1; bool pause = false; int filetime = -1;
        IntPtr hwnd; int m_lUserID = -1; bool playing = false;
        bool CH1Playing = false; bool CH1Pause = false; string fileallpath = null;
        public playform()
        {
            InitializeComponent();    
            hwnd = panelPlay1.Handle;           
            LauInterop.LCPLAYM4_InitPlayer(null);       
        }
        //public playform(int userID)
        //{
        //    InitializeComponent();
        //    hwnd = panelPlay1.Handle;
        //    LauInterop.LCPLAYM4_InitPlayer(null);      
               
        //}

        private void playform_Load(object sender, EventArgs e)
        {           
            dateTimePicker1.Value = DateTime.Now;
            LoadParameterXml();           
        }       

        private void btnFindVideo_Click(object sender, EventArgs e)
        {
            if (listviewfile.Items.Count > 0)
            {
                listviewfile.Items.Clear();
            }
            string starttime = dateTimePicker1.Value.ToString("yyyyMMdd") + "000000";
            string endtime = dateTimePicker1.Value.ToString("yyyyMMdd") + "235959";
            try
            {
                string[] files = Directory.GetFiles(filepath);
                foreach (string file in files)
                {
                    string exname = file.Substring(file.LastIndexOf(".") + 1);
                    if (".mp4".IndexOf(file.Substring(file.LastIndexOf(".") + 1)) > -1)
                    {
                        FileInfo fi = new FileInfo(file);
                        string filedate = fi.Name.Substring(4, 14);
                        if ((Int64.Parse(starttime) < Int64.Parse(filedate)) && (Int64.Parse(endtime) > Int64.Parse(filedate)))
                        {
                            ListViewItem listfile = new ListViewItem();
                            if (listviewfile.Items.Count <= 0)
                            {
                                listfile.SubItems[0].Text = "1";
                            }
                            else
                            {
                                listfile.SubItems[0].Text = (listviewfile.Items.Count + 1).ToString();
                            }
                            listfile.SubItems.Add(fi.Name);
                            listviewfile.Items.Add(listfile);
                        }
                    }
                    else if (".avi".IndexOf(file.Substring(file.LastIndexOf(".") + 1)) > -1)
                    {
                        FileInfo fi = new FileInfo(file);
                        string filedate = fi.Name.Substring(4, 14);
                        if ((Int64.Parse(starttime) < Int64.Parse(filedate)) && (Int64.Parse(endtime) > Int64.Parse(filedate)))
                        {
                            ListViewItem listfile = new ListViewItem();
                            if (listviewfile.Items.Count <= 0)
                            {
                                listfile.SubItems[0].Text = "1";
                            }
                            else
                            {
                                listfile.SubItems[0].Text = (listviewfile.Items.Count + 1).ToString();
                            }
                            listfile.SubItems.Add(fi.Name);
                            listviewfile.Items.Add(listfile);
                        }
                    }
                }
            }
            catch { }
        }
       
        private void LoadParameterXml()
        {           
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(Application.StartupPath + "\\" + "Parameter.xml");
            XmlNode xn = xDoc.SelectSingleNode("root");
            XmlNodeList xlist = xn.ChildNodes;
            foreach (XmlNode xmln in xlist)
            {
                ListViewItem eqitem = new ListViewItem();
                XmlElement xe = (XmlElement)xmln;
                XmlNodeList xlist1 = xe.ChildNodes;                   
                filepath = xlist1.Item(1).InnerText;
            }
        }

        private void listviewfile_DoubleClick(object sender, EventArgs e)
        {
            if (openhandle == -1 && CH1Playing==false)
            {
                playback();
            }
            else
            {
                //停止播放文件
                if (timer1.Enabled == true)
                {
                    LauInterop.LCPLAYM4_Stop(openhandle);
                    panelPlay1.Refresh();
                    openhandle = -1;
                    timer1.Enabled = false;
                    filetime = 0;
                    progressBarPlay.Value = 0;
                }
                else if (timer2.Enabled == true)
                {
                    LauInterop.HDVPLAY_Stop(openhandle);
                    panelPlay1.Refresh();
                    openhandle = -1;
                    timer2.Enabled = false;
                    filetime = 0;
                    progressBarPlay.Value = 0;
                    progressBarPlay.Maximum = 0;
                }
                else
                { }
                playback();
            }
        }

        private void playback()
        {
            string FileName = listviewfile.SelectedItems[0].SubItems[1].Text.ToString();
            //if (FileName.Substring(0, 4) != "CHN1")
            //{
            //    openhandle = LauInterop.LCPLAYM4_OpenFile(filepath + "\\" + FileName, false);
            //    if (openhandle == -1)
            //    {
            //        return;
            //    }
            //    filetime = LauInterop.LCPLAYM4_GetFileTime(openhandle, 0);
            //    progressBarPlay.Maximum = filetime;
            //    LauInterop.LCPLAYM4_Play(openhandle, hwnd);
            //    timer1.Enabled = true;
            //    playing = true;
            //}
            //else if (FileName.Substring(0, 4) == "CHN1")
            //{
                openhandle = LauInterop.HDVPLAY_OpenFile(filepath + "\\" + FileName);
                LauInterop.HDVPLAY_Play(openhandle, hwnd);
                filetime = LauInterop.HDVPLAY_GetFileTime(openhandle);
                progressBarPlay.Maximum = filetime;
                Thread.Sleep(500);
                CH1Playing = true;
                timer2.Enabled = true;    
            //}
        }
       
        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (CH1Playing == false)
            {
                LauInterop.LCPLAYM4_Play(openhandle, hwnd);
                timer1.Enabled = true;
            }
            else
            {
                if (CH1Pause == true)
                {
                    bool resumeOK = LauInterop.HDVPLAY_Resume(openhandle);
                    CH1Pause = false;
                    timer2.Enabled = true;
                }
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (CH1Playing == false)
            {
                LauInterop.LCPLAYM4_Pause(openhandle);
                timer1.Enabled = false;
                pause = true;
            }
            else
            {
                LauInterop.HDVPLAY_Pause(openhandle);
                timer2.Enabled = false;
                CH1Pause = true;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (playing == true)
            {
                LauInterop.LCPLAYM4_Stop(openhandle);
                panelPlay1.Refresh();
                openhandle = -1;
                timer1.Enabled = false;
                filetime = 0;
                playing = false;
                progressBarPlay.Value = 0;
            }
            else if (CH1Playing == true)
            {
                LauInterop.HDVPLAY_Stop(openhandle);
                panelPlay1.Refresh();
                openhandle = -1;
                timer2.Enabled = false;
                filetime = 0;
                CH1Playing = false;
                CH1Pause = false;
                progressBarPlay.Value = 0;
            }
            else
            { }
        }              

        private void timer1_Tick(object sender, EventArgs e)
        {           
            int pdtime= LauInterop.LCPLAYM4_GetPlayedTime(openhandle);
            if (pdtime != 0 && pdtime <= filetime)
            {
                if (pause == true)
                {
                    LauInterop.LCPLAYM4_SetPlayedTime(openhandle, pdtime);
                    pause = false;
                }
                progressBarPlay.Value = pdtime;
            }
            else
            {
                progressBarPlay.Value = 0;
                LauInterop.LCPLAYM4_Stop(openhandle);
                panelPlay1.Refresh();
                openhandle = -1;
               
                playing = false;
                filetime = -1;
                timer1.Enabled = false;
            }
            //Console.Write(pdtime.ToString() + "    ");
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            int pdtime = LauInterop.HDVPLAY_GetPlayTime(openhandle);
            //Console.Write(pdtime.ToString() + "    ");
            if (pdtime < filetime)
            {
                if (CH1Pause == true)
                {
                    LauInterop.HDVPLAY_Pause(openhandle);
                    pause = false;
                }
                progressBarPlay.Value = pdtime;
            }
            else
            {
                progressBarPlay.Value = 0;
                LauInterop.HDVPLAY_Stop(openhandle);
                panelPlay1.Refresh();
                openhandle = -1;
                CH1Playing = false;
                CH1Pause = false;
                filetime = -1;
                timer2.Enabled = false;
            }
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Multiselect = false;//不允许多选
            fd.Filter = "Video file|*.avi*";
            fd.ShowDialog();
            if (!string.IsNullOrEmpty(fd.FileName))
            {
                //openfile = fd.FileName;
                btnPlay_Click(null, null);
            }
        }
    }
}
