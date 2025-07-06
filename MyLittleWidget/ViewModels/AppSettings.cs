using CommunityToolkit.Mvvm.ComponentModel;
namespace MyLittleWidget.ViewModels;
public partial class AppSettings : ObservableObject
{
    public static AppSettings Instance { get; } = new();

    private AppSettings() { }
    [ObservableProperty]
    private double _baseUnit = 100.0;

    [ObservableProperty]
    private bool _isDarkTheme = true;
}