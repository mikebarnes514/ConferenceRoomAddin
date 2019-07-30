using System;
using System.Windows.Data;

namespace ConferenceRoomAddin.UI.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    class IntegerToDayOfWeekConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int dow = (int)value;
            string str_dow = "";

            switch(dow)
            {
                case 0:
                    str_dow = "Sunday";
                    break;
                case 1:
                    str_dow = "Monday";
                    break;
                case 2:
                    str_dow = "Tuesday";
                    break;
                case 3:
                    str_dow = "Wednesday";
                    break;
                case 4:
                    str_dow = "Thursday";
                    break;
                case 5:
                    str_dow = "Friday";
                    break;
                case 6:
                    str_dow = "Saturday";
                    break;
                default:
                    str_dow = "Unknown";
                    break;
            }

            return str_dow;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str_dow = value.ToString();
            int dow = -1;

            switch(str_dow)
            {
                case "Sunday":
                    dow = 0;
                    break;
                case "Monday":
                    dow = 1;
                    break;
                case "Tuesday":
                    dow = 2;
                    break;
                case "Wednesday":
                    dow = 3;
                    break;
                case "Thursday":
                    dow = 4;
                    break;
                case "Friday":
                    dow = 5;
                    break;
                case "Saturday":
                    dow = 6;
                    break;
                default:
                    dow = -1;
                    break;
            }

            return dow;
        }
    }
}
