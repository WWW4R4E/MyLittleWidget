using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;


namespace MyLittleWidget.Converter
{
  internal class BoolToColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      if (value is bool isSelected && isSelected)
      {
        return new SolidColorBrush(Colors.Gray);
      }
      return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }
  }
}