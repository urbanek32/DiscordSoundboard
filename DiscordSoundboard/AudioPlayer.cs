﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NAudio;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace DiscordSoundboard
{
    public class AudioPlayer
    {
        private WasapiOut _output;
        private WasapiOut _playbackOutput;
        private AudioFileReader _outputAudioFileReader;
        private AudioFileReader _playbackAudioFileReader;
        private WaveChannel32 _outputWaveChannel32;
        private WaveChannel32 _playbackWaveChannel32;
        private readonly string _filepath;

        public float OutputDeviceVolume { get; set; }
        public float PlaybackDeviceVolume { get; set; }
        public event Action PlaybackStopped;

        /// <summary>
        /// Create new instance for every audio file to play.
        /// </summary>
        public AudioPlayer(MMDevice outputDevice, MMDevice playbackDevice, string filepath, float? outputDeviceVolume = null, float? playbackDeviceVolume = null)
        {
            if (outputDevice == null || playbackDevice == null)
            {
                MessageBox.Show("Device is null", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(filepath))
            {
                MessageBox.Show($"File {filepath} not exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _filepath = filepath;
            OutputDeviceVolume = outputDeviceVolume ?? 1.0f;
            PlaybackDeviceVolume = playbackDeviceVolume ?? 1.0f;

            InitOutput(outputDevice);
            InitPlayback(playbackDevice);
        }

        public void Play()
        {
            if (_output != null)
            {
                _output.Play();
            }

            if (_playbackOutput != null)
            {
                _playbackOutput.Play();
            }
        }

        public void Stop()
        {
            if (_output != null)
            {
                _output.Stop();
            }

            if (_playbackOutput != null)
            {
                _playbackOutput.Stop();
            }
        }

        private void _output_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            Dispose();
            if (PlaybackStopped != null)
            {
                PlaybackStopped();
            }
        }

        private void InitOutput(MMDevice outputDevice)
        {
            _outputAudioFileReader = new AudioFileReader(_filepath)
            {
                Volume = OutputDeviceVolume
            };

            _output = new WasapiOut(outputDevice, AudioClientShareMode.Shared, true, 100);
            _output.PlaybackStopped += _output_PlaybackStopped;

            _outputWaveChannel32 = new WaveChannel32(_outputAudioFileReader)
            {
                PadWithZeroes = false
            };

            _output.Init(_outputWaveChannel32);
        }

        private void InitPlayback(MMDevice playbackDevice)
        {
            _playbackAudioFileReader = new AudioFileReader(_filepath)
            {
                Volume = PlaybackDeviceVolume
            };
            
            _playbackOutput = new WasapiOut(playbackDevice, AudioClientShareMode.Shared, true, 100);
            _playbackOutput.PlaybackStopped += _output_PlaybackStopped;

            _playbackWaveChannel32 = new WaveChannel32(_playbackAudioFileReader)
            {
                PadWithZeroes = false
            };
                
            _playbackOutput.Init(_playbackWaveChannel32);
        }

        public void Dispose()
        {
            if (_output != null)
            {
                if (_output.PlaybackState == PlaybackState.Playing)
                {
                    _output.Stop();
                }
                _output.Dispose();
                _output = null;
            }

            if (_outputAudioFileReader != null)
            {
                _outputAudioFileReader.Dispose();
                _outputAudioFileReader = null;
            }

            if (_outputWaveChannel32 != null)
            {
                _outputWaveChannel32.Dispose();
                _outputWaveChannel32 = null;
            }

            if (_playbackOutput != null)
            {
                if (_playbackOutput.PlaybackState == PlaybackState.Playing)
                {
                    _playbackOutput.Stop();
                }
                _playbackOutput.Dispose();
                _playbackOutput = null;
            }

            if (_playbackAudioFileReader != null)
            {
                _playbackAudioFileReader.Dispose();
                _playbackAudioFileReader = null;
            }

            if (_playbackWaveChannel32 != null)
            {
                _playbackWaveChannel32.Dispose();
                _playbackWaveChannel32 = null;
            }
        }
    }
}
