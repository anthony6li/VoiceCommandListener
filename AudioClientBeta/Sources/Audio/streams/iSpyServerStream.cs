using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using g711audio;
using System.Net.Sockets;
using System.Diagnostics;
using Timer = System.Timers.Timer;
using System.Timers;
using System.Runtime.InteropServices;
using Anthony.Logger;
using System.Reflection;

namespace AudioClientBeta.Sources.Audio.streams
{
    class iSpyServerStream : IAudioSource, IDisposable
    {
        private static ARLogger Logger = ARLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        Stopwatch sw;
        TimeSpan ts;
        private Timer speakTime;
        private delegate void SetLBTime(string value);
        private delegate void SetFormMin(bool setMin);
        public AudioClientBetaDemo MainForm;

        private static Socket sSocket;
        private string _source;
        private bool _listening;
        private ManualResetEvent _stopEvent;

        private Thread _thread;

        private BufferedWaveProvider _waveProvider;
        private SampleChannel _sampleChannel;

        public BufferedWaveProvider WaveOutProvider { get; set; }

        /// <summary>
        /// New frame event.
        /// </summary>
        /// 
        /// <remarks><para>Notifies clients about new available frame from audio source.</para>
        /// 
        /// <para><note>Since audio source may have multiple clients, each client is responsible for
        /// making a copy (cloning) of the passed audio frame, because the audio source disposes its
        /// own original copy after notifying of clients.</note></para>
        /// </remarks>
        /// 
        public event DataAvailableEventHandler DataAvailable;

        /// <summary>
        /// New frame event.
        /// </summary>
        /// 
        /// <remarks><para>Notifies clients about new available frame from audio source.</para>
        /// 
        /// <para><note>Since audio source may have multiple clients, each client is responsible for
        /// making a copy (cloning) of the passed audio frame, because the audio source disposes its
        /// own original copy after notifying of clients.</note></para>
        /// </remarks>
        /// 
        public event LevelChangedEventHandler LevelChanged;

        ///// <summary>
        ///// audio playing finished event.
        ///// </summary>
        ///// 
        ///// <remarks><para>This event is used to notify clients that the audio playing has finished.</para>
        ///// </remarks>
        ///// 
        public event AudioFinishedEventHandler AudioFinished;

        /// <summary>
        /// audio source.
        /// </summary>
        /// 
        /// <remarks>URL, which provides JPEG files.</remarks>
        /// 
        public virtual string Source
        {
            get { return _source; }
            set { _source = value; }
        }

        public bool Listening
        {
            get
            {
                if (IsRunning && _listening)
                    return true;
                return false;

            }
            set
            {
                if (RecordingFormat == null)
                {
                    _listening = false;
                    return;
                }

                if (WaveOutProvider != null)
                {
                    if (WaveOutProvider.BufferedBytes > 0) WaveOutProvider.ClearBuffer();
                    WaveOutProvider = null;
                }
                if (value)
                {
                    WaveOutProvider = new BufferedWaveProvider(RecordingFormat) { DiscardOnBufferOverflow = true, BufferDuration = TimeSpan.FromMilliseconds(500) };
                }

                _listening = value;
            }
        }


        /// <summary>
        /// State of the audio source.
        /// </summary>
        /// 
        /// <remarks>Current state of audio source object - running or not.</remarks>
        /// 
        public bool IsRunning => _thread != null && !_thread.Join(TimeSpan.Zero);


        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDeviceStream"/> class.
        /// </summary>
        /// 
        public iSpyServerStream() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDeviceStream"/> class.
        /// </summary>
        /// 
        /// <param name="source">source, which provides audio data.</param>
        /// 
        public iSpyServerStream(string source, AudioClientBetaDemo parent)
        {
            _source = source;
            MainForm = parent;
        }

        private readonly object _lock = new object();

        /// <summary>
        /// Start audio source.
        /// </summary>
        /// 
        /// <remarks>Starts audio source and return execution to caller. audio source
        /// object creates background thread and notifies about new frames with the
        /// help of <see cref="DataAvailable"/> event.</remarks>
        /// 
        /// <exception cref="ArgumentException">audio source is not specified.</exception>
        /// 
        public void Start()
        {
            //if (IsRunning) return;
            sw = new Stopwatch();
            speakTime = new Timer(1000);
            speakTime.AutoReset = true;

            lock (_lock)
            {
                int port = 8092;
                sSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                bool BindSucessful = false;
                //端口会有一段时间无法释放，等待再连接。
                while (!BindSucessful)
                {
                    try
                    {
                        sSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                        sSocket.Listen(200);
                        BindSucessful = true;
                        Logger.Info("Socket Bind and Listen Succesful!!");
                    }
                    catch (Exception es)
                    {
                        Logger.Warn("Socket Bind error:"+es.Message);
                        Thread.Sleep(1000);
                        sSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                        sSocket.Listen(200);
                    }
                }

                _waveProvider = new BufferedWaveProvider(RecordingFormat);
                _sampleChannel = new SampleChannel(_waveProvider);
                _sampleChannel.Volume = 0.00f;
                _sampleChannel.PreVolumeMeter += SampleChannelPreVolumeMeter;

                _stopEvent = new ManualResetEvent(false);
                _thread = new Thread(SpyServerListener)
                {
                    Name = "iSpyServer Audio Receiver"
                };
                _thread.IsBackground = true;
                _thread.Start();
            }
        }

        void SampleChannelPreVolumeMeter(object sender, StreamVolumeEventArgs e)
        {
            LevelChanged?.Invoke(this, new LevelChangedEventArgs(e.MaxSampleValues));
        }

        private void SpyServerListener()
        {
            while (true)
            {
                if (sSocket != null)
                {
                    try
                    {
                        Socket clientSocket = sSocket.Accept();
                        Logger.Info(string.Format("来自【{0}】新的指挥请示已接入！开启计时器！", clientSocket.RemoteEndPoint.ToString()));
                        //获得请求后，开启计时器，显示指挥时间。
                        speakTime.Elapsed += SpeakTime_Elapsed;
                        sw.Start();
                        speakTime.Start();
                        SetForm(false);
                        //计时器开启后，开始接收数据并播放。
                        while (true)
                        {
                            try
                            {
                                byte[] dataSize = RecerveVarData(clientSocket);
                                if (dataSize.Length <= 0)
                                {
                                    Logger.Info("无语音流，指挥结束！！！");
                                    speakTime.Stop();
                                    sw.Stop();
                                    sw.Reset();
                                    SetLB(string.Format("00:00:00"));
                                    SetForm(true);
                                    if (clientSocket != null)
                                    {
                                        //接收不到语音流，关闭套接字
                                        clientSocket.Shutdown(SocketShutdown.Both);
                                        clientSocket.Close();
                                        clientSocket.Dispose();
                                        clientSocket = null;
                                    }
                                    break;
                                }
                                else
                                {
                                    byte[] dec;
                                    ALawDecoder.ALawDecode(dataSize, dataSize.Length, out dec);
                                    var da = DataAvailable;
                                    if (da != null)
                                    {
                                        //Logger.Info("接受一段语音流，进入播放！！！");
                                        if (_sampleChannel != null)
                                        {
                                            _waveProvider.AddSamples(dec, 0, dec.Length);

                                            var sampleBuffer = new float[dec.Length];
                                            int read = _sampleChannel.Read(sampleBuffer, 0, dec.Length);

                                            da(this, new DataAvailableEventArgs((byte[])dec.Clone(), read));

                                            if (Listening)
                                            {
                                                WaveOutProvider?.AddSamples(dec, 0, read);
                                            }

                                        }
                                    }
                                }
                            }
                            catch (SocketException se)
                            {
                                //sSocket.Shutdown(SocketShutdown.Both);
                                Logger.Error("通信出现异常，退出Socket. "+se.Message);
                                sSocket.Dispose();
                                sSocket = null;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (sSocket != null)
                        {
                            Logger.Error("通信出现异常，关闭Socket. " + e.Message);
                            //接收不到语音流，关闭套接字
                            sSocket.Close();
                            sSocket.Dispose();
                            sSocket = null;
                        }
                    }
                }
                else
                {
                    if (speakTime != null)
                    {
                        Logger.Error("指挥端通信结束，计时器停止。");
                        speakTime.Stop();
                    }
                    if (sw != null)
                    {
                        sw.Stop();
                    }

                    SetLB(string.Format("00:00:00"));
                    SetForm(true);

                    Logger.Info("ServerStream ReStart!!!");
                    Start();
                }
            }
        }

        #region 计时器功能  
        private void SpeakTime_Elapsed(object sender, ElapsedEventArgs e)
        {
            ts = sw.Elapsed;
            SetLB(string.Format("{0}:{1}:{2}", ts.Hours.ToString("00"), ts.Minutes.ToString("00"), ts.Seconds.ToString("00")));
        }

        private void SetLB(string value)
        {
            if (MainForm.InvokeRequired)
            {
                MainForm.Invoke(new SetLBTime(SetLB), value);
            }
            else
            {
                MainForm.lb_Time.Text = value;
            }
        }

        private void SetForm(bool setMin)
        {
            if (MainForm.InvokeRequired)
            {
                MainForm.Invoke(new SetFormMin(SetForm), setMin);
            }
            else
            {
                if (setMin)
                {
                    Logger.Info("窗体最小化！");
                    //MainForm.ShowInTaskbar = false;
                    MainForm.WindowState = FormWindowState.Minimized;
                    MainForm.FormBorderStyle = FormBorderStyle.None;
                    MainForm.Hide();
                }
                else
                {
                    Logger.Info("窗体前置显示。");
                    //SetForegroundWindow(MainForm.Handle);
                    MainForm.Show();
                    //MainForm.ShowInTaskbar = true;
                    MainForm.WindowState = FormWindowState.Normal;
                    MainForm.Activate();
                }
            }
        }
        #endregion

        public byte[] RecerveVarData(Socket s)
        {
            int total = 0;
            int recv;
            byte[] datasize = new byte[4];
            recv = s.Receive(datasize, 0, 4, SocketFlags.None);
            int size = BitConverter.ToInt32(datasize, 0);
            int dataleft = size;
            byte[] data = new byte[size];
            while (total < size)
            {
                recv = s.Receive(data, total, dataleft, SocketFlags.None);
                if (recv == 0)
                {
                    data = null;
                    break;
                }
                total += recv;
                dataleft -= recv;
            }
            return data;
        }

        /// <summary>
        /// Stop audio source.
        /// </summary>
        /// 
        /// <remarks><para>Stops audio source.</para>
        /// </remarks>
        /// 
        public void Stop()
        {
            if (_thread != null)
            {
                _thread.Abort();
            }
        }

        public WaveFormat RecordingFormat { get; set; }

        private bool _disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                _stopEvent?.Close();
            }
            _disposed = true;
        }
    }
}
