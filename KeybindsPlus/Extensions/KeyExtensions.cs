using System.Collections.Generic;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Utility;

namespace KeybindsPlus.Extensions;

public static class KeyExtensions
{
    private static readonly Dictionary<VirtualKey, string> ExtendedKeyNames = new()
    {
        [(VirtualKey)0x7C] = "F13",
        [(VirtualKey)0x7D] = "F14",
        [(VirtualKey)0x7E] = "F15",
        [(VirtualKey)0x7F] = "F16",
        [(VirtualKey)0x80] = "F17",
        [(VirtualKey)0x81] = "F18",
        [(VirtualKey)0x82] = "F19",
        [(VirtualKey)0x83] = "F20",
        [(VirtualKey)0x84] = "F21",
        [(VirtualKey)0x85] = "F22",
        [(VirtualKey)0x86] = "F23",
        [(VirtualKey)0x87] = "F24",
        [(VirtualKey)0xAD] = "Volume Mute",
        [(VirtualKey)0xAE] = "Volume Down",
        [(VirtualKey)0xAF] = "Volume Up",
        [(VirtualKey)0xB0] = "Media Next",
        [(VirtualKey)0xB1] = "Media Prev",
        [(VirtualKey)0xB2] = "Media Stop",
        [(VirtualKey)0xB3] = "Media Play/Pause",
        [(VirtualKey)0xA6] = "Browser Back",
        [(VirtualKey)0xA7] = "Browser Forward",
        [(VirtualKey)0xA8] = "Browser Refresh",
        [(VirtualKey)0xA9] = "Browser Stop",
        [(VirtualKey)0xAA] = "Browser Search",
        [(VirtualKey)0xAB] = "Browser Favorites",
        [(VirtualKey)0xAC] = "Browser Home",
        [(VirtualKey)0xB4] = "Launch Mail",
        [(VirtualKey)0xB5] = "Launch Media",
        [(VirtualKey)0xB6] = "Launch App 1",
        [(VirtualKey)0xB7] = "Launch App 2",
        [(VirtualKey)0x5F] = "Sleep",
        [(VirtualKey)0x0C] = "Clear",
        [(VirtualKey)0x6C] = "Numpad Separator",
    };

    public static string GetFriendlyName(this VirtualKey key)
    {
        if (ExtendedKeyNames.TryGetValue(key, out var customName))
            return customName;

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

        // Check custom extended key mappings
        foreach (var (vk, name) in ExtendedKeyNames)
        {
            if (name.Equals(trimmed, System.StringComparison.OrdinalIgnoreCase))
            {
                resolvedKey = vk;
                return true;
            }
        }

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
        foreach (var vk in System.Enum.GetValues<VirtualKey>())
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
