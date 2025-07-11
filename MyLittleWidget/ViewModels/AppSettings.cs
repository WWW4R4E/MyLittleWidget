﻿using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace MyLittleWidget.ViewModels;

public partial class AppSettings : ObservableObject
{
  public static AppSettings Instance { get; } = new();
  [JsonConstructor]
  private AppSettings(){ }

  [ObservableProperty]
  private double _baseUnit = 100.0;

  [ObservableProperty]
  private bool _isDarkTheme = true;
}