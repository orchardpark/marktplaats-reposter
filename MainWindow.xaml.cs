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
        public MainWindow()
        {
            InitializeComponent();
            bot = new MarktplaatsBot();

            
            advertList = new List<MarktplaatsGUIAdvert>
            {
            };

            DataContext = this;
        }

        private void Refresh_MouseUp(object sender, MouseButtonEventArgs e)
        {
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
            repostButton.IsEnabled = true;
            e.Handled = true;
        }

        private void Repost_MouseUp(object sender, MouseButtonEventArgs e)
        {
            advertList.ForEach(advert =>
            {
                if (advert.IsChecked)
                {
                    bot.RePost(advert.AdvertTitle);
                }
            });
            e.Handled = true;
        }

        private void SignIn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            bot.SignIn();
            refreshButton.IsEnabled = true;
            signInButton.IsEnabled = false;
            e.Handled = true;
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
