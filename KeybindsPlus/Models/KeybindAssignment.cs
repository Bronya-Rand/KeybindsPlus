using System;
namespace KeybindsPlus.Models
{
    [Serializable]
    public class KeybindAssignment : IKeybindOwner
    {
        public bool Enabled { get; set; } = false;

        public KeyChord PrimaryKey { get; set; } = new();
        public KeyChord SecondaryKey { get; set; } = new();
    }
}
