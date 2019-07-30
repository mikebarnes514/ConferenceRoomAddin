using ConferenceRoomAddin.Data;
using ConferenceRoomAddin.UI.Windows;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConferenceRoomAddin
{
    public class OutlookManager
    {
        private static List<AppointmentItem> Appointments { get; set; }

        private static bool ItemSaved;

        private static List<Items> CalendarItems { get; set; }

        public static bool AddReservation(AppointmentItem appointment)
        {
            ReservationWindow wnd = null;
            bool success = false;

            DisplayManager.ShowSplashScreen();
            try
            {
                wnd = new ReservationWindow(appointment.Application.Session.CurrentUser.Name, "", appointment.Start, appointment.End, appointment.IsRecurring ? appointment.GetRecurrencePattern() : null);
                wnd.Loaded += (s, e) => { DisplayManager.HideSplashScreen(); };
            }
            catch(System.Exception ex)
            {
                DisplayManager.HideSplashScreen();
                LogManager.LogException(ex);
                DisplayManager.ShowErrorWindow("Failed to load MRBS Window", ex.Message);
            }

            if ((wnd != null) && (wnd.ShowDialog() == true))
            {
                DisplayManager.ShowMessageWindow("Reserving Conference Room");
                if (wnd.Request.Description == "")
                {
                    string requestor = wnd.Request.Requestor;

                    if (requestor.Contains(", "))
                        requestor = requestor.Substring(0, requestor.IndexOf(", "));

                    wnd.Request.Description = requestor;
                }
                                
                try
                {
                    DisplayManager.UpdateMessageScreen("Saving entry to MRBS...");
                    string mtg_id = Database.ReserveConferenceRoom(wnd.Request);

                    UpdateOutlookAppointment(appointment, mtg_id, wnd.Request);
                    success = true;
                    DisplayManager.UpdateMessageScreen("Reservation was successful!");
                    Thread.Sleep(1200);
                }
                catch (System.Exception ex)
                {
                    string msg = ex.Message;

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        msg += "\n" + ex.Message;
                    }

                    LogManager.LogException(ex);
                    DisplayManager.ShowErrorWindow("Error reserving conference room", msg);
                    success = false;
                }
                finally
                {
                    DisplayManager.HideMessageScreen();
                }
            }

            return success;
        }

        public static bool RemoveReservation(AppointmentItem appointment)
        {
            string mrbs_uid = appointment.UserProperties["MJ-MRBS-ID"].Value.ToString();
            int mrbs_id = -1;
            string ical_uid = "";
            string[] splits = mrbs_uid.Split(';');
            bool success = true;

            DisplayManager.ShowMessageWindow("Removing Reservation");

            if (splits.Length > 0)
                int.TryParse(splits[0], out mrbs_id);
            if (splits.Length > 1)
                ical_uid = splits[1];

            try
            {
                DisplayManager.UpdateMessageScreen("Removing entry from MRBS...");
                if (appointment.RecurrenceState == OlRecurrenceState.olApptMaster)
                    Database.RemoveRepeatingEntry(mrbs_id, ical_uid);
                else
                    Database.RemoveEntry(mrbs_id, ical_uid);

                Thread.Sleep(1200);
            }
            catch (System.Exception ex)
            {
                string msg = ex.Message;

                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    msg += "\n" + ex.Message;
                }

                LogManager.LogException(ex);
                DisplayManager.ShowErrorWindow("Error removing reservation", msg);
                success = false;
            }
            finally
            {
                DisplayManager.HideMessageScreen();
            }

            return success;
        }

        public static bool ModifyReservation(AppointmentItem appointment)
        {
            ReservationWindow wnd = null;
            MeetingRequest existing_request = null;
            string mrbs_uid = appointment.UserProperties["MJ-MRBS-ID"].Value.ToString();
            int mrbs_id = -1;
            string ical_uid = "";
            string[] splits = mrbs_uid.Split(';');
            bool success = false;

            if (splits.Length > 0)
                int.TryParse(splits[0], out mrbs_id);
            if (splits.Length > 1)
                ical_uid = splits[1];

            DisplayManager.ShowSplashScreen();
            DisplayManager.UpdateSplashScreen("Retrieving reservation from MRBS...");
            try
            {
                existing_request = Database.GetRequestByEntry(mrbs_id, ical_uid, appointment.RecurrenceState == OlRecurrenceState.olApptMaster);
            }
            catch(KeyNotFoundException)
            {
                DisplayManager.HideSplashScreen();
                if (DisplayManager.ShowQuestionWindow("Reservation not found in MRBS", "The reservation linked with this appointment could not be found in MRBS.", "Do you want to remove the link?") == true)
                    DisassociateAppointment(appointment);

                return false;
            }
            catch(System.Exception ex)
            {
                string msg = ex.Message;

                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    msg += "\n" + ex.Message;
                }

                LogManager.LogException(ex);
                DisplayManager.HideSplashScreen();
                DisplayManager.ShowErrorWindow("Error reserving conference room", msg);
                return false;
            }

            wnd = new ReservationWindow(existing_request, mrbs_id);
            wnd.Loaded += (s, e) => { DisplayManager.HideSplashScreen(); };
            if (wnd.ShowDialog() == true)
            {
                bool roomChanged = existing_request.RoomId != wnd.Request.RoomId;
                bool timeChanged = (existing_request.Start != wnd.Request.Start) || (existing_request.End != wnd.Request.End);
                bool patternChanged = IsMRBSPatternDifferent(existing_request, wnd.Request);

                DisplayManager.ShowMessageWindow("Reserving Conference Room");
                if (wnd.Request.Description == "")
                {
                    string requestor = wnd.Request.Requestor;

                    if (requestor.Contains(", "))
                        requestor = requestor.Substring(0, requestor.IndexOf(", "));

                    wnd.Request.Description = requestor;
                }

                try
                {
                    DisplayManager.UpdateMessageScreen("Saving entry to MRBS...");
                    string mtg_id = "";

                    if (appointment.RecurrenceState == OlRecurrenceState.olApptMaster)
                    {
                        if(roomChanged || timeChanged || patternChanged)
                        {
                            Database.RemoveRepeatingEntry(mrbs_id, ical_uid);
                            mtg_id = Database.ReserveConferenceRoom(wnd.Request);
                        }
                        else
                            mtg_id = Database.UpdateRepeat(mrbs_id, ical_uid, wnd.Request);
                    }
                    else
                        mtg_id = Database.UpdateEntry(mrbs_id, ical_uid, wnd.Request);

                    UpdateOutlookAppointment(appointment, mtg_id, wnd.Request, patternChanged);
                    success = true;
                    DisplayManager.UpdateMessageScreen("Reservation was successful!");
                    Thread.Sleep(1200);
                }
                catch (System.Exception ex)
                {
                    string msg = ex.Message;

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        msg += "\n" + ex.Message;
                    }

                    LogManager.LogException(ex);
                    DisplayManager.ShowErrorWindow("Error reserving conference room", msg);
                    success = false;
                }
                finally
                {
                    DisplayManager.HideMessageScreen();
                }
            }

            return success;
        }
        
        public static bool SaveChangesToReservation(AppointmentItem appointment)
        {
            MeetingRequest request = null;
            string mrbs_uid = appointment.UserProperties["MJ-MRBS-ID"].Value.ToString();
            int mrbs_id = -1;
            string ical_uid = "";
            string[] splits = mrbs_uid.Split(';');
            bool success = true;

            DisplayManager.ShowMessageWindow("Updating Reservation");
            DisplayManager.UpdateMessageScreen("Checking for changes to entry...");
            if (splits.Length > 0)
                int.TryParse(splits[0], out mrbs_id);
            if (splits.Length > 1)
                ical_uid = splits[1];

            request = Database.GetRequestByEntry(mrbs_id, ical_uid, appointment.RecurrenceState == OlRecurrenceState.olApptMaster);
            if (request != null)
            {
                bool timeChanged = (request.Start != appointment.Start) || (request.End != appointment.End);
                //bool subjectChanged = request.Description != appointment.Subject;

                try
                {
                    string mtg_id = "";

                    if(timeChanged)
                    {
                        request.Start = appointment.Start;
                        request.End = appointment.End;
                    }

                    //if (subjectChanged)
                    //    request.Description = appointment.Subject;

                    if (appointment.RecurrenceState == OlRecurrenceState.olApptMaster)
                    {
                        RecurrencePattern pattern = appointment.GetRecurrencePattern();
                        bool patternChanged = IsOutlookPatternDifferent(request, pattern);

                        if(patternChanged)
                            request.LoadFromRecurrencePattern(pattern);

                        if (timeChanged || patternChanged/* || subjectChanged*/)
                        {
                            List<Entry> conflicts = Database.FindConflicts(request, mrbs_id);

                            if(conflicts.Count > 0)
                            {
                                if(!DisplayManager.ShowConflictWindow(conflicts, true))
                                {
                                    request = Database.GetRequestByEntry(mrbs_id, ical_uid, true);
                                    UpdateOutlookAppointment(appointment, mrbs_uid, request, true);
                                    DisplayManager.HideMessageScreen();
                                    return false;
                                }
                            }

                            DisplayManager.UpdateMessageScreen("Saving entry to MRBS...");
                            Database.RemoveRepeatingEntry(mrbs_id, ical_uid);
                            mtg_id = Database.ReserveConferenceRoom(request);
                            DisplayManager.UpdateMessageScreen("Reservation was successful!");
                            Thread.Sleep(1200);
                        }
                        else
                        {
                            DisplayManager.UpdateMessageScreen("No changes necessary");
                            Thread.Sleep(500);
                        }
                    }
                    else if (timeChanged/* || subjectChanged*/)
                    {
                        List<Entry> conflicts = Database.FindConflicts(request, mrbs_id);

                        if(conflicts.Count > 0)
                        {
                            DisplayManager.ShowConflictWindow(conflicts);
                            request = Database.GetRequestByEntry(mrbs_id, ical_uid, false);
                            UpdateOutlookAppointment(appointment, mrbs_uid, request);
                            DisplayManager.HideMessageScreen();
                            return false;
                        }

                        DisplayManager.UpdateMessageScreen("Saving entry to MRBS...");

                        mtg_id = Database.UpdateEntry(mrbs_id, ical_uid, request);
                        DisplayManager.UpdateMessageScreen("Reservation was successful!");
                        Thread.Sleep(1200);
                    }
                    else
                    {
                        DisplayManager.UpdateMessageScreen("No changes necessary");
                        Thread.Sleep(500);
                    }

                    success = true;
                }
                catch (System.Exception ex)
                {
                    string msg = ex.Message;

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        msg += "\n" + ex.Message;
                    }

                    LogManager.LogException(ex);
                    DisplayManager.ShowErrorWindow("Error reserving conference room", msg);
                    success = false;
                }
                finally
                {
                    DisplayManager.HideMessageScreen();
                }
            }

            return success;
        }

        private static void UpdateOutlookAppointment(AppointmentItem appointment, string mtg_id, MeetingRequest request, bool reload_pattern = false)
        {
            int mrbs_id = -1;
            
            int.TryParse(mtg_id.Substring(0, mtg_id.IndexOf(";")), out mrbs_id);
            
           // appointment.Subject = request.Description;

            if ((appointment.RecurrenceState != OlRecurrenceState.olApptMaster) || (appointment.EntryID == ""))
            {
                appointment.Start = request.Start;
                appointment.End = request.End;
            }

            appointment.Location = String.Format("{0} - {1}", Database.GetAreaNameByRoomId(request.RoomId), Database.GetRoomNameById(request.RoomId));
            
            if (request.RepeatTypeId > 0)
            {
                RecurrencePattern pattern;

                if (reload_pattern)
                    appointment.ClearRecurrencePattern();

                pattern = appointment.GetRecurrencePattern();

                switch (request.RepeatTypeId)
                {
                    case 1:
                        pattern.RecurrenceType = OlRecurrenceType.olRecursDaily;
                        pattern.Interval = 1;
                        break;
                    case 2:
                        char[] rev = request.RepeatWeeklyCode.ToCharArray();

                        pattern.RecurrenceType = OlRecurrenceType.olRecursWeekly;
                        pattern.Interval = request.RepeatNumberOfWeeks;
                        pattern.DayOfWeekMask = (OlDaysOfWeek)Convert.ToInt32(new string(request.RepeatWeeklyCode.ToCharArray().Reverse().ToArray()), 2);
                        break;
                    case 3:
                        if (request.RepeatMonthlyByWeekday)
                        {
                            pattern.RecurrenceType = OlRecurrenceType.olRecursMonthNth;
                            pattern.Interval = 1;
                            pattern.DayOfWeekMask = (OlDaysOfWeek)((int)Math.Pow(2, request.RepeatWeekdayOfMonth));
                            pattern.Instance = request.RepeatWeekdaysOfMonth;
                        }
                        else
                        {
                            pattern.RecurrenceType = OlRecurrenceType.olRecursMonthly;
                            pattern.Interval = 1;
                            pattern.DayOfMonth = request.RepeatDayOfMonth;
                        }
                        break;
                    case 4:
                        pattern.RecurrenceType = OlRecurrenceType.olRecursYearly;
                        break;
                }

                pattern.Duration = (request.End - request.Start).Minutes;
                pattern.EndTime = request.RepeatEnd.Date + request.End.TimeOfDay;
                pattern.NoEndDate = false;
                pattern.PatternStartDate = request.Start;
                pattern.PatternEndDate = request.RepeatEnd + request.End.TimeOfDay;
                pattern.StartTime = request.Start;
            }

            try
            {
                appointment.UserProperties.Add("MJ-MRBS-ID", OlUserPropertyType.olText, true, OlFormatText.olFormatTextText);
            }
            catch(UnauthorizedAccessException)
            {
                //appointment.UserProperties.Add("MJ-MRBS-ID", OlUserPropertyType.olText, true, OlFormatText.olFormatTextText);
            }

            appointment.UserProperties["MJ-MRBS-ID"].Value = mtg_id;
            if (!appointment.Body.Contains("has been linked to a reservation in MRBS."))
                appointment.Body += String.Format("\n\n\n***** This {0} has been linked to a reservation in MRBS. To change the {0} location, you must use the Modify Reservation button in the ribbon. *****", appointment.MeetingStatus == OlMeetingStatus.olNonMeeting ? "appointment" : "meeting");

            appointment.Save();
            if (request.RepeatTypeId > 0)
            {
                RecurrencePattern pattern = appointment.GetRecurrencePattern();
                List<Entry> entries = Database.GetAllEntries(mrbs_id);
                List<Entry> conflicts = Database.FindConflicts(request, mrbs_id);

                foreach (Entry conflict in conflicts)
                {
                    DateTime start_time = Database.UnixTimeStampToDateTime(conflict.start_time);

                    AppointmentItem item = pattern.GetOccurrence(start_time);
                    item.Delete();
                }

                foreach (Entry entry in entries)
                {
                    try
                    {
                        DateTime start_time = Database.UnixTimeStampToDateTime(entry.start_time);
                        AppointmentItem item = pattern.GetOccurrence(start_time);
                        item.UserProperties.Add("MJ-MRBS-ID", OlUserPropertyType.olText, true, OlFormatText.olFormatTextText);
                        item.UserProperties["MJ-MRBS-ID"].Value = String.Format("{0};{1}", entry.id, entry.ical_uid);
                        item.Save();
                    }
                    catch { }
                }

                appointment.Save();
            }

            
        }

        private static void DisassociateAppointment(AppointmentItem appointment)
        {
            if (appointment.UserProperties["MJ-MRBS-ID"] != null)
            {
                appointment.UserProperties["MJ-MRBS-ID"].Delete();
                if(appointment.Body.Contains("*****"))
                    appointment.Body = appointment.Body.Substring(0, appointment.Body.IndexOf("*****")) + appointment.Body.Substring(appointment.Body.LastIndexOf("*****") + 5);
                
                appointment.Save();
            }
        }

        private static bool IsMRBSPatternDifferent(MeetingRequest before, MeetingRequest after)
        {
            bool different = false;

            different |= before.RepeatTypeId != after.RepeatTypeId;
            different |= before.RepeatWeeklyCode != after.RepeatWeeklyCode;
            different |= before.RepeatMonthlyCode != after.RepeatMonthlyCode;
            different |= before.RepeatDayOfMonth != after.RepeatDayOfMonth;
            different |= before.RepeatEnd != after.RepeatEnd;

            return different;
        }

        private static bool IsOutlookPatternDifferent(MeetingRequest mrbs, RecurrencePattern pattern)
        {
            bool different = false;

            switch (mrbs.RepeatTypeId)
            {
                case 1:
                    different |= pattern.RecurrenceType != OlRecurrenceType.olRecursDaily;
                    break;
                case 2:
                    char[] rev = mrbs.RepeatWeeklyCode.ToCharArray();

                    different |= pattern.RecurrenceType != OlRecurrenceType.olRecursWeekly;
                    different |= pattern.Interval != mrbs.RepeatNumberOfWeeks;
                    different |= pattern.DayOfWeekMask != (OlDaysOfWeek)Convert.ToInt32(new string(mrbs.RepeatWeeklyCode.ToCharArray().Reverse().ToArray()), 2);
                    break;
                case 3:
                    if (mrbs.RepeatMonthlyByWeekday)
                    {
                        different |= pattern.RecurrenceType != OlRecurrenceType.olRecursMonthNth;
                        different |= pattern.DayOfWeekMask != (OlDaysOfWeek)((int)Math.Pow(2, mrbs.RepeatWeekdayOfMonth));
                        different |= pattern.Instance != mrbs.RepeatWeekdaysOfMonth;
                    }
                    else
                    {
                        different |= pattern.RecurrenceType != OlRecurrenceType.olRecursMonthly;
                        different |= pattern.DayOfMonth != mrbs.RepeatDayOfMonth;
                    }
                    break;
                case 4:
                    different |= pattern.RecurrenceType != OlRecurrenceType.olRecursYearly;
                    break;
            }

            different |= pattern.Duration != (mrbs.End - mrbs.Start).Minutes;
            different |= pattern.EndTime != (mrbs.RepeatEnd.Date + mrbs.End.TimeOfDay);
            different |= pattern.PatternStartDate != mrbs.Start;
            different |= pattern.PatternEndDate != (mrbs.RepeatEnd + mrbs.End.TimeOfDay);
            different |= pattern.StartTime != mrbs.Start;

            return different;
        }
    }
}
