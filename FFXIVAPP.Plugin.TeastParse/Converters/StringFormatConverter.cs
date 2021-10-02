using System;
using System.Globalization;
using System.Text;
using Avalonia.Data.Converters;

namespace FFXIVAPP.Plugin.TeastParse.Converters
{
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return parameter;

            var formatter = parameter as string;
            var insideBlock = false;
            var name = new StringBuilder();
            var result = new StringBuilder();
            var t = value.GetType();

            foreach(var c in formatter)
            {
                if (insideBlock)
                {
                    if (c == '}')
                    {
                        insideBlock = false;

                        var prop = t.GetProperty(name.ToString());
                        if (prop != null)
                        {
                            result.Append(prop.GetValue(value)?.ToString()??"");
                        }

                        name.Clear();
                    }
                    else
                    {
                        name.Append(c);
                    }

                    continue;
                }
                else if (c == '{')
                {
                    insideBlock = true;
                    continue;
                }

                result.Append(c);
            }

            return result.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}