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
        public static bool Matches(this IKeybindOwner owner, VirtualKey key, bool ctrl, bool alt, bool shift)
        {
            if (!owner.Enabled) return false;
            Plugin.Log.Debug($"Checking keybind for {owner.GetType().Name}: {key} (ctrl: {ctrl}, shift: {shift}, alt: {alt})");
            return owner.PrimaryKey.Matches(key, ctrl, shift, alt) || owner.SecondaryKey.Matches(key, ctrl, shift, alt);
        }
    }
}
