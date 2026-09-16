namespace KeybindsPlus.Models
{
    public enum KeybindActionType
    {
        /// <summary>
        /// Represents a command action (e.g. /gpose, /penumbra, etc.).
        /// </summary>
        Command = 0,

        /// <summary>
        /// Multiple commands executed in sequence
        /// </summary>
        Macro = 1,
    }
    public static class KeybindActionTypeExtensions
    {
        public static string ToFriendlyString(this KeybindActionType actionType)
        {
            return actionType switch
            {
                KeybindActionType.Command => "Command",
                KeybindActionType.Macro => "Macro",
                _ => "Unknown"
            };
        }
    }
}
