using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using KeybindsPlus.Models;
using KeybindsPlus.Services;
using KeybindsPlus.Windows.Components;
using KeybindsPlus.Windows.Tabs;

namespace KeybindsPlus.Windows;

public class MainWindow : Window, IDisposable
{
    internal readonly Plugin Plugin;

    internal const ImGuiTableFlags TableFlags =
            ImGuiTableFlags.Borders |
            ImGuiTableFlags.RowBg |
            ImGuiTableFlags.Resizable |
            ImGuiTableFlags.ScrollY |
            ImGuiTableFlags.SizingStretchProp;

    internal readonly KeybindEditorModal KeybindEditorModal;
    internal readonly KeybindConflictService ConflictService;
    public readonly KeybindSetModal KeybindSetModal = new();

    private readonly CustomTab customTab;
    private readonly PluginsTab pluginsTab;
    private readonly GameKeybindsTab<TargetingActions> targetingTab;
    private readonly GameKeybindsTab<ShortcutAgents> shortcutsTab;
    private readonly GameKeybindsTab<ChatChannels> chatTab;
    private readonly GameKeybindsTab<HotbarKeybinds> hotbarTab;

    public MainWindow(Plugin plugin)
        : base($"{Constants.PluginName}##QBKeybindsPlus", ImGuiWindowFlags.None)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(650, 420),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.Plugin = plugin;
        this.ConflictService = new KeybindConflictService(plugin.Configuration, plugin.ExternalActionRegistry);

        KeybindEditorModal = new KeybindEditorModal()
        {
            OnSave = (entry) =>
            {
                if (!plugin.Configuration.Keybinds.Contains(entry))
                    plugin.Configuration.Keybinds.Add(entry);

                plugin.Configuration.Save();
            }
        };

        customTab = new CustomTab(this);
        pluginsTab = new PluginsTab(this);
        targetingTab = new GameKeybindsTab<TargetingActions>(
            this, "Targeting (XIV)", "##TargetingKeybindsTable",
            Enum.GetValues<TargetingActions>(),
            plugin.Configuration.TargetingKeybinds,
            action => action.GetMetadata());
        shortcutsTab = new GameKeybindsTab<ShortcutAgents>(
            this, "Shortcuts (XIV)", "##ShortcutKeybindsTable",
            Enum.GetValues<ShortcutAgents>(),
            plugin.Configuration.ShortcutKeybinds,
            agent => agent.GetMetadata());
        chatTab = new GameKeybindsTab<ChatChannels>(
            this, "Chat (XIV)", "##ChatKeybindsTable",
            Enum.GetValues<ChatChannels>(),
            plugin.Configuration.ChatKeybinds,
            channel => channel.GetMetadata());
        hotbarTab = new GameKeybindsTab<HotbarKeybinds>(
            this, "Hotbars (XIV)", "##HotbarKeybindsTable",
            Enum.GetValues<HotbarKeybinds>(),
            plugin.Configuration.HotbarKeybinds,
            bind => bind.GetMetadata());
    }

    public void Dispose() => GC.SuppressFinalize(this);

    public override void Draw()
    {
        using var tabBar = ImRaii.TabBar("##QB_MainTabs");
        if (tabBar.Success)
        {
            customTab.Draw();
            pluginsTab.Draw();
            targetingTab.Draw();
            shortcutsTab.Draw();
            chatTab.Draw();
            hotbarTab.Draw();
        }

        // Modals
        KeybindEditorModal.Draw();
        KeybindSetModal.Draw();
    }

    internal void DrawKeybindSlot(
        string title,
        KeyChord chord,
        int slot,
        object ownerItem,
        bool isEnabled,
        Action<KeyChord> onUpdate)
    {
        using var disabled = ImRaii.Disabled(!isEnabled);
        KeybindButton.Draw(
            $"Key{slot}",
            chord,
            title,
            slot,
            onSave: () => Plugin.Configuration.Save(),
            onClick: () => KeybindSetModal.Open(
                title,
                chord,
                slot,
                checkChord => ConflictService.FindConflict(checkChord, ownerItem, slot),
                updatedChord =>
                {
                    onUpdate(updatedChord);
                    Plugin.Configuration.Save();
                }));
    }
}
