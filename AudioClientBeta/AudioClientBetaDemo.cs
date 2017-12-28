using System;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Timers;
using Timer = System.Timers.Timer;
using System.Diagnostics;
using NAudio.Wave;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Drawing;
using System.Reflection;
using Anthony.Logger;

namespace AudioClientBeta
{
    public partial class AudioClientBetaDemo : Form
    {
        private static ARLogger Logger = ARLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        bool beginMove = false;
        int currentXPosition;
        int currentYPosition;
        public VolumeLevel OutVolumeLevel;
        private static Socket sSocket;

        public objectsMicrophone Mic;
        string ip = string.Empty;
        string name = string.Empty;
        public List<string> ddlDevice = new List<string>();

        public AudioClientBetaDemo()
        {
            //Form运行在屏幕右下角逻辑
            int x = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Width - this.Width*2 -35;
            int y = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Height - this.Height;
            Point p = new Point(x, y);
            this.PointToScreen(p);
            this.Location = p;
            InitializeComponent();
        }

        private objectsMicrophone AddMicrophone()
        {
            objectsMicrophone Mic = new objectsMicrophone
            {
                alerts = new objectsMicrophoneAlerts(),
                detector = new objectsMicrophoneDetector(),
                notifications = new objectsMicrophoneNotifications(),
                recorder = new objectsMicrophoneRecorder(),
                schedule = new objectsMicrophoneSchedule
                {
                    entries
                                                    =
                                                    new objectsMicrophoneScheduleEntry
                                                    [
                                                    0
                                                    ]
                }
            };
            Mic.settings = new objectsMicrophoneSettings();

            Mic.id

 = 1;
            //om.directory = RandomString(5);
            Mic.x = 0;
            Mic.y = 0;
            Mic.width = 160;
            Mic.height = 40;
            Mic.name

 = "MIC";
            Mic.description = "";
            Mic.newrecordingcount = 0;

            int port = 257;
            //foreach (objectsMicrophone om2 in Microphones)
            //{
            //    if (om2.port > port)
            //        port = om2.port + 1;
            //}
            Mic.port = port;

            Mic.settings.typeindex = 0;
            // if (audioSourceIndex == 2)
            //   om.settings.typeindex = 1;
            Mic.settings.deletewav = true;
            Mic.settings.buffer = 4;
            Mic.settings.samples = 8000;
            Mic.settings.bits = 16;
            Mic.settings.channels = 1;
            Mic.settings.decompress = true;
            Mic.settings.active = false;
            Mic.settings.notifyondisconnect = false;

            Mic.detector.sensitivity = 60;
            Mic.detector.nosoundinterval = 30;
            Mic.detector.soundinterval = 0;
            Mic.detector.recordondetect = true;

            Mic.alerts.mode = "sound";
            Mic.alerts.minimuminterval = 60;
            Mic.alerts.executefile = "";
            Mic.alerts.active = false;
            Mic.alerts.alertoptions = "false,false";

            Mic.recorder.inactiverecord = 5;
            Mic.recorder.maxrecordtime = 900;

            Mic.notifications.sendemail = false;
            Mic.notifications.sendsms = false;

            Mic.schedule.active = false;
            return Mic;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                this.AudioNotify.Visible = true;
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                this.Hide();
                Mic = AddMicrophone();
                for (int n = 0; n < WaveIn.DeviceCount; n++)
                {
                    ddlDevice.Add(WaveIn.GetCapabilities(n).ProductName);
                }
                OutVolumeLevel = new VolumeLevel(Mic,this);
                OutVolumeLevel.Listening = true;
                OutVolumeLevel.Enable();
                Logger.Info("窗口加载完成。");
            }
            catch (Exception ex)
            {
                Logger.Info("窗口加载出现异常。"+ex.Message);
            }
        }

        private void AudioClientBetaDemo_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sSocket != null)
            {
                sSocket.Close();
                sSocket = null;
            }
        }

        #region 窗体隐藏标题栏后的移动问题,最小化和关闭按钮
        private void btn_min_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
        }

        private void btn_closeForm_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void AudioClientBetaDemo_MouseDown(object sender, MouseEventArgs e)
        {
            //将鼠标坐标赋给窗体左上角坐标  
            beginMove = true;
            currentXPosition = MousePosition.X;
            currentYPosition = MousePosition.Y;
            this.Refresh();
        }

        private void AudioClientBetaDemo_MouseLeave(object sender, EventArgs e)
        {
            //设置初始状态  
            currentXPosition = 0;
            currentYPosition = 0;
            beginMove = false;
        }

        private void AudioClientBetaDemo_MouseMove(object sender, MouseEventArgs e)
        {
            if (beginMove)
            {
                //根据鼠标X坐标确定窗体X坐标  
                this.Left += MousePosition.X - currentXPosition;
                //根据鼠标Y坐标确定窗体Y坐标  
                this.Top += MousePosition.Y - currentYPosition;
                currentXPosition = MousePosition.X;
                currentYPosition = MousePosition.Y;
            }
        }

        private void AudioClientBetaDemo_MouseUp(object sender, MouseEventArgs e)
        {
            beginMove = false;
        }
        #endregion

        #region 任务栏图标以及控制逻辑
        private void AudioNotify_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                this.Hide();
            }
            else if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                this.Activate();
            }
        }

        private void tsMenuItem_ShowForm_Click(object sender, EventArgs e)
        {
            this.Show();
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void tsMenuItem_HideForm_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Hide();
        }

        private void tsMenuItem_CloseForm_Click(object sender, EventArgs e)
        {
            DialogResult result =  MessageBox.Show("你确定要退出程序吗？", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.OK)
            {
                this.AudioNotify.Visible = false;
                this.Close();
                this.Dispose();
                Application.Exit();
            }
        }
        #endregion
    }
}
