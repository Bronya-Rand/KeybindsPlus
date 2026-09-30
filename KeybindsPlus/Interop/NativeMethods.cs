using System;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Keys;

namespace KeybindsPlus.Interop
{
    /// <summary>
    /// Contains P/Invoke sigs for interacting with Windows native APIs for keyboard and mouse input.
    /// </summary>
    internal static partial class NativeMethods
    {
        #region Keyboard Input
        internal const int WM_KEYDOWN = 0x0100;
        internal const int WM_KEYUP = 0x0101;
        internal const int WM_SYSKEYDOWN = 0x0104;
        internal const int WM_SYSKEYUP = 0x0105;
        #endregion

        #region Mouse Input
        internal const int WM_MBUTTONDOWN = 0x0207;
        internal const int WM_MBUTTONUP = 0x0208;
        internal const int WM_XBUTTONDOWN = 0x020B;
        internal const int WM_XBUTTONUP = 0x020C;
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
        /// <param name="vKey">The virtual key code to check.</param>
        private static bool IsKeyDown(int vKey) => (GetAsyncKeyState(vKey) & 0x8000) != 0;

        /// <inheritdoc cref="IsKeyDown(int)"/>
        internal static bool IsKeyDown(VirtualKey key) => IsKeyDown((int)key);
    }
}
