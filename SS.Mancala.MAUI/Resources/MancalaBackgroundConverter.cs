using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace SS.Mancala.MAUI
{
    public class MancalaBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isMancala = (bool)value;
            return isMancala ? Colors.Gold : Colors.LightYellow;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
