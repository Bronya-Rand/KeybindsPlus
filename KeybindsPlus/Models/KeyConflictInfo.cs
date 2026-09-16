using System;

namespace KeybindsPlus.Models
{
    public class KeyConflictInfo
    {
        public string OwnerName { get; init; } = string.Empty;
        public int Slot { get; init; } = 1; // Keybind 1 or 2
        public bool IsNativeGameBind { get; init; }
        public Action? UnbindAction { get; init; }
    }
}
