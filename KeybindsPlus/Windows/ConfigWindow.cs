using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace KeybindsPlus.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    public ConfigWindow(Plugin plugin) : base($"{Constants.PluginName} Configuration###QB_Settings")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(232, 90);
        SizeCondition = ImGuiCond.Always;

        configuration = plugin.Configuration;
    }

    public void Dispose() => GC.SuppressFinalize(this);

    public override void Draw()
    {
        var hideUnsupported = configuration.HideUnsupportedKeybinds;
        if (ImGui.Checkbox("Hide Unsupported Keybinds", ref hideUnsupported))
        {
            configuration.HideUnsupportedKeybinds = hideUnsupported;
            configuration.Save();
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip($"Hides game keybinds that are not supported by {Constants.PluginName}.");
    }
}
