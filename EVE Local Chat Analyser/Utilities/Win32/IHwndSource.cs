using System;

namespace EveLocalChatAnalyser.Utilities.Win32
{
    public interface IHwndSource
    {
        IntPtr Handle { get; }
        MessageReceived this[int msgType] { get; set; }
    }
}