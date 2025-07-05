using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLittleWidget.Utils
{
    internal static class SingleInstanceHelper
    {
        private static Mutex _mutex;
        private const string MutexName = "c51f15b1-e8d6-4e1a-aca5-a0d63b14cc03"; // 请替换为您自己的GUID
        public const string EditorWindowTittle = "My Little Widget Editor";
        public static bool Register()
        {
            _mutex = new Mutex(true, MutexName, out bool isFirstInstance);
            return isFirstInstance;
        }

        public static void ActivateEditorWindow()
        {
            var mainwindow = ((App)Application.Current).window;
            if (mainwindow != null)
            {
                ((App)Application.Current).window.Activate();
            }
            else
            {
                ((App)Application.Current).window = new MainWindow();
                ((App)Application.Current).window.Activate();
            }
        }
        public static void Unregister()
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            _mutex = null;
        }
    }
}
