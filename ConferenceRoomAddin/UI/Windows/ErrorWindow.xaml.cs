using ConferenceRoomAddin.UI.ViewModels;
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

namespace ConferenceRoomAddin.UI.Windows
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window, ISplashScreen
    {
        private ErrorViewModel ViewModel { get; set; }

        public ErrorWindow(string msg)
        {
            InitializeComponent();
            ViewModel = new ErrorViewModel();
            ViewModel.ErrorMessage = msg;
            DataContext = ViewModel;
        }
        public void AddMessage(string message)
        {
            Dispatcher.Invoke((Action)delegate () { ViewModel.ErrorDetail = message; });
        }

        public void LoadComplete()
        {
            Dispatcher.InvokeShutdown();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
