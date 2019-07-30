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
    /// Interaction logic for YesNoWindow.xaml
    /// </summary>
    public partial class YesNoWindow : Window, ISplashScreen
    {
        private YesNoViewModel ViewModel { get; set; }

        public YesNoWindow(string msg, string sub, string question)
        {
            InitializeComponent();
            ViewModel = new YesNoViewModel();
            ViewModel.MainMessage = msg;
            ViewModel.SubMessage = sub;
            ViewModel.Question = question;
            DataContext = ViewModel;
        }

        public void AddMessage(string msg)
        {
            Dispatcher.Invoke((Action)delegate () { ViewModel.SubMessage = msg; });
        }
        public void LoadComplete()
        {
            Dispatcher.InvokeShutdown();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
