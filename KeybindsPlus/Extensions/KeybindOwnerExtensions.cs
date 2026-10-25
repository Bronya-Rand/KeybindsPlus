using Dalamud.Game.ClientState.Keys;
using KeybindsPlus.Models;
using KeybindsPlus.Services;

namespace KeybindsPlus.Extensions
{
    public static class KeybindOwnerExtensions
    {
        extension(IKeybindOwner owner)
        {
            public bool HasGameInputOverride() =>
                owner.Enabled && (KeybindConflictService.IsGameKeybindConflict(owner.PrimaryKey) ||
                                  KeybindConflictService.IsGameKeybindConflict(owner.SecondaryKey));

            public bool Matches(
                VirtualKey key,
                bool isLCtrlPressed, bool isRCtrlPressed,
                bool isLAltPressed, bool isRAltPressed,
                bool isLShiftPressed, bool isRShiftPressed)
            {
                if (!owner.Enabled) return false;
                return owner.PrimaryKey.Matches(key, isLCtrlPressed, isRCtrlPressed, isLAltPressed, isRAltPressed,
                                                isLShiftPressed, isRShiftPressed) ||
                       owner.SecondaryKey.Matches(key, isLCtrlPressed, isRCtrlPressed, isLAltPressed, isRAltPressed,
                                                  isLShiftPressed, isRShiftPressed);
            }
        }
    }
}
