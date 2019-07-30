using ConferenceRoomAddin.UI.ViewModels;
using System;
using System.Windows;

namespace ConferenceRoomAddin.UI.Windows
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window, ISplashScreen
    {
        private SplashViewModel ViewModel { get; set; }

        public SplashWindow()
        {
            InitializeComponent();
            ViewModel = new SplashViewModel();
            DataContext = ViewModel;
        }

        public void AddMessage(string message)
        {
            Dispatcher.Invoke((Action)delegate () { ViewModel.Message = message; LogManager.LogMessage(message); });
        }

        public void LoadComplete()
        {
            Dispatcher.InvokeShutdown();
        }
    }

    public interface ISplashScreen
    {
        void AddMessage(string message);

        void LoadComplete();
    }
}
