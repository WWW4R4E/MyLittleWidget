using MyLittleWidget.Models;
using System.Text.Json.Serialization;
namespace MyLittleWidget;

[JsonSerializable(typeof(ApplicationSaveData))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}