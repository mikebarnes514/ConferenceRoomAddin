using ConferenceRoomAddin.Data;
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
    /// Interaction logic for ConflictWindow.xaml
    /// </summary>
    public partial class ConflictWindow : Window
    {
        private ConflictViewModel ViewModel { get; set; }

        public ConflictWindow(List<Entry> conflicts, bool can_skip)
        {
            InitializeComponent();
            ViewModel = new ConflictViewModel(conflicts, can_skip);
            DataContext = ViewModel;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
