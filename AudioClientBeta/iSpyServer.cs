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

namespace AudioClientBeta
{
    public class iSpyServer
    {
        private TcpListener myListener = null;
        private AudioClientBetaDemo Parent;
        private Thread th = null;
        private static readonly List<Socket> MySockets = new List<Socket>();
        private static int _socketindex;

        public bool Running
        {
            get
            {
                if (th ==null)
                {
                    return  false;
                }
                else
                {
                    return th.IsAlive;
                }
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
                myListener = new TcpListener(IPAddress.Any, 8092) { ExclusiveAddressUse = false };
                myListener.Start(200);
                if (th!=null)
                {
                    while (th.ThreadState == ThreadState.AbortRequested)
                    {
                        Application.DoEvents();
                    }
                }
                th = new Thread(new ThreadStart(StartListen));
                th.Start();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(""+e.Message);
            }
        }

        public void StartListen()
        {
            while (Running)
            {
                try
                {
                    Socket mySocket = myListener.AcceptSocket();
                    if (MySockets.Count() < _socketindex + 1)
                    {
                        MySockets.Add(mySocket);
                    }
                    else
                        MySockets[_socketindex] = mySocket;
                    if (mySocket.Connected)
                    {
                        mySocket.NoDelay = true;
                        mySocket.ReceiveBufferSize = 8192;
                        mySocket.ReceiveTimeout = 1500;
                        try
                        {
                            string sBuffer;
                            string sHttpVersion;

                            Byte[] bReceive = new Byte[1024];
                            mySocket.Receive(bReceive);
                            sBuffer = Encoding.ASCII.GetString(bReceive);

                            if (sBuffer.Substring(0, 4) == "TALK")
                            {
                                var socket = mySocket;
                                var feed = new Thread(p => AudioIn(socket));
                                _socketindex++;
                                feed.Start();
                                continue;
                            }

                            if (sBuffer.Substring(0, 3) != "GET")
                            {
                                continue;
                            }


                            int iStartPos = sBuffer.IndexOf("HTTP", 1, StringComparison.Ordinal);

                            sHttpVersion = sBuffer.Substring(iStartPos, 8);


                            int cid = -1, vid = -1, camid = -1;
                            int w = -1, h = -1;

                            string qs = sBuffer.Substring(4);
                            qs = qs.Substring(0, qs.IndexOf(" ", StringComparison.Ordinal)).Trim('/').Trim('?');
                            string[] nvs = qs.Split('&');

                            foreach (string s in nvs)
                            {
                                string[] nv = s.Split('=');
                                switch (nv[0].ToLower())
                                {
                                    case "c":
                                        cid = Convert.ToInt32(nv[1]);
                                        break;
                                    case "w":
                                        w = Convert.ToInt32(nv[1]);
                                        break;
                                    case "h":
                                        h = Convert.ToInt32(nv[1]);
                                        break;
                                    case "camid":
                                        camid = Convert.ToInt32(nv[1]); //mjpeg
                                        break;
                                    case "micid":
                                        vid = Convert.ToInt32(nv[1]);
                                        break;

                                }
                            }
                            //if (vid != -1)
                            //{
                                VolumeLevel vl = Parent.OutVolumeLevel;
                                if (vl != null)
                                {
                                    String sResponse = "";

                                    sResponse += "HTTP/1.1 200 OK\r\n";
                                    sResponse += "Server: iSpy\r\n";
                                    sResponse += "Expires: 0\r\n";
                                    sResponse += "Pragma: no-cache\r\n";
                                    sResponse += "Content-Type: multipart/x-mixed-replace;boundary=--myboundary";
                                    sResponse += "\r\n\r\n";
                                    Byte[] bSendData = Encoding.ASCII.GetBytes(sResponse);
                                    SendToBrowser(bSendData, mySocket);
                                    vl.OutSockets.Add(mySocket);

                                    _socketindex++;
                                    continue;
                                }
                            //}
                            //else
                            //{
                            //    const string resp = "iSpy server is running";
                            //    SendHeader(sHttpVersion, "", resp.Length, " 200 OK", 0, ref mySocket);
                            //    SendToBrowser(resp, ref mySocket);
                            //}
                             
                            //String sResponse = "";
                            //sResponse += "HTTP/1.1 200 OK\r\n";
                            //sResponse += "Server: iSpy\r\n";
                            //sResponse += "Expires: 0\r\n";
                            //sResponse += "Pragma: no-cache\r\n";
                            //sResponse += "Content-Type: multipart/x-mixed-replace;boundary=--myboundary";
                            //sResponse += "<html>Hello</html>";
                            //sResponse += "\r\n\r\n";
                            //mySocket.Send(Encoding.ASCII.GetBytes(sResponse));
                            //mySocket.Send(Encoding.ASCII.GetBytes("Hello"));
                        }
                        catch (Exception)
                        {

                        }
                        finally
                        {
                            mySocket.Close();
                            mySocket = null;
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("" + e.Message);
                }
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
            if (myListener != null)
            {
                try
                {
                    myListener.Stop();
                    myListener = null;
                }
                catch (Exception)
                {

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
