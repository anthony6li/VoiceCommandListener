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

namespace AudioClientBeta
{
    public class VolumeLevel
    {
        private readonly object _lockobject = new object();
        public objectsMicrophone Micobject;
        public IWavePlayer WaveOut;
        public IAudioSource AudioSource;
        public event Delegates.NewDataAvailable DataAvailable; public event EventHandler AudioDeviceEnabled, AudioDeviceDisabled, AudioDeviceReConnected;

        public List<Socket> OutSockets = new List<Socket>();


        public VolumeLevel(objectsMicrophone om)
        {
            Micobject = om;
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
                        //(creates the waveoutprovider referenced below)
                        AudioSource.Listening = true;

                        WaveOut.Init(AudioSource.WaveOutProvider);
                        //Setting volume not supported on DirectSoundOut, adjust the volume on your WaveProvider instead
                        //WaveOut.Volume = 0;
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
                AudioSource = new iSpyServerStream(Micobject.settings.sourcename)
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

                    if (!AudioSource.IsRunning)
                    {
                        lock (_lockobject)
                        {
                            AudioSource.Start();
                        }
                    }
                    Listening = true;
                    Micobject.settings.active = true;
                }
            }
            catch (Exception ex)
            {
                
            }

        }
        public void AudioDeviceAudioFinished(object sender, PlayingFinishedEventArgs e)
        {
        }

        public void AudioDeviceDataAvailable(object sender, DataAvailableEventArgs e)
        {
            try
            {
                DataAvailable?.Invoke(this, new NewDataAvailableArgs((byte[])e.RawData.Clone()));
            }
            catch (Exception)
            {
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
            }
        }
    }
}
