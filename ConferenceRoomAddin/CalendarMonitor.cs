using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceRoomAddin
{
    class CalendarMonitor
    {
        private Explorer _explorer;
        private List<string> _folderPaths;
        private List<MAPIFolder> _calendarFolders;
        private List<Items> _calendarItems;
        private MAPIFolder _deletedItemsFolder;
        private bool _itemSaved;

        public event EventHandler<EventArgs<AppointmentItem>> AppointmentAdded;
        public event EventHandler<EventArgs<AppointmentItem>> AppointmentModified;
        public event EventHandler<CancelEventArgs<AppointmentItem>> AppointmentDeleting;

        public CalendarMonitor(Explorer anExplorer)
        {
            _folderPaths = new List<string>();
            _calendarFolders = new List<MAPIFolder>();
            _calendarItems = new List<Items>();
            _itemSaved = false;
            _explorer = anExplorer;
            _explorer.BeforeFolderSwitch += new ExplorerEvents_10_BeforeFolderSwitchEventHandler(Explorer_BeforeFolderSwitch);

            NameSpace session = _explorer.Session;
            try
            {
                _deletedItemsFolder = session.GetDefaultFolder(OlDefaultFolders.olFolderDeletedItems);
                HookupDefaultCalendarEvents(session);
            }
            finally
            {
                Marshal.ReleaseComObject(session);
                session = null;
            }
        }

        private void HookupDefaultCalendarEvents(NameSpace aSession)
        {
            MAPIFolder folder = aSession.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
            
            if (folder != null)
            {
                try
                {
                    HookupCalendarEvents(folder);
                }
                finally
                {
                }
            }
        }

        private void Explorer_BeforeFolderSwitch(object aNewFolder, ref bool Cancel)
        {
            MAPIFolder folder = (aNewFolder as MAPIFolder);
            CalendarModule modCalendar = (CalendarModule)Globals.ThisAddIn.Application.ActiveExplorer().NavigationPane.Modules.GetNavigationModule(OlNavigationModuleType.olModuleCalendar);

            if (folder != null)
            {
                try
                {
                    if (folder.DefaultItemType == OlItemType.olAppointmentItem)
                    {
                        HookupCalendarEvents(folder);
                    }
                }
                finally
                {
                }
            }
        }

        private void HookupCalendarEvents(MAPIFolder aCalendarFolder)
        {
            //BackgroundWorker worker = new BackgroundWorker();

            if (aCalendarFolder.DefaultItemType != OlItemType.olAppointmentItem)
            {
                throw new ArgumentException("The MAPIFolder must use AppointmentItems as the default type.");
            }

            if ((_folderPaths.Contains(aCalendarFolder.FolderPath) == false) /*&& (IsUsersCalendar(aCalendarFolder))*/)
            {
                //worker.DoWork += Worker_DoWork;
                //worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                //worker.RunWorkerAsync(aCalendarFolder);
                Task.Factory.StartNew(() =>
                {
                    Items items = aCalendarFolder.Items;

                    _folderPaths.Add(aCalendarFolder.FolderPath);
                    _calendarFolders.Add(aCalendarFolder);
                    _calendarItems.Add(items);
                    ((MAPIFolderEvents_12_Event)aCalendarFolder).BeforeItemMove += new MAPIFolderEvents_12_BeforeItemMoveEventHandler(Calendar_BeforeItemMove);
                    items.ItemChange += new ItemsEvents_ItemChangeEventHandler(CalendarItems_ItemChange);
                    items.ItemAdd += new ItemsEvents_ItemAddEventHandler(CalendarItems_ItemAdd);
                    foreach (AppointmentItem item in items)
                        item.AfterWrite += Item_AfterWrite;
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);
            }
        }

        

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;

            worker.DoWork -= Worker_DoWork;
            worker.RunWorkerCompleted -= Worker_RunWorkerCompleted;
            worker.Dispose();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            MAPIFolder aCalendarFolder = (MAPIFolder)e.Argument;
            Items items = aCalendarFolder.Items;
            
            _folderPaths.Add(aCalendarFolder.FolderPath);
            _calendarFolders.Add(aCalendarFolder);
            _calendarItems.Add(items);
            ((MAPIFolderEvents_12_Event)aCalendarFolder).BeforeItemMove += new MAPIFolderEvents_12_BeforeItemMoveEventHandler(Calendar_BeforeItemMove);
            items.ItemChange += new ItemsEvents_ItemChangeEventHandler(CalendarItems_ItemChange);
            items.ItemAdd += new ItemsEvents_ItemAddEventHandler(CalendarItems_ItemAdd);
            foreach (AppointmentItem item in items)
            {
                item.AfterWrite += Item_AfterWrite;
            }
        }

        private void Item_AfterWrite()
        {
            _itemSaved = true;
        }

        private void CalendarItems_ItemAdd(object anItem)
        {
            AppointmentItem appointment = (anItem as AppointmentItem);
            if (appointment != null)
            {
                try
                {
                    if (this.AppointmentAdded != null)
                    {
                        this.AppointmentAdded(this, new EventArgs<AppointmentItem>(appointment));
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(appointment);
                    appointment = null;
                }
            }
        }

        private void CalendarItems_ItemChange(object anItem)
        {
            AppointmentItem appointment = (anItem as AppointmentItem);
            if ((appointment != null) && _itemSaved)
            {
                try
                {
                    if (this.AppointmentModified != null)
                    {
                        this.AppointmentModified(this, new EventArgs<AppointmentItem>(appointment));
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(appointment);
                    appointment = null;
                    _itemSaved = false;
                }
            }
        }

        private void Calendar_BeforeItemMove(object anItem, MAPIFolder aMoveToFolder, ref bool Cancel)
        {
            if ((aMoveToFolder == null) || (IsDeletedItemsFolder(aMoveToFolder)))
            {
                AppointmentItem appointment = (anItem as AppointmentItem);
                if (appointment != null)
                {
                    try
                    {
                        if (this.AppointmentDeleting != null)
                        {
                            //
                            // Listeners to the AppointmentDeleting event can cancel the move operation if moving
                            // to the deleted items folder.
                            //
                            CancelEventArgs<AppointmentItem> args = new CancelEventArgs<AppointmentItem>(appointment);
                            this.AppointmentDeleting(this, args);
                            Cancel = args.Cancel;
                        }
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(appointment);
                        appointment = null;
                    }
                }
            }
        }

        private bool IsUsersCalendar(MAPIFolder aFolder)
        {
            //
            // This is based purely on my observations so far - a better way?
            //
            return (aFolder.Store != null);
        }

        private bool IsDeletedItemsFolder(MAPIFolder aFolder)
        {
            return (aFolder.EntryID == _deletedItemsFolder.EntryID);
        }
    }

    public class EventArgs<T> : EventArgs
    {
        private T _value;

        public EventArgs(T aValue)
        {
            _value = aValue;
        }

        public T Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }

    public class CancelEventArgs<T> : EventArgs<T>
    {
        private bool _cancel;

        public CancelEventArgs(T aValue)
            : base(aValue)
        {
        }

        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }
    }
}
