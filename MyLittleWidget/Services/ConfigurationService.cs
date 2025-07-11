using MyLittleWidget.ViewModels;
using System.Diagnostics;
using System.Text.Json;

namespace MyLittleWidget.Services
{
  internal class ConfigurationService
  {
    private readonly string _filePath;

    public ConfigurationService()
    {
      string localFolder =ApplicationData.Current.LocalFolder.Path;
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
        return JsonSerializer.Deserialize<ApplicationSaveData>(json);
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"Error loading settings: {ex.Message}");
        return null; 
      }
    }

    public void Save()
    {
      var dataToSave = new ApplicationSaveData
      {
        GlobalSettings = AppSettings.Instance,

        WidgetConfigs = SharedViewModel.Instance.WidgetBases
                                       .Select(widget => widget.Config)
                                       .ToList()
      };

      var options = new JsonSerializerOptions { WriteIndented = true };
      var json = JsonSerializer.Serialize(dataToSave, options);
      File.WriteAllText(_filePath, json);
    }
  }
}
