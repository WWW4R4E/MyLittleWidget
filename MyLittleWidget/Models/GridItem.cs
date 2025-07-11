using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLittleWidget.Models
{
  internal class GridItem
  {
    public bool _isSelected = false;
    public bool _isEnabled = true;
    public Windows.Foundation.Point _position;

    internal static GridItem Create(int i, int j)
    {
      return new GridItem
      {
        _position = new Windows.Foundation.Point(i, j),
        _isSelected = false,
        _isEnabled = true
      };
    }
  }
}