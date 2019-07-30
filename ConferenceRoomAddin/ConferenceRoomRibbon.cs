using ConferenceRoomAddin.Data;
using ConferenceRoomAddin.UI.Windows;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Office.Tools.Ribbon;
using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace ConferenceRoomAddin
{
    public partial class ConferenceRoomRibbon
    {
        private bool _processWrite = true;

        private void ConferenceRoomRibbon_Load(object sender, RibbonUIEventArgs e)
        {
            LogManager.LogMessage("Loading ribbon");
            Inspector inspector = (sender as OfficeRibbon).Context as Inspector;
            AppointmentItem appt = inspector.CurrentItem as AppointmentItem;

            ReserveButton.Visible = (appt.UserProperties.Find("MJ-MRBS-ID") == null);
            ModifyButton.Visible = (appt.UserProperties.Find("MJ-MRBS-ID") != null);
            appt.Write += Appt_Write;
            appt.BeforeDelete += Appt_BeforeDelete;
            LogManager.LogMessage("Ribbon complete");
        }

        private void Appt_BeforeDelete(object Item, ref bool Cancel)
        {
            AppointmentItem appt = Item as AppointmentItem;
            var creator = appt.PropertyAccessor.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x0C1A001E");

            if ((appt.Organizer != Globals.ThisAddIn.Application.Session.CurrentUser.Name) && (creator != Globals.ThisAddIn.Application.Session.CurrentUser.Name))
                return;

            if (appt.UserProperties.Find("MJ-MRBS-ID") != null)
                Cancel = !OutlookManager.RemoveReservation(appt);
        }

        private void Appt_Write(ref bool Cancel)
        {
            Inspector inspector = Globals.ThisAddIn.Application.ActiveInspector();
            AppointmentItem appt = inspector.CurrentItem as AppointmentItem;
            var creator = appt.PropertyAccessor.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x0C1A001E");

            if ((appt.Organizer != Globals.ThisAddIn.Application.Session.CurrentUser.Name) && (creator != Globals.ThisAddIn.Application.Session.CurrentUser.Name))
                return;
            if (!_processWrite)
                return;

            if (appt.UserProperties.Find("MJ-MRBS-ID") != null)
                OutlookManager.SaveChangesToReservation(appt);
        }

        private void ReserveButton_Click(object sender, RibbonControlEventArgs e)
        {
            Inspector inspector = (sender as RibbonButton).Ribbon.Context as Inspector;
            AppointmentItem appt = inspector.CurrentItem as AppointmentItem;

            _processWrite = false;
            if(OutlookManager.AddReservation(appt))
            {
                ReserveButton.Visible = false;
                ModifyButton.Visible = true;
            }
            _processWrite = true;
        }

        private void ModifyButton_Click(object sender, RibbonControlEventArgs e)
        {
            Inspector inspector = (sender as RibbonButton).Ribbon.Context as Inspector;
            AppointmentItem appt = inspector.CurrentItem as AppointmentItem;
            var creator = appt.PropertyAccessor.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x0C1A001E");

            if ((appt.Organizer != inspector.Session.CurrentUser.Name) && (creator != inspector.Session.CurrentUser.Name))
            {
                DisplayManager.ShowErrorWindow("Modifying this resvervation is not allowed.", "Only the meeting organizer can modify the reservation in MRBS.");
                return;
            }

            _processWrite = false;
            OutlookManager.ModifyReservation(appt);
            _processWrite = true;
            ConferenceRoomRibbon_Load((sender as RibbonButton).Ribbon, null);
        }

        private void ConferenceRoomRibbon_Close(object sender, EventArgs e)
        {
            LogManager.LogMessage("Closing Ribbon");
        }
    }
}
