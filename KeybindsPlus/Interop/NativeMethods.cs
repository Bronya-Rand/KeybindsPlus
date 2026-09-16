using System;
using System.Runtime.InteropServices;

namespace KeybindsPlus.Interop
{
    internal static partial class NativeMethods
    {
        #region Keyboard Input
        internal const int WM_KEYDOWN = 0x0100;
        internal const int WM_KEYUP = 0x0101;
        internal const int WM_SYSKEYDOWN = 0x0104;
        internal const int WM_SYSKEYUP = 0x0105;
        #endregion

        internal delegate IntPtr WndSubclassProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, nuint uIdSubclass, IntPtr dwRefData);

        [LibraryImport("comctl32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetWindowSubclass(IntPtr hWnd, WndSubclassProc wndSubclass, nuint uIdSubclass, IntPtr dwRefData);

        [LibraryImport("comctl32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool RemoveWindowSubclass(IntPtr hWnd, WndSubclassProc wndSubclass, nuint uIdSubclass);

        [LibraryImport("comctl32.dll")]
        internal static partial IntPtr DefSubclassProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [LibraryImport("user32.dll")]
        internal static partial IntPtr GetForegroundWindow();

        [LibraryImport("user32.dll")]
        internal static partial short GetAsyncKeyState(int vKey);

        /// <summary>
        /// Checks if a key is currently held down using the OS-level key state.
        /// </summary>
        internal static bool IsKeyDown(int vKey) => (GetAsyncKeyState(vKey) & 0x8000) != 0;
    }
}
