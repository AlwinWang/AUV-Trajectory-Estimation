using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CCWin;
using CCWin.SkinClass;
using CCWin.SkinControl;
using Microsoft.Win32;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Net;
using System.Timers;

namespace vvlaistick
{
    public struct JOYINFOEX
    {
        public int wSize; public int wFlags; public int wXpos; public int wYpos; public int wZpos; public int wRpos;
        public int wUpos; public int wVpos; public int wButtons; public int dwButtonNumber; public int wPOV; public int dwReserved1; public int dwReserved2;
    }

    public partial class mainform : Skin_Mac
    {
        #region 参数
        //手柄
        [DllImport("winmm.dll")]
        public static extern int joyGetPosEx(uint uJoyID, ref JOYINFOEX pji);
        //串口      
        public SerialPort comm = new SerialPort(); private StringBuilder builder = new StringBuilder(); string port = null; string baude = null; string filepath = "C:";
        Byte[] commbyte = new Byte[27] { 0x66, 0x66, 0x66, 0x66, 0x0, 0x64, 0x64, 0x64, 0x64, 0x0, 0x0, 0x0, 0x0, 0x00, 0x00, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xA5, 0x0, 0x3, 0xCD };
        string byte4A = "00"; string byte4B = "00"; string byte4C = "00"; string byte4D = "00"; string byte18A = "0000000";string byte18B = "0";
        bool StickOK = false;  public bool commListening = false;
        string OSDCompass = "0000"; string OSDTemp = "0000"; string OSDTemp2 = "0000"; string OSDShidu = "0000";
        //清零
        double Depthclear = 0.0; bool Clear = false; double Depth = 0.0;
        //指南针
        string OSDPitch = "0000"; string OSDRoll = "0000"; int pitch = 0; int roll = 0; string OSDDepth = "0000";
        bool JiGuangOK = false;bool AutoHX = false;bool AutoSD = false;
        //摄像机      
        string FileNameQ = null; bool videoingQ = false; string FileNameH = null; bool videoingH = false;
        int NewLoginID200Q = -1; private int m_nPlayHandle200Q = -1; private int m_nRealHandle200Q = -1; S_REALDATA_INFO m_sRealDataInfo200Q; LauInterop.CBRealData m_fRealData200Q = null;
        int NewLoginID200H = -1; private int m_nPlayHandle200H = -1; private int m_nRealHandle200H = -1; S_REALDATA_INFO m_sRealDataInfo200H; LauInterop.CBRealData m_fRealData200H = null;
        private bool m_bRecordHK1 = false;
        private bool m_bRecordHK2 = false;

        private bool m_bInitSDKHK = false;
        int m_lUserID = -1;
        private uint iLastErr = 0;
        public CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo;
        private Int32 m_lRealHandleHK1 = -1;
        private Int32 m_lRealHandleHK2 = -1;
        string FileNameHK1 = "";
        string FileNameHK2 = "";
        //网络通信
        bool OK485 = false; public static Socket newclient = null; private byte[] m_receiveBuffer = new byte[39];
         private DateTime TimeStart = DateTime.Now;

        bool ArmUDOK = false;bool ArmQHOK = false;bool ZL = false;
        #endregion

        #region 初始化
        public mainform()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            System.Diagnostics.Process currentprocess = System.Diagnostics.Process.GetCurrentProcess();
            System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcessesByName(currentprocess.ProcessName);
            if (myProcesses.Length > 1)
            {
                MessageBox.Show("当前程序已运行！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Process.GetCurrentProcess().Kill();
                this.Close();
                this.Dispose();
                Application.ExitThread();
                Application.Exit();
            }
        }

        private void mainform_Load(object sender, EventArgs e)
        {           
            LoadParameterXml();
            timerGW1 = new System.Timers.Timer();
            timerGW1.Interval = 50;
            timerGW1.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
            //摄像机
            LauInterop.IPCNET_Init();
            LauInterop.HDVPLAY_Init();
            m_bInitSDKHK = CHCNetSDK.NET_DVR_Init();
            if (m_bInitSDKHK == false)
            {
                MessageBox.Show("模拟采集初始化失败!");
                return;
            }
            //LoginDeviceNew200Q(camera.IP1, camera.Port1, camera.UserName1, camera.UserPwd1, camera.Channel1, camera.handle2);
        }
        #endregion

        #region 摄像机       
        private void LoginDeviceNew200Q(string IP, string Port, string UserName, string UserPwd, string Channel, int handle)
        {
            if (m_nRealHandle200Q == -1)
            {
                int m_nLoginID = LauInterop.IPCNET_Login(IP, 90, UserName, UserPwd);
                if (m_nLoginID != -1)
                {
                    NewLoginID200Q = m_nLoginID;
                    m_fRealData200Q = new LauInterop.CBRealData(RealDataCallBack200Q);
                    m_sRealDataInfo200Q = new S_REALDATA_INFO();
                    m_sRealDataInfo200Q.lChannel = 0;
                    m_sRealDataInfo200Q.lStreamMode = 0;
                    m_sRealDataInfo200Q.eEncodeType = E_ENCODE_TYPE.ENCODE_H264;
                    m_nRealHandle200Q = LauInterop.IPCNET_StartRealData(m_nLoginID, ref m_sRealDataInfo200Q, m_fRealData200Q, Play1.Handle);

                    if (m_nRealHandle200Q != -1)
                    {
                        camera.handle1 = m_nRealHandle200Q;
                        btnConnect.ForeColor = Color.Lime;
                        timerOSD.Enabled = true;
                    }
                }
                else
                {
                    MessageBox.Show("前摄像机连接失败,稍后重试");
                }
            }
            else
            {
                LauInterop.IPCNET_StopRealData(m_nRealHandle200Q);
                LauInterop.IPCNET_Logout(NewLoginID200Q);
                Play1.Refresh();
                btnConnect.ForeColor = Color.White;
                m_nRealHandle200Q = -1;
                NewLoginID200Q = -1;
            }
        }
        public void RealDataCallBack200Q(int lRealHandle, E_REALDATA_TYPE eDataType, IntPtr pBuffer, uint lBufSize, IntPtr pUserData)
        {
            switch (eDataType)
            {
                case E_REALDATA_TYPE.REALDATA_HEAD:
                    m_nPlayHandle200Q = LauInterop.HDVPLAY_OpenStream(pBuffer, lBufSize);
                    if (m_nPlayHandle200Q == -1)
                    {
                        return;
                    }
                    LauInterop.HDVPLAY_Play(m_nPlayHandle200Q, pUserData);
                    break;
                case E_REALDATA_TYPE.REALDATA_VIDEO:
                case E_REALDATA_TYPE.REALDATA_AUDIO:
                    LauInterop.HDVPLAY_InputData(m_nPlayHandle200Q, pBuffer, lBufSize);
                    break;
                default:
                    break;
            }
        }
        private void LoginDeviceNew200H(string IP, string Port, string UserName, string UserPwd, string Channel, int handle)
        {
            if (m_nRealHandle200H == -1)
            {
                int m_nLoginID = LauInterop.IPCNET_Login(IP, 90, UserName, UserPwd);
                if (m_nLoginID != -1)
                {
                    NewLoginID200H = m_nLoginID;
                    m_fRealData200H = new LauInterop.CBRealData(RealDataCallBack200H);
                    m_sRealDataInfo200H = new S_REALDATA_INFO();
                    m_sRealDataInfo200H.lChannel = 0;
                    m_sRealDataInfo200H.lStreamMode = 0;
                    m_sRealDataInfo200H.eEncodeType = E_ENCODE_TYPE.ENCODE_H264;
                    m_nRealHandle200H = LauInterop.IPCNET_StartRealData(m_nLoginID, ref m_sRealDataInfo200H, m_fRealData200H, Play2.Handle);

                    if (m_nRealHandle200H != -1)
                    {
                        camera.handle2 = m_nRealHandle200H;
                        btnConnect2.ForeColor = Color.Lime;
                        //timerOSD.Enabled = true;
                    }
                }
                else
                {
                    MessageBox.Show("后摄像机连接失败,稍后重试");
                }
            }
            else
            {
                LauInterop.IPCNET_StopRealData(m_nRealHandle200H);
                LauInterop.IPCNET_Logout(NewLoginID200H);
                Play2.Refresh();
                btnConnect2.ForeColor = Color.White;
                m_nRealHandle200H = -1;
                NewLoginID200H = -1;
            }
        }
        public void RealDataCallBack200H(int lRealHandle, E_REALDATA_TYPE eDataType, IntPtr pBuffer, uint lBufSize, IntPtr pUserData)
        {
            switch (eDataType)
            {
                case E_REALDATA_TYPE.REALDATA_HEAD:
                    m_nPlayHandle200H = LauInterop.HDVPLAY_OpenStream(pBuffer, lBufSize);
                    if (m_nPlayHandle200H == -1)
                    {
                        return;
                    }
                    LauInterop.HDVPLAY_Play(m_nPlayHandle200H, pUserData);
                    break;
                case E_REALDATA_TYPE.REALDATA_VIDEO:
                case E_REALDATA_TYPE.REALDATA_AUDIO:
                    LauInterop.HDVPLAY_InputData(m_nPlayHandle200H, pBuffer, lBufSize);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 发送数据     
        private void timer1_Tick(object sender, EventArgs e)
        {                                                   
            int propFB = Convert.ToInt16((int)trackBar1.Value);
            int propUD = Convert.ToInt16((int)trackBar2.Value);
            int propLR = Convert.ToInt16((int)trackBar3.Value);
            if (commbyte[6] == 0x64)
            {                  
                commbyte[9] = commbyte[10] = commbyte[11] = BitConverter.GetBytes(propFB * 25)[0];
            }
            else
            {                  
                commbyte[9] = commbyte[10] = commbyte[11] = BitConverter.GetBytes(propLR * 25)[0];
            }
            commbyte[12] =  BitConverter.GetBytes(propUD * 25)[0];//升降速度  
            //定深
            if (DSOK == true)
            {
                int DSNUM = Convert.ToInt16(numericUpDown2.Value * 10) + 1000;
                if (DSNUM > 4096)
                {
                    DSNUM = 4096;
                }
                byte[] combyteDep = BitConverter.GetBytes(DSNUM);
                commbyte[13] = combyteDep[1];
                commbyte[14] = combyteDep[0];
            }
            else
            {
                commbyte[13] = 0x0;
                commbyte[14] = 0x0;
            }

            //定向
            if (DXOK == true)
            {
                int DXNUM = int.Parse(Math.Floor(double.Parse(numericUpDown1.Value.ToString())  + 1000).ToString());
                if (DXNUM > 4096)
                {
                    DXNUM = 4096;
                }
                byte[] combyteDep = BitConverter.GetBytes(DXNUM);
                commbyte[15] = combyteDep[1];
                commbyte[16] = combyteDep[0];
            }
            else
            {
                commbyte[15] = 0x0;
                commbyte[16] = 0x0;
            }
            //自动方向深度保持
            string byte4B1 = "0"; string byte4B2 = "0";
            if (AutoSD)
            {
                byte4B1 = "1";
            }
            else
            {
                byte4B1 = "0";
            }
            if (AutoHX)
            {
                byte4B2 = "1";
            }
            else
            {
                byte4B2 = "0";
            }
            byte4B = byte4B1 + byte4B2;

            //LED灯强度
            //int LEDQD = Convert.ToInt16(lblHT.Text) + 25;
            //commbyte[17] = BitConverter.GetBytes(LEDQD)[0];
            int byte4 = Convert.ToInt32(byte4A + byte4B + byte4C + byte4D , 2);
            commbyte[4] = BitConverter.GetBytes(byte4)[0];

            int byte18 = Convert.ToInt32(byte18A + byte18B, 2);
            commbyte[18] = BitConverter.GetBytes(byte18)[0];           
            if (LX == false)
            {
                if (KZT == "KT")
                {
                    commbyte[21] = 0x00;
                }
                else if (KZT == "KK")
                {
                    if (CurrentQH >= 30)
                    {
                        commbyte[21] = 0x01;
                    }
                    else
                    {
                        numericUpDown4.Value = 165;
                        commbyte[21] = 0x00;
                    }
                }
                else if (KZT == "KG")
                {
                    if (CurrentQH >= 30)
                    {
                        commbyte[21] = 0x02;
                    }
                    else
                    {
                        numericUpDown4.Value = 165;
                        commbyte[21] = 0x00;
                    }
                }
                if (ArmUDOK == true)
                {
                    if (numericUpDown3.Value != 0)
                    {
                        if (ZQ() == true)
                        {
                            commbyte[22] = BitConverter.GetBytes((int)numericUpDown3.Value)[0];
                        }
                        else
                        {
                            numericUpDown4.Value = 165;
                            commbyte[23] = 0xA5;
                        }
                    }
                    else
                    {
                        commbyte[22] = BitConverter.GetBytes((int)numericUpDown3.Value)[0];
                    }
                }
                if (ArmQHOK == true)
                {
                    if (numericUpDown4.Value != 0xA5)
                    {
                        if (UP() == true)
                        {
                            commbyte[23] = BitConverter.GetBytes((int)numericUpDown4.Value)[0];
                        }
                        else
                        {
                            numericUpDown3.Value = 0;
                            commbyte[22] = 0x00;
                        }
                    }
                    else
                    {
                        commbyte[23] = BitConverter.GetBytes((int)numericUpDown4.Value)[0];
                    }
                }
            }
            else
            {
                if (ArmUDOK == true)
                {
                    commbyte[22] = BitConverter.GetBytes((int)numericUpDown3.Value)[0];
                }
                if (ArmQHOK == true)
                {
                    commbyte[23] = BitConverter.GetBytes((int)numericUpDown4.Value)[0];
                }
            }
            int adc = 0;
            for (int num = 0; num < 25; num++)
            {
                adc += Convert.ToInt32(commbyte[num]);
            }
            byte[] combyte2 = BitConverter.GetBytes(adc);
            commbyte[25] = combyte2[1];
            commbyte[26] = combyte2[0];
            try
            {
                newclient.SendTimeout = 200;
                newclient.Send(commbyte);

            }
            catch { }                        
        }
        #endregion
                  
        #region 串口开关
        EventWaitHandle allDone = new EventWaitHandle(false, EventResetMode.ManualReset);
        private void lblCOM_Click(object sender, EventArgs e)
        {               
            if (OK485 == false)
            {
                newclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ie = new IPEndPoint(IPAddress.Parse("192.168.1.78"), int.Parse("20108"));
                try
                {
                    allDone.Reset();
                    newclient.BeginConnect(ie, new AsyncCallback(CallBackMethod), newclient);
                    allDone.WaitOne(2000);
                    newclient.BeginReceive(m_receiveBuffer, 0, m_receiveBuffer.Length, 0, new AsyncCallback(ReceiveCallBack), newclient);
                    timer1.Enabled = true;
                    timer4.Enabled = true;
                    timerZT.Start();
                    lblCOM.BackColor = Color.Lime;
                    OK485 = true;
                }
                catch (SocketException ex)
                {
                    MessageBox.Show("连接服务器失败！");
                    return;
                }
            }
            else
            {
                timer1.Enabled = false;
                timerZT.Stop();
                newclient.Shutdown(SocketShutdown.Both);
                newclient.Disconnect(true);
                newclient.Close();

                lblCOM.BackColor = Color.DarkGreen;
                lblCOM.ForeColor = Color.White;
                OK485 = false;
            }           
        }
        private void CallBackMethod(IAsyncResult asyncresult)
        {
            try
            {
                newclient = asyncresult.AsyncState as Socket;
                if (newclient != null)
                {
                    newclient.EndConnect(asyncresult);
                    allDone.Set();
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                allDone.Set();
            }
        }
        #endregion

        #region 网络接收数据
        int compass = 0;int JXSKH = 0;int CJKKH = 0;
        private void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {               
                builder = new StringBuilder();
                builder.Length = 0;
                int REnd = newclient.EndReceive(ar);
                if (REnd > 0)
                {
                    foreach (byte b in m_receiveBuffer)
                    {
                        builder.Append(b.ToString("X2"));
                    }
                    string rece = builder.ToString();
                    int index = rece.IndexOf("FFFEFFFE");
                    if (index == 0)
                    {
                        string recesub = rece.Substring(index, 78);      
                                                           
                        #region 指南针
                        string Compass = recesub.Substring(8, 2) + recesub.Substring(10, 2);
                        Int32 comp = Convert.ToInt32(Compass, 16) / 100;
                      
                        if (comp > 0 && comp < 360)
                        {
                            compass = comp;
                            OSDCompass = comp.ToString().PadLeft(4, '0');
                            CurrentJD = comp;
                            if (comp == 0 || comp == 360)
                            {
                                lblHX.Text = "正北";
                            }
                            else if (comp > 0 && comp <= 45)
                            {
                                lblHX.Text = "北偏东" + comp.ToString() + "°";
                            }
                            else if (comp > 45 && comp < 90)
                            {
                                lblHX.Text = "东偏北" + (90 - comp).ToString() + "°";
                            }
                            else if (comp == 90)
                            {
                                lblHX.Text = "正东";
                            }
                            else if (comp > 90 && comp <= 135)
                            {
                                lblHX.Text = "东偏南" + (comp - 90).ToString() + "°";
                            }
                            else if (comp > 135 && comp < 180)
                            {
                                lblHX.Text = "南偏东" + (180 - comp).ToString() + "°";
                            }
                            else if (comp == 180)
                            {
                                lblHX.Text = "正南";
                            }
                            else if (comp > 180 && comp <= 225)
                            {
                                lblHX.Text = "南偏西" + (comp - 180).ToString() + "°";
                            }
                            else if (comp > 225 && comp < 270)
                            {
                                lblHX.Text = "西偏南" + (270 - comp).ToString() + "°";
                            }
                            else if (comp == 270)
                            {
                                lblHX.Text = "正西";
                            }
                            else if (comp > 270 && comp <= 315)
                            {
                                lblHX.Text = "西偏北" + (comp - 270).ToString() + "°";
                            }
                            else if (comp > 315 && comp < 360)
                            {
                                lblHX.Text = "北偏西" + (360 - comp).ToString() + "°";
                            }
                        }
                        else
                        {                            
                            lblHX.Text = "N/A";
                        }
                        #endregion

                        #region 倾斜
                        //前后俯仰角度 

                        string biaoFY = recesub.Substring(56, 2);
                        Int32 biaoFY1 = Convert.ToInt32(biaoFY, 16);
                        string jiaoduFY = recesub.Substring(58, 2) + recesub.Substring(60, 2);
                        Int32 jiaoduFY1 = Convert.ToInt32(jiaoduFY, 16);

                        if (biaoFY1 == 0)
                        {
                            if (jiaoduFY1 == 0)
                            {
                                pitch = 0;
                                OSDPitch = "0000";
                                lblFY.Text = "前后平衡";
                            }
                            else
                            {
                                pitch = -jiaoduFY1;
                                OSDPitch = "-" + jiaoduFY1.ToString().PadLeft(3, '0');
                                lblFY.Text = "前倾 " + jiaoduFY1.ToString() + "°";
                            }
                        }
                        else if (biaoFY1 == 1)
                        {
                            if (jiaoduFY1 == 0)
                            {
                                pitch = 0;
                                OSDPitch = "0000";
                                lblFY.Text = "前后平衡";
                            }
                            else
                            {
                                pitch = jiaoduFY1;
                                OSDPitch = jiaoduFY1.ToString().PadLeft(4, '0');
                                lblFY.Text = "后倾 " + jiaoduFY1.ToString() + "°";
                            }
                        }
                        //左右倾斜角度                        
                        string biaoQX = recesub.Substring(62, 2);
                        Int32 biaoQX1 = Convert.ToInt32(biaoQX, 16);
                        string jiaoduQX = recesub.Substring(64, 2) + recesub.Substring(66, 2);
                        Int32 jiaoduQX1 = Convert.ToInt32(jiaoduQX, 16);
                        if (biaoQX1 == 0)
                        {
                            if (jiaoduQX1 == 0)
                            {
                                roll = 0;
                                OSDRoll = "0000";
                                lblHG.Text = "左右平衡";
                            }
                            else
                            {
                                roll = jiaoduQX1;
                                OSDRoll = jiaoduQX1.ToString().PadLeft(4, '0');
                                lblHG.Text = "右倾 " + jiaoduQX1.ToString() + "°";
                            }
                        }
                        else if (biaoQX1 == 1)
                        {
                            if (jiaoduQX1 == 0)
                            {
                                roll = 0;
                                OSDRoll = "0000";
                                lblHG.Text = "左右平衡";
                            }
                            else
                            {
                                roll = -jiaoduQX1;
                                OSDRoll = "-" + jiaoduQX1.ToString().PadLeft(3, '0');
                                lblHG.Text = "左倾 " + jiaoduQX1.ToString() + "°";
                            }
                        }
                        #endregion

                        #region 深度压力
                        Int32 yl = Convert.ToInt32(recesub.Substring(12, 2) + recesub.Substring(14, 2),16);
                        if (Clear == true)
                        {
                            //已校准,计算后显示
                            if (Depthclear < 0)//负值校准
                            {
                                if (yl > 10000)//负值
                                {
                                    Depth = -1 * Depthclear - (yl - 10000) / 10.0;
                                }
                                else if (yl == 10000)
                                {
                                    Depth = -1 * Depthclear;
                                }
                                else
                                {
                                    Depth = yl / 10.0 + (-1) * Depthclear;
                                }
                            }
                            else
                            {
                                if (yl >= 10000)//负值
                                {
                                    Depth = -1 * Depthclear - (yl - 10000) / 10.0;
                                }
                                else
                                {
                                    Depth = yl / 10.0 - Depthclear;
                                }
                                //Depth = (yl) / 10.0 - Depthclear;                                
                            }
                        }
                        else
                        {
                            //未校准,按照实际接收的显示
                            if (yl > 10000)
                            {
                                Depth = (10000 - yl) / 10.0;
                            }
                            else if (yl == 10000)
                            {
                                Depth = 0.0;
                            }
                            else
                            {
                                Depth = (yl) / 10.0;
                            }
                        }

                        lblDepth.Text = (Depth).ToString("F1");
                        OSDDepth =  (Depth).ToString("F1").PadLeft(4, '0');


                        //string yali = recesub.Substring(12, 2) + recesub.Substring(14, 2);
                        //Int32 yl = Convert.ToInt32(yali, 16);
                        //lblDepth.Text = ((double)yl / 100).ToString("F2");
                        //OSDDepth = ((double)yl / 100).ToString("F2").PadLeft(4, '0');                   
                        #endregion

                        #region 内温
                        string neiwenb = recesub.Substring(68, 2);
                        Int32 neiwenbz = Convert.ToInt32(neiwenb, 16);
                        string neiwen = recesub.Substring(70, 2);
                        Int32 neiwenz = Convert.ToInt32(neiwen, 16);
                        if (neiwenbz == 1)
                        {
                            lblTemp.Text = (-neiwenz).ToString("F0") + "℃";
                            OSDTemp = "-" + (-neiwenz).ToString("F0").PadLeft(3, '0');
                        }
                        else
                        {
                            lblTemp.Text = neiwenz.ToString("F0") + "℃";
                            OSDTemp = neiwenz.ToString("F0").PadLeft(4, '0');
                        }
                        #endregion

                        #region 湿度
                        string shidu = recesub.Substring(72, 2);
                        Int32 shiduz = Convert.ToInt32(shidu, 16);
                        lblshidu.Text = shiduz.ToString() + "%";
                        OSDShidu = shiduz.ToString("F0").PadLeft(4, '0');
                        #endregion

                        #region 水温        
                        Int32 shuiwenz = Convert.ToInt32(recesub.Substring(74, 2) + recesub.Substring(76, 2),16);
                        int shuiwenzhi = shuiwenz;
                        if (shuiwenzhi > 10000)
                        {
                            lblShuiwen.Text = ((10000 - (double)shuiwenzhi) / 10).ToString("F1");
                            OSDTemp2 = "WT:-" + (((double)shuiwenzhi - 10000) / 10).ToString("F1").PadLeft(2, '0');
                        }
                        else
                        {
                            lblShuiwen.Text = ((double)shuiwenzhi / 10).ToString("F1");
                            OSDTemp2 = ((double)shuiwenzhi / 10).ToString("F1").PadLeft(3, '0');
                        }
                        //string shuiwen = recesub.Substring(74, 2) + recesub.Substring(76, 2);
                        //Int32 shuiwenz = Convert.ToInt32(shuiwen, 16);
                        //double shuiwenzhi = (double)shuiwenz / 10;
                        //if (shuiwenzhi < 0)
                        //{
                        //    lblShuiwen.Text = (-shuiwenzhi).ToString("F1") + "℃";
                        //    OSDTemp2 = "-" + (-shuiwenzhi).ToString("F1").PadLeft(3, '0');
                        //}
                        //else
                        //{
                        //    lblShuiwen.Text = shuiwenzhi.ToString("F1") + "℃";
                        //    OSDTemp2 = shuiwenzhi.ToString("F1").PadLeft(4, '0');
                        //}
                        #endregion

                        #region 速度反馈
                        string speed1 = recesub.Substring(24, 2) + recesub.Substring(26, 2);
                        Int32 speed11 = Convert.ToInt32(speed1, 16) * 10;
                        if (speed11 > 0)
                        {
                            lblSPZ.Text = speed11.ToString();
                            axiMotorX1.FanOn = true;
                        }
                        else
                        {
                            lblSPZ.Text = "0";
                            axiMotorX1.FanOn = false;
                        }

                        string speed2 = recesub.Substring(28, 2) + recesub.Substring(30, 2);
                        Int32 speed22 = Convert.ToInt32(speed2, 16) * 10;
                        if (speed22 > 0)
                        {
                            lblSPY.Text = speed22.ToString();
                            axiMotorX2.FanOn = true;
                        }
                        else
                        {
                            lblSPY.Text = "0";
                            axiMotorX2.FanOn = false;
                        }
                        string speed3 = recesub.Substring(32, 2) + recesub.Substring(34, 2);
                        Int32 speed33 = Convert.ToInt32(speed3, 16) * 10;
                        if (speed33 > 0)
                        {
                            lblSPZS.Text = speed33.ToString();
                            axiMotorX3.FanOn = true;
                        }
                        else
                        {
                            lblSPZS.Text = "0";
                            axiMotorX3.FanOn = false;
                        }

                        string speed4 = recesub.Substring(36, 2) + recesub.Substring(38, 2);
                        Int32 speed44 = Convert.ToInt32(speed4, 16) * 10;
                        if (speed44 > 0)
                        {
                            lblSPYS.Text = speed44.ToString();
                            axiMotorX4.FanOn = true;
                        }
                        else
                        {
                            lblSPYS.Text = "0";
                            axiMotorX4.FanOn = false;
                        }

                        #endregion

                        #region 机械臂反馈
                        string ArmUDValue = recesub.Substring(40, 2);
                        Int32 ArmUDValue1 = Convert.ToInt32(ArmUDValue, 16);
                        lblArmUD.Text = ArmUDValue1.ToString();
                        CurrentUD = ArmUDValue1;

                        string ArmQHValue = recesub.Substring(42, 2);
                        Int32 ArmQHValue1 = Convert.ToInt32(ArmQHValue, 16);
                        lblArmQH.Text = ArmQHValue1.ToString();
                        CurrentQH = ArmQHValue1;
                        #endregion

                        #region 角度反馈
                        string jiaodu = recesub.Substring(16, 2);
                        Int32 shiJDD = Convert.ToInt32(jiaodu, 16);
                        lblCAMJD.Text = (90- shiJDD).ToString();
                        //if (zuoshiJDD < 146)
                        //{
                        //    lblCAMJD.Text = (-(146 - zuoshiJDD) * 90 / 58).ToString();
                        //}
                        //else
                        //{
                        //    lblCAMJD.Text = ((zuoshiJDD - 146) * 90 / 60).ToString();
                        //}
                        #endregion



                        JXSKH= Convert.ToInt32(recesub.Substring(44, 2),16);
                        CJKKH = Convert.ToInt32(recesub.Substring(46, 2), 16);
                        if (Convert.ToInt32(recesub.Substring(48, 2), 16) == 1)
                        {
                            lblZL.BackColor = Color.Lime;
                            ZL = true;
                        }
                        else
                        {
                            lblZL.BackColor = Color.DarkGreen;
                            ZL = false;
                        }


                    }
                    else
                    {

                    }
                    newclient.BeginReceive(m_receiveBuffer, 0, m_receiveBuffer.Length, 0, new AsyncCallback(ReceiveCallBack), newclient);
                }
                else
                {
                    timer1.Enabled = false;
                    newclient.Shutdown(SocketShutdown.Both);
                    newclient.Disconnect(true);
                    newclient.Close();

                    lblCOM.BackColor = Color.DarkGreen;
                    lblCOM.ForeColor = Color.White;
                    OK485 = false;
                }
            }
            catch (Exception ex)
            {                

            }
        }
        #endregion       

        #region 手柄代码
        Thread CheckBtn; System.Timers.Timer timerGW1 = null;
        private  void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (SOK == false)
            {
                if (UP() == false)
                {
                    numericUpDown3.Value = 0;//最上
                    return;
                }
                if (CJKZT != "Open")
                {
                    KZT = "KK";
                    commbyte[21] = 0x01;//框开
                    return;
                }
                else
                {
                    KZT = "KT";
                    commbyte[21] = 0x00;//框停                    
                }
                if (ZH() == false)
                {
                    ArmQHOK = true;
                    btnArmQH.ForeColor = Color.Lime;
                    numericUpDown4.Value = 0;//最后
                    return;
                }

                SOK = true;
            }
            else
            {
                if (JXSZT == "Open")
                {
                    SOK = false;
                    btnCJGW.ForeColor = Color.White;
                    btnCJGW.Enabled = true;

                    btnArmCB.Enabled = true;
                    btnLX.Enabled = true;
                    btnJXSOpen.Enabled = true;
                    btnJXSClose.Enabled = true;
                    btnKK.Enabled = true;
                    btnKG.Enabled = true;
                    btnArmUDMax.Enabled = true;
                    btnArmUDMid.Enabled = true;
                    btnArmUDMin.Enabled = true;
                    bnArmFBF.Enabled = true;
                    btnArmFBM.Enabled = true;
                    btnArmFBB.Enabled = true;
                    commbyte[24] = 0x00;
                    CJGW = false;
                    //timerGW1.Elapsed -= new System.Timers.ElapsedEventHandler(OnTimedEvent);                   
                    timerGW1.Stop();
                    //timerGW1.Close();
                    //timerGW1.Dispose();                   
                }
                else
                {
                    commbyte[24] = 0x01;//爪开                            
                    return;
                }
            }
        }

        private void lblStick_Click(object sender, EventArgs e)
        {
            if (lblStick.BackColor == Color.DarkGreen)
            {
                StickOK = true;
                lblStick.BackColor = Color.Lime;
                CheckBtn = new Thread(CheckBtnR);
                Control.CheckForIllegalCrossThreadCalls = false;
                CheckBtn.Start();
            }
            else
            {
                StickOK = false;
                lblStick.BackColor = Color.DarkGreen;
                CheckBtn.Abort();
            }
        }

        bool TELE = false; bool WIDE = false; bool FAR = false; bool NEAR = false;string FX = null;
        void CheckBtnR()
        {
            while (true)
            {
                JOYINFOEX status = new JOYINFOEX();
                status.wSize = Marshal.SizeOf(typeof(JOYINFOEX));
                status.wFlags = 0x00000080;
                joyGetPosEx(0, ref status);
                
                #region XY轴前后左右
                if ((status.wXpos < 41000) && (status.wXpos > 25000) & (status.wYpos < 41000) && (status.wYpos > 25000))
                {                    
                    commbyte[5] = 0x64;
                    commbyte[6] = 0x64;
                    if (FX == "左转" || FX == "右转")
                    {
                        if (cbxCompass.Checked == true)
                        {
                            numericUpDown1.Value = int.Parse(OSDCompass);
                        }
                    }
                    FX = null;
                    btnRight.ForeColor = Color.White;
                    btnLeft.ForeColor = Color.White;
                    btnForward.ForeColor = Color.White;
                    btnBack.ForeColor = Color.White;
                }
                else if ((status.wXpos > status.wYpos) && (status.wXpos + status.wYpos > 65535))
                {
                    commbyte[6] = 0x32;//右转
                    commbyte[5] = 0x64;
                    FX = "右转";
                    btnRight.ForeColor = Color.Red;
                    btnLeft.ForeColor = Color.White;
                }
                else if ((status.wXpos < status.wYpos) && (status.wXpos + status.wYpos < 65535))
                {
                    commbyte[6] = 0xBB;//左转
                    commbyte[5] = 0x64;
                    FX = "左转";
                    btnLeft.ForeColor = Color.Red;
                    btnRight.ForeColor = Color.White;
                }
                else if ((status.wXpos > status.wYpos) && (status.wXpos + status.wYpos < 65535))
                {                    
                    commbyte[5] = 0xBB;//前进           
                    commbyte[6] = 0x64;
                    if (FX != "前进")
                    {
                        FX = "前进";
                        if (cbxCompass.Checked == true)
                        {
                            numericUpDown1.Value = int.Parse(OSDCompass);
                        }
                    }                    
                    btnForward.ForeColor = Color.Red;
                    btnBack.ForeColor = Color.White;
                }
                else if ((status.wXpos < status.wYpos) && (status.wXpos + status.wYpos > 65535))
                {                   
                    commbyte[5] = 0x32;//后退             
                    commbyte[6] = 0x64;
                    if (FX != "后退")
                    {
                        FX = "后退";
                        if (cbxCompass.Checked == true)
                        {
                            numericUpDown1.Value = int.Parse(OSDCompass);
                        }
                    }                    
                    btnBack.ForeColor = Color.Red;
                    btnForward.ForeColor = Color.White;
                }
                else
                {

                    if (FX == "左转" || FX == "右转")
                    {
                        if (cbxCompass.Checked == true)
                        {
                            numericUpDown1.Value = int.Parse(OSDCompass);
                        }
                    }
                    FX = null;
                    commbyte[5] = 0x64;
                    commbyte[6] = 0x64;
                    btnRight.ForeColor = Color.White;
                    btnLeft.ForeColor = Color.White;
                    btnForward.ForeColor = Color.White;
                    btnBack.ForeColor = Color.White;
                }
                #endregion

                #region 1-8按钮操作
                if (status.wButtons.ToString() == "1")
                {
                    if (LX == false)
                    {
                        btnCJGW_Click(null, null);                       
                    }
                }
                else if (status.wButtons.ToString() == "2")
                {
                    if (LX == false)
                    {
                        btnKK.ForeColor = Color.Red;
                        btnKG.ForeColor = Color.White;
                        KZT = "KK";
                    }
                }
                else if (status.wButtons.ToString() == "4")//3
                {
                    if (LX == false)
                    {
                        btnKG.ForeColor = Color.Red;
                        btnKK.ForeColor = Color.White;
                        KZT = "KG";
                    }
                }
                else if (status.wButtons.ToString() == "8")//4
                {
                    if (LX == false)
                    {
                        btnArmCB.ForeColor = Color.Red;
                        ArmUDOK = true;
                        btnArmUD.ForeColor = Color.Lime;
                        numericUpDown3.Value = 0;
                        ArmQHOK = true;
                        btnArmQH.ForeColor = Color.Lime;
                        numericUpDown4.Value = 0;
                    }
                }
                else if (status.wButtons.ToString() == "16")//5
                {
                    
                }
                else if (status.wButtons.ToString() == "32")//6
                {
                   
                }
                else if (status.wButtons.ToString() == "64")//7开灯
                {
                    lblLED.BackColor = Color.Lime;
                    byte18A = "0000001";
                }
                else if (status.wButtons.ToString() == "128")//8关灯
                {
                    lblLED.BackColor = Color.DarkGreen;
                    byte18A = "0000000";
                }               
                else
                {
                    if (LX == false)
                    {
                        btnArmCB.ForeColor = Color.White;
                        btnKK.ForeColor = Color.White;
                        btnKG.ForeColor = Color.White;
                        KZT = "KT";
                    }
                }
                #endregion

                #region Z轴控制上升下降
                if (status.wZpos > 32800 && status.wZpos <= 65535)
                {
                    byte4A = "01";
                    btnUp.ForeColor = Color.Red;
                    btnDown.ForeColor = Color.White;
                }
                else if (status.wZpos > 0 && status.wZpos < 32700)
                {
                    byte4A = "10";
                    btnUp.ForeColor = Color.White;
                    btnDown.ForeColor = Color.Red;
                }
                else
                {
                    byte4A = "00";
                    btnUp.ForeColor = Color.White;
                    btnDown.ForeColor = Color.White;
                }
                #endregion

                #region RX,RY控制抬头低头爪子张开闭合
                if ((status.wRpos < 41000) && (status.wRpos > 25000) & (status.wUpos < 41000) && (status.wUpos > 25000))
                {
                    commbyte[7] = 0x7F;
                    btnCAMup.ForeColor = Color.White;
                    btnCAMDown.ForeColor = Color.White;
                    if (CJGW == false)
                    {
                        commbyte[24] = 0x00;
                        btnJXSOpen.ForeColor = Color.White;
                        btnJXSClose.ForeColor = Color.White;
                    }
                }
                else if ((status.wRpos > status.wUpos) && (status.wRpos + status.wUpos > 65535))
                {
                    commbyte[7] = BitConverter.GetBytes(127 + int.Parse(numericUpDown5.Text))[0];
                    btnCAMup.ForeColor = Color.White;
                    btnCAMDown.ForeColor = Color.Red;

                    commbyte[24] = 0x00;
                    btnJXSOpen.ForeColor = Color.White;
                    btnJXSClose.ForeColor = Color.White;
                }
                else if ((status.wRpos < status.wUpos) && (status.wRpos + status.wUpos < 65535))
                {
                    commbyte[7] = BitConverter.GetBytes(127 - int.Parse(numericUpDown5.Text))[0];
                    btnCAMup.ForeColor = Color.Red;
                    btnCAMDown.ForeColor = Color.White;

                    commbyte[24] = 0x00;
                    btnJXSOpen.ForeColor = Color.White;
                    btnJXSClose.ForeColor = Color.White;
                }
                else if ((status.wRpos > status.wUpos) && (status.wRpos + status.wUpos < 65535))
                {
                    commbyte[24] = 0x01;
                    btnJXSOpen.ForeColor = Color.Red;
                    btnJXSClose.ForeColor = Color.White;

                    commbyte[7] = 0x7F;                   
                    btnCAMup.ForeColor = Color.White;
                    btnCAMDown.ForeColor = Color.White;
                }
                else if ((status.wRpos < status.wUpos) && (status.wRpos + status.wUpos > 65535))
                {
                    commbyte[24] = 0x02;
                    btnJXSOpen.ForeColor = Color.White;
                    btnJXSClose.ForeColor = Color.Red;

                    commbyte[7] = 0x7F;
                    btnCAMup.ForeColor = Color.White;
                    btnCAMDown.ForeColor = Color.White;
                }
                else
                {
                    commbyte[7] = 0x7F;                    
                    btnCAMup.ForeColor = Color.White;
                    btnCAMDown.ForeColor = Color.White;
                    if (CJGW == false)
                    {
                        commbyte[24] = 0x00;
                        btnJXSOpen.ForeColor = Color.White;
                        btnJXSClose.ForeColor = Color.White;
                    }                  
                }               
                #endregion

                #region POV控制横推电机
                if (status.wPOV.ToString() == "0")
                {
                    btnArmUDMax_Click(null, null);
                    btnArmUDMax.ForeColor = Color.Red;
                    btnArmUDMin.ForeColor = Color.White;
                }
                else if (status.wPOV.ToString() == "9000")
                {
                    bnArmFBF_Click(null, null);
                    bnArmFBF.ForeColor = Color.Red;
                    btnArmFBB.ForeColor = Color.White;
                }
                else if (status.wPOV.ToString() == "18000")
                {
                    btnArmUDMin_Click(null, null);
                    btnArmUDMin.ForeColor = Color.Red;
                    btnArmUDMax.ForeColor = Color.White;
                }
                else if (status.wPOV.ToString() == "27000")
                {
                    btnArmFBB_Click(null, null);
                    btnArmFBB.ForeColor = Color.Red;
                    bnArmFBF.ForeColor = Color.White;
                }
                else
                {
                    btnArmUDMax.ForeColor = Color.White;
                    btnArmUDMin.ForeColor = Color.White;
                    btnArmFBB.ForeColor = Color.White;
                    bnArmFBF.ForeColor = Color.White;
                }
                #endregion

                Thread.Sleep(100);

            }
        }
        #endregion      
        
        #region 速度控制
       
        //横推速度加
        private void btnHTJia_Click(object sender, EventArgs e)
        {
            //if (int.Parse(lblHT.Text) < 10)
            //{
            //    lblHT.Text = (int.Parse(lblHT.Text) + 1).ToString();
            //    lblHT.Text = lblHT.Text;
            //}
        }
        //横推速度减
        private void btnHTJian_Click(object sender, EventArgs e)
        {
            //if (int.Parse(lblHT.Text) > 1)
            //{
            //    lblHT.Text = (int.Parse(lblHT.Text) - 1).ToString();
            //    lblHT.Text = lblHT.Text;
            //}
        }
        #endregion              

        #region 录像截图
        //录像
        private void btnVideo_Click(object sender, EventArgs e)
        {
            if (camera.handle1 >= 0)
            {
                if (videoingQ==false)
                {
                    FileNameQ = filepath + "\\VIDEO_LEFT_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".avi";
                    bool chn1OK = LauInterop.HDVPLAY_StartRecord(m_nRealHandle200Q, FileNameQ, E_RECORD_TYPE.RECORD_AVI);
                    if (chn1OK == true)
                    {
                        videoingQ = true;
                        btnVideo.Image = imageList1.Images[1];
                        timer2.Enabled = true;
                    }
                }
                else
                {
                    LauInterop.HDVPLAY_StopRecord(m_nRealHandle200Q);
                    FileNameQ = "";
                    videoingQ = false;
                    btnVideo.Image = imageList1.Images[0];
                    timer2.Enabled = false;
                }
            }
        }

        //截图
        private void btnSnap_Click(object sender, EventArgs e)
        {
            if (camera.handle1 >= 0)
            {
                try
                {
                    string PICName = filepath + "\\SNAP_LEFT_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bmp";
                    bool snapcaptureOK = LauInterop.HDVPLAY_CapturePicture(camera.handle1, PICName, E_PIC_TYPE.PIC_BMP);                    
                }
                catch { }
            }
        }
        private void btnVideoH_Click(object sender, EventArgs e)
        {
            if (camera.handle2 >= 0)
            {
                if (videoingH == false)
                {
                    FileNameH = filepath + "\\VIDEO_RIGHT_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".avi";
                    bool chn2OK = LauInterop.HDVPLAY_StartRecord(m_nRealHandle200H, FileNameH, E_RECORD_TYPE.RECORD_AVI);
                    if (chn2OK == true)
                    {
                        videoingH = true;
                        btnVideoH.Image = imageList1.Images[1];
                        timer3.Enabled = true;
                    }
                }
                else
                {
                    LauInterop.HDVPLAY_StopRecord(m_nRealHandle200H);
                    FileNameH = "";
                    videoingH = false;
                    btnVideoH.Image = imageList1.Images[0];
                    timer3.Enabled = false;
                }
            }
        }

        private void btnCaptureH_Click(object sender, EventArgs e)
        {
            if (camera.handle2 >= 0)
            {
                try
                {
                    string PICName = filepath + "\\SNAP_RIGHT_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bmp";
                    bool snapcaptureOK = LauInterop.HDVPLAY_CapturePicture(camera.handle2, PICName, E_PIC_TYPE.PIC_BMP);
                }
                catch { }
            }
        }
        #endregion

        #region 窗体关闭
        private void mainform_FormClosing(object sender, FormClosingEventArgs e)
        {            
            if (MessageBox.Show("确定要退出吗？", "提示", MessageBoxButtons.OKCancel,MessageBoxIcon.Question)==DialogResult.OK)
            {
                timer1.Enabled = false;
                timerOSD.Enabled = false;                              
                if (videoingQ == true)
                {
                    btnVideo_Click(null, null);
                }
                if (videoingH == true)
                {
                    btnVideoH_Click(null, null);
                }
                Application.DoEvents();
                this.Dispose();
                this.Close();
                Application.Exit();
                Application.ExitThread();
                Process.GetCurrentProcess().Kill();
                System.Environment.Exit(System.Environment.ExitCode);
            }
            else
            {
                e.Cancel = true;
            }
        }
        #endregion       

        #region 控制操作
        //上升
        private void btnUp_MouseDown(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                byte4A = "01";
            }
        }

        private void btnUp_MouseUp(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                byte4A = "00";                
            }
        }
        //下降
        private void btnDown_MouseDown(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                byte4A = "10";
            }
        }

        private void btnDown_MouseUp(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                byte4A = "00";               
            }
        }
        //前进
        private void btnForward_MouseDown(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {                
                commbyte[5] = 0xBB;                                      
            }
        }

        private void btnForward_MouseUp(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                commbyte[5] = 0x64;
            }
        }
        //后退
        private void btnBack_MouseDown(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {                
                commbyte[5] = 0x32;              
            }
        }

        private void btnBack_MouseUp(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                commbyte[5] = 0x64;
            }
        }
        //左转
        private void btnLeft_MouseDown(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                commbyte[6] = 0xBB;
            }
        }

        private void btnLeft_MouseUp(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                commbyte[6] = 0x64;               
            }
        }
        //右转
        private void btnRight_MouseDown(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                commbyte[6] = 0x32;
            }
        }

        private void btnRight_MouseUp(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                commbyte[6] = 0x64;                
            }
        }       
        //机械手张开闭合
        private void btnJXSClose_MouseDown(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                commbyte[24] = 0x02;
            }
        }

        private void btnJXSClose_MouseUp(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                commbyte[24] = 0x00;
            }
        }

        private void btnJXSOpen_MouseDown(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                commbyte[24] = 0x01;
            }
        }

        private void btnJXSOpen_MouseUp(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                commbyte[24] = 0x00;
            }
        }
        //机械手左转右转
        private void btnArmLeft_MouseDown(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                byte4D = "10";
            }
        }

        private void btnArmLeft_MouseUp(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                byte4D = "00";
            }
        }

        private void btnArmRight_MouseDown(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                byte4D = "01";
            }
        }

        private void btnArmRight_MouseUp(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                byte4D = "00";
            }
        }
        //摄像头抬头低头
        private void btnCAMUp_MouseDown(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {                               
                commbyte[7] = BitConverter.GetBytes(127 - int.Parse(numericUpDown5.Text))[0];
            }
        }

        private void btnCAMUp_MouseUp(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                commbyte[7] = 0x7F;
            }
        }

        private void btnCAMDown_MouseDown(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {                                
                commbyte[7] = BitConverter.GetBytes(127 + int.Parse(numericUpDown5.Text))[0];
            }
        }

        private void btnCAMDown_MouseUp(object sender, MouseEventArgs e)
        {
            if (StickOK == false)
            {
                commbyte[7] = 0x7F;
            }
        }
        #endregion

        #region 读写系统配置
        private void LoadParameterXml()
        {
            if (File.Exists(Application.StartupPath + "\\" + "Parameter.xml"))
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
                    filepath = xlist1.Item(0).InnerText;                    
                }
            }
            else
            {
                StreamWriter sw = File.AppendText(Application.StartupPath + "\\" + "Parameter.xml");
                sw.Flush();
                sw.Close();

                XmlDocument xmlDoc = new XmlDocument();
                //创建类型声明节点  
                XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "gb2312", "");
                xmlDoc.AppendChild(node);
                //创建根节点  

                XmlNode root1 = xmlDoc.CreateElement("root");
                xmlDoc.AppendChild(root1);

                XmlNode root = xmlDoc.CreateElement("Equipment");
                root1.AppendChild(root);                       
                CreateNode(xmlDoc, root, "filepath", "C:\\");               
                try
                {
                    xmlDoc.Save(Application.StartupPath + "\\" + "Parameter.xml");
                }
                catch (Exception e)
                {
                    //显示错误信息  
                    //Console.WriteLine(e.Message);
                }
                LoadParameterXml();
            }
        }
        public void CreateNode(XmlDocument xmlDoc, XmlNode parentNode, string name, string value)
        {
            XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, name, null);
            node.InnerText = value;
            parentNode.AppendChild(node);
        }
        #endregion

        #region 菜单
        private void mainform_SysBottomClick(object sender, SysButtonEventArgs e)
        {
            Point l = PointToScreen(e.SysButton.Location);
            l.Y += e.SysButton.Size.Height + 1;
            ContextMenuStrip.Show(l);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            setform setfrm = new setform();
            setfrm.ShowDialog();
        }

        private void 本地文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", filepath);
        }
        private void 本地截图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setform setfrm = new setform();
            setfrm.ShowDialog();
        }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            aboutform aboutfrm = new aboutform();
            aboutfrm.ShowDialog();
        }
        #endregion                         

        #region 字符叠加      
        private void timerOSD_Tick(object sender, EventArgs e)
        {         
            //try
            //{
            //    string sUrl = "http://192.168.1.18/cgi/image_set?Channel=1&Group=OSDInfo&TextOSDStatus=1&TextOSDTitle=" + OSDCompass + "  " + OSDPitch + "  " + OSDRoll + "  " + OSDDepth + "  " + OSDTemp + "  " + OSDShidu + "  " + OSDTemp2 + "&TextOSDX =0&TextOSDY=92&TimeOSDStatus=1&TimeOSDX=95&TimeOSDY=2";
            //    byte[] CSTmp = new byte[2048];
            //    LauInterop.IPCCGI_SetValue(sUrl, CSTmp, "admin", "admin", 200);               

            //}
            //catch { }
           
        }        
        #endregion             

        #region 图片旋转
        public Image RotateImg(Image b, int angle)
        {

            angle = angle % 360;


            //弧度转换  

            double radian = angle * Math.PI / 180.0;

            double cos = Math.Cos(radian);

            double sin = Math.Sin(radian);


            //原图的宽和高  

            int w = b.Width;

            int h = b.Height;

            int W = (int)(Math.Max(Math.Abs(w * cos - h * sin), Math.Abs(w * cos + h * sin)));

            int H = (int)(Math.Max(Math.Abs(w * sin - h * cos), Math.Abs(w * sin + h * cos)));


            //目标位图  

            Bitmap dsImage = new Bitmap(W, H);

            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(dsImage);

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;


            //计算偏移量  

            Point Offset = new Point((W - w) / 2, (H - h) / 2);


            //构造图像显示区域：让图像的中心与窗口的中心点一致  

            Rectangle rect = new Rectangle(Offset.X, Offset.Y, w, h);

            Point center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);


            g.TranslateTransform(center.X, center.Y);

            g.RotateTransform(360 - angle);


            //恢复图像在水平和垂直方向的平移  

            g.TranslateTransform(-center.X, -center.Y);

            g.DrawImage(b, rect);


            //重至绘图的所有变换  

            g.ResetTransform();


            g.Save();

            g.Dispose();

            //保存旋转后的图片  

            b.Dispose();

            dsImage.Save("FocusPoint.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            return dsImage;

        }
        public Image RotateImg(string filename, int angle)
        {

            return RotateImg(GetSourceImg(filename), angle);

        }
        public Image GetSourceImg(string filename)
        {

            Image img;



            img = Bitmap.FromFile(filename);


            return img;

        }

        #endregion

        #region 摄像头校时
        private void metroDMButton1_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dtlast = new DateTime(1970, 1, 1);
                DateTime dtThis = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                int interval = new TimeSpan(dtThis.Ticks - dtlast.Ticks).Days * 24 * 60 * 60;
                int intervalseond = (DateTime.Now.Hour - 8) * 60 * 60 + DateTime.Now.Minute * 60 + DateTime.Now.Second + interval;
                string sUrl = "http://192.168.1.20/cgi/sys_set?Group=TimeInfo&TimeMethod=0&CameraTime=" + intervalseond;
                byte[] CSTmp = new byte[2048];
                LauInterop.IPCCGI_SetValue(sUrl, CSTmp, "admin", "admin", 500);

                string sUrl1 = "http://192.168.1.18/cgi/sys_set?Group=TimeInfo&TimeMethod=0&CameraTime=" + intervalseond;
                byte[] CSTmp1 = new byte[2048];
                LauInterop.IPCCGI_SetValue(sUrl1, CSTmp1, "admin", "admin", 500);
            }
            catch { }
        }

        #endregion        
               
        #region 灯开关
        private void lblLED_Click(object sender, EventArgs e)
        {
            if (lblLED.BackColor == Color.DarkGreen)
            {
                lblLED.BackColor = Color.Lime;
                byte18A = "0000001";
            }
            else
            {
                lblLED.BackColor = Color.DarkGreen;
                byte18A = "0000000";
            }
        }
        #endregion

        #region 图像位置切换
        private void Play1_DoubleClick(object sender, EventArgs e)
        {
            //int X = Play1.Location.X; int Y = Play1.Location.Y;
            //int W = Play1.Width; int H = Play1.Height;
            //Play1.Location = new Point(Play2.Location.X, Play2.Location.Y);
            //Play1.Size = new Size(Play2.Width, Play2.Height);
            //Play2.Location = new Point(X, Y);
            //Play2.Size = new Size(W, H);
        }

        private void Play2_DoubleClick(object sender, EventArgs e)
        {
            //int X = Play2.Location.X; int Y = Play2.Location.Y;
            //int W = Play2.Width; int H = Play2.Height;
            //Play2.Location = new Point(Play1.Location.X, Play1.Location.Y);
            //Play2.Size = new Size(Play1.Width, Play1.Height);
            //Play1.Location = new Point(X, Y);
            //Play1.Size = new Size(W, H);
        }
        #endregion

        #region 录像红圈显示
        private void timer2_Tick(object sender, EventArgs e)
        {
            Graphics g = Play1.CreateGraphics();           
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Brush bush = new SolidBrush(Color.Red);//填充的颜色
            g.FillEllipse(bush, 10, 10, 15, 15);//
            g.Dispose();
        }
        private void timer3_Tick(object sender, EventArgs e)
        {
            Graphics g = Play2.CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Brush bush = new SolidBrush(Color.Red);//填充的颜色
            g.FillEllipse(bush, 10, 10, 15, 15);//
            g.Dispose();
        }
        #endregion

        #region 指南针
        private void timer4_Tick(object sender, EventArgs e)
        {
            headingIndicatorInstrumentControl1.SetHeadingIndicatorParameters(compass);
            attitudeIndicatorInstrumentControl1.SetAttitudeIndicatorParameters(pitch, roll);
        }
        #endregion

        #region 调光
        private void skinTrackBar1_ValueChanged(object sender, EventArgs e)
        {
            //lblHT.Text = skinTrackBar1.Value.ToString();
        }
        #endregion

        #region 定向
        bool DXOK = false;
        private void btnDingX_Click(object sender, EventArgs e)
        {
            if (DXOK == false)
            {
                cbxCompass.Enabled = false;              
                numericUpDown1.Enabled = false;
                DXOK = true;
                btnDingX.Text = "解锁";
                btnDingX.ForeColor = Color.Red;                
            }
            else
            {
                numericUpDown1.Enabled = true;
                DXOK = false;
                btnDingX.Text = "锁定";
                btnDingX.ForeColor = Color.White;
                cbxCompass.Enabled = true;
            }            
        }
        #endregion

        #region 定深
        bool DSOK = false;
        private void btnDingS_Click(object sender, EventArgs e)
        {
            if (DSOK == false)
            {
                cbxDepth.Enabled = false;         
                numericUpDown2.Enabled = false;
                DSOK = true;
                btnDingS.Text = "解锁";
                btnDingS.ForeColor = Color.Red;               
            }
            else
            {
                cbxDepth.Enabled = true;
                numericUpDown2.Enabled = true;
                DSOK = false;
                btnDingS.Text = "锁定";
                btnDingS.ForeColor = Color.White;               
            }
        }

        #endregion

        #region 连接摄像机
        private void btnConnect_Click(object sender, EventArgs e)
        {
            LoginDeviceNew200Q(camera.IP1, camera.Port1, camera.UserName1, camera.UserPwd1, camera.Channel1, camera.handle2);
            
        }
        private void btnConnect2_Click(object sender, EventArgs e)
        {
            LoginDeviceNew200H(camera.IP2, camera.Port2, camera.UserName2, camera.UserPwd2, camera.Channel2, camera.handle2);
        }

        #endregion

        #region 激光灯开关
        private void lblJG_Click(object sender, EventArgs e)
        {
            if (JiGuangOK == false)
            {
                byte18B = "1";
                JiGuangOK = true;
                //lblJG.BackColor = Color.Lime;
                //lblJG.ForeColor = Color.Red;
            }
            else
            {
                byte18B = "0";
                JiGuangOK = false;
                //lblJG.BackColor = Color.DarkGreen;
                //lblJG.ForeColor = Color.White;
            }
        }
        #endregion

        #region 自动方向锁定
        private void cbxCompass_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxCompass.Checked == true)
            {
                btnDingX.Enabled = false;
                AutoHX = true;
            }
            else
            {
                btnDingX.Enabled = true;
                AutoHX = false;
            }
        }
        #endregion

        #region 自动深度锁定
        private void cbxDepth_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxDepth.Checked == true)
            {
                btnDingS.Enabled = false;
                AutoSD = true;
            }
            else
            {
                btnDingS.Enabled = true;
                AutoSD = false;
            }
        }
        #endregion

        #region 调焦
        private void btnCAMDa_MouseDown(object sender, MouseEventArgs e)
        {
            LauInterop.IPCNET_PTZControl(NewLoginID200Q, 0, E_PTZ_COMMAND.ZOOM_TELE, 6, 6, false);
        }

        private void btnCAMDa_MouseUp(object sender, MouseEventArgs e)
        {
            LauInterop.IPCNET_PTZControl(NewLoginID200Q, 0, E_PTZ_COMMAND.ZOOM_TELE, 6, 6, true);
        }

        private void btnCAMXiao_MouseDown(object sender, MouseEventArgs e)
        {
            LauInterop.IPCNET_PTZControl(NewLoginID200Q, 0, E_PTZ_COMMAND.ZOOM_WIDE, 6, 6, false);
        }

        private void btnCAMXiao_MouseUp(object sender, MouseEventArgs e)
        {
            LauInterop.IPCNET_PTZControl(NewLoginID200Q, 0, E_PTZ_COMMAND.ZOOM_WIDE, 6, 6, true);
        }

        private void btnCAMJY_MouseDown(object sender, MouseEventArgs e)
        {
            LauInterop.IPCNET_PTZControl(NewLoginID200Q, 0, E_PTZ_COMMAND.FOCUS_NEAR, 6, 6, false);
        }

        private void btnCAMJY_MouseUp(object sender, MouseEventArgs e)
        {
            LauInterop.IPCNET_PTZControl(NewLoginID200Q, 0, E_PTZ_COMMAND.FOCUS_NEAR, 6, 6, true);
        }

        private void btnCAMJJ_MouseDown(object sender, MouseEventArgs e)
        {
            LauInterop.IPCNET_PTZControl(NewLoginID200Q, 0, E_PTZ_COMMAND.FOCUS_FAR, 6, 6, false);
        }

        private void btnCAMJJ_MouseUp(object sender, MouseEventArgs e)
        {
            LauInterop.IPCNET_PTZControl(NewLoginID200Q, 0, E_PTZ_COMMAND.FOCUS_FAR, 6, 6, true);
        }


        #endregion

        #region 录像截图
        private void btnConnect3_Click(object sender, EventArgs e)
        {
            if (m_lUserID < 0)
            {
                m_lUserID = CHCNetSDK.NET_DVR_Login_V30("192.168.1.68", 8000, "admin", "admin12345", ref DeviceInfo);
                if (m_lUserID < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    MessageBox.Show("模拟摄像机采集登录失败,请稍后重试！" + iLastErr);
                    return;
                }
            }
            if (m_lRealHandleHK1 < 0)
            {
                CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
                lpPreviewInfo.hPlayWnd = Play3.Handle;//预览窗口 live view window
                lpPreviewInfo.lChannel = 3;//预览的设备通道 the device channel number
                lpPreviewInfo.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                lpPreviewInfo.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
                lpPreviewInfo.bBlocked = true; //0- 非阻塞取流，1- 阻塞取流
                lpPreviewInfo.dwDisplayBufNum = 0; //播放库显示缓冲区最大帧数

                IntPtr pUser = IntPtr.Zero;//用户数据 user data               
                m_lRealHandleHK1 = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null/*RealData*/, pUser);
                if (m_lRealHandleHK1 < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    MessageBox.Show("通道1预览失败！" + iLastErr);
                    return;
                }
                else
                {
                    //预览成功
                    //DebugInfo("NET_DVR_RealPlay_V40 succ!");
                    //btnPreview.Text = "Stop View";    
                    btnConnect3.ForeColor = Color.Lime;
                }
            }
            else
            {
                if (!CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandleHK1))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    //str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
                    //DebugInfo(str);
                    MessageBox.Show("模拟通道1停止预览失败！" + iLastErr);
                    return;
                }
                else
                {
                    m_lRealHandleHK1 = -1;
                    Play3.Refresh();
                    btnConnect3.ForeColor = Color.White;
                }
            }
        }

        private void btnConnect4_Click(object sender, EventArgs e)
        {
            if (m_lUserID < 0)
            {
                m_lUserID = CHCNetSDK.NET_DVR_Login_V30("192.168.1.68", 8000, "admin", "admin12345", ref DeviceInfo);
                if (m_lUserID < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    MessageBox.Show("模拟摄像机采集登录失败,请稍后重试！" + iLastErr);
                    return;
                }
            }
            if (m_lRealHandleHK2 < 0)
            {
                CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
                lpPreviewInfo.hPlayWnd = Play4.Handle;//预览窗口 live view window
                lpPreviewInfo.lChannel = 4;//预览的设备通道 the device channel number
                lpPreviewInfo.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                lpPreviewInfo.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
                lpPreviewInfo.bBlocked = true; //0- 非阻塞取流，1- 阻塞取流
                lpPreviewInfo.dwDisplayBufNum = 0; //播放库显示缓冲区最大帧数

                IntPtr pUser = IntPtr.Zero;//用户数据 user data               
                m_lRealHandleHK2 = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null/*RealData*/, pUser);
                if (m_lRealHandleHK2 < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    MessageBox.Show("通道2预览失败！" + iLastErr);
                    return;
                }
                else
                {
                    //预览成功
                    //DebugInfo("NET_DVR_RealPlay_V40 succ!");
                    //btnPreview.Text = "Stop View";   
                    btnConnect4.ForeColor = Color.Lime;
                }
            }
            else
            {
                if (!CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandleHK2))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    MessageBox.Show("通道2停止预览失败！" + iLastErr);
                    return;
                }
                else
                {
                    m_lRealHandleHK2 = -1;
                    Play4.Refresh();
                    btnConnect4.ForeColor = Color.White;
                }
            }
        }

        private void btnVideo3_Click(object sender, EventArgs e)
        {
            if (m_lRealHandleHK1 != -1)
            {
                if (m_bRecordHK1 == false)
                {
                    FileNameHK1 = filepath + "\\VIDEO_JXS_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".mp4";
                    CHCNetSDK.NET_DVR_MakeKeyFrame(m_lUserID, 1);
                    bool chn1OK = CHCNetSDK.NET_DVR_SaveRealData(m_lRealHandleHK1, FileNameHK1);
                    if (chn1OK == true)
                    {
                        m_bRecordHK1 = true;
                    }
                    else
                    {
                        MessageBox.Show("通道1录像失败！");
                    }
                }
                else
                {
                    CHCNetSDK.NET_DVR_StopSaveRealData(m_lRealHandleHK1);
                    m_bRecordHK1 = false;
                    FileNameHK1 = "";
                }
            }
        }

        private void btnVideo4_Click(object sender, EventArgs e)
        {
            if (m_lRealHandleHK2 != -1)
            {
                if (m_bRecordHK2 == false)
                {
                    FileNameHK2 = filepath + "\\VIDEO_CJK_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".mp4";
                    CHCNetSDK.NET_DVR_MakeKeyFrame(m_lUserID, 2);
                    bool chn2OK = CHCNetSDK.NET_DVR_SaveRealData(m_lRealHandleHK2, FileNameHK2);
                    if (chn2OK == true)
                    {
                        m_bRecordHK2 = true;
                    }
                    else
                    {
                        MessageBox.Show("通道3录像失败！");
                    }
                }
                else
                {
                    CHCNetSDK.NET_DVR_StopSaveRealData(m_lRealHandleHK2);
                    m_bRecordHK2 = false;
                    FileNameHK2 = "";
                }
            }
        }

        private void btnCapture3_Click(object sender, EventArgs e)
        {
            if (m_lRealHandleHK1 != -1)
            {
                string PICNameHK1 = filepath + "\\SNAP_JXS_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bmp";
                CHCNetSDK.NET_DVR_CapturePicture(m_lRealHandleHK1, PICNameHK1);

            }
            else
            {
                MessageBox.Show("请先开启辅摄像头！");
            }
        }

        private void btnCapture4_Click(object sender, EventArgs e)
        {
            if (m_lRealHandleHK2 != -1)
            {
                string PICNameHK2 = filepath + "\\SNAP_CJK_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bmp";
                CHCNetSDK.NET_DVR_CapturePicture(m_lRealHandleHK2, PICNameHK2);

            }
            else
            {
                MessageBox.Show("请先开启辅摄像头！");
            }

        }
        public CHCNetSDK.NET_DVR_TIME m_struTimeCfg;
        private void skinButton8_Click(object sender, EventArgs e)
        {
            m_struTimeCfg.dwYear = UInt32.Parse(DateTime.Now.Year.ToString());
            m_struTimeCfg.dwMonth = UInt32.Parse(DateTime.Now.Month.ToString());
            m_struTimeCfg.dwDay = UInt32.Parse(DateTime.Now.Day.ToString());
            m_struTimeCfg.dwHour = UInt32.Parse(DateTime.Now.Hour.ToString());
            m_struTimeCfg.dwMinute = UInt32.Parse(DateTime.Now.Minute.ToString());
            m_struTimeCfg.dwSecond = UInt32.Parse(DateTime.Now.Second.ToString());

            Int32 nSize = Marshal.SizeOf(m_struTimeCfg);
            IntPtr ptrTimeCfg = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(m_struTimeCfg, ptrTimeCfg, false);

            if (!CHCNetSDK.NET_DVR_SetDVRConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_TIMECFG, -1, ptrTimeCfg, (UInt32)nSize))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //strErr = "NET_DVR_SET_TIMECFG failed, error code= " + iLastErr;
                ////设置时间失败，输出错误号 Failed to set the time of device and output the error code
                //MessageBox.Show(strErr);
                MessageBox.Show("校时失败！");
            }
            else
            {
                MessageBox.Show("校时成功！");
            }

            Marshal.FreeHGlobal(ptrTimeCfg);
        }
    
    #endregion

        #region 机械臂操作
        private void btnArmUD_Click(object sender, EventArgs e)
        {
            if (ArmUDOK == false)
            {
                ArmUDOK = true;
                btnArmUD.ForeColor = Color.Lime;
            }
            else
            {
                ArmUDOK = false;
                btnArmUD.ForeColor = Color.White;
            }
        }

        private void btnArmQH_Click(object sender, EventArgs e)
        {
            if (ArmQHOK == false)
            {
                ArmQHOK = true;
                btnArmQH.ForeColor = Color.Lime;
            }
            else
            {
                ArmQHOK = false;
                btnArmQH.ForeColor = Color.White;
            }
        }

        private void btnArmUDMax_Click(object sender, EventArgs e)
        {
            numericUpDown3.Value = 0;
            ArmUDOK = true;
            btnArmUD.ForeColor = Color.Lime;
        }

        private void btnArmUDMid_Click(object sender, EventArgs e)
        {
            numericUpDown3.Value = 105;
            ArmUDOK = true;
            btnArmUD.ForeColor = Color.Lime;
        }

        private void btnArmUDMin_Click(object sender, EventArgs e)
        {
            numericUpDown3.Value = 210;
            ArmUDOK = true;
            btnArmUD.ForeColor = Color.Lime;
        }

        private void bnArmFBF_Click(object sender, EventArgs e)
        {
            numericUpDown4.Value = 165;
            ArmQHOK = true;
            btnArmQH.ForeColor = Color.Lime;
        }

        private void btnArmFBM_Click(object sender, EventArgs e)
        {
            numericUpDown4.Value = 82;
            ArmQHOK = true;
            btnArmQH.ForeColor = Color.Lime;
        }

        private void btnArmFBB_Click(object sender, EventArgs e)
        {
            numericUpDown4.Value = 0;
            ArmQHOK = true;
            btnArmQH.ForeColor = Color.Lime;
        }

        private void btnArmCB_Click(object sender, EventArgs e)
        {
            ArmUDOK = true;
            btnArmUD.ForeColor = Color.Lime;
            numericUpDown3.Value = 0;
            ArmQHOK = true;
            btnArmQH.ForeColor = Color.Lime;
            numericUpDown4.Value = 0;
        }
        string KZT = "KT";
        private void btnKK_MouseDown(object sender, MouseEventArgs e)
        {
            //commbyte[21] = 0x01;
            KZT = "KK";
        }

        private void btnKK_MouseUp(object sender, MouseEventArgs e)
        {
            //commbyte[21] = 0x00;
            KZT = "KT";
        }

        private void btnKG_MouseDown(object sender, MouseEventArgs e)
        {
            //commbyte[21] = 0x02;
            KZT = "KG";
        }

        private void btnKG_MouseUp(object sender, MouseEventArgs e)
        {
            //commbyte[21] = 0x00;
            KZT = "KT";
        }
        #endregion

        #region 深度清零
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (MessageBox.Show("深度清零会将当前深度重置为0，确定要进行此操作吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                try
                {
                    Depthclear = double.Parse(lblDepth.Text);
                    Clear = true;
                    linkLabel1.Enabled = false;
                }
                catch { }
            }
        }
        #endregion

        #region 轮循采集
        bool LX = false;int LXJD = 0;int CurrentJD = 0;int CurrentUD = 0;int CurrentQH = 0;
        private bool JD()
        {
            if (Math.Abs(CurrentJD - LXJD) <= 5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }     
        private void btnLX_Click(object sender, EventArgs e)
        {
            if (LX == false)
            {
                if (StickOK == true)
                {
                    StickOK = false;
                    lblStick.BackColor = Color.DarkGreen;
                    CheckBtn.Abort();
                }
                

                LX = true;
                btnLX.ForeColor = Color.Lime;
                ArmUDOK = true;
                ArmQHOK = true;
                ZT = "抓";
                timerLX.Start();
                numericUpDown4.Value = 165;
                cbxCompass.Enabled = false;
                numericUpDown1.Value = (decimal)CurrentJD;
                numericUpDown1.Enabled = false;
                LXJD = (int)numericUpDown1.Value;
                DXOK = true;
                btnDingX.Text = "解锁";
                btnDingX.ForeColor = Color.Red;

                btnArmCB.Enabled = false;
                btnCJGW.Enabled = false;
                btnJXSOpen.Enabled = false;
                btnJXSClose.Enabled = false;
                btnKK.Enabled = false;
                btnKG.Enabled = false;
                btnArmUDMax.Enabled = false;
                btnArmUDMid.Enabled = false;
                btnArmUDMin.Enabled = false;
                bnArmFBF.Enabled = false;
                btnArmFBM.Enabled = false;
                btnArmFBB.Enabled = false;
                

            }
            else
            {
                ArmUDOK = false;
                ArmQHOK = false;
                LX = false;
                btnLX.ForeColor = Color.White;
                timerLX.Stop();

                btnArmCB.Enabled = true;
                btnCJGW.Enabled = true;
                btnJXSOpen.Enabled = true;
                btnJXSClose.Enabled = true;
                btnKK.Enabled = true;
                btnKG.Enabled = true;
                btnArmUDMax.Enabled = true;
                btnArmUDMid.Enabled = true;
                btnArmUDMin.Enabled = true;
                bnArmFBF.Enabled = true;
                btnArmFBM.Enabled = true;
                btnArmFBB.Enabled = true;

                numericUpDown1.Enabled = true;
                DXOK = false;
                byte4A = "00";
                btnDingX.Text = "锁定";
                btnDingX.ForeColor = Color.White;
                cbxCompass.Enabled = true;

                cbxDepth.Enabled = true;
                numericUpDown2.Enabled = true;
                DSOK = false;
                btnDingS.Text = "锁定";
                btnDingS.ForeColor = Color.White;

                ActNum = 0;
                ActNum2 = 0;
            }
        }
        private bool UP()
        {
            if (CurrentUD <= 5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool DOWN()
        {
            if (CurrentUD >=205)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool ZQ()
        {
            if (CurrentQH >=160)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool ZH()
        {
            if (CurrentQH <=5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }      
        string ZT = "抓";bool ZOK = false;bool SOK = false;bool QZZL = false;int XJNum = 0;int RunNum = 0;int ActNum = 0; int ActNum2 = 0;
        private void timerLX_Tick(object sender, EventArgs e)
        {
            RunNum++;//50ms运行一次，6000次是5min
            lblTime.Text = RunNum.ToString();
            if (1==1)//2min            
            {
                if (ZT == "抓")
                {
                    //if (JD() == true)
                    //{
                    if (ZL == true || QZZL == true)
                    {                        
                        XJNum = 0;
                        byte4A = "10";//下降--着陆后维持着陆状态
                        if (ZOK == false)
                        {
                            if (ZQ() == false)
                            {
                                //ActNum++;
                                //if (ActNum <= 200)
                                //{
                                numericUpDown4.Value = 165;//最前
                                return;
                                //}
                                //else//执行超过10s--翻到最后再执行翻到最前
                                //{
                                //    ActNum2++;
                                //    if (ZH() == false)
                                //    {
                                //        if (ActNum2 <= 200)
                                //        {
                                //            numericUpDown4.Value = 0;//最后
                                //            return;
                                //        }
                                //        else
                                //        {
                                //            ActNum2 = 0;
                                //            ActNum = 0;
                                //        }
                                //    }
                                //    else
                                //    {
                                //        ActNum = 0;
                                //    }
                                //}
                            }
                            //else
                            //{
                            //    ActNum = 0;
                            //    ActNum2 = 0;
                            //}
                            if (CJKZT != "Close")
                            {
                                commbyte[21] = 0x02;//框关    
                                return;
                            }
                            if (JXSZT != "Open")
                            {
                                commbyte[24] = 0x01;//爪开    
                                return;
                            }                           
                            if (DOWN() == false)
                            {                                
                                ActNum++;
                                if (ActNum <= 200)
                                {
                                    numericUpDown3.Value = 210;//最下
                                    return;
                                }
                                else
                                {
                                    ZOK = true;
                                    ActNum = 0;
                                    
                                }
                            }
                            else
                            {
                                ActNum = 0;
                            }
                            ZOK = true;
                        }

                        if (JXSZT == "Close")
                        {
                            ZT = "收";
                            ZOK = false;
                            byte4A = "00";//停止下降                                               
                        }
                        else
                        {
                            commbyte[24] = 0x02;//爪合                       
                            return;
                        }
                    }
                    else
                    {
                        byte4A = "10";//下降至着陆
                        XJNum++;
                        if (XJNum >= 200)//50ms一次，200次=10s
                        {
                            //假设已经着陆
                            QZZL = true;
                        }
                    }
                    //}
                }
                else if (ZT == "收")
                {
                    //if (ZL == true || QZZL == true)
                    //{
                    //byte4A = "01";//上升
                    //通过定深-设定比当前深度米数低的数值使机器人上浮例如15.2-0.2=15.0，当前15.2米设置上浮0.2米至15.0米位置
                    if ((decimal)Depth - numericUpDown6.Value < 0)
                    {
                        numericUpDown2.Value = (decimal)0.0;
                    }
                    else
                    {
                        numericUpDown2.Value = (decimal)Depth - numericUpDown6.Value;
                    }

                    cbxDepth.Enabled = false;
                    numericUpDown2.Enabled = false;
                    DSOK = true;
                    btnDingS.Text = "解锁";
                    btnDingS.ForeColor = Color.Red;

                    QZZL = false;//强制着陆设置为false
                                 //}
                                 //if (ZL == false)
                                 //{
                    if (SOK == false)
                    {
                        if (UP() == false)
                        {
                            numericUpDown3.Value = 0;//最上
                            return;
                        }
                        if (CJKZT != "Open")
                        {
                            commbyte[21] = 0x01;//框开
                            return;
                        }
                        //else
                        //{
                        //    ActNum = 0;
                        //}
                        if (ZH() == false)
                        {
                            //ActNum++;
                            //if (ActNum <= 200)
                            //{
                                numericUpDown4.Value = 0;//最后
                                return;
                            //}
                            //else
                            //{
                            //    ActNum2++;
                            //    if (ZQ() == false)
                            //    {
                            //        if (ActNum2 <= 200)
                            //        {
                            //            numericUpDown4.Value = 165;//最前
                            //            return;
                            //        }
                            //        else
                            //        {
                            //            ActNum2 = 0;
                            //            ActNum = 0;
                            //        }
                            //    }
                            //    else
                            //    {                                   
                            //        ActNum = 0;
                            //    }
                            //}
                        }
                        //else
                        //{
                        //    ActNum = 0;
                        //    ActNum2 = 0;
                        //}
                        SOK = true;
                    }
                    else
                    {
                        if (JXSZT == "Open")
                        {
                            if (ZQ() == false)
                            {
                                //ActNum++;
                                //if (ActNum <= 200)
                                //{
                                    numericUpDown4.Value = 165;//最前
                                    return;
                                //}
                                //else//执行超过10s--翻到最后再执行翻到最前
                                //{
                                //    ActNum2++;
                                //    if (ZH() == false)
                                //    {
                                //        if (ActNum2 <= 200)
                                //        {
                                //            numericUpDown4.Value = 0;//最后
                                //            return;
                                //        }
                                //        else
                                //        {
                                //            ActNum2 = 0;
                                //            ActNum = 0;
                                //        }
                                //    }
                                //    else
                                //    {
                                //        ActNum = 0;
                                //    }
                                //}
                            }
                            //else
                            //{
                            //    ActNum = 0;
                            //    ActNum2 = 0;
                            //}
                            if (CJKZT != "Close")
                            {
                                commbyte[21] = 0x02;//框关    
                                return;
                            }
                            else
                            {
                                if ((int)numericUpDown1.Value + 30 > 359)
                                {
                                    numericUpDown1.Value = numericUpDown1.Value + (decimal)30 - 359;
                                    LXJD = (int)numericUpDown1.Value;
                                }
                                else
                                {
                                    numericUpDown1.Value = numericUpDown1.Value + (decimal)30;
                                    LXJD = (int)numericUpDown1.Value;
                                }
                                //停止定深
                                cbxDepth.Enabled = true;
                                numericUpDown2.Enabled = true;
                                DSOK = false;
                                btnDingS.Text = "锁定";
                                btnDingS.ForeColor = Color.White;
                                //跳转至抓取状态
                                ZT = "抓";
                                SOK = false;
                            }
                        }
                        else
                        {
                            commbyte[24] = 0x01;//爪开                            
                            return;
                        }
                    }
                    //}
                    //else
                    //{

                    //}
                }
            }
            else
            {
                //if (ZT == "收")
                //{
                //    if (UP() == false)
                //    {
                //        numericUpDown3.Value = 0;//最上
                //        return;
                //    }
                //    if (CJKZT != "Open")
                //    {
                //        commbyte[21] = 0x01;//框开
                //        return;
                //    }
                //    if (ZH() == false)
                //    {
                //        numericUpDown4.Value = 0;//最后
                //        return;
                //    }
                //    if (JXSZT == "Open")
                //    {
                //        if (ZQ() == false)
                //        {
                //            numericUpDown4.Value = 165;//最前
                //            return;
                //        }
                //        if (CJKZT != "Close")
                //        {
                //            commbyte[21] = 0x02;//框关                                
                //        }
                //    }
                //    else
                //    {
                //        commbyte[24] = 0x01;//爪开                            
                //        return;
                //    }
                //}

                ////运行5min
                ////停止定深
                //cbxDepth.Enabled = true;
                //numericUpDown2.Enabled = true;
                //DSOK = false;
                //btnDingS.Text = "锁定";
                //btnDingS.ForeColor = Color.White;
                ////停止定向
                //numericUpDown1.Enabled = true;
                //DXOK = false;
                //btnDingX.Text = "锁定";
                //btnDingX.ForeColor = Color.White;
                //cbxCompass.Enabled = true;

                //byte4A = "00";//停止下降     
                //ZT = "抓";
                //ZOK = false;
                //SOK = false;

                //RunNum = 0;
                //ActNum = 0;

                //lblInitNum.Text = (int.Parse(lblInitNum.Text) + 1).ToString();
                //LX = true;
                //btnLX.ForeColor = Color.Lime;
                //ArmUDOK = true;
                //ArmQHOK = true;
                //ZT = "抓";
                //timerLX.Start();
                //numericUpDown4.Value = 165;
                //cbxCompass.Enabled = false;
                //numericUpDown1.Value = (decimal)CurrentJD;
                //numericUpDown1.Enabled = false;
                //LXJD = (int)numericUpDown1.Value;
                //DXOK = true;
                //btnDingX.Text = "解锁";
                //btnDingX.ForeColor = Color.Red;
            }
        }
        #endregion

        #region 机械手采集框状态
        string JXSZT = "Null"; string CJKZT = "Null";
        private void timerZT_Tick(object sender, EventArgs e)
        {
            if (JXSKH==1)
            {
                JXSZT = "Open";
                lblZK.BackColor = Color.Lime;
                lblZH.BackColor = Color.DarkGreen;
            }
            else if (JXSKH ==2)
            {
                JXSZT = "Close";
                lblZK.BackColor = Color.DarkGreen;
                lblZH.BackColor = Color.Lime;
            }
            else
            {
                JXSZT = "Null";
                lblZK.BackColor = Color.DarkGreen;
                lblZH.BackColor = Color.DarkGreen;
            }


            if (CJKKH==1)
            {
                CJKZT = "Open";
                lblKK.BackColor = Color.Lime;
                lblKH.BackColor = Color.DarkGreen;
            }
            else if (CJKKH == 2)
            {
                CJKZT = "Close";
                lblKK.BackColor = Color.DarkGreen;
                lblKH.BackColor = Color.Lime;
            }
            else
            {
                CJKZT = "Null";
                lblKK.BackColor = Color.DarkGreen;
                lblKH.BackColor = Color.DarkGreen;
            }
        }
        #endregion

        #region 设置
        private void btnSet_Click(object sender, EventArgs e)
        {
            setform setform1 = new setform();
            setform1.ShowDialog();
        }
        #endregion

        #region 采集归位
        private void timerGW_Tick(object sender, EventArgs e)
        {
            //if (SOK == false)
            //{
            //    if (UP() == false)
            //    {
            //        numericUpDown3.Value = 0;//最上
            //        return;
            //    }
            //    if (CJKZT != "Open")
            //    {
            //        KZT = "KK";
            //        commbyte[21] = 0x01;//框开
            //        return;
            //    }
            //    else
            //    {
            //        KZT = "KT";
            //        commbyte[21] = 0x00;//框停                    
            //    }
            //    if (ZH() == false)
            //    {
            //        ArmQHOK = true;
            //        btnArmQH.ForeColor = Color.Lime;
            //        numericUpDown4.Value = 0;//最后
            //        return;
            //    }

            //    SOK = true;
            //}
            //else
            //{
            //    if (JXSZT == "Open")
            //    {                    
            //        SOK = false;
            //        btnCJGW.ForeColor = Color.White;
            //        btnCJGW.Enabled = true;

            //        btnArmCB.Enabled = true;
            //        btnLX.Enabled = true;
            //        btnJXSOpen.Enabled = true;
            //        btnJXSClose.Enabled = true;
            //        btnKK.Enabled = true;
            //        btnKG.Enabled = true;
            //        btnArmUDMax.Enabled = true;
            //        btnArmUDMid.Enabled = true;
            //        btnArmUDMin.Enabled = true;
            //        bnArmFBF.Enabled = true;
            //        btnArmFBM.Enabled = true;
            //        btnArmFBB.Enabled = true;
            //        timerGW.Stop();
            //    }
            //    else
            //    {
            //        commbyte[24] = 0x01;//爪开                            
            //        return;
            //    }
            //}
        }
        bool CJGW = false;
        private void btnCJGW_Click(object sender, EventArgs e)
        {
            if (LX == false)
            {
                btnCJGW.ForeColor = Color.Lime;
                btnCJGW.Enabled = false;
                //
                btnArmCB.Enabled = false;
                btnLX.Enabled = false;
                btnJXSOpen.Enabled = false;
                btnJXSClose.Enabled = false;
                btnKK.Enabled = false;
                btnKG.Enabled = false;
                btnArmUDMax.Enabled = false;
                btnArmUDMid.Enabled = false;
                btnArmUDMin.Enabled = false;
                bnArmFBF.Enabled = false;
                btnArmFBM.Enabled = false;
                btnArmFBB.Enabled = false;
                CJGW = true;
                timerGW1.Start();
            }
            else
            {
                MessageBox.Show("轮循采集正在进行，请先停止轮循采集！", "提示");
            }
        }
        #endregion       
    }
}
