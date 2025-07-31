
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace MyLittleWidget.Contracts.AppShortcut;

public class AppShortcut : WidgetBase
{
  internal WidgetConfig WidgetConfig;
  internal IWidgetToolService WidgetToolService;
  public AppShortcut(WidgetConfig config, IApplicationSettings settings, IWidgetToolService widgetTool) : base(config, settings, widgetTool)
  {
    WidgetConfig = config;
    WidgetToolService = widgetTool ;
    var appShortcutContent = new AppShortcutContent(config, widgetTool);
    if (Content is Border basBorder)
    {
      basBorder.Child = appShortcutContent;
    }

  }
  protected override void ConfigureWidget()
  {
    base.ConfigureWidget();
    Config.Name = "快捷启动";
    Config.UnitWidth = 2;
    Config.UnitHeight = 2;
    Config.WidgetType = GetType().FullName;
    Config.SetExecuteMethod(SettingPageRun);
  }
  private void SettingPageRun()
  {
    var _window = new Window()
    {
      SystemBackdrop = new MicaBackdrop(),
      ExtendsContentIntoTitleBar =true
    };
    var settingsPage = new AppShortcutSettingPage(WidgetConfig, WidgetToolService);
    settingsPage.ConfigurationSaved += OnConfigurationSaved;
    _window.Closed += (sender, args) =>
    {
      settingsPage.ConfigurationSaved -= OnConfigurationSaved;
    };
    _window.Content = settingsPage;
    _window.Activate();
  }

  private void OnConfigurationSaved(WidgetConfig _config)
  {
    WidgetConfig = _config;
    if (this.Content is Border basBorder && basBorder.Child is AppShortcutContent currentContent)
    {
      currentContent.UpdateContent();
    }
  }
}