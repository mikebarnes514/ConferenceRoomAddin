using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MJ.Scheduling
{
    [TemplatePart(Name = "PART_Timeline", Type = typeof(UniformGrid))]
    class TimelinePanel : Control
    {
        #region Members
        private UniformGrid _timelineGrid;
        #endregion

        #region Dependency Properties
        public static DependencyProperty BeginsProperty = DependencyProperty.Register("Begins", typeof(DateTime), typeof(TimelinePanel), new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.AffectsMeasure, OnBeginsChanged));
        public static DependencyProperty EndsProperty = DependencyProperty.Register("Ends", typeof(DateTime), typeof(TimelinePanel), new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.AffectsMeasure, OnEndsChanged));
        public static DependencyProperty ShowDateProperty = DependencyProperty.Register("ShowDate", typeof(bool), typeof(TimelinePanel), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure));
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

        public bool ShowDate
        {
            get { return (bool)GetValue(ShowDateProperty); }
            set { SetValue(ShowDateProperty, value); }
        }
        #endregion

        #region Constructors
        static TimelinePanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelinePanel), new FrameworkPropertyMetadata(typeof(TimelinePanel)));
        }
        #endregion

        #region Control Overrides
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _timelineGrid = GetTemplateChild("PART_Timeline") as UniformGrid;
            UpdateTimeline();
        }
        #endregion

        #region Event Handlers
        private static void OnBeginsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as TimelinePanel).UpdateTimeline();
        }

        private static void OnEndsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as TimelinePanel).UpdateTimeline();
        }
        #endregion

        #region Methods
        public void UpdateTimeline()
        {
            if (_timelineGrid != null)
            {
                TimeSpan span = Ends - Begins;
                bool issame = false;

                _timelineGrid.Children.Clear();
                _timelineGrid.Columns = span.Hours;
                for (int i = 0; i < span.Hours; i++)
                {
                    DateTime time = Begins + new TimeSpan(i, 0, 0);
                    TextBlock txt = new TextBlock();

                    if (i > 0)
                        issame = time.ToString("tt") == time.AddHours(-1).ToString("tt");

                    txt.Text = issame ? time.ToString("h:mm") : time.ToShortTimeString();
                    _timelineGrid.Children.Add(txt);
                }
            }
        }
        #endregion
    }
}
