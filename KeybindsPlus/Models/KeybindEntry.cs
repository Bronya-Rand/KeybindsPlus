using System;

namespace KeybindsPlus.Models
{
    [Serializable]
    public class KeybindEntry : IKeybindOwner
    {
        public bool Enabled { get; set; } = true;

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "New Keybind";
        public string Description { get; set; } = string.Empty;

        // XIV normally supports 2 keybinds per action
        public KeyChord PrimaryKey { get; set; } = new();
        public KeyChord SecondaryKey { get; set; } = new();

        public KeybindActionType ActionType { get; set; } = KeybindActionType.Command;

        // Command-specific properties
        public string Command { get; set; } = string.Empty;

        // Macro-specific properties
        public string MacroText { get; set; } = string.Empty;
    }
}
