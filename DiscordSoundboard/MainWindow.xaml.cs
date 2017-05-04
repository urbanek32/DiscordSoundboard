using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace DiscordSoundboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AudioPlayer _audioPlayer;
        private MMDevice _currentSelectedDevice;

        private ObservableCollection<ComboBoxItem> outputDevicesCollection;

        public MainWindow()
        {
            InitializeComponent();

            outputDevicesCollection = new ObservableCollection<ComboBoxItem>();
        }

        // load
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            
        }

        // play
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void OnPlaybackStopped()
        {
            if (_audioPlayer != null)
            {
                textBlock.Text = "stopped";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Dispose();
            }
        }

        // load dev
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            LoadDevicesToLists();
        }

        private void LoadDevicesToLists()
        {
            outputDevicesCollection.Clear();
            var enumerator = new MMDeviceEnumerator();

            foreach (var endpoint in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                var cb = new ComboBoxItem
                {
                    Content = endpoint.FriendlyName,
                    Tag = endpoint.ID
                };
                outputDevicesCollection.Add(cb);
            }
            outputDeviceComboBox.ItemsSource = outputDevicesCollection;
            outputDeviceComboBox.SelectedIndex = 0;
        }

        private void outputDeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var cb = e.AddedItems[0] as ComboBoxItem;
                textBlock.Text = cb.Content.ToString();
                textBlock.Text += cb.Tag;

                ReloadCurrentOutputDevice(cb.Tag.ToString());
            }
        }

        private void ReloadCurrentOutputDevice(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                MessageBox.Show("DeviceId is empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_audioPlayer != null)
            {
                _audioPlayer.Dispose();
                _audioPlayer = null;
            }

            _currentSelectedDevice = new MMDeviceEnumerator().GetDevice(deviceId);
        }

        private void play1_Click(object sender, RoutedEventArgs e)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Dispose();
                _audioPlayer = null;
            }

            _audioPlayer = new AudioPlayer(_currentSelectedDevice, @"C:\Windows\Media\Alarm01.wav");
            _audioPlayer.PlaybackStopped += OnPlaybackStopped;
            _audioPlayer.Play();
        }

        private void play2_Click(object sender, RoutedEventArgs e)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Dispose();
                _audioPlayer = null;
            }

            _audioPlayer = new AudioPlayer(_currentSelectedDevice, @"C:\Windows\Media\Alarm02.wav");
            _audioPlayer.Play();
        }

        private void play3_Click(object sender, RoutedEventArgs e)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Dispose();
                _audioPlayer = null;
            }

            _audioPlayer = new AudioPlayer(_currentSelectedDevice, @"C:\Windows\Media\Alarm03.wav");
            _audioPlayer.Play();
        }
    }
}
