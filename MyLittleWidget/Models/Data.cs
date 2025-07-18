namespace MyLittleWidget.Models;

public class ApplicationSaveData
{
  public AppSettingsData GlobalSettings { get; set; }

  public List<WidgetConfigData> WidgetConfigs { get; set; }


}
public record AppSettingsData(double BaseUnit, bool IsDarkTheme);

public record WidgetConfigData(
  int Id,
  string Name,
  int UnitWidth,
  int UnitHeight,
  double PositionX,
  double PositionY,
  string WidgetType
);