using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using MyLittleWidget.Utils;
using MyLittleWidget.Views;

namespace MyLittleWidget
{
    public partial class App : Application
    {
        public Window? window;
        public Window childWindow;

        public App()
        {
            InitializeComponent();
        }
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (!SingleInstanceHelper.Register())
            {
                SingleInstanceHelper.ActivateEditorWindow();
                Environment.Exit(0);
                return;
            }
            if (GetShowMainWindow())
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
            childWindow = new ChildenWindow();

        }
        private static void SetShowMainWindow(bool show)
        {
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values["ShowMainWindow"] = show;
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
