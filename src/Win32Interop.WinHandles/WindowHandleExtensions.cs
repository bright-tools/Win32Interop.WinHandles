using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Win32Interop.WinHandles.Internal;
using System.Runtime.InteropServices;

namespace Win32Interop.WinHandles
{
    /// <summary> Extension methods for <see cref="WindowHandle"/> </summary>
    public static class WindowHandleExtensions
    {
        /// <summary>Enumeration of the different ways of showing a window using 
        /// ShowWindow</summary>
        public enum WindowShowStyle : uint
        {
            /// <summary>Hides the window and activates another window.</summary>
            /// <remarks>See SW_HIDE</remarks>
            Hide = 0,
            /// <summary>Activates and displays a window. If the window is minimized 
            /// or maximized, the system restores it to its original size and 
            /// position. An application should specify this flag when displaying 
            /// the window for the first time.</summary>
            /// <remarks>See SW_SHOWNORMAL</remarks>
            ShowNormal = 1,
            /// <summary>Activates the window and displays it as a minimized window.</summary>
            /// <remarks>See SW_SHOWMINIMIZED</remarks>
            ShowMinimized = 2,
            /// <summary>Activates the window and displays it as a maximized window.</summary>
            /// <remarks>See SW_SHOWMAXIMIZED</remarks>
            ShowMaximized = 3,
            /// <summary>Maximizes the specified window.</summary>
            /// <remarks>See SW_MAXIMIZE</remarks>
            Maximize = 3,
            /// <summary>Displays a window in its most recent size and position. 
            /// This value is similar to "ShowNormal", except the window is not 
            /// actived.</summary>
            /// <remarks>See SW_SHOWNOACTIVATE</remarks>
            ShowNormalNoActivate = 4,
            /// <summary>Activates the window and displays it in its current size 
            /// and position.</summary>
            /// <remarks>See SW_SHOW</remarks>
            Show = 5,
            /// <summary>Minimizes the specified window and activates the next 
            /// top-level window in the Z order.</summary>
            /// <remarks>See SW_MINIMIZE</remarks>
            Minimize = 6,
            /// <summary>Displays the window as a minimized window. This value is 
            /// similar to "ShowMinimized", except the window is not activated.</summary>
            /// <remarks>See SW_SHOWMINNOACTIVE</remarks>
            ShowMinNoActivate = 7,
            /// <summary>Displays the window in its current size and position. This 
            /// value is similar to "Show", except the window is not activated.</summary>
            /// <remarks>See SW_SHOWNA</remarks>
            ShowNoActivate = 8,
            /// <summary>Activates and displays the window. If the window is 
            /// minimized or maximized, the system restores it to its original size 
            /// and position. An application should specify this flag when restoring 
            /// a minimized window.</summary>
            /// <remarks>See SW_RESTORE</remarks>
            Restore = 9,
            /// <summary>Sets the show state based on the SW_ value specified in the 
            /// STARTUPINFO structure passed to the CreateProcess function by the 
            /// program that started the application.</summary>
            /// <remarks>See SW_SHOWDEFAULT</remarks>
            ShowDefault = 10,
            /// <summary>Windows 2000/XP: Minimizes a window, even if the thread 
            /// that owns the window is hung. This flag should only be used when 
            /// minimizing windows from a different thread.</summary>
            /// <remarks>See SW_FORCEMINIMIZE</remarks>
            ForceMinimized = 11
        }

        [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 4)]
        public struct Point
        {
            [FieldOffset(0)]
            public int x;
            [FieldOffset(4)]
            public int y;
        };

        [StructLayout(LayoutKind.Explicit, Size = 16, Pack = 4)]
        public struct Rect
        {
            [FieldOffset(0)]
            public int left;
            [FieldOffset(4)]
            public int top;
            [FieldOffset(8)]
            public int right;
            [FieldOffset(12)]
            public int bottom;
        }

        [StructLayout(LayoutKind.Explicit, Size = 44, Pack = 4)]
        public struct WINDOWPLACEMENT
        {
            [FieldOffset(0)]
            public uint length;
            [FieldOffset(4)]
            public uint flags;
            [FieldOffset(8)]
            public WindowShowStyle showCmd;
            [FieldOffset(12)]
            public Point ptMinPosition;
            [FieldOffset(20)]
            public Point ptMaxPosition;
            [FieldOffset(28)]
            public Rect rcNormalPosition;
        };



        /// <summary> Check if the given window handle is currently visible. </summary>
        /// <param name="windowHandle"> The window to act on. </param>
        /// <returns> true if the window is visible, false if not. </returns>
        public static bool IsVisible(this WindowHandle windowHandle)
        {
            return NativeMethods.IsWindowVisible(windowHandle.RawPtr);
        }

        /// <summary> Gets the Win32 class name of the given window. </summary>
        /// <param name="windowHandle"> The window handle to act on. </param>
        /// <returns> The class name of the passed in window. </returns>
        public static string GetClassName(this WindowHandle windowHandle)
        {
            int size = 255;
            int actualSize = 0;
            StringBuilder builder;
            do
            {
                builder = new StringBuilder(size);
                actualSize = NativeMethods.GetClassName(windowHandle.RawPtr, builder, builder.Capacity);
                size *= 2;
            } while (actualSize == size - 1);

            return builder.ToString();
        }

        /// <summary> Gets the text associated with the given window handle. </summary>
        /// <param name="windowHandle"> The window handle to act on. </param>
        /// <returns> The window text. </returns>
        [NotNull]
        public static string GetWindowText(this WindowHandle windowHandle)
        {
            int size = NativeMethods.GetWindowTextLength(windowHandle.RawPtr);
            if (size > 0)
            {
                var builder = new StringBuilder(size + 1);
                NativeMethods.GetWindowText(windowHandle.RawPtr, builder, builder.Capacity);
                return builder.ToString();
            }

            return String.Empty;
        }

        private static IntPtr GetProcessHandle(this WindowHandle windowHandle)
        {
            uint pid;
            NativeMethods.GetWindowThreadProcessId(windowHandle.RawPtr, out pid);
            return NativeMethods.OpenProcess(NativeMethods.ProcessAccessFlags.VMRead | NativeMethods.ProcessAccessFlags.QueryInformation, true, pid);
        }

        /// <summary> Gets the executable associated with the given window handle. </summary>
        /// <param name="windowHandle"> The window handle to act on. </param>
        /// <returns> The executable filename. </returns>
        [NotNull]
        public static string GetWindowExec(this WindowHandle windowHandle)
        {
            IntPtr proc = GetProcessHandle(windowHandle);
            var buff = new StringBuilder(1024);
            NativeMethods.GetModuleFileNameEx(proc, (IntPtr)0, buff, buff.Capacity);
            return buff.ToString();
        }

        public static void SetWindowXY(this WindowHandle windowHandle, int x, int y)
        {
            NativeMethods.SetWindowPos(windowHandle.RawPtr, IntPtr.Zero, x, y, 100, 100, NativeMethods.WindowPosFlags.NOZORDER | NativeMethods.WindowPosFlags.NOSIZE );
        }

        public static void SetWindowSize(this WindowHandle windowHandle, int w, int h)
        {
            NativeMethods.SetWindowPos(windowHandle.RawPtr, IntPtr.Zero, 0, 0, w, h, NativeMethods.WindowPosFlags.NOZORDER | NativeMethods.WindowPosFlags.NOMOVE );
        }

        public static void SetZPosition(this WindowHandle windowHandle, IntPtr p_pos )
        {
            NativeMethods.SetWindowPos(windowHandle.RawPtr, p_pos, 0, 0, 0, 0, NativeMethods.WindowPosFlags.NOACTIVATE | NativeMethods.WindowPosFlags.NOSIZE | NativeMethods.WindowPosFlags.NOMOVE);
        }

        public static bool GetWindowPlacement(this WindowHandle windowHandle, ref WINDOWPLACEMENT placement)
        {
            placement.length = (uint)(Marshal.SizeOf(placement));
            return NativeMethods.GetWindowPlacement(windowHandle.RawPtr, ref placement);
        }

        public static bool MaximizeWindow(this WindowHandle windowHandle)
        {
            return NativeMethods.ShowWindow(windowHandle.RawPtr, WindowShowStyle.ShowMaximized);
        }

        public static bool MinimizeWindow(this WindowHandle windowHandle)
        {
            return NativeMethods.ShowWindow(windowHandle.RawPtr, WindowShowStyle.ShowMinimized);
        }

        public static bool ShowWindow(this WindowHandle windowHandle)
        {
            return NativeMethods.ShowWindow(windowHandle.RawPtr, WindowShowStyle.ShowNormal);
        }

        public static bool RestoreWindow(this WindowHandle windowHandle)
        {
            return NativeMethods.ShowWindow(windowHandle.RawPtr, WindowShowStyle.Restore);
        }
    }
}