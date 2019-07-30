using System.ComponentModel;

namespace ConferenceRoomAddin.UI.ViewModels
{
    class MessageViewModel : INotifyPropertyChanged
    {
        private string main;
        private string sub;

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

        public MessageViewModel() { }

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
