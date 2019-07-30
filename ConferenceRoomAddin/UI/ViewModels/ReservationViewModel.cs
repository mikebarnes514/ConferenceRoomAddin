using ConferenceRoomAddin.Data;
using Microsoft.Office.Interop.Outlook;
using MJ.Scheduling;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ConferenceRoomAddin.UI.ViewModels
{
    class ReservationViewModel : INotifyPropertyChanged
    {
        #region Members
        private List<Area> areas;
        private ObservableCollection<string> floors;
        private List<Room> allRooms;
        private ObservableCollection<Room> rooms;
        private ObservableCollection<Room> availableRooms;
        private List<User> users;
        private List<RoomLayout> layouts;
        private ObservableCollection<RoomLayout> roomLayouts;
        private List<RepeatType> repeatTypes;

        private Area selectedArea;
        private string selectedFloor;
        private Room selectedRoom;
        private DateTime selectedDate;
        private RoomLayout selectedLayout;

        private DateTime calendarStart;
        private DateTime calendarEnd;
        private DateTime meetingStart;
        private EndTime meetingEnd;
        private ObservableCollection<CalendarRoom> calendarRooms;
        private Dictionary<string, CalendarRoom> roomCalendars;
        private MeetingRequest _request;
        private int current_mtg_id;
        #endregion

        #region Properties
        public List<Area> Areas
        {
            get { return areas; }
        }

        public List<User> Users
        {
            get { return users; }
        }

        public List<RoomLayout> Layouts
        {
            get { return layouts; }
        }

        public ObservableCollection<RoomLayout> RoomLayouts
        {
            get { return roomLayouts; }
        }

        public List<Room> AllRooms
        {
            get { return allRooms; }
        }

        public ObservableCollection<Room> AvailableRooms
        {
            get { return availableRooms; }
        }

        public ObservableCollection<Room> Rooms
        {
            get { return rooms; }
        }

        public ObservableCollection<string> Floors
        {
            get { return floors; }
        }

        public List<RepeatType> RepeatTypes
        {
            get { return repeatTypes; }
        }

        public Area SelectedArea
        {
            get { return selectedArea; }
            set
            {
                selectedArea = value;
                FilterFloorsByArea(selectedArea.id);
                FilterRoomsByArea(selectedArea.id);
                UpdateCalendarDates();
                ShowAvailableRooms();
                if (IsDescriptionLocked)
                    SetDescriptionToRequestor();

                OnPropertyChanged("SelectedArea");
                OnPropertyChanged("IsDescriptionLocked");
            }
        }

        public string SelectedFloor
        {
            get { return selectedFloor; }
            set { selectedFloor = value; FilterRoomsByFloor(selectedFloor); OnPropertyChanged("SelectedFloor"); }
        }

        public Room SelectedRoom
        {
            get { return selectedRoom; }
            set
            {
                selectedRoom = value;
                if (selectedRoom != null && selectedRoom.room_name != "-All-")
                {
                    Request.RoomId = selectedRoom.id;
                    FilterLayoutsByRoom(selectedRoom.id);
                }

                ShowRoomCalendar();
                
                OnPropertyChanged("SelectedRoom");
            }
        }

        public RoomLayout SelectedLayout
        {
            get { return selectedLayout; }
            set { selectedLayout = value; Request.RoomLayoutId = selectedLayout == null ? 0 : selectedLayout.id; OnPropertyChanged("SelectedLayout"); }
        }

        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set
            {
                selectedDate = value;
                UpdateCalendarDates();
                ShowRoomCalendar();
                UpdateMeetingTimes(true);
                if (value != null && MeetingStartTimes.Any(t => t.TimeOfDay == MeetingStart.TimeOfDay))
                    MeetingStart = MeetingStartTimes.Single(t => t.TimeOfDay == MeetingStart.TimeOfDay);

                OnPropertyChanged("SelectedDate");
            }
        }

        public DateTime CalendarStart
        {
            get { return calendarStart; }
            set { calendarStart = value; OnPropertyChanged("CalendarStart"); }
        }

        public DateTime CalendarEnd
        {
            get { return calendarEnd; }
            set { calendarEnd = value; OnPropertyChanged("CalendarEnd"); }
        }

        public DateTime MeetingStart
        {
            get { return meetingStart; }
            set
            {
                TimeSpan offset;

                if (meetingEnd == null)
                    offset = new TimeSpan(0, 30, 0);
                else
                    offset = meetingEnd.Value - meetingStart;

                meetingStart = value;
                Request.Start = value;
                UpdateMeetingTimes(false);
                MeetingEnd = MeetingEndTimes.Any(t => t.Value == meetingStart + offset) ? MeetingEndTimes.Single(t => t.Value == meetingStart + offset) : MeetingEndTimes.Last();
                ShowAvailableRooms();
                OnPropertyChanged("MeetingStart");
            }
        }

        public EndTime MeetingEnd
        {
            get { return meetingEnd; }
            set
            {
                meetingEnd = value;
                if(meetingEnd != null)
                    Request.End = meetingEnd.Value;

                ShowAvailableRooms();
                OnPropertyChanged("MeetingEnd");
            }
        }

        public bool IsDescriptionLocked
        {
            get { return SelectedArea.area_name.Contains("Shared"); }
        }

        public ObservableCollection<CalendarRoom> CalendarRooms
        {
            get { return calendarRooms; }
            set { calendarRooms = value; OnPropertyChanged("CalendarRooms"); }
        }

        public MeetingRequest Request
        {
            get { return _request; }
            set { _request = value; OnPropertyChanged("Request"); }
        }

        public ObservableCollection<DateTime> MeetingStartTimes { get; set; }
        public ObservableCollection<EndTime> MeetingEndTimes { get; set; }

        public List<int> DaysOfWeek { get; set; }
        public List<int> DaysOfMonth { get; set; }
        public List<string> NumberWeekdaysOfMonth { get; set; } 

        public int CurrentMeetingId { get { return current_mtg_id; } }
        #endregion

        #region Constructor
        public ReservationViewModel()
        {
            DisplayManager.SplashScreen.AddMessage("Retrieving data from server, please wait...");
            current_mtg_id = -1;
            _request = new MeetingRequest();
            roomLayouts = new ObservableCollection<RoomLayout>();
            floors = new ObservableCollection<string>();
            rooms = new ObservableCollection<Room>();
            calendarRooms = new ObservableCollection<CalendarRoom>();
            roomCalendars = new Dictionary<string, CalendarRoom>();
            MeetingStartTimes = new ObservableCollection<DateTime>();
            MeetingEndTimes = new ObservableCollection<EndTime>();
            availableRooms = new ObservableCollection<Room>();
            DaysOfWeek = new List<int>();
            for (int i = 0; i <= 6; i++)
                DaysOfWeek.Add(i);

            DaysOfMonth = new List<int>();
            for (int i = 1; i <= 31; i++)
                DaysOfMonth.Add(i);

            NumberWeekdaysOfMonth = new List<string>();
            NumberWeekdaysOfMonth.Add("First");
            NumberWeekdaysOfMonth.Add("Second");
            NumberWeekdaysOfMonth.Add("Third");
            NumberWeekdaysOfMonth.Add("Fourth");
            NumberWeekdaysOfMonth.Add("Last");
            NumberWeekdaysOfMonth.Add("Second Last");
            NumberWeekdaysOfMonth.Add("Third Last");
            NumberWeekdaysOfMonth.Add("Fourth Last");
            NumberWeekdaysOfMonth.Add("Fifth Last");

            DisplayManager.UpdateSplashScreen("Retrieving areas...");
            areas = Database.GetAreas();
            DisplayManager.UpdateSplashScreen("Retrieving rooms...");
            allRooms = Database.GetAllRooms();
            DisplayManager.UpdateSplashScreen("Retrieving users...");
            users = Database.GetAllUsers();
            DisplayManager.UpdateSplashScreen("Retrieving room layouts...");
            layouts = Database.GetRoomLayouts();
            DisplayManager.UpdateSplashScreen("Retrieving data from server, please wait...");
            repeatTypes = Database.GetRepeatTypes();

            selectedDate = DateTime.Today;
            calendarStart = DateTime.Today.AddHours(7);
            calendarEnd = DateTime.Today.AddHours(19);             
            if (Database.CurrentUser != null)
            {
                DisplayManager.UpdateSplashScreen("Setting views for current user...");
                if (areas.Any(a => a.area_name.ToUpper() == Database.CurrentUser.location.ToUpper()))
                    SelectedArea = areas.First(a => a.area_name.ToUpper() == Database.CurrentUser.location.ToUpper());
            }
            else
            {
                SelectedArea = areas.FirstOrDefault();
            }

            UpdateMeetingTimes(true);
        }

        public ReservationViewModel(MeetingRequest request, int mtg_id) : this()
        {
            _request = request.Copy();
            current_mtg_id = mtg_id;
            UpdateMeetingTimes(true);
        }
        #endregion

        #region Methods
        private void FilterFloorsByArea(int areaId)
        {
            floors.Clear();
            foreach(string floor in allRooms.Where(r=>r.area_id == areaId).Select(f=>f.location).OrderBy<string, string>(f=>f, new FloorComparer()))
            {
                if (!floors.Contains(floor))
                    floors.Add(floor);
            }

            floors.Insert(0, "-All-");
            SelectedFloor = floors.FirstOrDefault();
            OnPropertyChanged("Floors");
        }

        private void FilterRoomsByArea(int areaId)
        {
            rooms.Clear();
            foreach (Room room in allRooms.Where(r => r.area_id == areaId).OrderBy(r=>r.sort_key))
                rooms.Add(room);

            rooms.Insert(0, new Room() { id = -1, room_name = "-All-" });
            SelectedRoom = rooms.FirstOrDefault();
            OnPropertyChanged("Rooms");
        }

        private void FilterRoomsByFloor(string floor)
        {
            rooms.Clear();
            if (floor == "-All-")
            {
                foreach (Room room in allRooms.Where(r=>r.area_id == SelectedArea.id).OrderBy(r => r.sort_key))
                    rooms.Add(room);
            }
            else
            {
                foreach (Room room in allRooms.Where(r => r.location == floor).OrderBy(r => r.sort_key))
                    rooms.Add(room);
            }

            rooms.Insert(0, new Room() { id = -1, room_name = "-All-" });
            SelectedRoom = rooms.FirstOrDefault();
            OnPropertyChanged("Rooms");
        }

        private void FilterLayoutsByRoom(int roomId)
        {
            roomLayouts.Clear();
            foreach (RoomLayout l in layouts.Where(lay => lay.room_id == roomId).OrderBy(lay => lay.sort_key))
                roomLayouts.Add(l);

            if (roomLayouts.Count > 0)
                Request.RoomLayoutId = roomLayouts[0].id;
            else
                Request.RoomLayoutId = 0;

            OnPropertyChanged("RoomLayouts");
            OnPropertyChanged("Request");
        }

        private void UpdateMeetingTimes(bool updateStartTimes)
        {
            if (SelectedDate != null && SelectedArea != null)
            {
                DateTime first = SelectedDate.AddHours(SelectedArea.morningstarts.HasValue ? SelectedArea.morningstarts.Value : 7).AddMinutes(SelectedArea.morningstarts_minutes.HasValue ? SelectedArea.morningstarts_minutes.Value : 0);
                DateTime last = SelectedDate.AddHours(SelectedArea.eveningends.HasValue ? SelectedArea.eveningends.Value : 7).AddMinutes(SelectedArea.eveningends_minutes.HasValue ? SelectedArea.eveningends_minutes.Value : 0);
                DateTime current;
                int count = 0;

                if (updateStartTimes)
                    MeetingStartTimes.Clear();
                else
                    first = MeetingStart;

                current = first;
                MeetingEndTimes.Clear();
                if (current.Date != last.Date)
                    last = current.Date + last.TimeOfDay;

                while ((current < last) && (count < 48))
                {
                    count++;
                    if(updateStartTimes)
                        MeetingStartTimes.Add(current);

                    current = current.AddMinutes(30);
                    MeetingEndTimes.Add(new EndTime() { Value = current, Start = first });
                }
            }
        }

        public void SetMeetingTimes(DateTime start, DateTime end)
        {
            MeetingStart = start;

            if (MeetingEndTimes.Any(t => t.Value == end))
                MeetingEnd = MeetingEndTimes.Single(t => t.Value == end);
            else if (MeetingEndTimes.Any(t => t.Value == start.AddHours(1)))
                MeetingEnd = MeetingEndTimes.Single(t => t.Value == start.AddHours(1));
            else
                MeetingEnd = MeetingEndTimes.First();

            OnPropertyChanged("MeetingStart");
            OnPropertyChanged("MeetingEnd");
        }

        private void UpdateCalendarDates()
        {
            if(SelectedArea != null)
            {
                if (SelectedDate > CalendarStart)
                {
                    CalendarEnd = SelectedDate.AddHours(SelectedArea.eveningends.Value).AddHours(SelectedArea.eveningends_minutes > 0 ? 1 : 0);
                    CalendarStart = SelectedDate.AddHours(SelectedArea.morningstarts.Value);
                }
                else
                {
                    CalendarStart = SelectedDate.AddHours(SelectedArea.morningstarts.Value);
                    CalendarEnd = SelectedDate.AddHours(SelectedArea.eveningends.Value).AddHours(SelectedArea.eveningends_minutes > 0 ? 1 : 0);
                }

                
            }
        }

        private void LoadRoomCalendar(Room room)
        {
            if (room != null)
            {
                string roomKey = String.Format("{0}|{1}", room.Area.area_name, room.room_name);

                if (!roomCalendars.ContainsKey(roomKey))
                {
                    CalendarRoom r = new CalendarRoom();
                    List<Entry> entries = Database.GetFutureAppointments(room);

                    r.StartTime = CalendarStart;
                    r.EndTime = CalendarEnd;
                    r.Name = room.room_name;

                    foreach (Entry e in entries)
                    {
                        CalendarAppointment appt = new CalendarAppointment();

                        appt.BeverageService = e.coffee != "";
                        appt.Description = e.name;
                        appt.Finish = Database.UnixTimeStampToDateTime(e.end_time);
                        appt.FoodService = e.food_needed.HasValue ? e.food_needed.Value : false;
                        appt.Location = e.Room.room_name;
                        appt.Requestor = e.requestor;
                        appt.Start = Database.UnixTimeStampToDateTime(e.start_time);
                        r.Appointments.Add(appt);
                    }

                    roomCalendars.Add(roomKey, r);
                }
            }
        }

        private void ShowRoomCalendar()
        {
            if (SelectedRoom != null)
            {
                if (SelectedRoom.id == -1)
                {
                    //Selected All
                    CalendarRooms.Clear();
                    for (int i = 1; i < rooms.Count; i++)
                    {
                        Room r = rooms[i];
                        String roomKey = String.Format("{0}|{1}", r.Area.area_name, r.room_name);
                        CalendarRoom cal = null;

                        if (!roomCalendars.ContainsKey(roomKey))
                            LoadRoomCalendar(r);

                        if (roomCalendars.ContainsKey(roomKey))
                            cal = roomCalendars[roomKey];

                        if (cal != null)
                        {
                            cal.StartTime = CalendarStart;
                            cal.EndTime = CalendarEnd;
                            CalendarRooms.Add(cal);
                        }
                    }
                }
                else
                {
                    String roomKey = String.Format("{0}|{1}", SelectedRoom.Area.area_name, SelectedRoom.room_name);
                    CalendarRoom cal = null;

                    CalendarRooms.Clear();
                    if (!roomCalendars.ContainsKey(roomKey))
                        LoadRoomCalendar(SelectedRoom);

                    if (roomCalendars.ContainsKey(roomKey))
                        cal = roomCalendars[roomKey];

                    if (cal != null)
                    {
                        cal.StartTime = CalendarStart;
                        cal.EndTime = CalendarEnd;
                        CalendarRooms.Add(cal);
                    }
                }
            }
        }

        private void ShowAvailableRooms()
        {
            if ((SelectedDate != null) && (SelectedArea != null) && (MeetingStart != null) && (MeetingEnd != null))
            {
                AvailableRooms.Clear();
                foreach (Room r in Database.GetAvailableRooms(SelectedArea, MeetingStart, MeetingEnd.Value, CurrentMeetingId))
                    AvailableRooms.Add(r);
            }
        }

        public void SetDescriptionToRequestor()
        {
            string requestor = Request.Requestor;

            if (requestor.Contains(", "))
                requestor = requestor.Substring(0, requestor.IndexOf(", "));

            Request.Description = requestor;
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

    class EndTime
    {
        public DateTime Value { get; set; }
        public DateTime Start { get; set; }
        public string Span
        {
            get
            {
                TimeSpan s = Value - Start;
                string val = "";

                if (s.Hours == 0)
                    val = String.Format("{0:h:mm tt} ({1} minutes)", Value, s.Minutes);
                else
                    val = String.Format("{0:h:mm tt} ({1} hours)", Value, s.TotalHours);

                return val;
            }
        }
    }    

    class FloorComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            Regex pattern = new Regex(@"^(?<floor>\d+)th");
            Match matchx = pattern.Match(x);
            Match matchy = pattern.Match(y);
            int intx = 0;
            int inty = 0;

            int.TryParse(matchx.Groups["floor"].Value, out intx);
            int.TryParse(matchy.Groups["floor"].Value, out inty);

            if ((intx > 0) && (inty > 0))
                return intx.CompareTo(inty);

            return x.CompareTo(y);
        }
    }
}
