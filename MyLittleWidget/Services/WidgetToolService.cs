using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using MyLittleWidget.Contracts;
using MyLittleWidget.Models;
using MyLittleWidget.ViewModels;
using System.Text.Json;
using Windows.Storage.Pickers;

namespace MyLittleWidget.Services;

public class WidgetToolService : IWidgetToolService
{
  private readonly IntPtr _windowHandle;

  public WidgetToolService(IntPtr windowHandle)
  {
    _windowHandle = windowHandle;
  }



  public async Task<object?> PickFileOrFolderAsync(bool allowMultiple = false, params string[] fileTypeFilters)
  {
    // 处理空过滤器
    if (fileTypeFilters == null || fileTypeFilters.Length == 0)
    {
      fileTypeFilters = new[] { "*" }; // 默认允许所有文件
    }

    // 检查是否为文件夹选择
    bool isFolderPicker = fileTypeFilters.Length == 1 && fileTypeFilters[0].Equals("folder", StringComparison.OrdinalIgnoreCase);

    if (isFolderPicker)
    {
      // 文件夹选择器
      var folderPicker = new FolderPicker
      {
        SuggestedStartLocation = PickerLocationId.Desktop
      };
      folderPicker.FileTypeFilter.Add("*"); // FolderPicker 需要至少一个过滤器

      InitializeWithWindow.Initialize(folderPicker, _windowHandle);

      // 文件夹选择器不支持多选，始终返回单个文件夹
      var folder = await folderPicker.PickSingleFolderAsync();
      return folder; 
    }
    else
    {
      // 文件选择器
      var filePicker = new FileOpenPicker
      {
        SuggestedStartLocation = PickerLocationId.Desktop
      };

      // 设置文件类型过滤器
      foreach (var filter in fileTypeFilters)
      {
        filePicker.FileTypeFilter.Add(filter);
      }

      InitializeWithWindow.Initialize(filePicker, _windowHandle);

      if (allowMultiple)
      {
        // 多选文件
        IReadOnlyList<StorageFile> files = await filePicker.PickMultipleFilesAsync();
        return files?.ToList() ?? new List<StorageFile>(); // 返回文件列表，空列表作为默认值
      }
      else
      {
        // 单选文件
        var file = await filePicker.PickSingleFileAsync();
        return file; // 返回单个 StorageFile 或 null
      }
    }
  }



  public async Task ShowNotificationAsync(string title, string message)
  {
    var notification = new AppNotificationBuilder()
        .AddText(title)
        .AddText(message)
        .BuildNotification();

    AppNotificationManager.Default.Show(notification);
    await Task.CompletedTask;
  }


  public async Task ShowInteractiveNotificationAsync(
      string title,
      string message,
      string? comboBoxId = null,
      (string Id, string DisplayText)[]? comboBoxItems = null,
      string? defaultComboBoxSelection = null,
      string? textBoxId = null,
      string? textBoxPlaceholder = null,
      string? buttonLabel = null,
      (string Key, string Value)? buttonAction = null)
  {
    var builder = new AppNotificationBuilder()
        .AddText(title)
        .AddText(message);

    // 添加 ComboBox（如果提供）
    if (!string.IsNullOrEmpty(comboBoxId) && comboBoxItems != null && comboBoxItems.Length > 0)
    {
      var comboBox = new AppNotificationComboBox(comboBoxId);
      foreach (var item in comboBoxItems)
      {
        comboBox.AddItem(item.Id, item.DisplayText);
      }
      if (!string.IsNullOrEmpty(defaultComboBoxSelection))
      {
        comboBox.SetSelectedItem(defaultComboBoxSelection);
      }
      builder.AddComboBox(comboBox);
    }

    // 添加 TextBox（如果提供）
    if (!string.IsNullOrEmpty(textBoxId))
    {
      builder.AddTextBox(textBoxId, textBoxPlaceholder ?? string.Empty, string.Empty);
    }

    // 添加 Button（如果提供）
    if (!string.IsNullOrEmpty(buttonLabel))
    {
      var button = new AppNotificationButton(buttonLabel);
      if (buttonAction.HasValue)
      {
        button.AddArgument(buttonAction.Value.Key, buttonAction.Value.Value);
      }
      builder.AddButton(button);
    }

    var notification = builder.BuildNotification();
    AppNotificationManager.Default.Show(notification);
    await Task.CompletedTask; 
}

  public string SaveWidgetFileAsync(string widgetType, string fileName, byte[] content)
  {
    string localFolder = ApplicationData.Current.LocalFolder.Path;
    var dirPath = Path.Combine(localFolder, widgetType);
    var filePath = Path.Combine(dirPath, fileName);

    // 确保目标文件夹存在
    if (!Directory.Exists(dirPath))
    {
      Directory.CreateDirectory(dirPath);
    }

    System.Diagnostics.Debug.WriteLine(filePath);
    File.WriteAllBytes(filePath, content);
    return filePath;
  }

  public void SaveWidegtDataAsync()
  {
   var configurationService = new ConfigurationService();
    configurationService.Save();
  }
}