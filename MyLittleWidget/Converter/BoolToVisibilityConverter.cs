﻿using Microsoft.UI.Xaml.Data;

namespace MyLittleWidget.Converter
{
  public class BoolToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      if (value is bool boolValue)
      {
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
      }
      return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }
  }
}
