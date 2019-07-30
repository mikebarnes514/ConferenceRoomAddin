using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.Linq;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace ConferenceRoomAddin.Data
{
    public class Database
    {
        #region Members
        private static MRBSEntities _db;
        #endregion

        #region Properties
        public static User CurrentUser
        {
            get
            {
                string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                User currentUser = null;

                try
                {
                    if (username.Contains("\\"))
                        username = username.Substring(username.IndexOf("\\") + 1) + "@";

                    if (_db.Users.Any(u => u.email.StartsWith(username)))
                        currentUser = _db.Users.First(u => u.email.StartsWith(username));
                }
                catch { }

                return currentUser;
            }
        }
        #endregion

        #region Constructors
        static Database()
        {
            if (_db == null)
            {
                string path = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath + ".config";
                ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                Configuration config;

                map.ExeConfigFilename = path;
                LogManager.LogMessage("Config path: " + path);
                config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                _db = new MRBSEntities(config.ConnectionStrings.ConnectionStrings["MRBSEntities"].ConnectionString);
            }
        }
        #endregion

        #region Query Methods
        public static List<Area> GetAreas()
        {
            return (from a in _db.Areas where a.disabled == false select a).ToList();
        }

        public static List<Room> GetAllRooms()
        {
            return (from r in _db.Rooms where r.disabled == false orderby r.sort_key select r).ToList();
        }

        public static List<User> GetAllUsers()
        {
            return (from u in _db.Users where u.active == true select u).ToList();
        }

        public static List<RoomLayout> GetRoomLayouts()
        {
            return (from l in _db.RoomLayouts where l.is_active == true select l).ToList();
        }

        public static List<RepeatType> GetRepeatTypes()
        {
            List<RepeatType> types = new List<RepeatType>();

            types.Add(new RepeatType() { Id = 0, Description = "None" });
            types.Add(new RepeatType() { Id = 1, Description = "Daily" });
            types.Add(new RepeatType() { Id = 2, Description = "Weekly" });
            types.Add(new RepeatType() { Id = 3, Description = "Monthly" });
            types.Add(new RepeatType() { Id = 4, Description = "Annually" });
            return types;
        }

        public static List<Entry> GetFutureAppointments(Room room)
        {
            double unixTime = DateTimeToUnixTimestamp(DateTime.Today);

            return (from e in _db.Entries where e.end_time > unixTime && e.room_id == room.id select e).ToList();
        }

        public static string GetRoomNameById(int roomId)
        {
            return (from r in _db.Rooms where r.id == roomId select r.room_name).Single();
        }

        public static string GetAreaNameByRoomId(int roomId)
        {
            return (from r in _db.Rooms join a in _db.Areas on r.area_id equals a.id where r.id == roomId select a.area_name).Single();
        }

        public static List<Room> GetAvailableRooms(Area area, DateTime start, DateTime end, int current_mtg_id)
        {
            int start_time = (int)DateTimeToUnixTimestamp(start);
            int end_time = (int)DateTimeToUnixTimestamp(end);
            var entries = (from r in _db.Rooms join e in _db.Entries on r.id equals e.room_id where e.start_time < end_time && e.end_time > start_time && e.id != current_mtg_id && e.repeat_id != current_mtg_id select r.id);

            return (from r in _db.Rooms where r.disabled == false && r.area_id == area.id && !entries.Contains(r.id) orderby r.sort_key select r).ToList();
        }

        public static List<Entry> FindConflicts(MeetingRequest request, int meeting_id)
        {
            List<DateTime> dates = new List<DateTime>();
            Dictionary<int, int> times = new Dictionary<int, int>();
            int first = 0, last = 0;

            switch(request.RepeatTypeId)
            {
                case 0:
                    //Single Meeting Only
                    dates.Add(request.Start.Date);
                    break;
                case 1:
                    //Daily Repeating Meeting
                    dates = GetRepeatingDailySchedule(request);
                    break;
                case 2:
                    //Weekly Repeating Meeting
                    dates = GetRepeatingWeeklySchedule(request);
                    break;
                case 3:
                    //Monthly Repeating Meeting (can be on same day, or same nth day of week
                    dates = request.RepeatMonthlyByDay ? GetRepeatingMonthlyByDaySchedule(request) : GetRepeatingMonthlyByWeekdaySchedule(request);
                    break;
                case 4:
                    //Annually Repeating Meeting
                    dates = GetRepeatingAnnuallySchedule(request);
                    break;
            }

            foreach(DateTime date in dates)
                times.Add((int)DateTimeToUnixTimestamp(date + request.Start.TimeOfDay), (int)DateTimeToUnixTimestamp(date + request.End.TimeOfDay));

            first = times.First().Key;
            last = times.Last().Value;
            var query1 = (from t in times select new { RoomId = request.RoomId, StartTime = t.Key, EndTime = t.Value }).ToList();
            var query2 = (from e in _db.Entries where e.start_time < last && e.end_time > first && e.id != meeting_id && e.repeat_id != meeting_id select e).ToList();
            return (from e in query2 join t in query1 on e.room_id equals t.RoomId where e.start_time < t.EndTime && e.end_time > t.StartTime select e).ToList();
        }

        public static MeetingRequest GetRequestByEntry(int entry_id, string ical_uid, bool repeating)
        {
            MeetingRequest request = new MeetingRequest();

            if (repeating && _db.Repeats.Any(r => r.id == entry_id))
            {
                Repeat repeat = _db.Repeats.Single(r => r.id == entry_id);

                request.Start = UnixTimeStampToDateTime(repeat.start_time);
                request.End = UnixTimeStampToDateTime(repeat.end_time);
                request.RepeatTypeId = repeat.rep_type;
                request.RepeatEnd = UnixTimeStampToDateTime(repeat.end_date).Date;
                if (repeat.rep_type == 2)
                {
                    request.RepeatWeeklyOnSunday = repeat.rep_opt.Length > 0 && repeat.rep_opt.Substring(0, 1) == "1";
                    request.RepeatWeeklyOnMonday = repeat.rep_opt.Length > 1 && repeat.rep_opt.Substring(1, 1) == "1";
                    request.RepeatWeeklyOnTuesday = repeat.rep_opt.Length > 2 && repeat.rep_opt.Substring(2, 1) == "1";
                    request.RepeatWeeklyOnWednesday = repeat.rep_opt.Length > 3 && repeat.rep_opt.Substring(3, 1) == "1";
                    request.RepeatWeeklyOnThursday = repeat.rep_opt.Length > 4 && repeat.rep_opt.Substring(4, 1) == "1";
                    request.RepeatWeeklyOnFriday = repeat.rep_opt.Length > 5 && repeat.rep_opt.Substring(5, 1) == "1";
                    request.RepeatWeeklyOnSaturday = repeat.rep_opt.Length > 6 && repeat.rep_opt.Substring(6, 1) == "1";
                }

                request.RoomId = repeat.room_id;
                request.Description = repeat.name;
                request.Details = repeat.description;
                request.RepeatNumberOfWeeks = (int)repeat.rep_num_weeks;
                if (repeat.month_relative != null)
                {
                    request.RepeatMonthlyByWeekday = true;
                    request.RepeatMonthlyByDay = false;
                    switch (repeat.month_relative.Substring(0, 1))
                    {
                        case "1":
                            request.RepeatNumberWeekdaysOfMonth = "First";
                            break;
                        case "2":
                            request.RepeatNumberWeekdaysOfMonth = "Second";
                            break;
                        case "3":
                            request.RepeatNumberWeekdaysOfMonth = "Third";
                            break;
                        case "4":
                            request.RepeatNumberWeekdaysOfMonth = "Fourth";
                            break;
                        case "5":
                            request.RepeatNumberWeekdaysOfMonth = "Fifth";
                            break;
                    }

                    switch (repeat.month_relative.Substring(1))
                    {
                        case "SU":
                            request.RepeatWeekdayOfMonth = 0;
                            break;
                        case "MO":
                            request.RepeatWeekdayOfMonth = 1;
                            break;
                        case "TU":
                            request.RepeatWeekdayOfMonth = 2;
                            break;
                        case "WE":
                            request.RepeatWeekdayOfMonth = 3;
                            break;
                        case "TH":
                            request.RepeatWeekdayOfMonth = 4;
                            break;
                        case "FR":
                            request.RepeatWeekdayOfMonth = 5;
                            break;
                        case "SA":
                            request.RepeatWeekdayOfMonth = 6;
                            break;
                    }
                }
                else if (repeat.month_absolute.HasValue)
                {
                    request.RepeatMonthlyByDay = true;
                    request.RepeatMonthlyByWeekday = false;
                    request.RepeatDayOfMonth = (int)repeat.month_absolute.Value;
                }

                request.IsRegularCoffee = repeat.coffee.Contains("C");
                request.IsDecafCoffee = repeat.coffee.Contains("D");
                request.IsIce = repeat.coffee.Contains("I");
                request.IsTea = repeat.coffee.Contains("T");
                request.IsFoodService = repeat.food_needed.HasValue && repeat.food_needed.Value;
                request.IsNotFoodService = repeat.food_needed.HasValue && !repeat.food_needed.Value;
                request.Requestor = repeat.requestor;
                if (repeat.room_layout_id.HasValue && repeat.room_layout_id.Value > 0)
                    request.RoomLayoutId = repeat.room_layout_id.Value;
            }
            else if (_db.Entries.Any(e => e.id == entry_id))
            {
                Entry entry = _db.Entries.Single(e => e.id == entry_id);

                request.Start = UnixTimeStampToDateTime(entry.start_time);
                request.End = UnixTimeStampToDateTime(entry.end_time);
                request.RepeatTypeId = entry.entry_type;
                request.RoomId = entry.room_id;
                request.Description = entry.name;
                request.Details = entry.description;
                request.IsRegularCoffee = entry.coffee.Contains("C");
                request.IsDecafCoffee = entry.coffee.Contains("D");
                request.IsIce = entry.coffee.Contains("I");
                request.IsTea = entry.coffee.Contains("T");
                request.IsFoodService = entry.food_needed.HasValue && entry.food_needed.Value;
                request.IsNotFoodService = entry.food_needed.HasValue && !entry.food_needed.Value;
                request.Requestor = entry.requestor;
                if (entry.room_layout_id.HasValue && entry.room_layout_id.Value > 0)
                    request.RoomLayoutId = entry.room_layout_id.Value;
            }
            else
                throw new KeyNotFoundException("Reservation was not found in MRBS.");

            return request;
        }

        public static List<Entry> GetAllEntries(int repeat_id)
        {
            List<Entry> entries = new List<Entry>();

            if (_db.Repeats.Any(r => r.id == repeat_id))
                entries = _db.Repeats.Single(r => r.id == repeat_id).Entries.ToList();

            return entries;
        }
        #endregion 

        #region Insert Methods
        public static string ReserveConferenceRoom(MeetingRequest request)
        {
            Uri url = new Uri(Properties.Settings.Default.MRBSWebServer);
            Random random = new Random();
            string guid = Guid.NewGuid().ToString().Replace("-", "").ToLower();
            string hash = CreateMD5Hash(request.Description + random.Next(0, 10000).ToString()).ToLower();
            string ical_uid = String.Format("MRBS-{0}-{1}@{2}", guid.Substring(0, 13), hash.Substring(0, 8), url.Host);
            List<Entry> conflicts = FindConflicts(request, -1);
            int db_id = -1;

            switch(request.RepeatTypeId)
            {
                case 0:
                    //Single Meeting Only
                    if(conflicts.Count == 0)
                        db_id = AddEntry(request, request.Start.Date, ical_uid);
                    break;
                case 1:
                    //Daily Repeating Meeting
                    db_id = AddRepeatingEntries(request, GetRepeatingDailySchedule(request), conflicts, ical_uid);
                    break;
                case 2:
                    //Weekly Repeating Meeting
                    db_id = AddRepeatingEntries(request, GetRepeatingWeeklySchedule(request), conflicts, ical_uid);
                    break;
                case 3:
                    //Monthly Repeating Meeting (can be on same day, or same nth day of week
                    db_id = AddRepeatingEntries(request, request.RepeatMonthlyByDay ? GetRepeatingMonthlyByDaySchedule(request) : GetRepeatingMonthlyByWeekdaySchedule(request), conflicts, ical_uid);
                    break;
                case 4:
                    //Annually Repeating Meeting
                    db_id = AddRepeatingEntries(request, GetRepeatingAnnuallySchedule(request), conflicts, ical_uid);
                    break;
            }

            return String.Format("{0};{1}", db_id, ical_uid);
        }

        private static int AddEntry(MeetingRequest request, DateTime date, string ical_uid, int repeat_id = -1)
        {
            Entry entry = new Entry();
            DateTime start = date.Date + request.Start.TimeOfDay;
            DateTime end = date.Date + request.End.TimeOfDay;
            int entry_id = -1;

            entry.start_time = (int)DateTimeToUnixTimestamp(start);
            entry.end_time = (int)DateTimeToUnixTimestamp(end);
            entry.entry_type = request.RepeatTypeId;
            entry.room_id = request.RoomId;
            entry.timestamp = DateTime.Now;
            entry.create_by = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            entry.modified_by = "";
            entry.name = request.Description;
            entry.type = "I";
            entry.description = request.Details;
            entry.status = 0;
            entry.ical_uid = ical_uid;
            entry.ical_sequence = 0;
            entry.ical_recur_id = start.ToUniversalTime().ToString("yyyyMMddTHHmmssK");
            entry.coffee = request.BeverageCode;
            entry.food_needed = request.IsFoodService;
            entry.catering = request.CateringInstructions;
            entry.requestor = request.Requestor;
            if(request.RoomLayoutId > 0)
                entry.room_layout_id = request.RoomLayoutId;
            if (repeat_id > -1)
                entry.repeat_id = repeat_id;

            using (var tr = _db.Database.BeginTransaction())
            {
                try
                {
                    entry = _db.Entries.Add(entry);
                    _db.SaveChanges();
                    tr.Commit();
                    entry_id = entry.id;
                }
                catch (DbEntityValidationException e)
                {
                    string msg = "Failed to reserve conference room:\n";

                    tr.Rollback();
                    foreach (var x in e.EntityValidationErrors)
                    {
                        foreach (var y in x.ValidationErrors)
                            msg += String.Format("{0} is invalid: {1}\n", y.PropertyName, y.ErrorMessage);
                    }

                    throw new Exception(msg);
                }
                catch (Exception ex)
                {
                    tr.Rollback();
                    System.Diagnostics.Debug.WriteLine("ConferenceRoomAddin Error: " + ex.Message);
                    System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                    throw new Exception("Failed to reserve conference room", ex);
                }
            }

            return entry_id;
        }

        private static int AddRepeat(MeetingRequest request, string ical_uid)
        {
            Repeat repeat = new Repeat();
            int repeat_id = -1;

            repeat.start_time = (int)DateTimeToUnixTimestamp(request.Start);
            repeat.end_time = (int)DateTimeToUnixTimestamp(request.End);
            repeat.rep_type = request.RepeatTypeId;
            repeat.end_date = (int)DateTimeToUnixTimestamp(request.RepeatEnd.Date);
            repeat.rep_opt = "0";
            if (request.RepeatTypeId == 2)
                repeat.rep_opt = request.RepeatWeeklyCode;
            repeat.room_id = request.RoomId;
            repeat.timestamp = DateTime.Now;
            repeat.create_by = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            repeat.modified_by = "";
            repeat.name = request.Description;
            repeat.type = "I";
            repeat.description = request.Details;
            repeat.rep_num_weeks = (short)request.RepeatNumberOfWeeks;

            if(request.RepeatMonthlyByDay)
                repeat.month_absolute = (short)request.RepeatDayOfMonth;
            if (request.RepeatMonthlyByWeekday)
                repeat.month_relative = request.RepeatMonthlyCode;

            repeat.status = 0;
            repeat.ical_uid = ical_uid;
            repeat.ical_sequence = 0;
            repeat.coffee = request.BeverageCode;
            repeat.food_needed = request.IsFoodService;
            repeat.catering = request.CateringInstructions;
            repeat.requestor = request.Requestor;
            if (request.RoomLayoutId > 0)
                repeat.room_layout_id = request.RoomLayoutId;

            using (var tr = _db.Database.BeginTransaction())
            {
                try
                {
                    repeat = _db.Repeats.Add(repeat);
                    _db.SaveChanges();
                    tr.Commit();
                    repeat_id = repeat.id;
                }
                catch (DbEntityValidationException e)
                {
                    string msg = "Failed to reserve conference room:\n";

                    tr.Rollback();
                    foreach (var x in e.EntityValidationErrors)
                    {
                        foreach (var y in x.ValidationErrors)
                            msg += String.Format("{0} is invalid: {1}\n", y.PropertyName, y.ErrorMessage);
                    }

                    throw new Exception(msg);
                }
                catch (Exception ex)
                {
                    tr.Rollback();
                    System.Diagnostics.Debug.WriteLine("ConferenceRoomAddin Error: " + ex.Message);
                    System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                    throw new Exception("Failed to reserve conference room", ex);
                }
            }

            return repeat_id;
        }

        private static int AddRepeatingEntries(MeetingRequest request, List<DateTime> dates, List<Entry> conflicts, string ical_uid)
        {
            int repeat_id = -1;

            if (dates.Count > conflicts.Count)
            {
                repeat_id = AddRepeat(request, ical_uid);
                foreach (DateTime appointment in dates)
                {
                   if(!conflicts.Any(c=>Database.UnixTimeStampToDateTime(c.start_time).Date == appointment.Date))
                        AddEntry(request, appointment, ical_uid, repeat_id);
                }
            }

            return repeat_id;
        }
        #endregion

        #region Delete Methods
        public static void RemoveEntry(int entry_id, string ical_uid)
        {
            if (_db.Entries.Any(e => e.id == entry_id && e.ical_uid == ical_uid))
            {
                Entry e = _db.Entries.Single(ent => ent.id == entry_id && ent.ical_uid == ical_uid);

                using (var tr = _db.Database.BeginTransaction())
                {
                    try
                    {
                        _db.Entries.Remove(e);
                        _db.SaveChanges();
                        tr.Commit();
                    }
                    catch(Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                }
            }
        }

        public static void RemoveRepeatingEntry(int repeat_id, string ical_uid)
        {
            List<int> entry_ids = new List<int>();

            if(_db.Repeats.Any(r=>r.id == repeat_id && r.ical_uid == ical_uid))
            {
                Repeat repeat = _db.Repeats.Single(r => r.id == repeat_id && r.ical_uid == ical_uid);

                using (var tr = _db.Database.BeginTransaction())
                {
                    try
                    {
                        entry_ids = (from e in repeat.Entries select e.id).ToList();
                        foreach (int e_id in entry_ids)
                            RemoveEntry(e_id, ical_uid);

                        _db.Repeats.Remove(repeat);
                        _db.SaveChanges();
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                }
            }
        }
        #endregion

        #region Update Methods
        public static string UpdateEntry(int entry_id, string ical_uid, MeetingRequest request)
        {
            Entry entry = null;
            string ret_id = String.Format("{0};{1}", entry_id, ical_uid);

            if (!_db.Entries.Any(e => e.id == entry_id && e.ical_uid == ical_uid))
                return ret_id;

            entry = _db.Entries.Single(e => e.id == entry_id && e.ical_uid == ical_uid);
            entry.start_time = (int)DateTimeToUnixTimestamp(request.Start);
            entry.end_time = (int)DateTimeToUnixTimestamp(request.End);
            entry.entry_type = request.RepeatTypeId;
            entry.room_id = request.RoomId;
            entry.timestamp = DateTime.Now;
            entry.modified_by = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            entry.name = request.Description;
            entry.type = "I";
            entry.description = request.Details;
            entry.status = 0;
            entry.ical_uid = ical_uid;
            entry.ical_sequence++;
            entry.ical_recur_id = request.Start.ToUniversalTime().ToString("yyyyMMddTHHmmssK");
            entry.coffee = request.BeverageCode;
            entry.food_needed = request.IsFoodService;
            entry.catering = request.CateringInstructions;
            entry.requestor = request.Requestor;
            if (request.RoomLayoutId > 0)
                entry.room_layout_id = request.RoomLayoutId;

            using (var tr = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.SaveChanges();
                    tr.Commit();
                }
                catch (DbEntityValidationException e)
                {
                    string msg = "Failed to reserve conference room:\n";

                    tr.Rollback();
                    foreach (var x in e.EntityValidationErrors)
                    {
                        foreach (var y in x.ValidationErrors)
                            msg += String.Format("{0} is invalid: {1}\n", y.PropertyName, y.ErrorMessage);
                    }

                    throw new Exception(msg);
                }
                catch (Exception ex)
                {
                    tr.Rollback();
                    System.Diagnostics.Debug.WriteLine("ConferenceRoomAddin Error: " + ex.Message);
                    System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                    throw new Exception("Failed to reserve conference room", ex);
                }
            }

            return ret_id;
        }

        public static string UpdateRepeat(int repeat_id, string ical_uid, MeetingRequest request)
        {
            Repeat repeat = null;
            string ret_id = string.Format("{0};{1}", repeat_id, ical_uid);

            if (!_db.Repeats.Any(r => r.id == repeat_id && r.ical_uid == ical_uid))
                return ret_id;

            repeat = _db.Repeats.Single(e => e.id == repeat_id && e.ical_uid == ical_uid);
            repeat.start_time = (int)DateTimeToUnixTimestamp(request.Start);
            repeat.end_time = (int)DateTimeToUnixTimestamp(request.End);
            repeat.rep_type = request.RepeatTypeId;
            repeat.room_id = request.RoomId;
            repeat.timestamp = DateTime.Now;
            repeat.modified_by = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            repeat.name = request.Description;
            repeat.type = "I";
            repeat.description = request.Details;
            repeat.status = 0;
            repeat.ical_uid = ical_uid;
            repeat.ical_sequence++;
            repeat.coffee = request.BeverageCode;
            repeat.food_needed = request.IsFoodService;
            repeat.catering = request.CateringInstructions;
            repeat.requestor = request.Requestor;
            if (request.RoomLayoutId > 0)
                repeat.room_layout_id = request.RoomLayoutId;

            foreach(Entry e in repeat.Entries)
            {
                e.timestamp = DateTime.Now;
                e.modified_by = repeat.modified_by;
                e.name = repeat.name;
                e.description = repeat.description;
                e.ical_sequence++;
                e.coffee = repeat.coffee;
                e.food_needed = repeat.food_needed;
                e.catering = repeat.catering;
                e.requestor = repeat.requestor;
                if (request.RoomLayoutId > 0)
                    e.room_layout_id = repeat.room_layout_id;
            }

            using (var tr = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.SaveChanges();
                    tr.Commit();
                }
                catch (DbEntityValidationException e)
                {
                    string msg = "Failed to reserve conference room:\n";

                    tr.Rollback();
                    foreach (var x in e.EntityValidationErrors)
                    {
                        foreach (var y in x.ValidationErrors)
                            msg += String.Format("{0} is invalid: {1}\n", y.PropertyName, y.ErrorMessage);
                    }

                    throw new Exception(msg);
                }
                catch (Exception ex)
                {
                    tr.Rollback();
                    System.Diagnostics.Debug.WriteLine("ConferenceRoomAddin Error: " + ex.Message);
                    System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                    throw new Exception("Failed to reserve conference room", ex);
                }
            }

            return ret_id;
        }
        #endregion

        #region Helper Methods
        public static DateTime UnixTimeStampToDateTime(int unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }

        private static string CreateMD5Hash(string seed)
        {
            byte[] bytes = new UTF8Encoding().GetBytes(seed);
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(bytes);

            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        private static DateTime FindNextAppointment(DateTime from, int dow, int num_weeks)
        {
            int days = dow - (int)from.DayOfWeek;

            if (days <= 0)
                days += 7;

            if (num_weeks > 0)
                days += ((num_weeks - 1) * 7);
            else
                days += (num_weeks * 7);

            return from.AddDays(days);
        }

        private static List<DateTime> GetRepeatingDailySchedule(MeetingRequest request)
        {
            List<DateTime> dates = new List<DateTime>();
            DateTime date = request.Start.Date;

            while(date <= request.RepeatEnd)
            {
                dates.Add(date);
                date = date.AddDays(1);
            }

            return dates.OrderBy(d => d).ToList();
        }

        private static List<DateTime> GetRepeatingWeeklySchedule(MeetingRequest request)
        {
            List<DateTime> dates = new List<DateTime>();

            dates.Add(request.Start.Date);
            if (request.RepeatWeeklyOnSunday)
            {
                DateTime nextAppointment = FindNextAppointment(request.Start.Date, (int)DayOfWeek.Sunday, request.RepeatNumberOfWeeks);
                while (nextAppointment <= request.RepeatEnd)
                {
                    dates.Add(nextAppointment.Date);
                    nextAppointment = FindNextAppointment(nextAppointment, (int)DayOfWeek.Sunday, request.RepeatNumberOfWeeks);
                }
            }

            if (request.RepeatWeeklyOnMonday)
            {
                DateTime nextAppointment = FindNextAppointment(request.Start.Date, (int)DayOfWeek.Monday, request.RepeatNumberOfWeeks);
                while (nextAppointment <= request.RepeatEnd)
                {
                    dates.Add(nextAppointment.Date);
                    nextAppointment = FindNextAppointment(nextAppointment, (int)DayOfWeek.Monday, request.RepeatNumberOfWeeks);
                }
            }

            if (request.RepeatWeeklyOnTuesday)
            {
                DateTime nextAppointment = FindNextAppointment(request.Start.Date, (int)DayOfWeek.Tuesday, request.RepeatNumberOfWeeks);
                while (nextAppointment <= request.RepeatEnd)
                {
                    dates.Add(nextAppointment.Date);
                    nextAppointment = FindNextAppointment(nextAppointment, (int)DayOfWeek.Tuesday, request.RepeatNumberOfWeeks);
                }
            }

            if (request.RepeatWeeklyOnWednesday)
            {
                DateTime nextAppointment = FindNextAppointment(request.Start.Date, (int)DayOfWeek.Wednesday, request.RepeatNumberOfWeeks);
                while (nextAppointment <= request.RepeatEnd)
                {
                    dates.Add(nextAppointment.Date);
                    nextAppointment = FindNextAppointment(nextAppointment, (int)DayOfWeek.Wednesday, request.RepeatNumberOfWeeks);
                }
            }

            if (request.RepeatWeeklyOnThursday)
            {
                DateTime nextAppointment = FindNextAppointment(request.Start.Date, (int)DayOfWeek.Thursday, request.RepeatNumberOfWeeks);
                while (nextAppointment <= request.RepeatEnd)
                {
                    dates.Add(nextAppointment.Date);
                    nextAppointment = FindNextAppointment(nextAppointment, (int)DayOfWeek.Thursday, request.RepeatNumberOfWeeks);
                }
            }

            if (request.RepeatWeeklyOnFriday)
            {
                DateTime nextAppointment = FindNextAppointment(request.Start.Date, (int)DayOfWeek.Friday, request.RepeatNumberOfWeeks);
                while (nextAppointment <= request.RepeatEnd)
                {
                    dates.Add(nextAppointment.Date);
                    nextAppointment = FindNextAppointment(nextAppointment, (int)DayOfWeek.Friday, request.RepeatNumberOfWeeks);
                }
            }

            if (request.RepeatWeeklyOnSaturday)
            {
                DateTime nextAppointment = FindNextAppointment(request.Start.Date, (int)DayOfWeek.Saturday, request.RepeatNumberOfWeeks);
                while (nextAppointment <= request.RepeatEnd)
                {
                    dates.Add(nextAppointment.Date);
                    nextAppointment = FindNextAppointment(nextAppointment, (int)DayOfWeek.Saturday, request.RepeatNumberOfWeeks);
                }
            }

            return dates.OrderBy(d => d).ToList();
        }

        private static List<DateTime> GetRepeatingMonthlyByDaySchedule(MeetingRequest request)
        {
            List<DateTime> dates = new List<DateTime>();
            DateTime nextAppointment = request.Start.Date;
            int offset = request.RepeatDayOfMonth - nextAppointment.Day;

            dates.Add(nextAppointment);
            nextAppointment = nextAppointment.AddDays(offset);
            if (offset <= 0)
                nextAppointment = nextAppointment.AddMonths(1);
                        
            while(nextAppointment <= request.RepeatEnd)
            {
                while ((nextAppointment.Day != request.RepeatDayOfMonth) && (nextAppointment.AddDays(1).Month == nextAppointment.Month))
                    nextAppointment = nextAppointment.AddDays(1);

                dates.Add(nextAppointment);
                nextAppointment = nextAppointment.AddMonths(1);
            }

            return dates.OrderBy(d => d).ToList();
        }

        private static List<DateTime> GetRepeatingMonthlyByWeekdaySchedule(MeetingRequest request)
        {
            List<DateTime> dates = new List<DateTime>();
            DateTime nextAppointment = request.Start.Date;
            DateTime month = new DateTime(nextAppointment.Year, nextAppointment.Month, 1);
            int num_wks = request.RepeatWeekdaysOfMonth;

            if (num_wks > 0)
                num_wks--;
            else if(num_wks < 0)
                month = month.AddMonths(1);

            dates.Add(nextAppointment);
            nextAppointment = FindNextAppointment(month, request.RepeatWeekdayOfMonth, num_wks);
            
            while(nextAppointment <= request.RepeatEnd)
            {
                if (!dates.Any(d => d.Date == nextAppointment.Date))
                    dates.Add(nextAppointment);

                month = month.AddMonths(1);
                nextAppointment = FindNextAppointment(month, request.RepeatWeekdayOfMonth, num_wks);
            }

            return dates.OrderBy(d => d).ToList();
        }

        private static List<DateTime> GetRepeatingAnnuallySchedule(MeetingRequest request)
        {
            List<DateTime> dates = new List<DateTime>();
            DateTime nextAppointment = request.Start.Date;

            while(nextAppointment <= request.RepeatEnd)
            {
                dates.Add(nextAppointment);
                nextAppointment = nextAppointment.AddYears(1);
            }

            return dates.OrderBy(d => d).ToList();
        }
        #endregion
    }

    public class RepeatType
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
}
