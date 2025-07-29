using MyLittleWidget.Contracts;
using MyLittleWidget.Models;
using MyLittleWidget.ViewModels;
using System.Text.Json.Serialization;
namespace MyLittleWidget;

[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(WidgetConfig))]
[JsonSerializable(typeof(ApplicationSaveData))]

internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}