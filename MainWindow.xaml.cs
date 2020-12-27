using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

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

            DataContext = this;
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            refreshButton.IsEnabled = false;
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
            refreshButton.IsEnabled = true;
        }

        private void RepostClick(object sender, RoutedEventArgs e)
        {
            repostButton.IsEnabled = false;
            advertList.ToList().ForEach(advert =>
            {
                if (advert.IsChecked)
                {
                    bot.RePost(advert.AdvertTitle);
                }
            });
            repostButton.IsEnabled = true;
        }

        private void SignInClick(object sender, RoutedEventArgs e)
        {
            signInButton.IsEnabled = false;
            bot.SignIn();
            refreshButton.IsEnabled = true;
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
