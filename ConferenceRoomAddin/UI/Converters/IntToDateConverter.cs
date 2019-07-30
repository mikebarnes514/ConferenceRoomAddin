using ConferenceRoomAddin.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ConferenceRoomAddin.UI.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    class IntToDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime dateVal = Database.UnixTimeStampToDateTime(System.Convert.ToInt32(value));

            return dateVal.ToString("dddd, MMMM dd");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
