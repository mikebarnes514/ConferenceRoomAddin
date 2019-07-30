using ConferenceRoomAddin.Data;
using ConferenceRoomAddin.UI.ViewModels;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ConferenceRoomAddin.UI.Windows
{
    /// <summary>
    /// Interaction logic for ReservationWindow.xaml
    /// </summary>
    public partial class ReservationWindow : Window
    {
        private ReservationViewModel ViewModel { get; set; }
        public int ReservationId { get; set; }

        public MeetingRequest Request { get {return ViewModel.Request;} }

        public ReservationWindow(string requestor, string subject, DateTime startTime, DateTime endTime, RecurrencePattern recurrence = null)
        {
            InitializeComponent();
            ViewModel = new ReservationViewModel();
            ViewModel.Request.Description = subject;
            ViewModel.SelectedDate = startTime.Date;
            ViewModel.SetMeetingTimes(startTime, endTime);
            ViewModel.Request.Requestor = requestor;
            if (recurrence != null)
                ViewModel.Request.LoadFromRecurrencePattern(recurrence);

            DataContext = ViewModel;
            ReservationId = -1;
        }

        public ReservationWindow(MeetingRequest request, int mrbs_id)
        {
            InitializeComponent();
            LogManager.LogMessage(String.Format("Initializing reservation window for {0}", mrbs_id));
            ViewModel = new ReservationViewModel(request, mrbs_id);
            ViewModel.SelectedDate = request.Start.Date;
            ViewModel.SetMeetingTimes(request.Start, request.End);
            
            if(ViewModel.Rooms.Any(r=>r.id == request.RoomId))
                ViewModel.SelectedRoom = ViewModel.Rooms.Single(r => r.id == request.RoomId);

            DataContext = ViewModel;
            ReservationId = mrbs_id;
            LogManager.LogMessage("Complete");
        }

        private void ReserveButton_Click(object sender, RoutedEventArgs e)
        {
            List<Entry> conflicts = null;

            if (!ViewModel.Request.IsFoodService && !ViewModel.Request.IsNotFoodService)
            {
                DisplayManager.ShowErrorWindow("Cannot complete reservation.", "Please select an option for food service.");
                return;
            }

            if(ViewModel.Request.RoomId <= 0)
            {
                DisplayManager.ShowErrorWindow("Cannot complete reservation.", "Please select a room for this reservation.");
                return;
            }

            conflicts = Database.FindConflicts(ViewModel.Request, ReservationId);
            if(conflicts.Count == 0)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                ConflictWindow wnd = new ConflictWindow(conflicts, ViewModel.Request.RepeatTypeId > 0);

                if (wnd.ShowDialog() == true)
                {
                    DialogResult = true;
                    Close();
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Requestor_Changed(object sender, SelectionChangedEventArgs e)
        {
            ComboBox requestors = sender as ComboBox;

            if((requestors.SelectedItem != null) && (ViewModel.IsDescriptionLocked))
            {
                ViewModel.SetDescriptionToRequestor();
            }
        }
    }

    class ComboBoxItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DropDownTemplate { get; set; }
        public DataTemplate SelectedTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ComboBoxItem cbItem = VisualTreeHelpers.GetVisualParent<ComboBoxItem>(container);

            if (cbItem != null)
                return DropDownTemplate;

            return SelectedTemplate;
        }
    }

    class VisualTreeHelpers
    {
        public static T GetVisualParent<T>(object childObject) where T : Visual
        {
            DependencyObject child = childObject as DependencyObject;

            while ((child != null) && !(child is T))
                child = VisualTreeHelper.GetParent(child);

            return child as T;
        }
    }
}
