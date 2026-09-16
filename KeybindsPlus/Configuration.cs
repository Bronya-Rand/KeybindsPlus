using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Configuration;
using KeybindsPlus.Models;

namespace KeybindsPlus
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        public List<KeybindEntry> Keybinds { get; set; } = [];
        public Dictionary<ShortcutAgents, KeybindAssignment> ShortcutKeybinds { get; set; } = [];
        public Dictionary<ChatChannels, KeybindAssignment> ChatKeybinds { get; set; } = [];
        public Dictionary<HotbarKeybinds, KeybindAssignment> HotbarKeybinds { get; set; } = [];
        public Dictionary<TargetingActions, KeybindAssignment> TargetingKeybinds { get; set; } = [];
        public Dictionary<string, Dictionary<string, KeybindAssignment>> ExternalPluginKeybinds { get; set; } = [];
        public bool HideUnsupportedKeybinds { get; set; } = false;
        public void Save()
        {
            PruneDefaults(ShortcutKeybinds);
            PruneDefaults(ChatKeybinds);
            PruneDefaults(HotbarKeybinds);
            PruneDefaults(TargetingKeybinds);
            Plugin.PluginInterface.SavePluginConfig(this);
        }
        private static void PruneDefaults<TKey>(Dictionary<TKey, KeybindAssignment> dict) where TKey : notnull
        {
            var toRemove = dict
                .Where(kvp => !kvp.Value.Enabled && kvp.Value.PrimaryKey.IsEmpty && kvp.Value.SecondaryKey.IsEmpty)
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var key in toRemove)
                dict.Remove(key);
        }
    }
}
