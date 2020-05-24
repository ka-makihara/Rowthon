using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Data;  //for IValueConverter
using System.Globalization; //for CultureInfo

namespace RowThon
{
    [ValueConversion(typeof(UInt32),typeof(string))]
    public class IoValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            UInt32 data = (UInt32)value;

            return (data.ToString("X8"));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = (string)value;

            return( UInt32.Parse(str, NumberStyles.HexNumber) );
        }
    }
}
