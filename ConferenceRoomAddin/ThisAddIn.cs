using ConferenceRoomAddin.Data;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ConferenceRoomAddin
{
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            LogManager.LogMessage("Addin starting");
            MAPIFolder calendar = Application.GetNamespace("MAPI").GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
            //LogManager.LogMessage("Event monitor starting");
            //CalendarMonitor monitor = new CalendarMonitor(Application.ActiveExplorer());

            calendar.UserDefinedProperties.Add("MJ-MRBS-ID", OlUserPropertyType.olText);
            //monitor.AppointmentAdded += Monitor_AppointmentAdded;
            //monitor.AppointmentDeleting += Monitor_AppointmentDeleting;
            //monitor.AppointmentModified += Monitor_AppointmentModified;
            LogManager.LogMessage("Addin startup complete.");
        }

        private void Monitor_AppointmentModified(object sender, EventArgs<AppointmentItem> e)
        {
            var creator = e.Value.PropertyAccessor.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x0C1A001E");

            if ((e.Value.Organizer != Application.Session.CurrentUser.Name) && (creator != Application.Session.CurrentUser.Name))
                return;

            if (e.Value.UserProperties.Find("MJ-MRBS-ID") != null)
                OutlookManager.SaveChangesToReservation(e.Value);
        }

        private void Monitor_AppointmentDeleting(object sender, CancelEventArgs<AppointmentItem> e)
        {
            var creator = e.Value.PropertyAccessor.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x0C1A001E");

            if ((e.Value.Organizer != Application.Session.CurrentUser.Name) && (creator != Application.Session.CurrentUser.Name))
                return;

            if (e.Value.UserProperties.Find("MJ-MRBS-ID") != null)
                e.Cancel = !OutlookManager.RemoveReservation(e.Value);
        }

        private void Monitor_AppointmentAdded(object sender, EventArgs<AppointmentItem> e)
        {
            
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            // Note: Outlook no longer raises this event. If you have code that 
            //    must run when Outlook shuts down, see https://go.microsoft.com/fwlink/?LinkId=506785
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
