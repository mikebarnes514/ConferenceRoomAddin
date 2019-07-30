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

namespace MJ.Scheduling
{
    class SchedulePresenter : Panel
    {
        #region Dependency Properties
        public static DependencyProperty BeginsProperty = DependencyProperty.Register("Begins", typeof(DateTime), typeof(SchedulePresenter), new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.AffectsRender));
        public static DependencyProperty EndsProperty = DependencyProperty.Register("Ends", typeof(DateTime), typeof(SchedulePresenter), new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region Properties
        public DateTime Begins
        {
            get { return (DateTime)GetValue(BeginsProperty); }
            set { SetValue(BeginsProperty, value); }
        }

        public DateTime Ends
        {
            get { return (DateTime)GetValue(EndsProperty); }
            set { SetValue(EndsProperty, value); }
        }
        #endregion

        #region Constructors
        static SchedulePresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SchedulePresenter), new FrameworkPropertyMetadata(typeof(SchedulePresenter)));
        }
        #endregion

        #region Panel Overrides
        protected override Size MeasureOverride(Size availableSize)
        {
            var mySize = new Size();

            //foreach (UIElement element in this.Children)
            //{
            //    element.Measure(availableSize);
            //    mySize.Width += element.DesiredSize.Width;
            //}
            mySize.Width = availableSize.Width;
            if (double.IsInfinity(mySize.Width))
                mySize.Width = 0;

            mySize.Height = 30;
            return mySize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double containerRange = (this.Ends.Ticks - this.Begins.Ticks);

            foreach (ContentPresenter cp in Children)
            {
                if (cp.Content is CalendarAppointment)
                {
                    CalendarAppointment appt = cp.Content as CalendarAppointment;
                    double begin = (cp.Content as CalendarAppointment).Start.Ticks;
                    double end = (cp.Content as CalendarAppointment).Finish.Ticks;
                    double elementRange = end - begin;

                    if (((appt.Start >= Begins) && (appt.Start < Ends)) || ((appt.Finish > Begins) && (appt.Finish <= Ends)))
                    {
                        Size size = new Size();
                        size.Width = elementRange / containerRange * finalSize.Width;
                        size.Height = finalSize.Height;

                        Point location = new Point();
                        location.X = (begin - this.Begins.Ticks) / containerRange * finalSize.Width;
                        location.Y = 0;

                        cp.Arrange(new Rect(location, size));
                    }
                }
                else
                    cp.Arrange(new Rect(new Point(0, 0), finalSize));
            }

            return finalSize;
        }
        #endregion
    }
}
