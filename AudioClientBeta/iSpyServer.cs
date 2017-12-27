using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using NAudio.Wave;
using g711audio;
using NAudio.Wave.SampleProviders;
using AudioClientBeta.Sources.Audio;

namespace AudioClientBeta
{
    public class iSpyServer
    {
        private static Socket sSocket;
        private AudioClientBetaDemo Parent;
        private Thread th = null;
        private bool _listening;
        private static readonly List<Socket> MySockets = new List<Socket>();
        private BufferedWaveProvider _waveProvider;
        private SampleChannel _sampleChannel;
        public IWavePlayer WaveOut;
        public BufferedWaveProvider WaveOutProvider { get; set; }
        public event DataAvailableEventHandler DataAvailable;


        public WaveFormat RecordingFormat { get; set; }

        public bool IsRunning => th != null && !th.Join(TimeSpan.Zero);
       

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

        public iSpyServer(AudioClientBetaDemo _parent)
        {
            Parent = _parent;
        }

        public void StartServer()
        {
            try
            {
                _waveProvider = new BufferedWaveProvider(RecordingFormat);
                _sampleChannel = new SampleChannel(_waveProvider);
                //_sampleChannel.PreVolumeMeter += SampleChannelPreVolumeMeter;


                IPAddress ip = IPAddress.Parse("192.168.198.1");
                int port = 8092;
                sSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sSocket.Bind(new IPEndPoint(ip, port));
                sSocket.Listen(10);
                if (th!=null)
                {
                    while (th.ThreadState == ThreadState.AbortRequested)
                    {
                        Application.DoEvents();
                    }
                }
                th = new Thread(new ThreadStart(StartListen));
                th.IsBackground = true;
                th.Start();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(""+e.Message);
            }
        }

        public void StartListen()
        {
            while (true)
            {
                //if (sSocket != null)
                //{
                //    try
                //    {
                //        Socket clientSocket = sSocket.Accept();
                //        int recv = 0;
                //        while (true)
                //        {
                //            try
                //            {
                //                byte[] dataSize = RecerveVarData(clientSocket);
                //                if (recv < 0)
                //                {
                //                    break;
                //                }
                //                else
                //                {
                //                    byte[] dec;
                //                    ALawDecoder.ALawDecode(dataSize, dataSize.Length, out dec);
                //                    var da = DataAvailable;
                //                    if (da != null)
                //                    {
                //                        if (_sampleChannel != null)
                //                        {
                //                            _waveProvider.AddSamples(dec, 0, dec.Length);

                //                            var sampleBuffer = new float[dec.Length];
                //                            int read = _sampleChannel.Read(sampleBuffer, 0, dec.Length);

                //                            da(this, new DataAvailableEventArgs((byte[])dec.Clone(), read));


                //                            if (Listening)
                //                            {
                //                                WaveOutProvider?.AddSamples(dec, 0, read);
                //                            }

                //                        }
                //                    }
                //                }
                //            }
                //            catch (SocketException se)
                //            {
                //                sSocket.Close();
                //                sSocket = null;
                //            }
                //        }
                //        clientSocket.Close();
                //    }
                //    catch (Exception e)
                //    {
                //        System.Diagnostics.Debug.WriteLine("" + e.Message);
                //    }
                //}
            }
        }

        public void StopServer()
        {
            for (int i = 0; i < MySockets.Count; i++)
            {
                Socket mySocket = MySockets[i];
                if (mySocket != null)
                {
                    try
                    {
                        if (mySocket.Connected)
                            mySocket.Shutdown(SocketShutdown.Both);
                        mySocket.Close();
                        mySocket = null;
                    }
                    catch
                    {
                        try
                        {
                            mySocket.Close();
                        }
                        catch { }

                        mySocket = null;
                    }
                }
            }

            Application.DoEvents();
            if (th != null)
            {
                try
                {
                    if (th.ThreadState == ThreadState.Running)
                        th.Abort();
                    //while (th.ThreadState == ThreadState.AbortRequested)
                    //{
                    //    Application.DoEvents();
                    //}
                }
                catch (Exception)
                {

                }
                Application.DoEvents();
                th = null;
            }
        }

        public void Create(Socket client)
        {
            int recv = 0;
            while (true)
            {
                try
                {
                    byte[] dataSize = RecerveVarData(client);
                    if (recv < 0)
                    {
                        break;
                    }
                    else
                    {
                        byte[] dec;
                        //ALawDecoder.ALawDecode(data, recbytesize, out dec);
                        //var da = DataAvailable;
                        //if (da != null)
                        //{
                        //    if (_sampleChannel != null)
                        //    {
                        //        _waveProvider.AddSamples(dec, 0, dec.Length);

                        //        var sampleBuffer = new float[dec.Length];
                        //        int read = _sampleChannel.Read(sampleBuffer, 0, dec.Length);

                        //        da(this, new DataAvailableEventArgs((byte[])dec.Clone(), read));


                        //        if (Listening)
                        //        {
                        //            WaveOutProvider?.AddSamples(dec, 0, read);
                        //        }

                        //    }
                        //}
                    }
                }
                catch (SocketException se)
                {
                    sSocket.Close();
                    sSocket = null;
                }
            }
            client.Close();
        }

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

        private void AudioIn(Socket mySocket)
        {
            var wf = new WaveFormat(16000, 16, 1);
            DirectSoundOut dso;
            if (String.IsNullOrEmpty(Parent.DeviceName))
                dso = new DirectSoundOut(100);
            else
            {
                dso = new DirectSoundOut(Guid.Parse(Parent.DeviceName));
            }
            var bwp = new BufferedWaveProvider(wf);
            dso.Init(bwp);
            dso.Play();
            var bBuffer = new byte[3200];
            try
            {
                while (mySocket.Connected)
                {
                    int i = mySocket.Receive(bBuffer, 0, 3200, SocketFlags.None);
                    byte[] dec;
                    ALawDecoder.ALawDecode(bBuffer, i, out dec);
                    bwp.AddSamples(dec, 0, dec.Length);
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                mySocket.Close();
                mySocket = null;
            }
            dso.Stop();
            dso.Dispose();
        }

        public void SendHeader(string sHttpVersion, string sMIMEHeader, int iTotBytes, string sStatusCode, int CacheDays, ref Socket _socket)
        {

            String sBuffer = "";

            // if Mime type is not provided set default to text/html
            if (sMIMEHeader.Length == 0)
            {
                sMIMEHeader = "text/html";  // Default Mime Type is text/html
            }

            sBuffer += sHttpVersion + sStatusCode + "\r\n";
            sBuffer += "Server: iSpyServer\r\n";
            sBuffer += "Content-Type: " + sMIMEHeader + "\r\n";
            //sBuffer += "X-Content-Type-Options: nosniff\r\n";
            sBuffer += "Accept-Ranges: bytes\r\n";
            sBuffer += "Access-Control-Allow-Origin: *\r\n";
            sBuffer += "Content-Length: " + iTotBytes + "\r\n";
            //sBuffer += "Cache-Control:Date: Tue, 25 Jan 2011 08:18:53 GMT\r\nExpires: Tue, 08 Feb 2011 05:06:38 GMT\r\nConnection: keep-alive\r\n";
            if (CacheDays > 0)
            {
                //this is needed for video content to work in chrome/android
                DateTime d = DateTime.UtcNow;
                sBuffer += "Cache-Control: Date: " + d.ToUniversalTime().ToString("r") + "\r\nLast-Modified: Tue, 01 Jan 2011 12:00:00 GMT\r\nExpires: " + d.AddDays(CacheDays).ToUniversalTime().ToString("r") + "\r\nConnection: keep-alive\r\n";
            }

            sBuffer += "\r\n";

            Byte[] bSendData = Encoding.ASCII.GetBytes(sBuffer);

            SendToBrowser(bSendData, ref _socket);
            //Console.WriteLine("Total Bytes : " + iTotBytes);

        }

        public void SendToBrowser(String sData, ref Socket _socket)
        {
            SendToBrowser(Encoding.ASCII.GetBytes(sData), ref _socket);
        }
        public void SendToBrowser(Byte[] bSendData, Socket _socket)
        {
            try
            {
                if (_socket.Connected)
                {
                    int _sent = _socket.Send(bSendData);
                    if (_sent < bSendData.Length)
                    {
                        System.Diagnostics.Debug.WriteLine("Only sent " + _sent + " of " + bSendData.Length);
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Send To Browser Error: " + e.Message);
            }
        }

        /// <summary>
        /// Sends data to the browser (client)
        /// </summary>
        /// <param name="bSendData">Byte Array</param>
        /// <param name="_socket">Socket reference</param>
        public void SendToBrowser(Byte[] bSendData, ref Socket _socket)
        {
            try
            {
                if (_socket.Connected)
                {
                    int _sent = _socket.Send(bSendData);
                    if (_sent < bSendData.Length)
                    {
                        System.Diagnostics.Debug.WriteLine("Only sent " + _sent + " of " + bSendData.Length);
                    }
                    //if (_sent==-1)
                    //MainForm.LogExceptionToFile(new Exception("Socket Error cannot Send Packet"));
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Send To Browser Error: " + e.Message);
                //MainForm.LogExceptionToFile(e);
            }
        }
    }
}
