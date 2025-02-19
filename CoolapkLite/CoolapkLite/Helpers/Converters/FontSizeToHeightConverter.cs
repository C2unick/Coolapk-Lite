﻿using System;
using System.Reflection;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace CoolapkLite.Helpers.Converters
{
    public class FontSizeToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            object result = System.Convert.ToDouble(value) * 4 / 3;
            return result.Convert(targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            object result = System.Convert.ToDouble(value) * 3 / 4;
            return result.Convert(targetType);
        }
    }
}
