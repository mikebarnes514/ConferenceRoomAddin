using ConferenceRoomAddin.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ConferenceRoomAddin.UI.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    class RelativeToURLConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return "";

            return Settings.Default.MRBSWebServer + value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString().Replace(Settings.Default.MRBSWebServer, "");
        }
    }
}
