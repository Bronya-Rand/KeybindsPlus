using System;
using System.Text;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Utility;
using KeybindsPlus.Extensions;
using KeybindsPlus.Models;

namespace KeybindsPlus.Models
{
    /// <summary>
    /// Enum to specify which side of the keyboard a modifier key is on.
    /// </summary>
    /// <remarks>
    /// Any defaults to Ctrl/Shift/Alt while None means not required.
    /// Left and Right are for specifying a specific side of the keyboard for the modifier key.
    /// </remarks>
    public enum ModifierSide { None, Left, Right, Any };

    [Serializable]
    public class KeyChord : IEquatable<KeyChord>
    {
        public VirtualKey Key { get; set; } = VirtualKey.NO_KEY;
        public ModifierSide CtrlSide { get; set; } = ModifierSide.None;
        public ModifierSide AltSide { get; set; } = ModifierSide.None;
        public ModifierSide ShiftSide { get; set; } = ModifierSide.None;

        public bool IsEmpty => Key == VirtualKey.NO_KEY;

        public KeyChord() { } // For serialization

        public KeyChord(VirtualKey key, ModifierSide ctrl = ModifierSide.None, ModifierSide alt = ModifierSide.None, ModifierSide shift = ModifierSide.None)
        {
            Key = key;
            CtrlSide = ctrl;
            ShiftSide = shift;
            AltSide = alt;
        }

        public bool Matches(
            VirtualKey pressedKey,
            bool isLCtrlPressed, bool isRCtrlPressed,
            bool isLAltPressed, bool isRAltPressed,
            bool isLShiftPressed, bool isRShiftPressed)
        {
            if (IsEmpty || Key != pressedKey) return false;
            return MatchesModifer(CtrlSide, isLCtrlPressed, isRCtrlPressed) &&
                   MatchesModifer(AltSide, isLAltPressed, isRAltPressed) &&
                   MatchesModifer(ShiftSide, isLShiftPressed, isRShiftPressed);
        }

        public void Clear()
        {
            Key = VirtualKey.NO_KEY;
            CtrlSide = ModifierSide.None;
            ShiftSide = ModifierSide.None;
            AltSide = ModifierSide.None;
        }
        public string GetDisplayString()
        {
            if (IsEmpty) return "Not Bound";

            var sb = new StringBuilder();
            if (CtrlSide != ModifierSide.None)
                sb.Append(CtrlSide switch
                {
                    ModifierSide.Left => "Left Ctrl+",
                    ModifierSide.Right => "Right Ctrl+",
                    _ => "Ctrl+",
                });

            if (AltSide != ModifierSide.None)
                sb.Append(AltSide switch
                {
                    ModifierSide.Left => "Left Alt+",
                    ModifierSide.Right => "Right Alt+",
                    _ => "Alt+",
                });

            if (ShiftSide != ModifierSide.None)
                sb.Append(ShiftSide switch
                {
                    ModifierSide.Left => "Left Shift+",
                    ModifierSide.Right => "Right Shift+",
                    _ => "Shift+",
                });

            sb.Append(Key.GetFriendlyName());
            return sb.ToString();
        }
        public bool Equals(KeyChord? other) =>
            other != null && Key == other.Key && CtrlSide == other.CtrlSide &&
            AltSide == other.AltSide && ShiftSide == other.ShiftSide;

        public override bool Equals(object? obj) => Equals(obj as KeyChord);
        public override int GetHashCode() => HashCode.Combine(Key, CtrlSide, AltSide, ShiftSide);

        public static bool TryParse(string? input, out KeyChord chord)
        {
            chord = new KeyChord();
            if (input.IsNullOrEmpty()) return false;

            var parts = input.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return false;

            ModifierSide ctrl = ModifierSide.None, alt = ModifierSide.None, shift = ModifierSide.None;
            var parsedKey = VirtualKey.NO_KEY;

            foreach (var part in parts)
            {
                if (part.Equals("Left Ctrl", StringComparison.OrdinalIgnoreCase) || part.Equals("LCtrl", StringComparison.OrdinalIgnoreCase))
                    ctrl = ModifierSide.Left;
                else if (part.Equals("Right Ctrl", StringComparison.OrdinalIgnoreCase) || part.Equals("RCtrl", StringComparison.OrdinalIgnoreCase))
                    ctrl = ModifierSide.Right;
                else if (part.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) || part.Equals("Control", StringComparison.OrdinalIgnoreCase))
                    ctrl = ModifierSide.Any;
                else if (part.Equals("Left Shift", StringComparison.OrdinalIgnoreCase) || part.Equals("LShift", StringComparison.OrdinalIgnoreCase))
                    shift = ModifierSide.Left;
                else if (part.Equals("Right Shift", StringComparison.OrdinalIgnoreCase) || part.Equals("RShift", StringComparison.OrdinalIgnoreCase))
                    shift = ModifierSide.Right;
                else if (part.Equals("Shift", StringComparison.OrdinalIgnoreCase))
                    shift = ModifierSide.Any;
                else if (part.Equals("Left Alt", StringComparison.OrdinalIgnoreCase) || part.Equals("LAlt", StringComparison.OrdinalIgnoreCase))
                    alt = ModifierSide.Left;
                else if (part.Equals("Right Alt", StringComparison.OrdinalIgnoreCase) || part.Equals("RAlt", StringComparison.OrdinalIgnoreCase))
                    alt = ModifierSide.Right;
                else if (part.Equals("Alt", StringComparison.OrdinalIgnoreCase) || part.Equals("Menu", StringComparison.OrdinalIgnoreCase))
                    alt = ModifierSide.Any;
                else if (KeyExtensions.TryResolveVirtualKey(part, out var vk))
                    parsedKey = vk;
            }

            if (parsedKey == VirtualKey.NO_KEY)
                return false;

            chord = new KeyChord(parsedKey, ctrl, alt, shift);
            return true;
        }

        private static bool MatchesModifer(ModifierSide required, bool isLeftPressed, bool isRightPressed) => required switch
        {
            ModifierSide.None => !isLeftPressed && !isRightPressed,
            ModifierSide.Left => isLeftPressed,
            ModifierSide.Right => isRightPressed,
            ModifierSide.Any => isLeftPressed || isRightPressed,
            _ => false,
        };
    }
}
