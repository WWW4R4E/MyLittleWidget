using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLittleWidget.Utils.BackDrop;

public sealed partial class WindowMessageEventArgs : EventArgs
{
    internal WindowMessageEventArgs(IntPtr hwnd, uint messageId, nuint wParam, nint lParam)
    {
        Message = new Message(hwnd, messageId, wParam, lParam);
    }

    public nint Result { get; set; }

    public bool Handled { get; set; }

    public Message Message { get; }

    public uint MessageType => Message.MessageId;
}
