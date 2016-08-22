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
using Windows.UI.Xaml.Shapes;
using Windows.UI.ViewManagement;
using Windows.Phone.UI.Input;
using Windows.Phone.Devices.Notification;
using SharpDX.XAudio2;
using System.Threading.Tasks;
using SharpDX.Multimedia;
using SharpDX.IO;


namespace WordFall
{

    public sealed partial class GamePage : Page


    {
        DispatcherTimer Timer = new DispatcherTimer();
        Boolean run = true;
        static int count = 0;
        static int words = 0;
        static bool sound;
        static bool vibration;
        static Random rng = new Random();
        static List<Button> order = new List<Button>();
        static string appLanguage = "";
        static Rectangle[] rec = new Rectangle[36];
        List<string> wordList = new List<string>();

        //Sound
        XAudio2 xAudio;
        WaveFormat[] waveFormat = new WaveFormat[2];
        AudioBuffer[] buffer = new AudioBuffer[2];
        SoundStream[] soundstream = new SoundStream[2];


        public GamePage()
        {
            this.InitializeComponent();

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            //Loop over UI elements and find rectangles and save them to a list
            foreach (UIElement ctrl in InnerGamePanel.Children)
            {
                if (ctrl.GetType() == typeof(Rectangle))
                {
                    Rectangle r = ((Rectangle)ctrl);
                    string rNumber = r.Name.ToString().Replace("R", "");
                    rec[Int32.Parse(rNumber)] = r;
                }

            }
            HideStatusBar();

            //Start timer
            Timer.Tick += OnTimerTick;

        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            count = 0;
            words = 0;
            run = true;
            List<string> wordList = e.Parameter as List<string>;
            this.wordList = wordList;
            string[] par = wordList.Last().Split(',');
            int level = Convert.ToInt32(par[0]);
            sound = Convert.ToBoolean(par[2]);

            //Load audio files
            xAudio = new XAudio2();
            var masteringsound = new MasteringVoice(xAudio);
            xAudio.StopEngine();

            var nativefilestream = new NativeFileStream(@"Audio\tic.wav", NativeFileMode.Open, NativeFileAccess.Read, NativeFileShare.Read);
            var nativefilestream1 = new NativeFileStream(@"Audio\ping.wav", NativeFileMode.Open, NativeFileAccess.Read, NativeFileShare.Read);

            soundstream[0] = new SoundStream(nativefilestream);
            soundstream[1] = new SoundStream(nativefilestream1);

            waveFormat[0] = soundstream[0].Format;
            waveFormat[1] = soundstream[1].Format;

            //Create sound streams
            for (int i = 0; i < soundstream.Count(); i++)
            {
                buffer[i] = new AudioBuffer
                {
                    Stream = soundstream[i].ToDataStream(),
                    AudioBytes = (int)soundstream[i].Length,
                    Flags = BufferFlags.EndOfStream
                };
            }


            vibration = Convert.ToBoolean(par[3]);
            LevelTextBox.Text = "Level " + level;
            appLanguage = par[1];
            Timer.Interval = TimeSpan.FromMilliseconds(10000 / (level + 3));

            if (!Timer.IsEnabled)
            {
                Timer.Start();
            }
            
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            //If the back button is pressed go back
            Timer.Stop();
            run = false;
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null && rootFrame.CanGoBack)
            {
                e.Handled = true;
                this.Frame.Navigate(typeof(MainPage));
            }
        }

        private async void HideStatusBar() 
        {
            StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();

            // Hide the status bar
            await statusBar.HideAsync();
        } 


        void OnTimerTick(object sender, object e)
        {   
           //On every tick move down letters
           MoveDown(count % 5, NextLetter());    
           count++;
        }

        private string NextLetter() 
        {
            //Retruns a new random letter based on how frequvent the letter is 

            List<double> freq = new List<double>();
            List<string> letters=new List<string>();
            if (appLanguage == "Eng") 
            {
                //Frequencies of letters in the English language
                freq = new List<double>() {8.167,1.492,2.782,4.253,12.702,2.228,2.015,6.094,6.966,0.153,0.772,4.025,2.406,6.749,7.507,1.929,0.095,5.987,6.327,9.056,2.758,0.978,2.360,0.150,1.974,0.074 };
                letters = new List<string>() {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z"};
            }
            if (appLanguage == "Swe")
            {
                //Frequencies of letters in the Swedish language
                freq = new List<double>() { 9.383, 1.535, 1.486, 4.702, 10.149, 2.027, 2.862, 2.090, 5.817, 0.614, 3.140, 5.275, 3.471, 8.542, 4.482, 1.839, 0.020, 8.431, 6.590, 7.691, 1.919, 2.415, 0.142, 0.159, 0.708, 0.070, 1.338, 1.797, 1.305 };
                letters = new List<string>() { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Å", "Ä", "Ö" };
            }
            double r = rng.NextDouble() * 100;
            double sum = 0;
            string nextChar = "A";


            //Get a new random letter based on how common the letter is in the chosen language. 
            for (int i = 0; i < freq.Count; i++)
            {
                sum = sum + freq[i];

                if (sum > r)
                {
                    nextChar = letters[i];
                    break;
                }
            }
            return nextChar;
        }

        private void MoveDown(int position,string nextChar) 

        //Move all letters down
        {
            //List of all letters 
            List<String> values = new List<String>() {Button1.Content.ToString(),Button2.Content.ToString(),Button3.Content.ToString(),Button4.Content.ToString(), Button5.Content.ToString(),
                Button6.Content.ToString(),Button7.Content.ToString(),Button8.Content.ToString(),Button9.Content.ToString(),Button10.Content.ToString(),
                Button11.Content.ToString(),Button12.Content.ToString(),Button13.Content.ToString(),Button14.Content.ToString(),Button15.Content.ToString(),
                Button16.Content.ToString(),Button17.Content.ToString(),Button18.Content.ToString(),Button19.Content.ToString(),Button20.Content.ToString(),
                Button21.Content.ToString(),Button22.Content.ToString(),Button23.Content.ToString(),Button24.Content.ToString(),Button25.Content.ToString(),
                Button26.Content.ToString(),Button27.Content.ToString(),Button28.Content.ToString(),Button29.Content.ToString(),Button30.Content.ToString(),
                Button31.Content.ToString(),Button32.Content.ToString(),Button33.Content.ToString(),Button34.Content.ToString(),Button35.Content.ToString()};

            
            if (position == -1)
            {
                //Move letters down
                for (int col = 0; col < 5; col++)
                {
                    for (int row = 6; row >= 0; row--)
                    {
                        //If a position is empty look for letters above
                        if (values[row * 5 + col] == " ")
                        {
                            for (int i = 0; i <= row; i++)
                            {
                                if (values[(row - i) * 5 + col] != " ")
                                {
                                    values[(row) * 5 + col] = values[(row - i) * 5 + col];
                                    values[(row - i) * 5 + col] = " ";
                                    break;
                                }
                            }
                        }
                    }
                }
            } 
            else
            {
                foreach (UIElement ctrl in CenterPanel.Children)
                {
                    if (ctrl.GetType() == typeof(TextBlock))
                    {
                        TextBlock potentialTextBlock = ((TextBlock)ctrl);
                        if (potentialTextBlock.Name.Equals("T" + position.ToString()))
                        {
                            potentialTextBlock.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            potentialTextBlock.Visibility = Visibility.Collapsed;
                        }
                    }
                }


                //Add next letter if there is empty space
                if (values.Contains(" "))
                {
                    for (int i = 0; i < 6; i++)
                    {
                        //Check if there is a letter below and the current position is empty
                        if (values[(i + 1) * 5 + position] != " " && values[i * 5 + position] == " ")
                        {
                            values[i * 5 + position] = nextChar;
                            break;
                        }
                        if (values[i + 1 * 5 + position] == " " && i == 5)
                        {
                            values[(i + 1) * 5 + position] = nextChar;
                            break;
                        }
                    }
                }
                //If there is no more room the game ends
                else
                {
                    if (LevelTextBox.Text != "Level 0") {
                        GameOver();
                        return;
                    }

                }

            }

            //Update labels
            foreach (UIElement ctrl in InnerGamePanel.Children)
            {
                if (ctrl.GetType() == typeof(Button))
                {
                   Button potentialButton = ((Button)ctrl);
                   string ButtonNumber = potentialButton.Name.ToString().Replace("Button", "");
                   potentialButton.Content = values[int.Parse(ButtonNumber) - 1];


                }
            }
        }

        private void GameOver()
        {
            //Stop the game and run the game over screen 
            Timer.Stop();
            xAudio.StopEngine();
            TempWord.Text = "GAME OVER";
            run = false;
            this.Frame.Navigate(typeof(GameOver), ScoreTextBox.Text +"," + words+"," + sound.ToString());
        }


        

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            //Run on button click
            if (run)
            { 
                var button = (Button)sender;
                string name = button.Name;
                int new_val = int.Parse(name.Replace("Button", ""));

                if (TempWord.Text.Contains("+"))
                {
                    TempWord.Text = "";
                }

                //Check if button is empty or arleady added
                if (button.Content.ToString() != " " && !order.Any(Button => Button.Name == name))
                {
                    //Check if the latest button is next to the last one
                    if (order.Count > 0)
                    {
                        int old_val = int.Parse(order.Last().Name.Replace("Button", ""));
                        int diff = new_val - old_val;

                        if ((diff == 1 || diff == 4 || diff == 5 || diff == 6 || diff == -1 || diff == -4 || diff == -5 || diff == -6) && (old_val %5 + new_val % 5 !=1))
                        {
                            PlaySound(0);

                            button.Background = (SolidColorBrush)this.Resources["LightColor"];
                            rec[new_val].Fill = (SolidColorBrush)this.Resources["LightColor"];
                            TempWord.Text = TempWord.Text + button.Content;
                            order.Add(button);
                        }
                    }
                    else 
                    {
                        PlaySound(0);

                        button.Background = (SolidColorBrush)this.Resources["LightColor"];
                        rec[new_val].Fill = (SolidColorBrush)this.Resources["LightColor"];
                        TempWord.Text = TempWord.Text + button.Content;
                        order.Add(button);
                    }
                }             
            }
        }

        private void My_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //Run on button release
            if (run) 
                {
                //Calculate score
                int score = Score(TempWord.Text);

                //if the score is larger than zero update the ui  play a sound
                if (score > 0)
                {
                    for (int i = 0; i < order.Count; i++)
                    {
                        order[i].Content = " ";
                    }

                    //Calculate the level
                    words++;
                    int level = words / 10;

                    TempWord.Text = TempWord.Text + " + " + score;
                    int old_score = int.Parse(ScoreTextBox.Text);
                    ScoreTextBox.Text = (old_score + score).ToString();
                    MoveDown(-1, " ");
                    
                    //If the level is updated vibrate and cahnge the game speed
                    if (level > int.Parse(LevelTextBox.Text.Replace("Level ", "")) && level < 11 && int.Parse(LevelTextBox.Text.Replace("Level ", "")) > 0)
                    {
                        LevelTextBox.Text = "Level " + level;

                        if (vibration) 
                        {
                            VibrationDevice testVibrationDevice = VibrationDevice.GetDefault();
                            testVibrationDevice.Vibrate(TimeSpan.FromMilliseconds(50));
                        }

                        Timer.Interval = TimeSpan.FromMilliseconds(10000 / (level + 3));
                    }
                    //Play sound
                    PlayAndStop(1);

                }
                else 
                {
                    TempWord.Text = "";
                    xAudio.StopEngine();
                }

                //Restore back color
                foreach (UIElement ctrl in InnerGamePanel.Children)
                {
                    if (ctrl.GetType() == typeof(Button))
                    {
                        Button potentialButton = ((Button)ctrl);
                        potentialButton.Background = (SolidColorBrush)this.Resources["VeryLightColor"];

                    }
                    if (ctrl.GetType() == typeof(Rectangle))
                    {
                        Rectangle r = ((Rectangle)ctrl);
                        r.Fill = (SolidColorBrush)this.Resources["VeryLightColor"];
                    }

                } 
                order.Clear();

            }
        }

        private void PlaySound(int i)
        {   
            // Play a sound
            if (sound) 
            {
                SourceVoice sourceVoice;
                sourceVoice = new SourceVoice(xAudio, waveFormat[i], true);
                sourceVoice.SubmitSourceBuffer(buffer[i], soundstream[i].DecodedPacketsInfo);
                xAudio.StartEngine();
                sourceVoice.Start();
            }
        }


        private async void PlayAndStop(int i) {
            // Play a sound and stop when the sound has finished
            if (sound)
            {
                SourceVoice sourceVoice;
                sourceVoice = new SourceVoice(xAudio, waveFormat[i], true);
                sourceVoice.SubmitSourceBuffer(buffer[i], soundstream[i].DecodedPacketsInfo);
                xAudio.StartEngine();
                sourceVoice.Start();
                int j = int.Parse((1000*soundstream[i].Length).ToString()) /soundstream[i].Format.AverageBytesPerSecond;;
                await Task.Delay(TimeSpan.FromMilliseconds(j));
                xAudio.StopEngine();
            }
        }

        private int Score(string word) 
        {
            //Calculate the score
            if (wordList.Contains(word))
            {

                int a = 1;
                int b = 1;
                int c = 1;

                for (int k = 1; k < word.Length; k++)
                {
                    c = a + b;
                    a = b;
                    b = c;
                }

                if (int.Parse(LevelTextBox.Text.Replace("Level ", "")) == 0)
                {
                    return c;
                }

                return c*int.Parse(LevelTextBox.Text.Replace("Level ", ""));
            }
            return 0;
        }
    }
}
