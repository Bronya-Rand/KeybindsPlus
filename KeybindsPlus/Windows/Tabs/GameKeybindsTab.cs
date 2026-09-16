using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using KeybindsPlus.Extensions;
using KeybindsPlus.Models;
using KeybindsPlus.Windows;

namespace KeybindsPlus.Windows.Tabs;

public class GameKeybindsTab<TKey> where TKey : notnull
{
    private readonly MainWindow mainWindow;
    private readonly string tabTitle;
    private readonly string tableId;
    private readonly IEnumerable<TKey> items;
    private readonly Dictionary<TKey, KeybindAssignment> storage;
    private readonly Func<TKey, ActionMetadata> getMetadata;
    private string searchText = string.Empty;

    public GameKeybindsTab(
        MainWindow mainWindow,
        string tabTitle,
        string tableId,
        IEnumerable<TKey> items,
        Dictionary<TKey, KeybindAssignment> storage,
        Func<TKey, ActionMetadata> getMetadata)
    {
        this.mainWindow = mainWindow;
        this.tabTitle = tabTitle;
        this.tableId = tableId;
        this.items = items;
        this.storage = storage;
        this.getMetadata = getMetadata;
    }

    public void Draw()
    {
        using var tab = ImRaii.TabItem(tabTitle);
        if (!tab.Success) return;

        // Search Box
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint($"##{tableId}_Search", "Search...", ref searchText, 100);

        ImGui.Spacing();

        var searchValue = searchText;
        var filteredItems = items.Where(item =>
        {
            var meta = getMetadata(item);

            // Hide unsupported keybinds if the option is enabled
            if (!meta.IsSupported && mainWindow.Plugin.Configuration.HideUnsupportedKeybinds)
                return false;

            if (searchValue.IsNullOrEmpty())
                return true;

            // Match name
            if (meta.FriendlyName.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                meta.Category.GetFriendlyName().Contains(searchValue, StringComparison.OrdinalIgnoreCase))
                return true;

            // Match keybind
            if (storage.TryGetValue(item, out var assignment))
            {
                if (!assignment.PrimaryKey.IsEmpty &&
                    assignment.PrimaryKey.GetDisplayString().Contains(searchValue, StringComparison.OrdinalIgnoreCase))
                    return true;

                if (!assignment.SecondaryKey.IsEmpty &&
                    assignment.SecondaryKey.GetDisplayString().Contains(searchValue, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }).ToList();

        if (filteredItems.Count == 0)
        {
            ImGui.TextDisabled("No keybinds match your search.");
            return;
        }

        using var table = ImRaii.Table(tableId, 4, MainWindow.TableFlags);
        if (!table) return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthStretch, 2f);
        ImGui.TableSetupColumn("Keybind 1", ImGuiTableColumnFlags.WidthFixed, 140f);
        ImGui.TableSetupColumn("Keybind 2", ImGuiTableColumnFlags.WidthFixed, 140f);
        ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthFixed, 20f);
        ImGui.TableHeadersRow();

        foreach (var item in filteredItems)
        {
            var meta = getMetadata(item);

            if (!storage.TryGetValue(item, out var assignment))
            {
                assignment = new KeybindAssignment();
                storage[item] = assignment;
            }

            if (!meta.IsSupported)
                assignment.Enabled = false;

            using var rowId = ImRaii.PushId($"Row_{item}");
            ImGui.TableNextRow();

            // Keybind Name Column
            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            if (!meta.IsSupported)
            {
                ImGui.TextDisabled(meta.FriendlyName);
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.DalamudGrey, "(Unavailable)");
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(meta.UnsupportedReason ?? "This keybind is not available.");
            }
            else if (!assignment.Enabled)
                ImGui.TextDisabled(meta.FriendlyName);
            else if (assignment.HasGameInputOverride())
                ImGui.TextColored(ImGuiColors.DalamudYellow, meta.FriendlyName);
            else
                ImGui.TextUnformatted(meta.FriendlyName);

            if (ImGui.IsItemHovered() && assignment.HasGameInputOverride())
                ImGui.SetTooltip(Constants.GameInputOverrideWarning);

            // Keybind 1 Column
            ImGui.TableNextColumn();
            if (!meta.IsSupported)
            {
                ImGui.TextDisabled("—");
            }
            else
            {
                mainWindow.DrawKeybindSlot(
                    meta.FriendlyName,
                    assignment.PrimaryKey,
                    1,
                    assignment,
                    assignment.Enabled,
                    updated => assignment.PrimaryKey = updated);
            }

            // Keybind 2 Column
            ImGui.TableNextColumn();
            if (!meta.IsSupported)
            {
                ImGui.TextDisabled("—");
            }
            else
            {
                mainWindow.DrawKeybindSlot(
                    meta.FriendlyName,
                    assignment.SecondaryKey,
                    2,
                    assignment,
                    assignment.Enabled,
                    updated => assignment.SecondaryKey = updated);
            }

            // Enabled Checkbox Column
            ImGui.TableNextColumn();
            using (ImRaii.Disabled(!meta.IsSupported))
            {
                var enabled = assignment.Enabled;
                if (ImGui.Checkbox("##Enabled", ref enabled) && meta.IsSupported)
                {
                    assignment.Enabled = enabled;
                    mainWindow.Plugin.Configuration.Save();
                }
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("Toggle to allow the plugin to manage this game keybind.");
            }
        }
    }
}
