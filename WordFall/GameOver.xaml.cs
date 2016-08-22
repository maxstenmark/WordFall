using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Phone.UI.Input;
using SharpDX.XAudio2;
using System.Threading.Tasks;
using SharpDX.Multimedia;
using SharpDX.IO;

namespace WordFall
{
    public sealed partial class GameOver : Page
    {

        //Audio settings 
        XAudio2 xAudio;
        WaveFormat[] waveFormat = new WaveFormat[2];
        AudioBuffer[] buffer = new AudioBuffer[2];
        SoundStream[] soundstream = new SoundStream[2];
        bool sound;

        public GameOver()
        {
            this.InitializeComponent();

            //Load audio files
            xAudio = new XAudio2();
            var masteringsound = new MasteringVoice(xAudio);
            xAudio.StopEngine();

            var nativefilestream = new NativeFileStream(@"Audio\Dissapoited.wav", NativeFileMode.Open, NativeFileAccess.Read, NativeFileShare.Read);
            var nativefilestream1 = new NativeFileStream(@"Audio\Winning.wav", NativeFileMode.Open, NativeFileAccess.Read, NativeFileShare.Read);

            soundstream[0] = new SoundStream(nativefilestream);
            soundstream[1] = new SoundStream(nativefilestream1);

            waveFormat[0] = soundstream[0].Format;
            waveFormat[1] = soundstream[1].Format;


            //Create audio assets
            for (int i = 0; i < soundstream.Count(); i++)
            {
                buffer[i] = new AudioBuffer
                {
                    Stream = soundstream[i].ToDataStream(),
                    AudioBytes = (int)soundstream[i].Length,
                    Flags = BufferFlags.EndOfStream
                };
            }
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string[] p = e.Parameter.ToString().Split(',');

            MyLevel.Text = p[1];
            MyScore.Text = p[0];
            sound =Convert.ToBoolean(p[2]);

            int rank = 11;

            //Get old high scores
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string[] scores = localSettings.Values["score"].ToString().Split(',');
            string[] words = localSettings.Values["words"].ToString().Split(',');

            //Check if score should be on high score
            for (int i = 0; i < 10; i++)
            {
                if (int.Parse(p[0]) > int.Parse(scores[i])) 
                {
                    rank = i;
                    break;
                }
            }

            //Move values in high score list and update ui
            if (rank < 10)
            {
                for (int i = 9; i > rank; i--)
                {
                    scores[i] = scores[i - 1];
                    words[i] = words[i - 1];
                }
                scores[rank] = p[0];
                words[rank] = p[1].Replace("Level ", "");

                My.Visibility = Visibility.Collapsed;
                MyScore.Visibility = Visibility.Collapsed;
                MyLevel.Visibility = Visibility.Collapsed;
            }
            else
            {
                My.Visibility = Visibility.Visible;
                MyScore.Visibility = Visibility.Visible;
                MyLevel.Visibility = Visibility.Visible;
            }
            
            //Play sounds
            if (rank < 9) 
            {
                PlayAndStop(1);
            }
            if (rank == 9)
            {
                PlayAndStop(0);
            } 

            //Set scores in ui list
            foreach (UIElement ctrl in InnerGamePanel2.Children)
            {
                if (ctrl.GetType() == typeof(TextBlock))
                {
                    TextBlock myTextBlock = ((TextBlock)ctrl);
                    int num = int.Parse(myTextBlock.Name.Replace("Right", ""));
                    if (rank == num)
                    {
                        myTextBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                        myTextBlock.FontSize = 30;
                    }
                    else 
                    {
                        myTextBlock.Foreground = (SolidColorBrush)this.Resources["DarkColor"];
                        myTextBlock.FontSize = 20;
                    }

                    myTextBlock.Text = scores[num];
                }
            }
            //Set levels in ui list
            foreach (UIElement ctrl in InnerGamePanel1.Children)
            {
                if (ctrl.GetType() == typeof(TextBlock))
                {
                    TextBlock myTextBlock = ((TextBlock)ctrl);
                    int num = int.Parse(myTextBlock.Name.Replace("Center", ""));
                    if (rank == num)
                    {
                        myTextBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                        myTextBlock.FontSize = 30;
                    }
                    else
                    {
                        myTextBlock.Foreground = (SolidColorBrush)this.Resources["DarkColor"];
                        myTextBlock.FontSize = 20;
                    }
                    myTextBlock.Text = words[num];
                }
            }

            //Save settings to local storage
            string score  = "";
            string word = "";
            for (int i = 0; i <10; i++)
            {
                score = score + scores[i]+"," ;
                word = word + words[i] + ",";
            }
            localSettings.Values["score"] = score;
            localSettings.Values["words"] = word;

        }


        private async void PlayAndStop(int i)
        {
            if (sound) {
                SourceVoice sourceVoice;
                sourceVoice = new SourceVoice(xAudio, waveFormat[i], true);
                sourceVoice.SubmitSourceBuffer(buffer[i], soundstream[i].DecodedPacketsInfo);
                xAudio.StartEngine();
                sourceVoice.Start();

                int j = int.Parse((1000 * soundstream[i].Length).ToString()) / soundstream[i].Format.AverageBytesPerSecond; ;
                await Task.Delay(TimeSpan.FromMilliseconds(j));
                xAudio.StopEngine();

            }

        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            //On button click leave game over screen
            Frame rootFrame = Window.Current.Content as Frame;
            try
            {
                rootFrame.BackStack.Remove(rootFrame.BackStack.Last());
            }
            catch
            {
            }

            if (rootFrame != null && rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
            }
            else
            {
                this.Frame.Navigate(typeof(MainPage));
            }
        }
    }
}
