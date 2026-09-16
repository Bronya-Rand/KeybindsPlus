namespace KeybindsPlus.Models;

public sealed class ExternalActionDefinition
{
    public required string PluginName { get; init; }
    public required string ActionId { get; init; }
    public required string DisplayName { get; set; }
    public string Description { get; set; } = string.Empty;
    public KeyChord? SuggestedDefault { get; set; }
    public bool IsActive { get; set; } = true;
}
