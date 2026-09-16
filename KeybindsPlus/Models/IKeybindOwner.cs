namespace KeybindsPlus.Models
{
    public interface IKeybindOwner
    {
        bool Enabled { get; set; }
        KeyChord PrimaryKey { get; set; }
        KeyChord SecondaryKey { get; set; }
    }
}
