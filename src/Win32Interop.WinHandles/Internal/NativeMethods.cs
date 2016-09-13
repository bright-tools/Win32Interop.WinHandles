using System;
using System.Runtime.InteropServices;
using System.Text;
using static Win32Interop.WinHandles.WindowHandleExtensions;

namespace Win32Interop.WinHandles.Internal
{
  internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

  /// <summary> Win32 methods. </summary>
  internal static class NativeMethods
  {
    public const bool EnumWindows_ContinueEnumerating = true;
    public const bool EnumWindows_StopEnumerating = false;

    [Flags]
    public enum ProcessAccessFlags : uint
    {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VMOperation = 0x00000008,
        VMRead = 0x00000010,
        VMWrite = 0x00000020,
        DupHandle = 0x00000040,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        Synchronize = 0x00100000
    };

    [Flags]
    public enum WindowPosFlags : uint
    {
        NOZORDER = 0x4,
        NOMOVE = 0x2,
        NOSIZE = 0x1
    };

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    internal static extern IntPtr FindWindow(string sClassName, string sAppName);

    [DllImport("user32.dll")]
    internal static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    internal static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("kernel32.dll")]
    internal static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwProcessId);

    [DllImport("psapi.dll")]
    internal static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, WindowPosFlags wFlags);

    /// <summary>
    /// Retrieves the show state and the restored, minimized, and maximized positions of the specified window.
    /// </summary>
    /// <param name="hWnd">
    /// A handle to the window.
    /// </param>
    /// <param name="lpwndpl">
    /// A pointer to the WINDOWPLACEMENT structure that receives the show state and position information.
    /// <para>
    /// Before calling GetWindowPlacement, set the length member to sizeof(WINDOWPLACEMENT). GetWindowPlacement fails if lpwndpl-> length is not set correctly.
    /// </para>
    /// </param>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// <para>
    /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
    /// </para>
    /// </returns>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll")]
    internal static extern int GetClassName(IntPtr hWnd,
                                            StringBuilder lpClassName,
                                            int nMaxCount);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ShowWindow(IntPtr hwnd, WindowShowStyle nCmdShow);
    }
}