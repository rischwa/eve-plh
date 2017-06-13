using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace EveLocalChatAnalyser.Themes
{
    public static class WindowExtension
    {
        public static void AddResizeHook(this Window window)
        {
            var mWindowHandle = (new WindowInteropHelper(window)).Handle;
            HwndSource.FromHwnd(mWindowHandle)
                .AddHook(WindowProc);
        }

        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    break;
            }

            return IntPtr.Zero;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            POINT lMousePosition;
            GetCursorPos(out lMousePosition);

            IntPtr lPrimaryScreen = MonitorFromPoint(new POINT(0, 0), MonitorOptions.MONITOR_DEFAULTTOPRIMARY);
            MONITORINFO lPrimaryScreenInfo = new MONITORINFO();
            if (GetMonitorInfo(lPrimaryScreen, lPrimaryScreenInfo) == false)
            {
                return;
            }

            IntPtr lCurrentScreen = MonitorFromPoint(lMousePosition, MonitorOptions.MONITOR_DEFAULTTONEAREST);

            MINMAXINFO lMmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            if (lPrimaryScreen.Equals(lCurrentScreen))
            {
                lMmi.ptMaxPosition.X = lPrimaryScreenInfo.rcWork.Left;
                lMmi.ptMaxPosition.Y = lPrimaryScreenInfo.rcWork.Top;
                lMmi.ptMaxSize.X = lPrimaryScreenInfo.rcWork.Right - lPrimaryScreenInfo.rcWork.Left;
                lMmi.ptMaxSize.Y = lPrimaryScreenInfo.rcWork.Bottom - lPrimaryScreenInfo.rcWork.Top;
            }
            else
            {
                lMmi.ptMaxPosition.X = lPrimaryScreenInfo.rcMonitor.Left;
                lMmi.ptMaxPosition.Y = lPrimaryScreenInfo.rcMonitor.Top;
                lMmi.ptMaxSize.X = lPrimaryScreenInfo.rcMonitor.Right - lPrimaryScreenInfo.rcMonitor.Left;
                lMmi.ptMaxSize.Y = lPrimaryScreenInfo.rcMonitor.Bottom - lPrimaryScreenInfo.rcMonitor.Top;
            }

            Marshal.StructureToPtr(lMmi, lParam, true);
        }


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);



        private enum MonitorOptions : uint
        {
            MONITOR_DEFAULTTONULL = 0x00000000,
            MONITOR_DEFAULTTOPRIMARY = 0x00000001,
            MONITOR_DEFAULTTONEAREST = 0x00000002
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
        }
    }

    public class WindowResizer
    {
        private Window target = null;

        private bool resizeRight = false;
        private bool resizeLeft = false;
        private bool resizeUp = false;
        private bool resizeDown = false;

        private Dictionary<UIElement, short> leftElements = new Dictionary<UIElement, short>();
        private Dictionary<UIElement, short> rightElements = new Dictionary<UIElement, short>();
        private Dictionary<UIElement, short> upElements = new Dictionary<UIElement, short>();
        private Dictionary<UIElement, short> downElements = new Dictionary<UIElement, short>();

        private PointAPI resizePoint = new PointAPI();
        private Size resizeSize = new Size();
        private Point resizeWindowPoint = new Point();

        private delegate void RefreshDelegate();

        public WindowResizer(Window target)
        {
            this.target = target;

            if (target == null)
            {
                throw new Exception("Invalid Window handle");
            }
        }

        #region add resize components

        private void connectMouseHandlers(UIElement element)
        {
            if (element == null)
                return;

            element.MouseLeftButtonDown += new MouseButtonEventHandler(element_MouseLeftButtonDown);
            element.MouseLeftButtonUp += new MouseButtonEventHandler(element_MouseLeftButtonUp);
            element.MouseEnter += new MouseEventHandler(element_MouseEnter);
            element.MouseLeave += new MouseEventHandler(setArrowCursor);
        }

        public void addResizerRight(UIElement element)
        {
            if (element == null)
                return;

            connectMouseHandlers(element);
            rightElements.Add(element, 0);
        }

        public void addResizerLeft(UIElement element)
        {
            if (element == null)
                return;

            connectMouseHandlers(element);
            leftElements.Add(element, 0);
        }

        public void addResizerUp(UIElement element)
        {
            if (element == null)
                return;

            connectMouseHandlers(element);
            upElements.Add(element, 0);
        }

        public void addResizerDown(UIElement element)
        {
            if (element == null)
                return;

            connectMouseHandlers(element);
            downElements.Add(element, 0);
        }

        public void addResizerRightDown(UIElement element)
        {
            if (element == null)
                return;

            connectMouseHandlers(element);
            rightElements.Add(element, 0);
            downElements.Add(element, 0);
        }

        public void addResizerLeftDown(UIElement element)
        {
            if (element == null)
                return;

            connectMouseHandlers(element);
            leftElements.Add(element, 0);
            downElements.Add(element, 0);
        }

        public void addResizerRightUp(UIElement element)
        {
            if (element == null)
                return;

            connectMouseHandlers(element);
            rightElements.Add(element, 0);
            upElements.Add(element, 0);
        }

        public void addResizerLeftUp(UIElement element)
        {
            if (element == null)
                return;

            connectMouseHandlers(element);
            leftElements.Add(element, 0);
            upElements.Add(element, 0);
        }

        #endregion

        #region resize handlers

        private void element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (sender as UIElement).CaptureMouse();

            GetCursorPos(out resizePoint);
            resizeSize = new Size(target.Width, target.Height);
            resizeWindowPoint = new Point(target.Left, target.Top);

            #region updateResizeDirection

            UIElement sourceSender = (UIElement) sender;
            if (leftElements.ContainsKey(sourceSender))
            {
                resizeLeft = true;
            }
            if (rightElements.ContainsKey(sourceSender))
            {
                resizeRight = true;
            }
            if (upElements.ContainsKey(sourceSender))
            {
                resizeUp = true;
            }
            if (downElements.ContainsKey(sourceSender))
            {
                resizeDown = true;
            }

            #endregion

            Thread t = new Thread(new ThreadStart(updateSizeLoop));
            t.Name = "Mouse Position Poll Thread";
            t.Start();
        }

        private void element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            (sender as UIElement).ReleaseMouseCapture();
        }

        private void updateSizeLoop()
        {
            try
            {
                while (resizeDown || resizeLeft || resizeRight || resizeUp)
                {
                    target.Dispatcher.Invoke(DispatcherPriority.Render, new RefreshDelegate(updateSize));
                    target.Dispatcher.Invoke(DispatcherPriority.Render, new RefreshDelegate(updateMouseDown));
                    Thread.Sleep(10);
                }

                target.Dispatcher.Invoke(DispatcherPriority.Render, new RefreshDelegate(setArrowCursor));
            }
            catch (Exception)
            {
            }
        }

        #region updates

        private void updateSize()
        {
            PointAPI p = new PointAPI();
            GetCursorPos(out p);

            if (resizeRight)
            {
                target.Width = Math.Max(0, this.resizeSize.Width - (resizePoint.X - p.X));
            }

            if (resizeDown)
            {
                target.Height = Math.Max(0, resizeSize.Height - (resizePoint.Y - p.Y));
            }

            if (resizeLeft)
            {
                target.Width = Math.Max(0, resizeSize.Width + (resizePoint.X - p.X));
                target.Left = Math.Max(0, resizeWindowPoint.X - (resizePoint.X - p.X));
            }

            if (resizeUp)
            {
                target.Height = Math.Max(0, resizeSize.Height + (resizePoint.Y - p.Y));
                target.Top = Math.Max(0, resizeWindowPoint.Y - (resizePoint.Y - p.Y));
            }
        }

        private void updateMouseDown()
        {
            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                resizeRight = false;
                resizeLeft = false;
                resizeUp = false;
                resizeDown = false;
            }
        }

        #endregion

        #endregion

        #region cursor updates

        private void element_MouseEnter(object sender, MouseEventArgs e)
        {
            bool resizeRight = false;
            bool resizeLeft = false;
            bool resizeUp = false;
            bool resizeDown = false;

            var window = (Window) ((FrameworkElement) sender).TemplatedParent;
            if (window != null && window.WindowState == WindowState.Maximized)
            {
                return;
            }

            UIElement sourceSender = (UIElement) sender;

            if (leftElements.ContainsKey(sourceSender))
            {
                resizeLeft = true;
            }
            if (rightElements.ContainsKey(sourceSender))
            {
                resizeRight = true;
            }
            if (upElements.ContainsKey(sourceSender))
            {
                resizeUp = true;
            }
            if (downElements.ContainsKey(sourceSender))
            {
                resizeDown = true;
            }

            if ((resizeLeft && resizeDown) || (resizeRight && resizeUp))
            {
                setNESWCursor(sender, e);
            }
            else if ((resizeRight && resizeDown) || (resizeLeft && resizeUp))
            {
                setNWSECursor(sender, e);
            }
            else if (resizeLeft || resizeRight)
            {
                setWECursor(sender, e);
            }
            else if (resizeUp || resizeDown)
            {
                setNSCursor(sender, e);
            }
        }

        private void setWECursor(object sender, MouseEventArgs e)
        {
            target.Cursor = Cursors.SizeWE;
        }

        private void setNSCursor(object sender, MouseEventArgs e)
        {
            target.Cursor = Cursors.SizeNS;
        }

        private void setNESWCursor(object sender, MouseEventArgs e)
        {
            target.Cursor = Cursors.SizeNESW;
        }

        private void setNWSECursor(object sender, MouseEventArgs e)
        {
            target.Cursor = Cursors.SizeNWSE;
        }

        private void setArrowCursor(object sender, MouseEventArgs e)
        {
            if (!resizeDown && !resizeLeft && !resizeRight && !resizeUp)
            {
                target.Cursor = Cursors.Arrow;
            }
        }

        private void setArrowCursor()
        {
            target.Cursor = Cursors.Arrow;
        }

        #endregion

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out PointAPI lpPoint);

        private struct PointAPI
        {
            public int X;
            public int Y;
        }
    }
}
