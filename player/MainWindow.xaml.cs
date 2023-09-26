using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> queue = new List<string>();
        Thread timer;
        int position = 0;
        bool IsPlaying = false; bool IsShuffled = false; bool IsCycling = false;
        public MainWindow()
        {
            InitializeComponent();
            timer = new Thread(_ => {
                while (true)
                {
                    Dispatcher.Invoke(() =>
                    {
                        CurrentSecond(); SecondsLeft(); UpdSlider();
                    });
                    Thread.Sleep(1000);
                }
            });
            timer.Start();
            
        }

        private void Open_folder_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog {IsFolderPicker = true};
            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                var files = Directory.GetFiles(dialog.FileName);
                foreach (string file in files)
                {
                    if (!file.EndsWith(".mp3") && !file.EndsWith(".m4a") && !file.EndsWith(".wav") && !file.EndsWith(".ogg")) continue;
                    queue.Add(file);
                }
                Enabler();
                Sort();
                UpdateTrackView();
                Play();
            }
        }
        private void Enabler()
        {
            BLeft.IsEnabled = true;
            BPlay.IsEnabled = true;
            BRight.IsEnabled = true;
            BRand.IsEnabled = true;
            BCycle.IsEnabled = true;
            Slider.IsEnabled = true;
        }
        private string PickTrack()
        {
            if (queue.Count == 0 || position > queue.Count - 1) return "";
            string file = queue[position];
            UpdateTrackView();
            return file;
        }
        private void UpdateTrackView()
        {
            TxtList.Text = string.Empty;
            for (int i = 0; i < queue.Count; i++)
            {
                string text = queue[i] + "\n";
                if (i == position) text = "🔷 " + text;
                TxtList.Text += text;
            }
        }
        private void LoadTrack()
        {
            string file = PickTrack();
            if (file == "") return;
            Player.Source = new Uri(file);
        }
        private void MediaOpened(object sender, EventArgs e)
        {
            if (!Player.NaturalDuration.HasTimeSpan) return;
            TimeRemaining.Content = FormatTime(Player.NaturalDuration.TimeSpan);
            Slider.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
        }
        private void MediaEnded(object sender, EventArgs e)
        {
            if(IsCycling)
            {
                Player.Position = TimeSpan.Zero;
                Play();
            }
            else
            {
               Next();
            }
            
        }
        private string FormatTime(TimeSpan timeSpan)
        {
            double minutes = Math.Floor(timeSpan.TotalMinutes);
            double seconds = Math.Floor(timeSpan.TotalSeconds - minutes * 60);
            return $"{minutes}:{seconds}";
        }

        private void BPlay_Click(object sender, RoutedEventArgs e)
        {
            if (IsPlaying == true)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        private void BLeft_Click(object sender, RoutedEventArgs e)
        {
            Previous();
        }

        private void BRight_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }
        private void BRand_Click(object sender, RoutedEventArgs e)
        {
            if (IsShuffled == true)
            {
                Sort();
            }
            else
            {
                Shuffle();
            }
        }
        private void BCycle_Click(object sender, RoutedEventArgs e)
        {
        IsCycling = !IsCycling;
        }
        private void Previous()
        {
            position = Math.Max(0, position - 1);
            LoadTrack();
            Play();
        }
        private void Next()
        {
            position = Math.Min(queue.Count - 1, position + 1);
            LoadTrack();
            Play();
        }
        private void Play()
        {
            Player.Play();
            IsPlaying = true;
        }
        private void Pause()
        {
            Player.Pause();
            IsPlaying = false;
        }
        private void Shuffle()
        {
            Random rand = new Random();

            for (int i = queue.Count - 1; i >= 1; i--)
            {
                int j = rand.Next(i + 1);
                (queue[i], queue[j]) = (queue[j], queue[i]);
            }
            position = 0;
            LoadTrack();
            IsShuffled = true;
        }
        private void Sort()
        {
            queue.Sort();
            position = 0;
            LoadTrack();
            IsShuffled = false;
        }
        private void CurrentSecond()
        {

            TimeCurrent.Content = FormatTime(Player.Position);
        }
        private void SecondsLeft()
        {
            if (!Player.NaturalDuration.HasTimeSpan) return;
            TimeRemaining.Content = FormatTime(TimeSpan.FromSeconds(Player.NaturalDuration.TimeSpan.TotalSeconds - Player.Position.TotalSeconds));
        }
        private void UpdSlider()
        {
            if (!Player.NaturalDuration.HasTimeSpan) return;
            Slider.Value = Player.Position.TotalSeconds;    
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.Position = TimeSpan.FromSeconds(Slider.Value);
        }
    }
}
