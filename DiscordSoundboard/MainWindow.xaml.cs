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
using DiscordSoundboard.Properties;
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
        private MMDevice _currentOutputDevice;
        private MMDevice _currentLocalPlaybackDevice;
        private float _currentOutputDeviceVolume;
        private float _currentPlaybackDeviceVolume;

        private readonly HotKeysController _hotKeysController;

        private readonly ObservableCollection<ComboBoxItem> _outputDevicesCollection;
        private readonly ObservableCollection<ComboBoxItem> _localPlaybackDevicesCollection;
        private ObservableCollection<AudioItem> AudioItems { get; set; }

        private bool _settingsRestored = false;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            AudioItems = new ObservableCollection<AudioItem>();
            var sc = this.Resources["SortedCollection"] as CollectionViewSource;
            if (sc != null)
            {
                sc.Source = AudioItems;
            }
            
            LoadListFromJson();

            _outputDevicesCollection = new ObservableCollection<ComboBoxItem>();
            _localPlaybackDevicesCollection = new ObservableCollection<ComboBoxItem>();
            LoadDevicesToLists();

            _hotKeysController = new HotKeysController(this);
            LoadSavedSettings();

            debugLogBox.Text = Settings.Default.Tester;
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

            _audioPlayer = new AudioPlayer(_currentOutputDevice, filepath, _currentOutputDeviceVolume, _currentPlaybackDeviceVolume);
            //_audioPlayer.PlaybackStopped += OnPlaybackStopped;
            _audioPlayer.Play();

            debugLogBox.Text = filepath;
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
                debugLogBox.Text = "stopped event";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Dispose();
                _audioPlayer = null;
            }

            Settings.Default.Save();
            //MessageBox.Show("There is no escape", "42", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //e.Cancel = true;
        }

        private void LoadDevicesToLists()
        {
            _outputDevicesCollection.Clear();
            _localPlaybackDevicesCollection.Clear();
            var enumerator = new MMDeviceEnumerator();

            foreach (var endpoint in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                var cb = new ComboBoxItem
                {
                    Content = endpoint.FriendlyName,
                    Tag = endpoint.ID
                };
                _outputDevicesCollection.Add(cb);
                _localPlaybackDevicesCollection.Add(cb);
            }

            outputDeviceComboBox.ItemsSource = _outputDevicesCollection;
            //outputDeviceComboBox.SelectedIndex = 0;

            localPlaybackDeviceComboBox.ItemsSource = _localPlaybackDevicesCollection;
            //localPlaybackDeviceComboBox.SelectedIndex = 0;
        }

        private void outputDeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var cb = e.AddedItems[0] as ComboBoxItem;
                debugLogBox.Text = cb.Content.ToString();
                debugLogBox.Text += cb.Tag;

                SetNewAudioDevice(cb.Tag.ToString(), false);
            }
        }

        private void localPlaybackDeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var cb = e.AddedItems[0] as ComboBoxItem;
                debugLogBox.Text = cb.Content.ToString();
                debugLogBox.Text += cb.Tag;

                SetNewAudioDevice(cb.Tag.ToString(), true);
            }
        }

        private void SetNewAudioDevice(string deviceId, bool isPlaybackDevice)
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

            if (isPlaybackDevice)
            {
                _currentLocalPlaybackDevice = new MMDeviceEnumerator().GetDevice(deviceId);
                Settings.Default.PlaybackDeviceId = deviceId;
            }
            else
            {
                _currentOutputDevice = new MMDeviceEnumerator().GetDevice(deviceId);
                Settings.Default.OutputDeviceId = deviceId;
            }
        }

        private void play1_Click(object sender, RoutedEventArgs e)
        {
            PlayAudio(@"C:\Windows\Media\tada.wav");
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
                AudioItems.Add(new AudioItem
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
                AudioItems.Remove(selectedItem);
                SaveListToJson();
            }
        }

        private void audioItemsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (audioItemsList.SelectedItems.Count > 0)
            {
                var item = audioItemsList.SelectedItems[0] as AudioItem;
                if (item != null)
                {
                    PlayAudio(item.Filepath);
                }
            }
        }

        private void SaveListToJson()
        {
            try
            {
                var json = JsonConvert.SerializeObject(AudioItems, Formatting.Indented);
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
                AudioItems.Clear();
                foreach (var item in list)
                {
                    AudioItems.Add(item);
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

        private void LoadSavedSettings()
        {
            _currentOutputDeviceVolume = Settings.Default.OutputDeviceVolume;
            outputDeviceSliderVolume.Value = _currentOutputDeviceVolume;

            _currentPlaybackDeviceVolume = Settings.Default.PlaybackDeviceVolume;
            playbackDeviceSliderVolume.Value = _currentPlaybackDeviceVolume;

            var outputDevice = _outputDevicesCollection.SingleOrDefault(a => a.Tag.Equals(Settings.Default.OutputDeviceId));
            if (outputDevice != null)
            {
                outputDeviceComboBox.SelectedItem = outputDevice;
            }
            else
            {
                outputDeviceComboBox.SelectedIndex = 0;
            }

            var playbackDevice = _localPlaybackDevicesCollection.SingleOrDefault(a => a.Tag.Equals(Settings.Default.PlaybackDeviceId));
            if (playbackDevice != null)
            {
                localPlaybackDeviceComboBox.SelectedItem = playbackDevice;
            }
            else
            {
                localPlaybackDeviceComboBox.SelectedIndex = 0;
            }

            _settingsRestored = true;
        }

        private void outputDeviceSliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_settingsRestored) return;

            _currentOutputDeviceVolume = (float)e.NewValue;
            Settings.Default.OutputDeviceVolume = _currentOutputDeviceVolume;
        }

        private void playbackDeviceSliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_settingsRestored) return;

            _currentPlaybackDeviceVolume = (float)e.NewValue;
            Settings.Default.PlaybackDeviceVolume = _currentPlaybackDeviceVolume;
        }
    }
}
