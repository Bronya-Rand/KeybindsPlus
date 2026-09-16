using System;

namespace KeybindsPlus.Models
{
    public enum ActionCategory
    {
        None,
        Movement,
        Targeting,
        Shortcuts,
        Chat,
        System,
        Hotbar
    }
    public static class ActionCategoryExtensions
    {
        public static string GetFriendlyName(this ActionCategory category)
        {
            return category switch
            {
                ActionCategory.None => "None",
                ActionCategory.Movement => "Movement",
                ActionCategory.Targeting => "Targeting",
                ActionCategory.Shortcuts => "Shortcuts",
                ActionCategory.Chat => "Chat",
                ActionCategory.System => "System",
                ActionCategory.Hotbar => "Hotbar",
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
            };
        }
    }
    public readonly record struct ActionMetadata
    (
        string FriendlyName,
        ActionCategory Category,
        bool IsSupported = true,
        string? UnsupportedReason = null
    );
}
