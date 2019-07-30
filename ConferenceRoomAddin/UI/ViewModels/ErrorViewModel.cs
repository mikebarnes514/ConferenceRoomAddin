using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceRoomAddin.UI.ViewModels
{
    class ErrorViewModel : INotifyPropertyChanged
    {
        private string main;
        private string detail;

        public string ErrorMessage
        {
            get { return main; }
            set { main = value; OnPropertyChanged("MainMessage"); }
        }

        public string ErrorDetail
        {
            get { return detail; }
            set { detail = value; OnPropertyChanged("SubMessage"); }
        }

        public ErrorViewModel() { }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
