using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.UI.ViewManagement;

namespace WordFall
{
    public sealed partial class MainPage : Page
    {
        List<string> wordList = new List<string>();
        string appLanguage = "Eng";
        Object value;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            //Get stored settings
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            value = localSettings.Values["score"];

            //Read default word list frome file
            readFile("DataEng.txt");

        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.Frame.BackStack.Clear();

        }

        private void Create_Settings()
        {
            //Initialize settings
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            //Default high scores
            string score ="10000,7500,5000,4000,3000,2000,1500,1000,750,500";
            string words = " , , , , , , , , , ";
            localSettings.Values["score"] = score;
            localSettings.Values["words"] = words;
        }

        private async void readFile(string fileName)
        {
            //Read wordlist frome file
            StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();

            // Hide the status bar
            await statusBar.HideAsync();

            string fileContent;
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///"+fileName));
            using (StreamReader sRead = new StreamReader(await file.OpenStreamForReadAsync()))
            fileContent = await sRead.ReadToEndAsync();
            wordList = fileContent.Split(',').ToList();

        }
        private  void Start_Click(object sender, RoutedEventArgs e)
        {
            //Add settings to wordlist
            wordList.Add(Level.Value.ToString() + "," + appLanguage + "," + Convert.ToString(Sound.IsOn) + "," + Convert.ToString(Vibration.IsOn));
            if (value == null)
            {
                Create_Settings();

                //Run tutorial
                this.Frame.Navigate(typeof(t1), wordList);
            }
            else
            {
                this.Frame.Navigate(typeof(GamePage), wordList);
            }
        }

        private void English(object sender, RoutedEventArgs e)
        {       
            //Change lanfuage settings
            Eng.Background = (SolidColorBrush)this.Resources["LightColor"];
            Swe.Background = (SolidColorBrush)this.Resources["VeryLightColor"];

            MyLanguage.Text = "Language : English";
            appLanguage = "Eng";
            readFile("DataEng.txt");
            
        }

        private void Swedish(object sender, RoutedEventArgs e)
        {
            //Change lanfuage settings
            var arrayOfAllKeys = Resources.Keys.ToArray();

            Swe.Background = (SolidColorBrush)this.Resources["LightColor"];
            Eng.Background = (SolidColorBrush)this.Resources["VeryLightColor"];

            MyLanguage.Text = "Language : Swedish";
            appLanguage = "Swe";
            readFile("DataSwe.txt");
        }

        private void Level_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            //Update ui on level change
            try
            {
                LevelText.Text = "Level " + Level.Value.ToString();
            }
            catch
            {
            }

        }

    }

}