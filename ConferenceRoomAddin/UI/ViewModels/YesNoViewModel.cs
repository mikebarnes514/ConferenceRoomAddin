using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceRoomAddin.UI.ViewModels
{
    class YesNoViewModel
    {
        private string main;
        private string sub;
        private string question;

        public string MainMessage
        {
            get { return main; }
            set { main = value; OnPropertyChanged("MainMessage"); }
        }

        public string SubMessage
        {
            get { return sub; }
            set { sub = value; OnPropertyChanged("SubMessage"); }
        }

        public string Question
        {
            get { return question; }
            set { question = value; OnPropertyChanged("Question"); }
        }

        public YesNoViewModel() { }

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
