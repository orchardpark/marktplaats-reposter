using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Threading;
namespace marktplaatsreposter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<MarktplaatsGUIAdvert> advertList = new ObservableCollection<MarktplaatsGUIAdvert>();
        private MarktplaatsBot bot;
        SynchronizationContext uiContext = SynchronizationContext.Current;
        public MainWindow()
        {
            InitializeComponent();
            bot = new MarktplaatsBot();
            bot.CheckSignedIn();

            advertListView.ItemsSource = advertList;
            statusText.DataContext = bot;
            signInButton.DataContext = bot;
            refreshButton.DataContext = bot;
            repostButton.DataContext = bot;

            DataContext = this;
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
           {
               var adverts = bot.GetAdverts();
               adverts.ForEach(compact =>
               {
                   uiContext.Send(x =>
                      advertList.Add(new MarktplaatsGUIAdvert()
                      {
                          AdvertTitle = compact.AdvertTitle,
                          Status = compact.Status,
                          Views = compact.Views,
                          IsChecked = false,
                          DeleteOldAd = false
                      }), null
                   );
               });
           }
            ).Start();
        }

        private void RepostClick(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
           {
               advertList.ToList().ForEach(advert =>
               {
                   if (advert.IsChecked)
                   {
                       bot.RePost(advert.AdvertTitle, advert.DeleteOldAd);
                       advert.IsChecked = false;
                       advert.DeleteOldAd = false;
                   }
               });
           }
            ).Start();
        }

        private void SignInClick(object sender, RoutedEventArgs e)
        {
            new Thread(() => { bot.SignIn(); }).Start();
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
