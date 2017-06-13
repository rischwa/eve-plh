using System;
using System.Runtime.InteropServices;
using System.Text;

namespace EveLocalChatAnalyser.Utilities.Win32
{
    public static class Win32ActiveWindow
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public static string GetActiveWindowTitle()
        {
            const int MAX_CHAR_COUNT = 256;
            var buff = new StringBuilder(MAX_CHAR_COUNT);
            var handle = GetForegroundWindow();

            return GetWindowText(handle, buff, MAX_CHAR_COUNT) > 0 ? buff.ToString() : "";
        }
    }
}