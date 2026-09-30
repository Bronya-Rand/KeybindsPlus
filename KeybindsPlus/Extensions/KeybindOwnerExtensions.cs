using Dalamud.Game.ClientState.Keys;
using KeybindsPlus.Models;
using KeybindsPlus.Services;

namespace KeybindsPlus.Extensions
{
    public static class KeybindOwnerExtensions
    {
        public static bool HasGameInputOverride(this IKeybindOwner owner) =>
            owner.Enabled && (KeybindConflictService.IsGameKeybindConflict(owner.PrimaryKey) ||
                KeybindConflictService.IsGameKeybindConflict(owner.SecondaryKey));
        public static bool Matches(
            this IKeybindOwner owner,
            VirtualKey key,
            bool isLCtrlPressed, bool isRCtrlPressed,
            bool isLAltPressed, bool isRAltPressed,
            bool isLShiftPressed, bool isRShiftPressed)
        {
            if (!owner.Enabled) return false;
            return owner.PrimaryKey.Matches(key, isLCtrlPressed, isRCtrlPressed, isLAltPressed, isRAltPressed, isLShiftPressed, isRShiftPressed) ||
                owner.SecondaryKey.Matches(key, isLCtrlPressed, isRCtrlPressed, isLAltPressed, isRAltPressed, isLShiftPressed, isRShiftPressed);
        }
    }
}
