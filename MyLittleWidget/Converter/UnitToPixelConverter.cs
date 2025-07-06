using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLittleWidget.Converter
{
    internal class UnitToPixelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int unitValue && parameter is string unitSizeString && double.TryParse(unitSizeString, out double unitSize))
            {
                return unitValue * unitSize;
            }
            return 0; 
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
