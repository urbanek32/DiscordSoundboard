using System;
using System.Collections.Generic;
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
        private AudioFileReader _audioFileReader;
        private float _currentVolume;
        private string _filepath;

        public event Action PlaybackStopped;

        /// <summary>
        /// Create new instance for every audio file to play.
        /// </summary>
        /// <param name="device">Device on which to play audio.</param>
        /// <param name="filepath">Path to audio file to load.</param>
        /// <param name="volume">From 0.0f to 1.0f</param>
        public AudioPlayer(MMDevice device, string filepath, float? volume = null)
        {
            if (device == null)
            {
                MessageBox.Show("Device is null", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _filepath = filepath;
            _currentVolume = volume ?? 1.0f;

            _audioFileReader = new AudioFileReader(_filepath)
            {
                Volume = _currentVolume
            };

            _output = new WasapiOut(device, AudioClientShareMode.Shared, true, 100);
            _output.PlaybackStopped += _output_PlaybackStopped;

            var wc = new WaveChannel32(_audioFileReader)
            {
                PadWithZeroes = false
            };

            _output.Init(wc);
        }

        public void Play(float? currentVolumeLevel = null)
        {
            if (_output != null)
            {
                _output.Play();
                _audioFileReader.Volume = currentVolumeLevel ?? _currentVolume;
            }
        }

        public void Stop()
        {
            if (_output != null)
            {
                _output.Stop();
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

            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }
        }
    }
}
