using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Nihon_Player
{
    public partial class MainWindow : Window
    {
        MediaPlayer MediaPlayer = new MediaPlayer();

        bool RepeatAudio { get; set; }
        string FileLocation = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AudioProgress.IsEnabled = false;

            MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

            if (File.Exists($"{Environment.CurrentDirectory}\\Settings.json"))
            {
                var Setting = JsonConvert.DeserializeObject<JToken>(File.ReadAllText($"{Environment.CurrentDirectory}\\Settings.json"));

                VolumeSlider.Value = Convert.ToDouble(Setting["Volume"]);
                AudioName.Content = Setting["LatestPlay"]?.ToString();
                MediaPlayer.Open(new Uri(Setting["AudioLocation"]?.ToString(), UriKind.Absolute));
                if (Convert.ToBoolean(Setting["RepeatTrack"]) == true) { RepeatIcon.Kind = PackIconKind.RepeatOnce; }
                if (Convert.ToBoolean(Setting["AllowSeek"]) == true) { AudioProgress.IsEnabled = true; }
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Play();
            PlayIcon.Kind = PackIconKind.Pause;
            PlayButton.Click += PauseButton_Click;
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Pause();
            PlayIcon.Kind = PackIconKind.Play;
            PlayButton.Click += PlayButton_Click;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { MediaPlayer.Volume = VolumeSlider.Value; }

        private void OpenAudioButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OpenFile = new OpenFileDialog()
            {
                Title = "Nihon Player - Audio Selector",
                Filter = "Audio Files (*.mp3)|*.mp3|All Files (*.*)|*.*",
                RestoreDirectory = true
            };

            if (OpenFile.ShowDialog() == true)
            {
                MediaPlayer.Open(new Uri(OpenFile.FileName));
                AudioName.Content = OpenFile.SafeFileName;
                FileLocation = OpenFile.FileName;
            }
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            if (RepeatIcon.Kind == PackIconKind.RepeatOnce) 
            {
                MediaPlayer.Position = TimeSpan.FromSeconds(0);
                MediaPlayer.Play(); 
            }
            else { return; }
        }

        private void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            DispatcherTimer ProgressTimer = new DispatcherTimer();
            ProgressTimer.Interval = TimeSpan.FromSeconds(1);
            ProgressTimer.Start();
            ProgressTimer.Tick += delegate (object Send, EventArgs Event)
            {
                AudioProgress.Minimum = 0.0;
                AudioProgress.Value = MediaPlayer.Position.TotalSeconds;
                AudioProgress.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                AudioTime.Content = $"{MediaPlayer.Position.ToString("mm\\:ss")}-{MediaPlayer.NaturalDuration.TimeSpan.ToString("mm\\:ss")}";
            };
        }

        private void LoopButton_Click(object sender, RoutedEventArgs e)
        {
            if (RepeatIcon.Kind == PackIconKind.Repeat) 
            { 
                RepeatAudio = true; 
                RepeatIcon.Kind = PackIconKind.RepeatOnce; 
            }
            else if (RepeatIcon.Kind == PackIconKind.RepeatOnce) 
            { 
                RepeatAudio = false; 
                RepeatIcon.Kind = PackIconKind.Repeat; 
            }
        }

        private void AudioProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { MediaPlayer.Position = TimeSpan.FromSeconds(AudioProgress.Value); }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings UserSettings = new Settings()
            {
                LatestPlay = AudioName.Content.ToString(),
                RepeatTrack = RepeatAudio,
                Volume = VolumeSlider.Value,
                AudioLocation = FileLocation,
                AllowSeek = AudioProgress.IsEnabled
            };

            var Json = JsonConvert.SerializeObject(UserSettings, Formatting.Indented);
            using (var StreamWriter = new StreamWriter($"{Environment.CurrentDirectory}\\Settings.json"))
            {
                StreamWriter.WriteLine(Json);
                StreamWriter.Close();
            }
        }
    }
}
