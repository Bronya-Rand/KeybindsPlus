using System;
using System.Diagnostics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using KeybindsPlus.Interop;

namespace KeybindsPlus.Services
{
    /// <summary>
    /// Intercepts input events from the game window and raises events for key presses and releases.
    /// </summary>
    public class InputInterceptorService : IDisposable
    {
        private const nuint SubclassId = 0x5142; // Plugin abbreviation as a unique identifier
        private readonly IntPtr gameHwnd;
        private readonly NativeMethods.WndSubclassProc subclassDelegate;

        public delegate bool KeyEventHandler(VirtualKey key, bool isDown);
        public event KeyEventHandler? OnKeyEvent;
        public Func<bool>? IsRecordingPredicate { get; set; }

        public InputInterceptorService(IFramework framework)
        {
            gameHwnd = Process.GetCurrentProcess().MainWindowHandle;
            Plugin.Log.Verbose($"Game window handle: {gameHwnd}");

            // Keep the delegate alive so GC doesn't collect it
            subclassDelegate = WndProc;

            // Run WindowSubclass on framework thread
            framework.RunOnFrameworkThread(() =>
            {
                var success = NativeMethods.SetWindowSubclass(gameHwnd, subclassDelegate, SubclassId, IntPtr.Zero);
                Plugin.Log.Verbose($"SetWindowSubclass result: {success}");

                if (!success)
                    Plugin.Log.Error("SetWindowSubclass failed — Plugin will be unable to intercept input.");
            });
        }

        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, nuint uIdSubclass, IntPtr dwRefData)
        {
            var isRecording = IsRecordingPredicate?.Invoke() == true;
            switch (msg)
            {
                case NativeMethods.WM_KEYDOWN:
                case NativeMethods.WM_SYSKEYDOWN:
                    {
                        var vkShort = (ushort)(wParam.ToInt64() & 0xFFFF);
                        var vkCode = (VirtualKey)vkShort;
                        var isRepeat = (lParam.ToInt64() & 0x40000000) != 0;

                        var condition = (NativeMethods.GetForegroundWindow() == gameHwnd && (isRecording || !ImGui.GetIO().WantCaptureKeyboard));

                        if (condition)
                        {
                            var isHandled = false;
                            if (!isRepeat)
                                isHandled = OnKeyEvent?.Invoke(vkCode, true) ?? false;

                            // Consume input if:
                            // 1. The plugin is recording keybinds
                            // 2. Pressed key matched a active keybind (isHandled = true)
                            if (isRecording || isHandled)
                                return IntPtr.Zero;
                        }
                        break;
                    }

                case NativeMethods.WM_KEYUP:
                case NativeMethods.WM_SYSKEYUP:
                    {
                        var vkShort = (ushort)(wParam.ToInt64() & 0xFFFF);
                        var vkCode = (VirtualKey)vkShort;

                        if (NativeMethods.GetForegroundWindow() == gameHwnd)
                        {
                            var isHandled = OnKeyEvent?.Invoke(vkCode, false) ?? false;

                            if (isRecording || isHandled)
                                return IntPtr.Zero;
                        }
                        break;
                    }

                case NativeMethods.WM_MBUTTONDOWN:
                    if (HandleMouseButton(VirtualKey.MBUTTON, true, isRecording))
                        return IntPtr.Zero;
                    break;
                case NativeMethods.WM_MBUTTONUP:
                    if (HandleMouseButton(VirtualKey.MBUTTON, false, isRecording))
                        return IntPtr.Zero;
                    break;

                case NativeMethods.WM_XBUTTONDOWN:
                    {
                        var vkShort = (ushort)((wParam.ToInt64() >> 16) & 0xFFFF);
                        var vkCode = vkShort == 1 ? VirtualKey.XBUTTON1 : VirtualKey.XBUTTON2;
                        if (HandleMouseButton(vkCode, true, isRecording))
                            return (IntPtr)1; // Processed WM_XBUTTONDOWN returns TRUE
                        break;
                    }
                case NativeMethods.WM_XBUTTONUP:
                    {
                        var vkShort = (ushort)((wParam.ToInt64() >> 16) & 0xFFFF);
                        var vkCode = vkShort == 1 ? VirtualKey.XBUTTON1 : VirtualKey.XBUTTON2;
                        if (HandleMouseButton(vkCode, false, isRecording))
                            return (IntPtr)1;
                        break;
                    }
            }

            return NativeMethods.DefSubclassProc(hWnd, msg, wParam, lParam);
        }
        public void Dispose()
        {
            NativeMethods.RemoveWindowSubclass(gameHwnd, subclassDelegate, SubclassId);
            GC.SuppressFinalize(this);
        }

        private bool HandleMouseButton(VirtualKey vkCode, bool isDown, bool isRecording)
        {
            // Don't intercept mouse input if ImGui is capturing it
            var condition = NativeMethods.GetForegroundWindow() == gameHwnd
                && (isRecording || !ImGui.GetIO().WantCaptureMouse);

            if (!condition) return false;

            var isHandled = OnKeyEvent?.Invoke(vkCode, isDown) ?? false;
            return isRecording || isHandled;
        }
    }
}
