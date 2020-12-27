using System;
using System.Collections.Generic;
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
using marktplaatsreposter;

namespace marktplaatsreposter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<MarktplaatsGUIAdvert> advertList { get; set; }
        private MarktplaatsBot bot;
        public BotStatus botStatus { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            bot = new MarktplaatsBot();
            bot.CheckSignedIn();

            
            advertList = new List<MarktplaatsGUIAdvert>
            {
            };
            botStatus = BotStatus.NOT_SIGNED_IN;

            DataContext = this;
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            refreshButton.IsEnabled = false;
            botStatus = BotStatus.PROCESSING;
            var adverts = bot.GetAdverts();
            advertList = adverts.Select(compact =>
            {
                return new MarktplaatsGUIAdvert()
                {
                    AdvertTitle = compact.AdvertTitle,
                    Status = compact.Status,
                    Views = compact.Views,
                    IsChecked = false
                };
            }).ToList();
            advertListView.ItemsSource = advertList;
            repostButton.IsEnabled = true;
            botStatus = BotStatus.READY;
            refreshButton.IsEnabled = true;
        }

        private void RepostClick(object sender, RoutedEventArgs e)
        {
            repostButton.IsEnabled = false;
            botStatus = BotStatus.PROCESSING;
            advertList.ForEach(advert =>
            {
                if (advert.IsChecked)
                {
                    bot.RePost(advert.AdvertTitle);
                }
            });
            botStatus = BotStatus.READY;
            repostButton.IsEnabled = true;
        }

        private void SignInClick(object sender, RoutedEventArgs e)
        {
            signInButton.IsEnabled = false;
            botStatus = BotStatus.PROCESSING;
            bot.SignIn();
            refreshButton.IsEnabled = true;
            botStatus = BotStatus.READY;
        }

        private void emailBox_KeyUp(object sender, KeyEventArgs e)
        {
            Properties.Settings.Default.email = emailBox.Text;
            Properties.Settings.Default.Save();
        }

        private void passwordBox_KeyUp(object sender, KeyEventArgs e)
        {
            Properties.Settings.Default.password = passwordBox.Password;
            Properties.Settings.Default.Save();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bot.Terminate();
        }
    }
}
