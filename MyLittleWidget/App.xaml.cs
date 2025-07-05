using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using MyLittleWidget.Utils;
using MyLittleWidget.Views;

namespace MyLittleWidget
{
    public partial class App : Application
    {
        public Window? window;
        public Window childWindow;

        public static DispatcherQueue DispatcherQueue { get; private set; }

        private const string AppKey = "A0C5839C-E53D-4A4E-B23A-56B93822C175";
        private AppInstance mainInstance;

        public App()
        {
            InitializeComponent();

            mainInstance = AppInstance.FindOrRegisterForKey(AppKey);
            if (!mainInstance.IsCurrent)
            {
                // 如果不是主实例，异步重定向并退出。
                // 使用 Task.Run 避免阻塞UI线程（尽管此时UI线程还没正式跑循环）
                Task.Run(async () =>
                {
                    var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
                    await mainInstance.RedirectActivationToAsync(activatedArgs);
                }).GetAwaiter().GetResult();

                Environment.Exit(0);
            }
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // 1. **关键**：在这里获取 DispatcherQueue，确保它肯定有效。
            DispatcherQueue = DispatcherQueue.GetForCurrentThread();

            // 2. **关键**：在这里注册激活事件。此时应用环境已准备好。
            mainInstance.Activated += OnAppActivated;

            // 3. 创建常驻窗口
            childWindow = new ChildenWindow();
            // childWindow.Activate(); // 常驻窗口一般不抢焦点

            // 4. 根据设置决定是否显示主窗口
            if (GetShowMainWindow())
            {
                ShowMainWindow();
            }
        }
        private void OnAppActivated(object? sender, AppActivationArguments args)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                ShowMainWindow();
            });
        }
        private static void SetShowMainWindow(bool show)
        {
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values["ShowMainWindow"] = show;
        }
        private void ShowMainWindow()
        {
            window = new MainWindow();
            window.Activate();
            DisplayArea displayArea = DisplayArea.GetFromWindowId(window.AppWindow.Id, DisplayAreaFallback.Primary);

            int targetWidth = window.AppWindow.Size.Width - 300;
            int targetHeight = window.AppWindow.Size.Height;

            // 计算居中位置
            int centerX = displayArea.WorkArea.Width / 2 - targetWidth / 2 + displayArea.WorkArea.X;
            int centerY = displayArea.WorkArea.Height / 2 - targetHeight / 2 + displayArea.WorkArea.Y;

            RectInt32 rect = new RectInt32
            {
                X = centerX,
                Y = centerY,
                Width = targetWidth,
                Height = targetHeight
            };

            window.AppWindow.MoveAndResize(rect, displayArea);
        }
        private static bool GetShowMainWindow()
        {
            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.TryGetValue("ShowMainWindow", out object? value) &&
                value is bool show)
            {
                return show;
            }
            return true;
        }
    }
}
