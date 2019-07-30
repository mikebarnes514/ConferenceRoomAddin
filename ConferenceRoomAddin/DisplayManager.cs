using ConferenceRoomAddin.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ConferenceRoomAddin
{
    public class DisplayManager
    {
        public static ISplashScreen SplashScreen;
        public static ISplashScreen MessageScreen;
        public static ISplashScreen ErrorScreen;
        private static ManualResetEvent resetSplash;
        private static Thread splashThread;

        #region Splash Screen Methods
        public static void ShowSplashScreen()
        {
            resetSplash = new ManualResetEvent(false);
            splashThread = new Thread(ShowSplash);
            splashThread.SetApartmentState(ApartmentState.STA);
            splashThread.IsBackground = true;
            splashThread.Name = "Splash Screen";
            splashThread.Start();
            resetSplash.WaitOne();
        }

        public static void HideSplashScreen()
        {
            SplashScreen.LoadComplete();
        }

        public static void UpdateSplashScreen(string msg)
        {
            SplashScreen.AddMessage(msg);
        }

        private static void ShowSplash()
        {
            SplashWindow wnd = new SplashWindow();

            SplashScreen = wnd;
            wnd.Show();
            resetSplash.Set();
            Dispatcher.Run();
        }
        #endregion

        #region Message Screen Methods
        public static void ShowMessageWindow(string msg)
        {
            resetSplash = new ManualResetEvent(false);
            splashThread = new Thread(ShowMessage);
            splashThread.SetApartmentState(ApartmentState.STA);
            splashThread.IsBackground = true;
            splashThread.Name = "Message Screen";
            splashThread.Start(msg);
            resetSplash.WaitOne();
        }

        public static void HideMessageScreen()
        {
            MessageScreen.LoadComplete();
        }

        public static void UpdateMessageScreen(string msg)
        {
            MessageScreen.AddMessage(msg);
        }

        private static void ShowMessage(object msg)
        {
            MessageWindow wnd = new MessageWindow(msg.ToString());

            MessageScreen = wnd;
            wnd.Show();
            resetSplash.Set();
            Dispatcher.Run();
        }
        #endregion

        #region Error Screen Methods
        public static void ShowErrorWindow(string msg, string detail)
        {
            ErrorWindow wnd = new ErrorWindow(msg);

            wnd.AddMessage(detail);
            wnd.ShowDialog();            
        }
        #endregion

        #region Conflict Screen Methods
        public static bool ShowConflictWindow(List<Data.Entry> conflicts, bool canSkip = false)
        {
            ConflictWindow wnd = new ConflictWindow(conflicts, canSkip);
            bool skip = false;

            if (wnd.ShowDialog() == true)
                skip = true;

            return skip;
        }
        #endregion

        #region Question Screen Methods
        public static bool ShowQuestionWindow(string msg, string sub, string question)
        {
            YesNoWindow wnd = new YesNoWindow(msg, sub, question);
            bool answer = false;

            if (wnd.ShowDialog() == true)
                answer = true;

            return answer;
        }
        #endregion
    }
}
