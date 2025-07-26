using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace MyLittleWidget.ViewModels;

public partial class AppSettings : ObservableObject, Contracts.IApplicationSettings
{
  public static AppSettings Instance { get; } = new();
  [JsonConstructor]
  public AppSettings(){ }

  [ObservableProperty]
  private double _baseUnit = 50.0;

  [ObservableProperty]
  private bool _isDarkTheme = true;
}