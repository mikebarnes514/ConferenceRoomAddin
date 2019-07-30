using System;

namespace MJ.Scheduling
{
    public class CalendarAppointment
    {
        #region Properties
        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
        public string Description { get; set; }
        public string Requestor { get; set; }
        public string Location { get; set; }
        public bool BeverageService { get; set; }
        public bool FoodService { get; set; }
        #endregion
    }
}
