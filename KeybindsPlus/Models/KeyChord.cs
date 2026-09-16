using System;
using System.Text;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Utility;
using KeybindsPlus.Extensions;
using KeybindsPlus.Models;

namespace KeybindsPlus.Models
{
    [Serializable]
    public class KeyChord : IEquatable<KeyChord>
    {
        public VirtualKey Key { get; set; } = VirtualKey.NO_KEY;
        public bool Ctrl { get; set; }
        public bool Shift { get; set; }
        public bool Alt { get; set; }

        public bool IsEmpty => Key == VirtualKey.NO_KEY;

        public KeyChord() { } // For serialization

        public KeyChord(VirtualKey key, bool ctrl = false, bool shift = false, bool alt = false)
        {
            Key = key;
            Ctrl = ctrl;
            Shift = shift;
            Alt = alt;
        }

        public bool Matches(VirtualKey pressedKey, bool isCtrlDown, bool isShiftDown, bool isAltDown)
        {
            if (IsEmpty) return false;
            return Key == pressedKey && Ctrl == isCtrlDown && Shift == isShiftDown && Alt == isAltDown;
        }
        public void Clear()
        {
            Key = VirtualKey.NO_KEY;
            Ctrl = false;
            Shift = false;
            Alt = false;
        }
        public string GetDisplayString()
        {
            if (IsEmpty) return "Not Bound";

            var sb = new StringBuilder();
            if (Ctrl) sb.Append("Ctrl+");
            if (Alt) sb.Append("Alt+");
            if (Shift) sb.Append("Shift+");
            sb.Append(Key.GetFriendlyName());

            return sb.ToString();
        }
        public bool Equals(KeyChord? other)
        {
            if (other is null) return false;
            return Key == other.Key && Ctrl == other.Ctrl && Shift == other.Shift && Alt == other.Alt;
        }

        public override bool Equals(object? obj) => Equals(obj as KeyChord);
        public override int GetHashCode() => HashCode.Combine(Key, Ctrl, Shift, Alt);

        public static bool TryParse(string? input, out KeyChord chord)
        {
            chord = new KeyChord();
            if (input.IsNullOrEmpty())
                return false;

            var parts = input.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return false;

            bool ctrl = false, shift = false, alt = false;
            var parsedKey = VirtualKey.NO_KEY;

            foreach (var part in parts)
            {
                if (part.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) ||
                    part.Equals("Control", StringComparison.OrdinalIgnoreCase))
                {
                    ctrl = true;
                }
                else if (part.Equals("Shift", StringComparison.OrdinalIgnoreCase))
                {
                    shift = true;
                }
                else if (part.Equals("Alt", StringComparison.OrdinalIgnoreCase) ||
                         part.Equals("Menu", StringComparison.OrdinalIgnoreCase))
                {
                    alt = true;
                }
                else
                {
                    if (KeyExtensions.TryResolveVirtualKey(part, out var vk))
                    {
                        parsedKey = vk;
                    }
                }
            }

            if (parsedKey == VirtualKey.NO_KEY)
                return false;

            chord = new KeyChord(parsedKey, ctrl, shift, alt);
            return true;
        }
    }
}
