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

namespace AudioClientBeta.Sources.Audio.streams
{
    class iSpyServerStream: IAudioSource, IDisposable
    {

        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        Stopwatch sw;
        TimeSpan ts;
        private Timer speakTime;
        private delegate void SetLBTime(string value);
        private delegate void SetFormMin(bool setMin);
        public AudioClientBetaDemo MainForm;

        private static Socket sSocket;
        private string _source;
        private float _gain;
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

        /// <summary>
        /// audio source error event.
        /// </summary>
        /// 
        /// <remarks>This event is used to notify clients about any type of errors occurred in
        /// audio source object, for example internal exceptions.</remarks>
        /// 
        //public event AudioSourceErrorEventHandler AudioSourceError;

        /// <summary>
        /// audio playing finished event.
        /// </summary>
        /// 
        /// <remarks><para>This event is used to notify clients that the audio playing has finished.</para>
        /// </remarks>
        /// 
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

        public float Gain
        {
            get { return _gain; }
            set
            {
                _gain = value;
                if (_sampleChannel != null)
                {
                    _sampleChannel.Volume = value;
                }
            }
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
                    if (WaveOutProvider.BufferedBytes>0) WaveOutProvider.ClearBuffer();
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
            if (IsRunning) return;

            sw = new Stopwatch();
            speakTime = new Timer(1000);
            speakTime.AutoReset = true;

            lock (_lock)
            {
                int port = 8092;
                sSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                sSocket.Listen(10);

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
                                    speakTime.Stop();
                                    sw.Stop();
                                    sw.Reset();
                                    SetLB(string.Format("00:00:00"));
                                    SetForm(true);
                                    if (clientSocket != null)
                                    {
                                        //接收不到语音流，关闭套接字
                                        clientSocket.Close();
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
                                sSocket.Close();
                                sSocket = null;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }

            //var data = new byte[3200];
            //try
            //{
            //    var request = (HttpWebRequest)WebRequest.Create(_source);
            //    request.Timeout = 10000;
            //    request.ReadWriteTimeout = 5000;
            //    var response = request.GetResponse();
            //    using (Stream stream = response.GetResponseStream())
            //    {
            //        if (stream == null)
            //            throw new Exception("Stream is null");
            //        stream.ReadTimeout = 5000;
            //        while (!_stopEvent.WaitOne(0, false))
            //        {
            //                int recbytesize = stream.Read(data, 0, 3200);
            //                if (recbytesize == 0)
            //                    throw new Exception("lost stream");
            //                byte[] dec;
            //                ALawDecoder.ALawDecode(data, recbytesize, out dec);
            //            var da = DataAvailable;
            //            if (da != null)
            //            {
            //                if (_sampleChannel != null)
            //                {
            //                    _waveProvider.AddSamples(dec, 0, dec.Length);
            //                    var sampleBuffer = new float[dec.Length];
            //                    int read = _sampleChannel.Read(sampleBuffer, 0, dec.Length);
            //                    da(this, new DataAvailableEventArgs((byte[])dec.Clone(), read));
            //                    if (Listening)
            //                    {
            //                        WaveOutProvider?.AddSamples(dec, 0, read);
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                break;
            //            }
            //            // need to stop ?
            //            if (_stopEvent.WaitOne(0, false))
            //                break;
            //        }
            //    }
            //    AudioFinished?.Invoke(this, new PlayingFinishedEventArgs(ReasonToFinishPlaying.StoppedByUser));
            //}
            //catch (Exception e)
            //{
            //    var af = AudioFinished;
            //    af?.Invoke(this, new PlayingFinishedEventArgs(ReasonToFinishPlaying.DeviceLost));
            //    //Logger.LogExceptionToFile(e,"ispyServer");
            //}

            if (_sampleChannel != null)
            {
                _sampleChannel.PreVolumeMeter -= SampleChannelPreVolumeMeter;
                _sampleChannel = null;
            }

            if (_waveProvider?.BufferedBytes > 0)
                _waveProvider.ClearBuffer();

            if (WaveOutProvider?.BufferedBytes > 0) WaveOutProvider?.ClearBuffer();
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
                    MainForm.ShowInTaskbar = false;
                    MainForm.WindowState = FormWindowState.Minimized;
                }
                else
                {
                    SetForegroundWindow(MainForm.Handle);
                    MainForm.ShowInTaskbar = true;
                    MainForm.WindowState = FormWindowState.Normal;
                    MainForm.Show();
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

        public void WaitForStop()
        {
            if (!IsRunning) return;
            // wait for thread stop
            _stopEvent.Set();

            try
            {
                while (_thread != null && !_thread.Join(0))
                    Application.DoEvents();
            }
            catch
            {
                // ignored
            }

            _stopEvent?.Close();
            _stopEvent = null;
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
            WaitForStop();
        }

        public WaveFormat RecordingFormat { get; set; }

        private bool _disposed;
        // Public implementation of Dispose pattern callable by consumers. 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _stopEvent?.Close();
            }

            // Free any unmanaged objects here. 
            //
            _disposed = true;
        }
    }
}
