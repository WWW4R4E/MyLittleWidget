using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using MyLittleWidget.Views;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyLittleWidget
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public Window? window;
        public Window childWindow;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
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



            // 获取桌面分辨率
   //         var screenWidth = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().ScreenWidthInRawPixels;
			//var screenHeight = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().ScreenHeightInRawPixels;

			//// 全屏设置
			//childWindow.AppWindow.MoveAndResize(new RectInt32
   //         {
			//	X = 0,
			//	Y = 0,
			//	Width = (int)screenWidth,
			//	Height = (int)screenHeight
			//});
		}
    }
}
