using ConferenceRoomAddin.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceRoomAddin
{
    public class LogManager
    {
        public static void LogException(Exception ex)
        {
            string company = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCompanyAttribute>().Company;
            string product = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
            string logpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), company, product, "error.log");

            if (!Directory.Exists(Path.GetDirectoryName(logpath)))
                Directory.CreateDirectory(Path.GetDirectoryName(logpath));

            try
            {
                File.AppendAllText(logpath, String.Format("{0}{1} -- {2}", Environment.NewLine, DateTime.Now.ToString("G"), ex.ToString()));
            }
            catch { }
        }

        public static void LogMessage(string msg)
        {
            string company = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCompanyAttribute>().Company;
            string product = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
            string logpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), company, product, "error.log");
            string checkpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), company, product, "enable.log");

            if (!File.Exists(checkpath))
                return;

            if (!Directory.Exists(Path.GetDirectoryName(logpath)))
                Directory.CreateDirectory(Path.GetDirectoryName(logpath));

            try
            {
                File.AppendAllText(logpath, String.Format("{0}{1} -- {2}", Environment.NewLine, DateTime.Now.ToString("G"), msg));
            }
            catch { }
        }
    }
}
