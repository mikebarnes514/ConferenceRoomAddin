using ConferenceRoomAddin.UI.ViewModels;
using System;
using System.Windows;

namespace ConferenceRoomAddin.UI.Windows
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window, ISplashScreen
    {
        private MessageViewModel ViewModel { get; set; }

        public MessageWindow(string msg)
        {
            InitializeComponent();
            ViewModel = new MessageViewModel();
            ViewModel.MainMessage = msg;
            DataContext = ViewModel;
        }
        public void AddMessage(string message)
        {
            Dispatcher.Invoke((Action)delegate () { ViewModel.SubMessage = message; });
        }

        public void LoadComplete()
        {
            Dispatcher.InvokeShutdown();
        }
    }
}
