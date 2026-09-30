using System.Linq;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Utility;

namespace KeybindsPlus.Extensions;

public static class KeyExtensions
{
    public static string GetFriendlyName(this VirtualKey key)
    {
        try
        {
            var fancy = key.GetFancyName();
            if (!string.IsNullOrWhiteSpace(fancy))
                return fancy;
        }
        catch
        {
            // Fallback for custom unmapped VK codes
        }

        return key != VirtualKey.NO_KEY ? $"VK_{(ushort)key:X2}" : "Not Bound";
    }
    public static bool TryResolveVirtualKey(string keyName, out VirtualKey resolvedKey)
    {
        resolvedKey = VirtualKey.NO_KEY;
        if (keyName.IsNullOrEmpty())
            return false;

        var trimmed = keyName.Trim();

        // Direct Enum parse
        if (System.Enum.TryParse<VirtualKey>(trimmed, ignoreCase: true, out var enumVk))
        {
            resolvedKey = enumVk;
            return true;
        }

        // Single alphanumeric character
        if (trimmed.Length == 1)
        {
            var ch = char.ToUpperInvariant(trimmed[0]);
            if ((ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
            {
                resolvedKey = (VirtualKey)ch;
                return true;
            }
        }

        // Common aliases
        switch (trimmed.ToLowerInvariant())
        {
            case "esc":
            case "escape":
                resolvedKey = VirtualKey.ESCAPE;
                return true;
            case "return":
            case "enter":
                resolvedKey = VirtualKey.RETURN;
                return true;
            case "space":
            case "spacebar":
                resolvedKey = VirtualKey.SPACE;
                return true;
            case "tab":
                resolvedKey = VirtualKey.TAB;
                return true;
            case "back":
            case "backspace":
                resolvedKey = VirtualKey.BACK;
                return true;
            case "del":
            case "delete":
                resolvedKey = VirtualKey.DELETE;
                return true;
            case "ins":
            case "insert":
                resolvedKey = VirtualKey.INSERT;
                return true;
            case "pgup":
            case "pageup":
                resolvedKey = VirtualKey.PRIOR;
                return true;
            case "pgdn":
            case "pagedown":
                resolvedKey = VirtualKey.NEXT;
                return true;
            case "home":
                resolvedKey = VirtualKey.HOME;
                return true;
            case "end":
                resolvedKey = VirtualKey.END;
                return true;
            case "up":
                resolvedKey = VirtualKey.UP;
                return true;
            case "down":
                resolvedKey = VirtualKey.DOWN;
                return true;
            case "left":
                resolvedKey = VirtualKey.LEFT;
                return true;
            case "right":
                resolvedKey = VirtualKey.RIGHT;
                return true;
            case "prtsc":
            case "printscreen":
                resolvedKey = VirtualKey.SNAPSHOT;
                return true;
            case "pause":
                resolvedKey = VirtualKey.PAUSE;
                return true;
            case "caps":
            case "capslock":
                resolvedKey = VirtualKey.CAPITAL;
                return true;
            case "numlock":
                resolvedKey = VirtualKey.NUMLOCK;
                return true;
            case "scrolllock":
                resolvedKey = VirtualKey.SCROLL;
                return true;
        }

        // Check friendly names across valid keys
        foreach (var vk in Plugin.KeyState.GetValidVirtualKeys().Concat(Plugin.KeyState.GetExtendedVirtualKeys()))
        {
            if (vk.GetFriendlyName().Equals(trimmed, System.StringComparison.OrdinalIgnoreCase))
            {
                resolvedKey = vk;
                return true;
            }
        }

        return false;
    }
}
