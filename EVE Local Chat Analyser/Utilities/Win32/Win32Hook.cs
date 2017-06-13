using System;
using System.Runtime.InteropServices;

namespace EveLocalChatAnalyser.Utilities.Win32
{
    internal class Win32Hook : IDisposable
    {
        public delegate void WinEventDelegate(
            IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread,
            uint dwmsEventTime);

        public const uint WINEVENT_OUTOFCONTEXT = 0;
        public const uint EVENT_SYSTEM_FOREGROUND = 3;
        private IntPtr _hook;
        public WinEventDelegate Event;
        private readonly WinEventDelegate _dele;

        public Win32Hook(uint eventMin, uint eventMax, uint flags)
        {
            _dele = OnEvent;
            _hook = SetWinEventHook(eventMin, eventMax, IntPtr.Zero, _dele, 0, 0, flags);
        }
        
        public void Dispose()
        {
            if (_hook == IntPtr.Zero)
            {
                return;
            }

            UnhookWindowsHookEx(_hook);
            Event = null;
            _hook = IntPtr.Zero;
        }

        ~Win32Hook()
        {
            Dispose();
        }

        private void OnEvent(IntPtr hwineventhook, uint eventtype, IntPtr hwnd, int idobject, int idchild,
                             uint dweventthread, uint dwmseventtime)
        {
            if (Event != null)
            {
                Event(hwineventhook, eventtype, hwnd, idobject, idchild, dweventthread, dwmseventtime);
            }
        }
        
        [DllImport("user32.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern bool UnhookWindowsHookEx(IntPtr idHook);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
                                                     WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread,
                                                     uint dwFlags);
    }
}