using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace EveLocalChatAnalyser.Utilities
{
    public static class WindowExtensions
    {
        private static void SanitizeWindowPosition(Window window)
        {
            if (window.Top < SystemParameters.VirtualScreenTop)
            {
                window.Top = SystemParameters.VirtualScreenTop;
            }
            else
            {
                if (window.Top > SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight)
                {
                    window.Top = 0;
                }
            }
            if (window.Left < SystemParameters.VirtualScreenLeft)
            {
                window.Left = SystemParameters.VirtualScreenLeft;
            }
            else
            {
                if (window.Left > SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth)
                {
                    window.Left = 0;
                }
            }
        }

        private static void SanitizeWindowSize(Window window)
        {
            if (window.Width > SystemParameters.VirtualScreenWidth)
            {
                window.Width = SystemParameters.VirtualScreenWidth;
            }
            if (window.Height > SystemParameters.VirtualScreenHeight)
            {
                window.Height = SystemParameters.VirtualScreenHeight;
            }
        }

        public static void SanitizeWindowSizeAndPosition(this Window window)
        {
            SanitizeWindowSize(window);
            SanitizeWindowPosition(window);
        }
        private static Screen GetScreen(this Window window)
        {
            return Screen.FromHandle(new WindowInteropHelper(window).Handle);
        }

        static Point RealPixelsToWpf(this Window w, Point p)
        {
            var t = PresentationSource.FromVisual(w).CompositionTarget.TransformFromDevice;
            return t.Transform(p);
        }

        public static void PlaceNearCursor(this Window window)
        {
            W32Point pt = new W32Point();
            if (!GetCursorPos(ref pt))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            // 0x00000002: return nearest monitor if pt is not contained in any monitor.
            IntPtr monHandle = MonitorFromPoint(pt, 0x00000002);
            W32MonitorInfo monInfo = new W32MonitorInfo();
            monInfo.Size = Marshal.SizeOf(typeof(W32MonitorInfo));

            if (!GetMonitorInfo(monHandle, ref monInfo))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            W32Rect workArea = monInfo.WorkArea;

            var wpfScreenTopLeft = window.RealPixelsToWpf(new Point(workArea.Left, workArea.Top));
            var wpfScreenBottomRight = window.RealPixelsToWpf(new Point(workArea.Right, workArea.Bottom));

            var wpfPt = RealPixelsToWpf(window, new Point(pt.X, pt.Y));

            const int OFFSET = 10;

            var xPosRight = wpfPt.X  + window.Width + OFFSET;
            if (xPosRight > wpfScreenBottomRight.X)
            {
                var xPosLeft = wpfPt.X - window.Width - OFFSET;
                if (xPosLeft < wpfScreenTopLeft.X)
                {
                    window.Left = wpfScreenTopLeft.X + OFFSET;
                }
                else
                {
                    window.Left = xPosLeft;
                }
            }
            else
            {
                window.Left = wpfPt.X + OFFSET;
            }

            var yPosBottom = wpfPt.Y + window.Height + OFFSET;
            if (yPosBottom > wpfScreenBottomRight.Y)
            {
                var yPosTop = wpfPt.Y - window.Height - OFFSET;
                if (yPosTop < wpfScreenTopLeft.Y)
                {
                    window.Top = wpfScreenTopLeft.Y + OFFSET;
                }
                else
                {
                    window.Top = yPosTop;
                }
            }
            else
            {
                window.Top = wpfPt.Y + OFFSET;
            }

        }



        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(ref W32Point pt);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref W32MonitorInfo lpmi);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(W32Point pt, uint dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        public struct W32Point
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct W32MonitorInfo
        {
            public int Size;
            public W32Rect Monitor;
            public W32Rect WorkArea;
            public uint Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct W32Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

    }
}
