using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using KeybindsPlus.Models;

namespace KeybindsPlus.Ipc;

public sealed class ExternalActionRegistry
{
    private readonly ConcurrentDictionary<(string PluginName, string ActionId), ExternalActionDefinition> definitions = new();

    public ExternalActionDefinition Register(
        string pluginName,
        string actionId,
        string displayName,
        string description = "",
        KeyChord? suggestedDefault = null)
    {
        var key = (pluginName, actionId);
        return definitions.AddOrUpdate(
            key,
            _ => new ExternalActionDefinition
            {
                PluginName = pluginName,
                ActionId = actionId,
                DisplayName = displayName,
                Description = description,
                SuggestedDefault = suggestedDefault,
                IsActive = true
            },
            (_, existing) =>
            {
                existing.DisplayName = displayName;
                existing.Description = description;
                if (suggestedDefault != null)
                    existing.SuggestedDefault = suggestedDefault;
                existing.IsActive = true;
                return existing;
            });
    }

    public void MarkInactive(string pluginName, string actionId)
    {
        if (definitions.TryGetValue((pluginName, actionId), out var def))
        {
            def.IsActive = false;
        }
    }

    public void MarkPluginInactive(string pluginName)
    {
        foreach (var def in definitions.Values.Where(d => d.PluginName == pluginName))
        {
            def.IsActive = false;
        }
    }

    public ExternalActionDefinition? GetDefinition(string pluginName, string actionId)
    {
        definitions.TryGetValue((pluginName, actionId), out var def);
        return def;
    }

    public string GetDisplayName(string pluginName, string actionId) =>
        definitions.TryGetValue((pluginName, actionId), out var def) ? def.DisplayName : actionId;

    public bool IsActive(string pluginName, string actionId) =>
        definitions.TryGetValue((pluginName, actionId), out var def) && def.IsActive;

    public IReadOnlyCollection<ExternalActionDefinition> GetAllDefinitions() => [.. definitions.Values];
}
