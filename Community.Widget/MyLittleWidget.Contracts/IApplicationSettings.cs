using System.ComponentModel;

namespace MyLittleWidget.Contracts
{
  public interface IApplicationSettings : INotifyPropertyChanged
  {
    // WidgetBase 需要知道 BaseUnit
    double BaseUnit { get; }

    // WidgetBase 需要知道主题
    bool IsDarkTheme { get; }
  }
}
