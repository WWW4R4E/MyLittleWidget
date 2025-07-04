using MyLittleWidget.Utils.BackDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MyLittleWidget.Utils
{
    internal partial class WindowsSystemDispatcherQueueHelper
    {
        public IntPtr m_dispatcherQueueController = IntPtr.Zero;
        public void EnsureWindowsSystemDispatcherQueueController()
        {
            if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
            {
                // one already exists, so we'll just use it.
                return;
            }

            if (m_dispatcherQueueController == IntPtr.Zero)
            {
                NativeValues.DispatcherQueueOptions options;
                options.dwSize = Unsafe.SizeOf<NativeValues.DispatcherQueueOptions>();
                options.threadType = 2;    // DQTYPE_THREAD_CURRENT
                options.apartmentType = 2; // DQTAT_COM_STA

                unsafe
                {
                    IntPtr dispatcherQueueController;
                    NativeMethods.CreateDispatcherQueueController(options, &dispatcherQueueController);
                    m_dispatcherQueueController = dispatcherQueueController;
                }
            }
        }
    }
}
