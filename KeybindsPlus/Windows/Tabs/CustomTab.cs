using System;
using System.Linq;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using KeybindsPlus.Extensions;
using KeybindsPlus.Models;

namespace KeybindsPlus.Windows.Tabs;

public class CustomTab
{
    private readonly MainWindow mainWindow;
    private string customSearchText = string.Empty;

    public CustomTab(MainWindow mainWindow)
    {
        this.mainWindow = mainWindow;
    }

    public void Draw()
    {
        using var tab = ImRaii.TabItem("Custom");
        if (!tab.Success) return;

        // Top Toolbar: [+] button + Search Bar
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Plus))
        {
            mainWindow.KeybindEditorModal.Open();
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Create a new custom command or macro keybind");

        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##CustomKeybinds_Search", "Search custom keybinds...", ref customSearchText, 100);

        ImGui.Spacing();

        var search = customSearchText;
        var filtered = mainWindow.Plugin.Configuration.Keybinds.Where(entry =>
        {
            if (search.IsNullOrEmpty()) return true;

            if (entry.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                entry.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                entry.ActionType.ToFriendlyString().Contains(search, StringComparison.OrdinalIgnoreCase))
                return true;

            if (entry.ActionType == KeybindActionType.Command &&
                entry.Command.Contains(search, StringComparison.OrdinalIgnoreCase))
                return true;

            if (entry.ActionType == KeybindActionType.Macro &&
                entry.MacroText.Contains(search, StringComparison.OrdinalIgnoreCase))
                return true;

            if (!entry.PrimaryKey.IsEmpty &&
                entry.PrimaryKey.GetDisplayString().Contains(search, StringComparison.OrdinalIgnoreCase))
                return true;

            if (!entry.SecondaryKey.IsEmpty &&
                entry.SecondaryKey.GetDisplayString().Contains(search, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }).ToList();

        if (filtered.Count == 0)
        {
            if (mainWindow.Plugin.Configuration.Keybinds.Count == 0)
                ImGui.TextDisabled("No custom keybinds configured. Click '+' above to create one.");
            else
                ImGui.TextDisabled("No custom keybinds match your search.");
            return;
        }

        using var table = ImRaii.Table("##CustomKeybindsTable", 4, MainWindow.TableFlags);
        if (!table) return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthStretch, 2f);
        ImGui.TableSetupColumn("Keybind 1", ImGuiTableColumnFlags.WidthFixed, 140f);
        ImGui.TableSetupColumn("Keybind 2", ImGuiTableColumnFlags.WidthFixed, 140f);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 80f);
        ImGui.TableHeadersRow();

        for (var i = 0; i < filtered.Count; i++)
        {
            var entry = filtered[i];
            using var rowId = ImRaii.PushId(entry.Id.ToString());

            ImGui.TableNextRow();

            // Column 0: Type Icon + Name + Tooltip Preview
            ImGui.TableNextColumn();
            DrawCustomEntryName(entry);

            // Column 1: Keybind 1
            ImGui.TableNextColumn();
            mainWindow.DrawKeybindSlot(
                entry.Name,
                entry.PrimaryKey,
                1,
                entry,
                entry.Enabled,
                updated => entry.PrimaryKey = updated);

            // Column 2: Keybind 2
            ImGui.TableNextColumn();
            mainWindow.DrawKeybindSlot(
                entry.Name,
                entry.SecondaryKey,
                2,
                entry,
                entry.Enabled,
                updated => entry.SecondaryKey = updated);

            // Column 3: Action Buttons (Enabled Checkbox, Edit, Delete)
            ImGui.TableNextColumn();
            using (ImRaii.Group())
            {
                var editingIsEnabled = entry.Enabled;
                if (ImGui.Checkbox("##Enabled", ref editingIsEnabled))
                {
                    entry.Enabled = editingIsEnabled;
                    mainWindow.Plugin.Configuration.Save();
                }
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("Toggle keybind on/off");

                ImGui.SameLine();
                if (ImGuiComponents.IconButton(FontAwesomeIcon.PencilAlt))
                {
                    mainWindow.KeybindEditorModal.Open(entry);
                }
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("Edit keybind");

                ImGui.SameLine();
                if (ImGuiComponents.IconButton(FontAwesomeIcon.Trash))
                {
                    mainWindow.Plugin.Configuration.Keybinds.Remove(entry);
                    mainWindow.Plugin.Configuration.Save();
                }
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("Delete keybind");
            }
        }
    }

    private static void DrawCustomEntryName(KeybindEntry entry)
    {
        ImGui.AlignTextToFramePadding();

        // Type Icon: Terminal for Command, Cogs for Macro
        var (icon, iconColor) = entry.ActionType == KeybindActionType.Command
            ? (FontAwesomeIcon.Terminal, ImGuiColors.DalamudViolet)
            : (FontAwesomeIcon.Cogs, ImGuiColors.ParsedGreen);

        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            ImGui.TextColored(iconColor, icon.ToIconString());
        }

        ImGui.SameLine();

        // Name with Status Coloring
        if (!entry.Enabled)
            ImGui.TextDisabled(entry.Name);
        else if (entry.HasGameInputOverride())
            ImGui.TextColored(ImGuiColors.DalamudYellow, entry.Name);
        else
            ImGui.TextUnformatted(entry.Name);

        // Hover Tooltip: Description + Command/Macro Preview
        if (ImGui.IsItemHovered())
        {
            using var tooltip = ImRaii.Tooltip();
            if (entry.HasGameInputOverride())
            {
                ImGui.TextColored(ImGuiColors.DalamudYellow, Constants.GameInputOverrideWarning);
                ImGui.Separator();
            }

            if (!string.IsNullOrEmpty(entry.Description))
            {
                ImGui.TextDisabled(entry.Description);
                ImGui.Separator();
            }

            if (entry.ActionType == KeybindActionType.Command)
            {
                ImGui.TextColored(ImGuiColors.DalamudViolet, "Command:");
                ImGui.TextUnformatted(!string.IsNullOrEmpty(entry.Command) ? entry.Command : "(None)");
            }
            else
            {
                ImGui.TextColored(ImGuiColors.ParsedGreen, "Macro:");
                ImGui.TextUnformatted(!string.IsNullOrEmpty(entry.MacroText) ? entry.MacroText : "(Empty)");
            }
        }
    }
}
