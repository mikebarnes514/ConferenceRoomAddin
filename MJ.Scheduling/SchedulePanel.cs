using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace MJ.Scheduling
{

    [TemplatePart(Name = "PART_Timeline", Type = typeof(UniformGrid))]
    class SchedulePanel : ItemsControl
    {
        #region Members
        private UniformGrid _timelineGrid;
        #endregion

        #region Dependency Properties
        public static DependencyProperty BeginsProperty = DependencyProperty.Register("Begins", typeof(DateTime), typeof(SchedulePanel), new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.AffectsMeasure, OnBeginsChanged));
        public static DependencyProperty EndsProperty = DependencyProperty.Register("Ends", typeof(DateTime), typeof(SchedulePanel), new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.AffectsMeasure, OnEndsChanged));
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
        static SchedulePanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SchedulePanel), new FrameworkPropertyMetadata(typeof(SchedulePanel)));
        }
        #endregion

        #region Control Overrides
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _timelineGrid = GetTemplateChild("PART_Timeline") as UniformGrid;
            if (Begins != Ends)
                UpdateTimeline();
        }
        #endregion

        #region Event Handlers
        private static void OnBeginsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as SchedulePanel).UpdateTimeline();
        }

        private static void OnEndsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as SchedulePanel).UpdateTimeline();
        }
        #endregion

        #region Methods
        public void UpdateTimeline()
        {
            if (_timelineGrid != null)
            {
                TimeSpan span = Ends - Begins;

                _timelineGrid.Children.Clear();
                _timelineGrid.Columns = span.Hours * 2;
                for (int i = 0; i < _timelineGrid.Columns; i++)
                {
                    Border border = new Border();

                    border.BorderThickness = new Thickness(i == 0 ? 1 : 0, 0, 1, 1);
                    border.BorderBrush = Brushes.SlateGray;
                    _timelineGrid.Children.Add(border);
                }
            }
        }
        #endregion
    }
}
