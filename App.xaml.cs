using System.Windows;
using Serilog;

namespace marktplaatsreposter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Setup logging
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("marktplaatsreposter_log.txt")
            .CreateLogger();

            // Create main window
            MainWindow window = new MainWindow
            {
                Title = "Marktplaats Reposter"
            };
            window.Show();
        }
    }
}
