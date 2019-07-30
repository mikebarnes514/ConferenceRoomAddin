using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceRoomAddin.Data
{
    public class MeetingRequest : INotifyPropertyChanged
    {
        #region Members
        private string description;
        private string requestor;
        private DateTime start;
        private DateTime end;
        private int room_id;
        private int room_layout_id;
        private string details;
        private int repeat_id;
        private DateTime repeat_end;
        private bool repeat_weekly_sun;
        private bool repeat_weekly_mon;
        private bool repeat_weekly_tue;
        private bool repeat_weekly_wed;
        private bool repeat_weekly_thu;
        private bool repeat_weekly_fri;
        private bool repeat_weekly_sat;
        private int repeat_numweeks;
        private int repeat_dom;
        private string repeat_num_wdom;
        private int repeat_num_wdom_int;
        private int repeat_wdom;
        private bool repeat_monthly_day;
        private bool repeat_monthly_weekday;
        private bool regular_coffee;
        private bool decaf_coffee;
        private bool ice;
        private bool tea;
        private bool food;
        private bool not_food;
        private string catering;
        #endregion

        #region Properties
        public string Description { get { return description; } set { description = value; OnPropertyChanged("Description"); } }
        public string Requestor { get { return requestor; } set { requestor = value; OnPropertyChanged("Requestor"); } }
        public DateTime Start { get { return start; } set { start = value; OnPropertyChanged("Start"); } }
        public DateTime End { get { return end; } set { end = value; OnPropertyChanged("End"); } }
        public int RoomId { get { return room_id; } set { room_id = value; OnPropertyChanged("RoomId"); } }
        public int RoomLayoutId { get { return room_layout_id; } set { room_layout_id = value; OnPropertyChanged("RoomLayoutId"); } }
        public string Details { get { return details; } set { details = value; OnPropertyChanged("Details"); } }
        public int RepeatTypeId { get { return repeat_id; } set { repeat_id = value; UpdateRepeatEndDate(repeat_id); OnPropertyChanged("RepeatTypeId"); } }
        public DateTime RepeatEnd { get { return repeat_end; } set { repeat_end = value; OnPropertyChanged("RepeatEnd"); } }
        public bool RepeatWeeklyOnSunday { get { return repeat_weekly_sun; } set { repeat_weekly_sun = value; OnPropertyChanged("RepeatWeeklyOnSunday"); } }
        public bool RepeatWeeklyOnMonday { get { return repeat_weekly_mon; } set { repeat_weekly_mon = value; OnPropertyChanged("RepeatWeeklyOnMonday"); } }
        public bool RepeatWeeklyOnTuesday { get { return repeat_weekly_tue; } set { repeat_weekly_tue = value; OnPropertyChanged("RepeatWeeklyOnTuesday"); } }
        public bool RepeatWeeklyOnWednesday { get { return repeat_weekly_wed; } set { repeat_weekly_wed = value; OnPropertyChanged("RepeatWeeklyOnWednesday"); } }
        public bool RepeatWeeklyOnThursday { get { return repeat_weekly_thu; } set { repeat_weekly_thu = value; OnPropertyChanged("RepeatWeeklyOnThursday"); } }
        public bool RepeatWeeklyOnFriday { get { return repeat_weekly_fri; } set { repeat_weekly_fri = value; OnPropertyChanged("RepeatWeeklyOnFriday"); } }
        public bool RepeatWeeklyOnSaturday { get { return repeat_weekly_sat; } set { repeat_weekly_sat = value; OnPropertyChanged("RepeatWeeklyOnSaturday"); } }
        public int RepeatNumberOfWeeks { get { return repeat_numweeks; } set { repeat_numweeks = value; OnPropertyChanged("RepeatNumberOfWeeks"); } }
        public int RepeatDayOfMonth { get { return repeat_dom; } set { repeat_dom = value; OnPropertyChanged("RepeatDayOfMonth"); } }
        public string RepeatNumberWeekdaysOfMonth { get { return repeat_num_wdom; } set { repeat_num_wdom = value; repeat_num_wdom_int = ConvertNumberWeekdaysOfMonthToInt(repeat_num_wdom); OnPropertyChanged("RepeatNumberWeekdaysOfMonth"); OnPropertyChanged("RepeatWeekdaysOfMonth"); } }
        public int RepeatWeekdaysOfMonth { get { return repeat_num_wdom_int; } }
        public int RepeatWeekdayOfMonth { get { return repeat_wdom; } set { repeat_wdom = value; OnPropertyChanged("RepeatWeekdayOfMonth"); } }
        public bool RepeatMonthlyByDay { get { return repeat_monthly_day; } set { repeat_monthly_day = value;OnPropertyChanged("RepeatMonthlyByDay"); } }
        public bool RepeatMonthlyByWeekday { get { return repeat_monthly_weekday; } set { repeat_monthly_weekday = value; OnPropertyChanged("RepeatMonthlyByWeekday"); } }
        public bool IsRegularCoffee { get { return regular_coffee; } set { regular_coffee = value; OnPropertyChanged("IsRegularCoffee"); } }
        public bool IsDecafCoffee { get { return decaf_coffee; } set { decaf_coffee = value; OnPropertyChanged("IsDecafCoffee"); } }
        public bool IsIce { get { return ice; } set { ice = value; OnPropertyChanged("IsIce"); } }
        public bool IsTea { get { return tea; } set { tea = value; OnPropertyChanged("IsTea"); } }
        public bool IsFoodService { get { return food; } set { food = value; if (food) IsNotFoodService = false; OnPropertyChanged("IsFoodService"); } }
        public bool IsNotFoodService { get { return not_food; } set { not_food = value; if (not_food) IsFoodService = false; OnPropertyChanged("IsNotFoodService"); } }
        public string CateringInstructions { get { return catering; } set { catering = value; OnPropertyChanged("CateringInstructions"); } }

        public string BeverageCode
        {
            get
            {
                return String.Format("{0}{1}{2}{3}", IsRegularCoffee ? "C" : "", IsDecafCoffee ? "D" : "", IsIce ? "I" : "", IsTea ? "T" : ""); 
            }
        }

        public string RepeatWeeklyCode
        {
            get { return String.Format("{0}{1}{2}{3}{4}{5}{6}", Convert.ToInt32(RepeatWeeklyOnSunday), Convert.ToInt32(RepeatWeeklyOnMonday), Convert.ToInt32(RepeatWeeklyOnTuesday), Convert.ToInt32(RepeatWeeklyOnWednesday), Convert.ToInt32(RepeatWeeklyOnThursday), Convert.ToInt32(RepeatWeeklyOnFriday), Convert.ToInt32(RepeatWeeklyOnSaturday)); }
        }

        public string RepeatMonthlyCode
        {
            get
            {
                string code = "";
                string dayofweek = "";
                
                switch(RepeatWeekdayOfMonth)
                {
                    case 0:
                        dayofweek = "SU";
                        break;
                    case 1:
                        dayofweek = "MO";
                        break;
                    case 2:
                        dayofweek = "TU";
                        break;
                    case 3:
                        dayofweek = "WE";
                        break;
                    case 4:
                        dayofweek = "TH";
                        break;
                    case 5:
                        dayofweek = "FR";
                        break;
                    case 6:
                        dayofweek = "SA";
                        break;
                }                

                code = String.Format("{0}{1}", RepeatWeekdaysOfMonth, dayofweek);
                return code;
            }
        }

        #endregion

        #region Constructors
        public MeetingRequest()
        {
            Description = "";
            RepeatEnd = DateTime.Today;
            RepeatNumberOfWeeks = 1;
            RepeatWeeklyOnSunday = DateTime.Today.DayOfWeek == DayOfWeek.Sunday;
            RepeatWeeklyOnMonday = DateTime.Today.DayOfWeek == DayOfWeek.Monday;
            RepeatWeeklyOnTuesday = DateTime.Today.DayOfWeek == DayOfWeek.Tuesday;
            RepeatWeeklyOnWednesday = DateTime.Today.DayOfWeek == DayOfWeek.Wednesday;
            RepeatWeeklyOnThursday = DateTime.Today.DayOfWeek == DayOfWeek.Thursday;
            RepeatWeeklyOnFriday = DateTime.Today.DayOfWeek == DayOfWeek.Friday;
            RepeatWeeklyOnSaturday = DateTime.Today.DayOfWeek == DayOfWeek.Saturday;
            RepeatWeekdayOfMonth = (int)DateTime.Today.DayOfWeek;
            RepeatDayOfMonth = DateTime.Today.Day;
            RepeatMonthlyByDay = true;

            if (RepeatDayOfMonth < 8)
                RepeatNumberWeekdaysOfMonth = "First";
            else if (RepeatDayOfMonth < 15)
                RepeatNumberWeekdaysOfMonth = "Second";
            else if (RepeatDayOfMonth < 22)
                RepeatNumberWeekdaysOfMonth = "Third";
            else if (RepeatDayOfMonth < 29)
                RepeatNumberWeekdaysOfMonth = "Fourth";
            else
                RepeatNumberWeekdaysOfMonth = "Last";

            IsFoodService = false;
            IsNotFoodService = false;
        }
        #endregion

        #region Methods
        public void LoadFromRecurrencePattern(RecurrencePattern pattern)
        {
            switch (pattern.RecurrenceType)
            {
                case OlRecurrenceType.olRecursDaily:
                    RepeatTypeId = 1;
                    break;
                case OlRecurrenceType.olRecursWeekly:
                    RepeatTypeId = 2;
                    RepeatNumberOfWeeks = pattern.Interval;
                    RepeatWeeklyOnSunday = (pattern.DayOfWeekMask & OlDaysOfWeek.olSunday) == OlDaysOfWeek.olSunday;
                    RepeatWeeklyOnMonday = (pattern.DayOfWeekMask & OlDaysOfWeek.olMonday) == OlDaysOfWeek.olMonday;
                    RepeatWeeklyOnTuesday = (pattern.DayOfWeekMask & OlDaysOfWeek.olTuesday) == OlDaysOfWeek.olTuesday;
                    RepeatWeeklyOnWednesday = (pattern.DayOfWeekMask & OlDaysOfWeek.olWednesday) == OlDaysOfWeek.olWednesday;
                    RepeatWeeklyOnThursday = (pattern.DayOfWeekMask & OlDaysOfWeek.olThursday) == OlDaysOfWeek.olThursday;
                    RepeatWeeklyOnFriday = (pattern.DayOfWeekMask & OlDaysOfWeek.olFriday) == OlDaysOfWeek.olFriday;
                    RepeatWeeklyOnSaturday = (pattern.DayOfWeekMask & OlDaysOfWeek.olSaturday) == OlDaysOfWeek.olSaturday;
                    break;
                case OlRecurrenceType.olRecursMonthly:
                    RepeatTypeId = 3;
                    RepeatMonthlyByDay = true;
                    RepeatMonthlyByWeekday = false;
                    RepeatDayOfMonth = pattern.DayOfMonth;
                    break;
                case OlRecurrenceType.olRecursMonthNth:
                    RepeatTypeId = 3;
                    RepeatMonthlyByDay = false;
                    RepeatMonthlyByWeekday = true;
                    RepeatWeekdayOfMonth = (int)Math.Log((double)pattern.DayOfWeekMask, 2.0);
                    switch (pattern.Instance)
                    {
                        case 1:
                            RepeatNumberWeekdaysOfMonth = "First";
                            break;
                        case 2:
                            RepeatNumberWeekdaysOfMonth = "Second";
                            break;
                        case 3:
                            RepeatNumberWeekdaysOfMonth = "Third";
                            break;
                        case 4:
                            RepeatNumberWeekdaysOfMonth = "Fourth";
                            break;
                        case 5:
                            RepeatNumberWeekdaysOfMonth = "Fifth";
                            break;
                    }
                    break;
                case OlRecurrenceType.olRecursYearly:
                    RepeatTypeId = 4;
                    break;
            }

            Start = pattern.PatternStartDate + pattern.StartTime.TimeOfDay;
            End = pattern.PatternStartDate + pattern.EndTime.TimeOfDay;
            RepeatEnd = pattern.PatternEndDate + pattern.EndTime.TimeOfDay;
        }

        private void UpdateRepeatEndDate(int repeatTypeId)
        {
            switch(repeatTypeId)
            {
                case 0:
                    RepeatEnd = DateTime.Today;
                    break;
                case 1:
                    RepeatEnd = DateTime.Today.AddDays(7);
                    break;
                case 2:
                    RepeatEnd = DateTime.Today.AddMonths(1);
                    break;
                case 3:
                case 4:
                    RepeatEnd = DateTime.Today.AddMonths(3);
                    break;
                default:
                    RepeatEnd = DateTime.Today;
                    break;
            }
        }

        public int ConvertNumberWeekdaysOfMonthToInt(string num_weekdays)
        {
            int num = 0;

            switch (num_weekdays)
            {
                case "First":
                    num = 1;
                    break;
                case "Second":
                    num = 2;
                    break;
                case "Third":
                    num = 3;
                    break;
                case "Fourth":
                    num = 4;
                    break;
                case "Fifth":
                    num = 5;
                    break;
                case "Last":
                    num = -1;
                    break;
                case "Second Last":
                    num = -2;
                    break;
                case "Third Last":
                    num = -3;
                    break;
                case "Fourth Last":
                    num = -4;
                    break;
                case "Fifth Last":
                    num = -5;
                    break;
            }

            return num;
        }

        public MeetingRequest Copy() { return (MeetingRequest)this.MemberwiseClone(); }
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
