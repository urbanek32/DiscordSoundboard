using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using Newtonsoft.Json;

namespace DiscordSoundboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string ListFilePath = "AudioList.json";
        private AudioPlayer _audioPlayer;
        private MMDevice _currentSelectedDevice;

        private readonly HotKeysController _hotKeysController;

        private readonly ObservableCollection<ComboBoxItem> _outputDevicesCollection;
        private readonly ObservableCollection<AudioItem> _audioItems;

        public MainWindow()
        {
            InitializeComponent();

            _audioItems = new ObservableCollection<AudioItem>();
            audioItemsList.ItemsSource = _audioItems;
            LoadListFromJson();

            _outputDevicesCollection = new ObservableCollection<ComboBoxItem>();
            LoadDevicesToLists();

            _hotKeysController = new HotKeysController(this);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            //_hotKeysController.OnSourceInitialized(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            _hotKeysController.OnClosed(e);
            base.OnClosed(e);
        }

        private void PlayAudio(string filepath)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Stop();
                _audioPlayer.Dispose();
                _audioPlayer = null;
            }

            _audioPlayer = new AudioPlayer(_currentSelectedDevice, filepath);
            //_audioPlayer.PlaybackStopped += OnPlaybackStopped;
            _audioPlayer.Play();

            textBlock.Text = filepath;
        }

        private void StopAudio()
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Stop();
                _audioPlayer.Dispose();
                _audioPlayer = null;
            }
        }

        private void OnPlaybackStopped()
        {
            if (_audioPlayer != null)
            {
                textBlock.Text = "stopped event";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Dispose();
                _audioPlayer = null;
            }

            //MessageBox.Show("There is no escape", "42", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //e.Cancel = true;
        }

        private void LoadDevicesToLists()
        {
            _outputDevicesCollection.Clear();
            var enumerator = new MMDeviceEnumerator();

            foreach (var endpoint in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                var cb = new ComboBoxItem
                {
                    Content = endpoint.FriendlyName,
                    Tag = endpoint.ID
                };
                _outputDevicesCollection.Add(cb);
            }
            outputDeviceComboBox.ItemsSource = _outputDevicesCollection;
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
            PlayAudio(@"C:\Windows\Media\Alarm01.wav");
        }

        private void play2_Click(object sender, RoutedEventArgs e)
        {
            PlayAudio(@"C:\Windows\Media\tada.wav");
        }

        private void play3_Click(object sender, RoutedEventArgs e)
        {
            PlayAudio(@"C:\Windows\Media\ringout.wav");
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            StopAudio();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "audio files (*.*)|*.mp3;*.wav|All files (*.*)|*.*",
                Title = "Please select an audio file to add"
            };

            if (dialog.ShowDialog() == true)
            {
                _audioItems.Add(new AudioItem
                {
                    Filepath = dialog.FileName,
                    Filename = dialog.SafeFileName
                });

                SaveListToJson();
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (audioItemsList.SelectedItems.Count > 0)
            {
                var selectedItem = audioItemsList.SelectedItems[0] as AudioItem;
                _audioItems.Remove(selectedItem);
                SaveListToJson();
            }
        }

        private void audioItemsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void SaveListToJson()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_audioItems, Formatting.Indented);
                File.WriteAllText(ListFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadListFromJson()
        {
            try
            {
                var file = File.ReadAllText(ListFilePath);
                var list = JsonConvert.DeserializeObject<List<AudioItem>>(file);
                _audioItems.Clear();
                foreach (var item in list)
                {
                    _audioItems.Add(item);
                }
            }
            catch (FileNotFoundException ex)
            {
                // ok
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
