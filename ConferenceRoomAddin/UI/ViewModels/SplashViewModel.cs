using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceRoomAddin.UI.ViewModels
{
    class SplashViewModel : INotifyPropertyChanged
    {
        private string message;
        private string version;

        public string Message
        {
            get { return message; }
            set { message = value; OnPropertyChanged("Message"); }
        }

        public string VersionNumber
        {
            get
            {
                return version;
            }
        }

        public SplashViewModel()
        {
            version = String.Format("v{0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

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
