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
using marktplaatsreposter;

namespace marktplaatsreposter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<MarktplaatsGUIAdvert> advertList = new ObservableCollection<MarktplaatsGUIAdvert>();
        private MarktplaatsBot bot;
        public MainWindow()
        {
            InitializeComponent();
            bot = new MarktplaatsBot();
            bot.CheckSignedIn();

            advertListView.ItemsSource = advertList;
            statusText.DataContext = bot;
            bot.Status = BotStatus.NOT_SIGNED_IN;

            DataContext = this;
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            refreshButton.IsEnabled = false;
            bot.Status = BotStatus.PROCESSING;
            var adverts = bot.GetAdverts();
            adverts.ForEach(compact =>
            {
                advertList.Add( new MarktplaatsGUIAdvert()
                {
                    AdvertTitle = compact.AdvertTitle,
                    Status = compact.Status,
                    Views = compact.Views,
                    IsChecked = false
                });
            });
            advertListView.ItemsSource = advertList;
            repostButton.IsEnabled = true;
            bot.Status = BotStatus.READY;
            refreshButton.IsEnabled = true;
        }

        private void RepostClick(object sender, RoutedEventArgs e)
        {
            repostButton.IsEnabled = false;
            bot.Status = BotStatus.PROCESSING;
            advertList.ToList().ForEach(advert =>
            {
                if (advert.IsChecked)
                {
                    bot.RePost(advert.AdvertTitle);
                }
            });
            bot.Status = BotStatus.READY;
            repostButton.IsEnabled = true;
        }

        private void SignInClick(object sender, RoutedEventArgs e)
        {
            signInButton.IsEnabled = false;
            bot.Status = BotStatus.PROCESSING;
            bot.SignIn();
            refreshButton.IsEnabled = true;
            bot.Status = BotStatus.READY;
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
