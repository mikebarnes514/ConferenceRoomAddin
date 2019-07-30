using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MJ.Scheduling
{
    public class CalendarRoom : INotifyPropertyChanged
    {
        #region Members
        private DateTime _start;
        private DateTime _end;
        private string _name;
        #endregion

        #region Properties
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }

        public DateTime StartTime
        {
            get { return _start; }
            set { _start = value; OnPropertyChanged("StartTime"); }
        }

        public DateTime EndTime
        {
            get { return _end; }
            set { _end = value; OnPropertyChanged("EndTime"); }
        }

        public ObservableCollection<CalendarAppointment> Appointments { get; set; }
        #endregion

        #region Constructors
        public CalendarRoom()
        {
            Appointments = new ObservableCollection<CalendarAppointment>();
        }
        #endregion

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
