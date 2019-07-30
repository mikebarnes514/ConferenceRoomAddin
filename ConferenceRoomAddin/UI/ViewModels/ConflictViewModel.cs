using ConferenceRoomAddin.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceRoomAddin.UI.ViewModels
{
    class ConflictViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Entry> conflicts;
        private bool canSkip;

        public ObservableCollection<Entry> ConflictList
        {
            get { return conflicts; }
            set { conflicts = value; OnPropertyChanged("ConflictList"); }
        }

        public bool CanSkip
        {
            get { return canSkip; }
            set { canSkip = value; OnPropertyChanged("CanSkip"); }
        }

        public ConflictViewModel(List<Entry> conflictList, bool can_skip) { conflicts = new ObservableCollection<Entry>(conflictList); canSkip = can_skip; }

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
