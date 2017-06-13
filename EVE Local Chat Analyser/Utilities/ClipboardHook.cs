using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EveLocalChatAnalyser.Utilities.Win32;
using Clipboard = System.Windows.Clipboard;

namespace EveLocalChatAnalyser.Utilities
{
    public interface IClipboardHook
    {
        event Action<object, string> NewClipboardAsciiData;
    }

    public class ClipboardHook : IClipboardHook
    {
        private readonly IHwndSource _targetWindow;
        private IntPtr _nextClipboardViewer;

        public ClipboardHook(IHwndSource targetWindow)
        {
            _targetWindow = targetWindow;
            SetHook(targetWindow);
        }

        [DllImport("User32.dll")]
        protected static extern int SetClipboardViewer(int hWndNewViewer);


        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);


        public event Action<object, string> NewClipboardAsciiData;


        private void OnNewClipboardAsciiData(string asciiData)
        {
            var handler = NewClipboardAsciiData;
            if (handler != null) handler(this, asciiData);
        }


        private void SetHook(IHwndSource targetWindow)
        {
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;
            targetWindow[WM_DRAWCLIPBOARD] += OnDrawClipboardMessageReceived;
            targetWindow[WM_CHANGECBCHAIN] += OnChangeCbChainMessageReceived;
            _nextClipboardViewer = (IntPtr)SetClipboardViewer(targetWindow.Handle.ToInt32());
        }

        private void OnChangeCbChainMessageReceived(Message msg)
        {
            if (msg.WParam == _nextClipboardViewer)
            {
                _nextClipboardViewer = msg.LParam;
            }
            else
            {
                SendMessage(_nextClipboardViewer, msg.Msg, msg.WParam, msg.LParam);
            }
        }

        private void OnDrawClipboardMessageReceived(Message msg)
        {
            HandleNewClipboardData();
            SendMessage(_nextClipboardViewer, msg.Msg, msg.WParam, msg.LParam);
        }


        private void HandleNewClipboardData()
        {
            if (Clipboard.ContainsText())
            {
                var asciiData = Clipboard.GetText();
                OnNewClipboardAsciiData(asciiData);
            }
        }

        ~ClipboardHook()
        {
            ChangeClipboardChain(_targetWindow.Handle, _nextClipboardViewer);
        }
    }
}