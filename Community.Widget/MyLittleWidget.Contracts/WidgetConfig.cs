using CommunityToolkit.Mvvm.ComponentModel;

namespace MyLittleWidget.Contracts
{
  public partial class WidgetConfig : ObservableObject
  {
    public int Id { get; init; }
    public string Name { get; set; }

    public int UnitWidth { get; set; } = 2;
    public int UnitHeight { get; set; } = 2;

    [ObservableProperty]
    private double _positionX;

    [ObservableProperty]
    private double _positionY;

    [ObservableProperty]
    private string _widgetType;
  }
}