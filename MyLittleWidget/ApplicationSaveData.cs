using MyLittleWidget.CustomBase;
using MyLittleWidget.ViewModels;

namespace MyLittleWidget
{
  internal class ApplicationSaveData
  {
    public AppSettings GlobalSettings { get; set; }
    public List<WidgetConfig> WidgetConfigs { get; set; }
  }
}
