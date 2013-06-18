using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;

namespace AanwezighedenSite.Converter
{
    public class ColorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!string.IsNullOrEmpty((string)value))
            {
                var rawValue = (string)value;

                switch (rawValue.ToUpper())
                {
                    case "AANWEZIG":
                    case "KEEPERSTRAINING":
                        return new SolidColorBrush(Colors.Green);
                    case "VERWITTIGD":
                    case "TE LAAT":
                        return new SolidColorBrush(Colors.Orange);
                    case "NIET VERWITTIGD":
                        return new SolidColorBrush(Colors.Red);
                }        

                //if (rawValue == "Aanwezig" || (rawValue == "Keeperstraining"))
                //{
                //    return new SolidColorBrush(Colors.Green);
                //}
            }

            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        #endregion
    }
}
