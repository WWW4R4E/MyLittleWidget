using System.Diagnostics;
using System.Text.Json;
using MyLittleWidget.Models;
using MyLittleWidget.ViewModels;

namespace MyLittleWidget.Services
{
  internal class ConfigurationService
  {
    private readonly string _filePath;

    public ConfigurationService()
    {
      string localFolder = ApplicationData.Current.LocalFolder.Path;
      // string localFolder = "C:/123/Desktop/";
      _filePath = Path.Combine(localFolder, "app_settings.json");
    }
    public ApplicationSaveData? Load()
    {
      if (!File.Exists(_filePath))
      {
        return null;
      }

      try
      {
        var json = File.ReadAllText(_filePath);
        var result = JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.ApplicationSaveData);
        return result;
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"Error loading settings: {ex.Message}");
        return null;
      }
    }
    public void Save()
    {
      {
        var dataToSave = new ApplicationSaveData
        {
          GlobalSettings = AppSettings.Instance,

          WidgetConfigs = SharedViewModel.Instance.WidgetList
            .Select(widget => widget.Config)
            .ToList()
        };
        var json = JsonSerializer.Serialize(dataToSave, AppJsonSerializerContext.Default.ApplicationSaveData);
        File.WriteAllText(_filePath, json);
      }
    }
  }
}
