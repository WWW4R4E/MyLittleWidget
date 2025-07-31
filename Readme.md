# MyLittleWidget - Win11桌面小组件工具

MyLittleWidget是一款专为Windows 11设计的桌面小组件工具，允许用户在桌面上放置和管理各种实用小组件，提升工作效率和桌面美观度。

## 功能特点

### 🎯 桌面小组件

MyLittleWidget提供了一个轻量级的框架，可以在Windows 11桌面上显示各种实用小组件，包括：

- 番茄钟计时器 (Pomodoro Clock)
- 每日一言 (One Line of Wisdom)
- 更多自定义组件正在开发中...

### 🖥️ 双模式界面

- **桌面小组件模式**: 轻量级的桌面覆盖窗口，只显示小组件
- **主控面板模式**: 完整的应用程序窗口，用于配置和管理小组件

### 🎨 现代化设计

- 基于WinUI 3构建的现代化界面
- 支持深色/浅色主题切换
- 支持个性化定制

### ⚙️ 核心特性

- **自动启动**: 应用程序支持开机自启
- **系统托盘**: 最小化到系统托盘，方便快速访问
- **组件管理**: 可以轻松添加、删除和配置小组件
- **位置记忆**: 小组件位置自动保存
- **主题适配**: 自动适配系统主题

## 技术架构

### 项目结构

```
MyLittleWidget/
├── MyLittleWidget/              # 主应用程序
├── MyLittleWidget.Contracts/    # 组件接口和基类定义
└── Community.Widget/            # 社区组件扩展(计划中)
```

### 使用方法

1. 首次运行会自动进入配置模式(主控面板)
2. 在主控面板中添加和配置需要的小组件
3. 关闭主控面板后，小组件将显示在桌面上
4. 点击系统托盘图标可以重新打开主控面板

## 开发说明

### 添加新的小组件

要创建新的小组件，需要:

1. 继承[WidgetBase](file://c:\Users\123\Desktop\MyLittleWidget\MyLittleWidget.Contracts\WidgetBase.cs#L10-L199)类
2. 实现构造函数和[ConfigureWidget](file://c:\Users\123\Desktop\MyLittleWidget\MyLittleWidget.Contracts\WidgetBase.cs#L58-L60)方法
3. 在[ComponentRegistryService](file://c:\Users\123\Desktop\MyLittleWidget\MyLittleWidget\Services\ComponentRegistryService.cs#L5-L90)中注册组件

### 示例组件代码结构

```csharp
public sealed partial class MyWidget : WidgetBase
{
    public MyWidget(WidgetConfig config, IApplicationSettings settings) 
        : base(config, settings)
    {
        // 初始化组件UI
        InitializeUI();
    }

    protected override void ConfigureWidget()
    {
        base.ConfigureWidget();
        Config.Name = "我的组件";
        Config.UnitWidth = 2;
        Config.UnitHeight = 2;
        Config.WidgetType = this.GetType().FullName;
    }
}
```

## 未来计划

- [ ] 更多实用小组件
- [ ] 社区组件支持
- [ ] 组件间数据共享机制
- [ ] 云端同步配置
- [ ] 更丰富的个性化选项

## 开发路线图

### 已完成功能

| 功能           | 描述                             |
| -------------- | -------------------------------- |
| ✅ 基础框架     | 小组件系统基础架构               |
| ✅ 例子组件     | 番茄钟和每日一言实现             |
| ✅ 双窗口模式   | 主控面板和小组件窗口分离         |
| ✅ 系统托盘支持 | 添加系统托盘图标以切换模式       |
| ✅ 通知功能     | 为组件基类添加发起通知功能       |
| ✅ 组件工具     | 给组件开发增加如FilePicker等工具 |


### 待实现功能

| 功能           | 描述                         | 优先级 | 状态   |
| -------------- | ---------------------------- | ------ | ------ |
| 🔲 设置页面     | 对应用总体进行配置管理       | 高     | 开始   |
| 🔲 组件设置面板 | 为部分小组件提供独立设置界面 | 中     | 开始   |
| 🔲 主题适配     | 支持深色/浅色主题切换        | 中     | 开始   |
| 🔲 组件市场     | 在线下载和安装社区组件       | 中     | 未开始 |
| 🔲 组件间通信   | 组件间数据共享机制           | 低     | 未开始 |
| 🔲 多显示器支持 | 支持多显示器环境             | 低     | 未开始 |
| 🔲 云端同步     | 配置和数据云端同步           | 低     | 未开始 |

### 已知问题

| 问题             | 描述                                  | 优先级 | 状态       |
| ---------------- | ------------------------------------- | ------ | ---------- |
| ⚠️ 主题更新不完整 | WidgetBase中的UpdateTheme方法需要完善 | 中     | 未开始修复 |

### 功能设想

| 设想             | 描述                   | 优先级 | 备注           |
| ---------------- | ---------------------- | ------ | -------------- |
| 💡 天气组件       | 显示实时天气信息       | 高     | 需要API支持    |
| 💡 应用启动组件   | 设置启动应用和图标设置 | 高     | 在折腾图标配置 |
| 💡 音乐播放器组件 | 基础音乐播放控制       | 高     | 需要去折腾smtc |
| 💡 日历组件       | 显示日历和事件提醒     | 高     |                |
| 💡 CPU监控组件    | 显示系统资源使用情况   | 中     |                |
| 💡 便签组件       | 桌面便签和提醒功能     | 中     |                |


