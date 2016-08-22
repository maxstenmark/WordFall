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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace WordFall
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class t4 : Page
    {

        List<string> wordList = new List<string>();

        public t4()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            List<string> wordList = e.Parameter as List<string>;
            this.wordList = wordList;
        }


        private void Next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //this.Frame.BackStack.Remove(this.Frame.BackStack.Last());
                //this.Frame.BackStack.Remove(this.Frame.BackStack.Last());
                //this.Frame.BackStack.Remove(this.Frame.BackStack.Last());
            }
            catch
            {
            }
            this.Frame.Navigate(typeof(GamePage), wordList);
        }
    }
}
