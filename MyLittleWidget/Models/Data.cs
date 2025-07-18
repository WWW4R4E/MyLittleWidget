using MyLittleWidget.Contracts;
using MyLittleWidget.ViewModels;

namespace MyLittleWidget.Models;
public class ApplicationSaveData
{
  public AppSettings GlobalSettings { get; set; }

  public List<WidgetConfig> WidgetConfigs { get; set; }
}