using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TouchpadGestures_Advanced.Converters
{
    public class BorderBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var val = (KeyValuePair<int, int>)values[0];
                var columnIndex = (int)values[1];
                var rowIndex = (int)values[2];

                if (val.Equals(new KeyValuePair<int, int>(columnIndex, rowIndex)))
                {
                    return Brushes.White;
                }
                else
                {
                    return Brushes.Transparent;
                }
            }
            catch (Exception)
            {
                return Brushes.Transparent;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
