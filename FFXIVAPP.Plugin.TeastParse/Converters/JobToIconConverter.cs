using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FFXIVAPP.ResourceFiles;

namespace FFXIVAPP.Plugin.TeastParse.Converters
{
    public class JobToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Game.GetIconByName(value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}