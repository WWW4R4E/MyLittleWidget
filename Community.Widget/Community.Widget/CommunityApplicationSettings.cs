using MyLittleWidget.Contracts;
using System.ComponentModel;

namespace Community.Widget;

public class CommunityApplicationSettings : IApplicationSettings
{
  public double BaseUnit => 50;

  public bool IsDarkTheme => false;

  public event PropertyChangedEventHandler? PropertyChanged;
}