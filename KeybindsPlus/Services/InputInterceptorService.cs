using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using KeybindsPlus.Interop;

namespace KeybindsPlus.Services
{
    public class InputInterceptorService : IDisposable
    {
        private const nuint SubclassId = 0x5142; // Plugin abbreviation as a unique identifier
        private readonly IntPtr gameHwnd;
        private readonly NativeMethods.WndSubclassProc subclassDelegate;
        private readonly HashSet<VirtualKey> pressedKeys = [];

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
            switch (msg)
            {
                case NativeMethods.WM_KEYDOWN:
                case NativeMethods.WM_SYSKEYDOWN:
                    {
                        var vkShort = (ushort)(wParam.ToInt64() & 0xFFFF);
                        var vkCode = (VirtualKey)vkShort;
                        var isRepeat = (lParam.ToInt64() & 0x40000000) != 0;

                        var isRecording = IsRecordingPredicate?.Invoke() == true;
                        var condition = (NativeMethods.GetForegroundWindow() == gameHwnd && (isRecording || !ImGui.GetIO().WantCaptureKeyboard));
                        Plugin.Log.Debug($"ForegroundWindowMatch: {NativeMethods.GetForegroundWindow() == gameHwnd}, IsRecording: {isRecording}, WantCaptureKeyboard: {ImGui.GetIO().WantCaptureKeyboard}, Condition: {condition}");

                        if (condition)
                        {
                            var isHandled = false;
                            if (!isRepeat)
                            {
                                pressedKeys.Add(vkCode);
                                isHandled = OnKeyEvent?.Invoke(vkCode, true) ?? false;
                            }

                            // Consume input if:
                            // 1. The plugin is recording keybinds
                            // 2. Pressed key matched a active keybind (isHandled = true)
                            // 3. Unsupported Keys (F13-F24) are pressed [prevents game/Windows from handling them]
                            if (isRecording || isHandled || vkShort is >= 0x7C and <= 0x87)
                                return IntPtr.Zero;
                        }
                        break;
                    }

                case NativeMethods.WM_KEYUP:
                case NativeMethods.WM_SYSKEYUP:
                    {
                        var vkShort = (ushort)(wParam.ToInt64() & 0xFFFF);
                        var vkCode = (VirtualKey)vkShort;
                        pressedKeys.Remove(vkCode);

                        if (NativeMethods.GetForegroundWindow() == gameHwnd)
                        {
                            var isHandled = OnKeyEvent?.Invoke(vkCode, false);

                            if (IsRecordingPredicate?.Invoke() == true || isHandled == true || vkShort is >= 0x7C and <= 0x87)
                                return IntPtr.Zero;
                        }
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
    }
}
