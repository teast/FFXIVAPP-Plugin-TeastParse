using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace FFXIVAPP.Plugin.TeastParse.Converters
{
    public class ActionToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ResourceReader.GetActionIcon(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}