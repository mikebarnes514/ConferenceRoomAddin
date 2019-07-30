using System;
using System.Windows;
using System.Windows.Controls;

namespace MJ.Scheduling
{    
    public class AreaCalendarControl : ItemsControl
    {
        #region Dependency Properties
        public static DependencyProperty CalendarBeginsProperty = DependencyProperty.Register("CalendarBegins", typeof(DateTime), typeof(AreaCalendarControl), new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static DependencyProperty CalendarEndsProperty = DependencyProperty.Register("CalendarEnds", typeof(DateTime), typeof(AreaCalendarControl), new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static DependencyProperty ShowDateProperty = DependencyProperty.Register("ShowDate", typeof(bool), typeof(AreaCalendarControl), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure));
        #endregion

        #region Properties
        public DateTime CalendarBegins
        {
            get { return (DateTime)GetValue(CalendarBeginsProperty); }
            set { SetValue(CalendarBeginsProperty, value); }
        }

        public DateTime CalendarEnds
        {
            get { return (DateTime)GetValue(CalendarEndsProperty); }
            set { SetValue(CalendarEndsProperty, value); }
        }

        public bool ShowDate
        {
            get { return (bool)GetValue(ShowDateProperty); }
            set { SetValue(ShowDateProperty, value); }
        }
        #endregion

        #region Constructors
        static AreaCalendarControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AreaCalendarControl), new FrameworkPropertyMetadata(typeof(AreaCalendarControl)));
        }
        #endregion
    }
}
