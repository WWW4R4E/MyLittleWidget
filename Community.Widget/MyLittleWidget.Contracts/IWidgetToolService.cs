using System.Threading.Tasks;

namespace MyLittleWidget.Contracts;

public interface IWidgetToolService
{
  /// <summary>
  /// 选择文件或文件夹，支持单选或多选。
  /// </summary>
  /// <param name="allowMultiple">是否允许多选，默认为 false（单选）。</param>
  /// <param name="fileTypeFilters">文件类型过滤器，空表示所有文件，包含 "folder" 表示选择文件夹。</param>
  /// <returns>单选时返回单个 StorageFile 或 StorageFolder，多选时返回 StorageFile 列表，文件夹选择时可能返回 null。</returns>
  Task<object?> PickFileOrFolderAsync(bool allowMultiple = false, params string[] fileTypeFilters);

  /// <summary>
  /// 显示简单的文本通知。
  /// </summary>
  /// <param name="title">通知标题</param>
  /// <param name="message">通知消息</param>
  /// <returns>Task</returns>
  Task ShowNotificationAsync(string title, string message);

  /// <summary>
  /// 显示交互式通知，支持 ComboBox、TextBox 和 Button。
  /// </summary>
  /// <param name="title">通知标题</param>
  /// <param name="message">通知消息</param>
  /// <param name="comboBoxId">ComboBox 的 ID（可选）</param>
  /// <param name="comboBoxItems">ComboBox 的选项，键值对（ID, 显示文本）（可选）</param>
  /// <param name="defaultComboBoxSelection">ComboBox 默认选中项的 ID（可选）</param>
  /// <param name="textBoxId">TextBox 的 ID（可选）</param>
  /// <param name="textBoxPlaceholder">TextBox 的占位符（可选）</param>
  /// <param name="buttonLabel">按钮的标签（可选）</param>
  /// <param name="buttonAction">按钮的动作键值对（可选）</param>
  /// <returns>Task</returns>
  Task ShowInteractiveNotificationAsync(
    string title,
    string message,
    string? comboBoxId ,
    (string Id, string DisplayText)[]? comboBoxItems,
    string? defaultComboBoxSelection,
    string? textBoxId ,
    string? textBoxPlaceholder ,
    string? buttonLabel,
    (string Key, string Value)? buttonAction);
}