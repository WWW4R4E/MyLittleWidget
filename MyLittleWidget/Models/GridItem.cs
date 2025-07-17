namespace MyLittleWidget.Models
{
  internal class GridItem
  {
    public bool _isSelected = false;
    public bool _isEnabled = true;
    public Point _position;

    internal static GridItem Create(int i, int j)
    {
      return new GridItem
      {
        _position = new Point(i, j),
        _isSelected = false,
        _isEnabled = true
      };
    }
  }
}