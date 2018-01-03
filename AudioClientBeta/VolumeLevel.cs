using AudioClientBeta.Sources.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioClientBeta.Sources.Audio.streams;
using NAudio;
using NAudio.Wave;
using AudioClientBeta.Sources;
using System.Net.Sockets;
using System.Diagnostics;
using Anthony.Logger;
using System.Reflection;

namespace AudioClientBeta
{
    public class VolumeLevel
    {
        private static ARLogger Logger = ARLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly object _lockobject = new object();
        public objectsMicrophone Micobject;
        public IWavePlayer WaveOut;
        public IAudioSource AudioSource;
        public AudioClientBetaDemo Parent;
        public event Delegates.NewDataAvailable DataAvailable; public event EventHandler AudioDeviceEnabled, AudioDeviceDisabled, AudioDeviceReConnected;



        public VolumeLevel(objectsMicrophone om, AudioClientBetaDemo parent)
        {
            Micobject = om;
            Parent = parent;
        }
        public bool Listening
        {
            get
            {
                if (WaveOut != null && WaveOut.PlaybackState == PlaybackState.Playing)
                    return true;
                return false;
            }
            set
            {
                if (WaveOut != null)
                {
                    if (value && AudioSource != null)
                    {
                        AudioSource.Listening = true;
                        WaveOut.Init(AudioSource.WaveOutProvider);
                        WaveOut.Play();
                    }
                    else
                    {
                        if (AudioSource != null) AudioSource.Listening = false;
                        WaveOut.Stop();
                    }
                }
            }
        }

        public void Enable()
        {
            try
            {
                AudioSource = new iSpyServerStream(Micobject.settings.sourcename, Parent)
                {
                    RecordingFormat = new WaveFormat(8000, 16, 1)
                };
                if (AudioSource != null)
                {
                    WaveOut = !string.IsNullOrEmpty(Micobject.settings.deviceout)
                        ? new DirectSoundOut(new Guid(Micobject.settings.deviceout), 100)
                        : new DirectSoundOut(100);
                    AudioSource.AudioFinished -= AudioDeviceAudioFinished;
                    AudioSource.DataAvailable -= AudioDeviceDataAvailable;

                    AudioSource.AudioFinished += AudioDeviceAudioFinished;
                    AudioSource.DataAvailable += AudioDeviceDataAvailable;

                    AudioDeviceEnabled?.Invoke(this, EventArgs.Empty);

                    //if (!AudioSource.IsRunning)
                    {
                        lock (_lockobject)
                        {
                            Logger.Info("ServerStream start!!!");
                            AudioSource.Start();
                        }
                    }
                    Listening = true;
                    Micobject.settings.active = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("VolumeLevel.Enable error occured:" + ex.Message);
            }

        }
        public void AudioDeviceAudioFinished(object sender, PlayingFinishedEventArgs e)
        {
            Logger.Info("AudioDeviceAudioFinished"+sender.ToString());
            Logger.Info(e.ReasonToFinishPlaying.ToString());
        }

        public void AudioDeviceDataAvailable(object sender, DataAvailableEventArgs e)
        {
            try
            {
                DataAvailable?.Invoke(this, new NewDataAvailableArgs((byte[])e.RawData.Clone()));
            }
            catch (Exception ee)
            {
                Logger.Error("AudioDeviceDataAvailable error occured:" + ee.Message);
            }
        }

        public void Disable(bool stopSource = true)
        {
            try
            {
                if (AudioSource != null)
                {
                    AudioSource.AudioFinished -= AudioDeviceAudioFinished;
                    AudioSource.DataAvailable -= AudioDeviceDataAvailable;
                    if (stopSource)
                    {
                        AudioSource.Stop();
                    }
                }


                Listening = false;

                //UpdateFloorplans(false);
                Micobject.settings.active = false;

                AudioDeviceDisabled?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Logger.Error("VolumeLevel.Disable error occured:"+ex.Message);
            }
        }
    }
}
