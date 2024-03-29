﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Hg.SaveHistory.Utilities
{
    // Lots of code is from:
    // https://www.pinvoke.net
    public class ScreenShots
    {
        #region Types

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private enum Gwl
        {
            GWL_WNDPROC = -4,
            GWL_HINSTANCE = -6,
            GWL_HWNDPARENT = -8,
            GWL_STYLE = -16,
            GWL_EXSTYLE = -20,
            GWL_USERDATA = -21,
            GWL_ID = -12
        }

        [StructLayout(LayoutKind.Sequential)]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private enum SystemMetric
        {
            SM_CXSCREEN = 0, // 0x00
            SM_CYSCREEN = 1, // 0x01
            SM_CXVSCROLL = 2, // 0x02
            SM_CYHSCROLL = 3, // 0x03
            SM_CYCAPTION = 4, // 0x04
            SM_CXBORDER = 5, // 0x05
            SM_CYBORDER = 6, // 0x06
            SM_CXDLGFRAME = 7, // 0x07
            SM_CXFIXEDFRAME = 7, // 0x07
            SM_CYDLGFRAME = 8, // 0x08
            SM_CYFIXEDFRAME = 8, // 0x08
            SM_CYVTHUMB = 9, // 0x09
            SM_CXHTHUMB = 10, // 0x0A
            SM_CXICON = 11, // 0x0B
            SM_CYICON = 12, // 0x0C
            SM_CXCURSOR = 13, // 0x0D
            SM_CYCURSOR = 14, // 0x0E
            SM_CYMENU = 15, // 0x0F
            SM_CXFULLSCREEN = 16, // 0x10
            SM_CYFULLSCREEN = 17, // 0x11
            SM_CYKANJIWINDOW = 18, // 0x12
            SM_MOUSEPRESENT = 19, // 0x13
            SM_CYVSCROLL = 20, // 0x14
            SM_CXHSCROLL = 21, // 0x15
            SM_DEBUG = 22, // 0x16
            SM_SWAPBUTTON = 23, // 0x17
            SM_CXMIN = 28, // 0x1C
            SM_CYMIN = 29, // 0x1D
            SM_CXSIZE = 30, // 0x1E
            SM_CYSIZE = 31, // 0x1F
            SM_CXSIZEFRAME = 32, // 0x20
            SM_CXFRAME = 32, // 0x20
            SM_CYSIZEFRAME = 33, // 0x21
            SM_CYFRAME = 33, // 0x21
            SM_CXMINTRACK = 34, // 0x22
            SM_CYMINTRACK = 35, // 0x23
            SM_CXDOUBLECLK = 36, // 0x24
            SM_CYDOUBLECLK = 37, // 0x25
            SM_CXICONSPACING = 38, // 0x26
            SM_CYICONSPACING = 39, // 0x27
            SM_MENUDROPALIGNMENT = 40, // 0x28
            SM_PENWINDOWS = 41, // 0x29
            SM_DBCSENABLED = 42, // 0x2A
            SM_CMOUSEBUTTONS = 43, // 0x2B
            SM_SECURE = 44, // 0x2C
            SM_CXEDGE = 45, // 0x2D
            SM_CYEDGE = 46, // 0x2E
            SM_CXMINSPACING = 47, // 0x2F
            SM_CYMINSPACING = 48, // 0x30
            SM_CXSMICON = 49, // 0x31
            SM_CYSMICON = 50, // 0x32
            SM_CYSMCAPTION = 51, // 0x33
            SM_CXSMSIZE = 52, // 0x34
            SM_CYSMSIZE = 53, // 0x35
            SM_CXMENUSIZE = 54, // 0x36
            SM_CYMENUSIZE = 55, // 0x37
            SM_ARRANGE = 56, // 0x38
            SM_CXMINIMIZED = 57, // 0x39
            SM_CYMINIMIZED = 58, // 0x3A
            SM_CXMAXTRACK = 59, // 0x3B
            SM_CYMAXTRACK = 60, // 0x3C
            SM_CXMAXIMIZED = 61, // 0x3D
            SM_CYMAXIMIZED = 62, // 0x3E
            SM_NETWORK = 63, // 0x3F
            SM_CLEANBOOT = 67, // 0x43
            SM_CXDRAG = 68, // 0x44
            SM_CYDRAG = 69, // 0x45
            SM_SHOWSOUNDS = 70, // 0x46
            SM_CXMENUCHECK = 71, // 0x47
            SM_CYMENUCHECK = 72, // 0x48
            SM_SLOWMACHINE = 73, // 0x49
            SM_MIDEASTENABLED = 74, // 0x4A
            SM_MOUSEWHEELPRESENT = 75, // 0x4B
            SM_XVIRTUALSCREEN = 76, // 0x4C
            SM_YVIRTUALSCREEN = 77, // 0x4D
            SM_CXVIRTUALSCREEN = 78, // 0x4E
            SM_CYVIRTUALSCREEN = 79, // 0x4F
            SM_CMONITORS = 80, // 0x50
            SM_SAMEDISPLAYFORMAT = 81, // 0x51
            SM_IMMENABLED = 82, // 0x52
            SM_CXFOCUSBORDER = 83, // 0x53
            SM_CYFOCUSBORDER = 84, // 0x54
            SM_TABLETPC = 86, // 0x56
            SM_MEDIACENTER = 87, // 0x57
            SM_STARTER = 88, // 0x58
            SM_SERVERR2 = 89, // 0x59
            SM_MOUSEHORIZONTALWHEELPRESENT = 91, // 0x5B
            SM_CXPADDEDBORDER = 92, // 0x5C
            SM_DIGITIZER = 94, // 0x5E
            SM_MAXIMUMTOUCHES = 95, // 0x5F

            SM_REMOTESESSION = 0x1000, // 0x1000
            SM_SHUTTINGDOWN = 0x2000, // 0x2000
            SM_REMOTECONTROL = 0x2001, // 0x2001


            SM_CONVERTIBLESLATEMODE = 0x2003,
            SM_SYSTEMDOCKED = 0x2004
        }

        #endregion

        #region Fields & Properties

        private static readonly int _titleBarHeight = GetTitleBarHeight();

        private const int WS_CAPTION = 0x00C00000;

        #endregion

        #region Members

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc,
            CopyPixelOperation dwRop);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        ///     Client area bounds
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static Rectangle GetClientBounds(IntPtr handle)
        {
            Rect rectWin = new Rect();
            GetClientRect(handle, ref rectWin);

            Rectangle bounds = new Rectangle(
                rectWin.Left,
                rectWin.Top,
                rectWin.Right - rectWin.Left,
                rectWin.Bottom - rectWin.Top);

            return bounds;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        public static Rectangle GetSize(IntPtr handle)
        {
            Rect rectWin = new Rect();
            GetWindowRect(handle, ref rectWin);

            Rect rectClient = new Rect();
            GetClientRect(handle, ref rectClient);

            if (HasTitlebar(handle))
            {
                Rectangle bounds = new Rectangle(
                    rectWin.Left + 7 + 1,
                    rectWin.Top + 1 + _titleBarHeight,
                    rectClient.Right,
                    rectClient.Bottom - _titleBarHeight);
                return bounds;
            }
            else
            {
                Rectangle bounds = new Rectangle(
                    rectWin.Left + 7 + 1,
                    rectWin.Top + 1,
                    rectClient.Right,
                    rectClient.Bottom);
                return bounds;
            }
        }

        public static int GetTitleBarHeight()
        {
            int height = GetSystemMetrics(SystemMetric.SM_CYFRAME) +
                         GetSystemMetrics(SystemMetric.SM_CYCAPTION) +
                         GetSystemMetrics(SystemMetric.SM_CXPADDEDBORDER);
            return height;
        }

        /// <summary>
        ///     Screen coordinate of windows of handle
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static Rectangle GetWindowBounds(IntPtr handle)
        {
            Rect rectWin = new Rect();
            GetWindowRect(handle, ref rectWin);

            Rectangle bounds = new Rectangle(
                rectWin.Left,
                rectWin.Top,
                rectWin.Right - rectWin.Left,
                rectWin.Bottom - rectWin.Top);

            return bounds;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        // This static method redirect to the correct windows api based on 32/64 bits
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
            {
                return GetWindowLongPtr64(hWnd, nIndex);
            }

            return GetWindowLongPtr32(hWnd, nIndex);
        }

        public static bool HasTitlebar(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                return false;
            }

            int value = (int)GetWindowLongPtr(handle, (int)Gwl.GWL_STYLE);
            return (value & WS_CAPTION) == WS_CAPTION;
        }

        public static bool? IsWindowFocused(IntPtr processPtr)
        {
            IntPtr handle = GetForegroundWindow();
            if (handle == IntPtr.Zero)
            {
                return false;
            }

            return handle == processPtr;
        }

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hdo);

        [DllImport("user32.dll")]
        private static extern IntPtr GetClientRect(IntPtr hWnd, ref Rect rect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(SystemMetric smIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        #endregion
    }
}