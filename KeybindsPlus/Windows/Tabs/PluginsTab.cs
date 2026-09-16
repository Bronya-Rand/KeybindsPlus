using System;
using System.Collections.Generic;
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

public class PluginsTab
{
    private readonly MainWindow mainWindow;
    private string pluginsSearchText = string.Empty;

    public PluginsTab(MainWindow mainWindow)
    {
        this.mainWindow = mainWindow;
    }

    public void Draw()
    {
        using var tab = ImRaii.TabItem("Plugins");
        if (!tab.Success) return;

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##Plugins_Search", "Search plugin keybinds...", ref pluginsSearchText, 100);

        ImGui.Spacing();

        // Ensure all active definitions in ExternalActionRegistry have an entry in Configuration
        foreach (var def in mainWindow.Plugin.ExternalActionRegistry.GetAllDefinitions())
        {
            if (!mainWindow.Plugin.Configuration.ExternalPluginKeybinds.TryGetValue(def.PluginName, out var pluginActions))
            {
                pluginActions = [];
                mainWindow.Plugin.Configuration.ExternalPluginKeybinds[def.PluginName] = pluginActions;
            }
            if (!pluginActions.ContainsKey(def.ActionId))
            {
                pluginActions[def.ActionId] = new KeybindAssignment { Enabled = true };
            }
        }

        if (mainWindow.Plugin.Configuration.ExternalPluginKeybinds.Count == 0)
        {
            ImGui.TextDisabled("No external plugins have registered keybinds yet.");
            ImGui.TextDisabled("Third-party plugins can register custom keybinds via KeybindsPlus's IPC.");
            return;
        }

        var search = pluginsSearchText;
        var pluginGroups = new List<(string PluginName, Dictionary<string, KeybindAssignment> Actions, List<(string ActionId, KeybindAssignment Assignment)> MatchingActions)>();

        foreach (var (pluginName, actions) in mainWindow.Plugin.Configuration.ExternalPluginKeybinds)
        {
            var matches = actions.Where(kvp =>
            {
                if (search.IsNullOrEmpty()) return true;

                if (pluginName.Contains(search, StringComparison.OrdinalIgnoreCase))
                    return true;

                var def = mainWindow.Plugin.ExternalActionRegistry.GetDefinition(pluginName, kvp.Key);
                var displayName = def?.DisplayName ?? kvp.Key;
                if (displayName.Contains(search, StringComparison.OrdinalIgnoreCase))
                    return true;

                if (def?.Description.Contains(search, StringComparison.OrdinalIgnoreCase) == true)
                    return true;

                if (!kvp.Value.PrimaryKey.IsEmpty &&
                    kvp.Value.PrimaryKey.GetDisplayString().Contains(search, StringComparison.OrdinalIgnoreCase))
                    return true;

                if (!kvp.Value.SecondaryKey.IsEmpty &&
                    kvp.Value.SecondaryKey.GetDisplayString().Contains(search, StringComparison.OrdinalIgnoreCase))
                    return true;

                return false;
            }).Select(kvp => (ActionId: kvp.Key, Assignment: kvp.Value)).ToList();

            if (matches.Count > 0)
            {
                pluginGroups.Add((pluginName, actions, matches));
            }
        }

        if (pluginGroups.Count == 0)
        {
            ImGui.TextDisabled("No plugin keybinds match your search.");
            return;
        }

        List<string>? emptyPluginsToRemove = null;

        foreach (var (pluginName, actions, matchingActions) in pluginGroups)
        {
            var anyActive = actions.Any(kvp => mainWindow.Plugin.ExternalActionRegistry.IsActive(pluginName, kvp.Key));
            var headerTitle = anyActive
                ? $"{pluginName}###PluginHeader_{pluginName}"
                : $"{pluginName} (Offline)###PluginHeader_{pluginName}";

            if (ImGui.CollapsingHeader(headerTitle, ImGuiTreeNodeFlags.DefaultOpen))
            {
                using var table = ImRaii.Table($"##PluginTable_{pluginName}", 4, MainWindow.TableFlags);
                if (table)
                {
                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthStretch, 2f);
                    ImGui.TableSetupColumn("Keybind 1", ImGuiTableColumnFlags.WidthFixed, 140f);
                    ImGui.TableSetupColumn("Keybind 2", ImGuiTableColumnFlags.WidthFixed, 140f);
                    ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 80f);
                    ImGui.TableHeadersRow();

                    List<string>? actionsToDelete = null;

                    for (var i = 0; i < matchingActions.Count; i++)
                    {
                        var (actionId, assignment) = matchingActions[i];
                        var def = mainWindow.Plugin.ExternalActionRegistry.GetDefinition(pluginName, actionId);
                        var isActive = def?.IsActive ?? false;
                        var displayName = def?.DisplayName ?? actionId;

                        using var rowId = ImRaii.PushId(actionId);
                        ImGui.TableNextRow();

                        // Column 0: Status + DisplayName + Tooltip
                        ImGui.TableNextColumn();
                        ImGui.AlignTextToFramePadding();

                        if (isActive)
                        {
                            ImGui.TextColored(ImGuiColors.ParsedGreen, "[Active]");
                        }
                        else
                        {
                            ImGui.TextColored(ImGuiColors.DalamudGrey, "[Offline]");
                        }
                        ImGui.SameLine();

                        if (!assignment.Enabled || !isActive)
                            ImGui.TextDisabled(displayName);
                        else if (assignment.HasGameInputOverride())
                            ImGui.TextColored(ImGuiColors.DalamudYellow, displayName);
                        else
                            ImGui.TextUnformatted(displayName);

                        if (ImGui.IsItemHovered())
                        {
                            using var tooltip = ImRaii.Tooltip();
                            if (assignment.HasGameInputOverride())
                            {
                                ImGui.TextColored(ImGuiColors.DalamudYellow, Constants.GameInputOverrideWarning);
                                ImGui.Separator();
                            }
                            ImGui.TextColored(ImGuiColors.DalamudViolet, $"Plugin: {pluginName}");
                            ImGui.TextColored(ImGuiColors.DalamudGrey, $"Action ID: {actionId}");
                            if (!string.IsNullOrEmpty(def?.Description))
                            {
                                ImGui.Separator();
                                ImGui.TextUnformatted(def.Description);
                            }
                            if (def?.SuggestedDefault != null && !def.SuggestedDefault.IsEmpty)
                            {
                                ImGui.Separator();
                                ImGui.TextDisabled($"Suggested Default: {def.SuggestedDefault.GetDisplayString()}");
                            }
                        }

                        // Column 1: Keybind 1
                        ImGui.TableNextColumn();
                        mainWindow.DrawKeybindSlot(
                            $"[{pluginName}] {displayName}",
                            assignment.PrimaryKey,
                            1,
                            assignment,
                            assignment.Enabled && isActive,
                            updated => assignment.PrimaryKey = updated);

                        // Column 2: Keybind 2
                        ImGui.TableNextColumn();
                        mainWindow.DrawKeybindSlot(
                            $"[{pluginName}] {displayName}",
                            assignment.SecondaryKey,
                            2,
                            assignment,
                            assignment.Enabled && isActive,
                            updated => assignment.SecondaryKey = updated);

                        // Column 3: Controls (Enabled checkbox, apply suggested default, or delete if offline)
                        ImGui.TableNextColumn();
                        using (ImRaii.Group())
                        {
                            var enabled = assignment.Enabled;
                            using (ImRaii.Disabled(!isActive))
                            {
                                if (ImGui.Checkbox("##Enabled", ref enabled) && isActive)
                                {
                                    assignment.Enabled = enabled;
                                    mainWindow.Plugin.Configuration.Save();
                                }
                                if (ImGui.IsItemHovered())
                                    ImGui.SetTooltip(isActive ? "Toggle keybind on/off" : "Plugin is offline");
                            }

                            if (!isActive)
                            {
                                ImGui.SameLine();
                                if (ImGuiComponents.IconButton(FontAwesomeIcon.Trash))
                                {
                                    actionsToDelete ??= [];
                                    actionsToDelete.Add(actionId);
                                }
                                if (ImGui.IsItemHovered())
                                    ImGui.SetTooltip("Remove saved keybind for this offline action");
                            }
                            else if (def?.SuggestedDefault != null && !def.SuggestedDefault.IsEmpty && assignment.PrimaryKey.IsEmpty)
                            {
                                ImGui.SameLine();
                                if (ImGuiComponents.IconButton(FontAwesomeIcon.Magic))
                                {
                                    assignment.PrimaryKey = def.SuggestedDefault;
                                    mainWindow.Plugin.Configuration.Save();
                                }
                                if (ImGui.IsItemHovered())
                                    ImGui.SetTooltip($"Apply suggested default: {def.SuggestedDefault.GetDisplayString()}");
                            }
                        }
                    }

                    if (actionsToDelete != null)
                    {
                        foreach (var delActionId in actionsToDelete)
                        {
                            actions.Remove(delActionId);
                        }
                        if (actions.Count == 0)
                        {
                            emptyPluginsToRemove ??= [];
                            emptyPluginsToRemove.Add(pluginName);
                        }
                        mainWindow.Plugin.Configuration.Save();
                    }
                }
            }
        }

        if (emptyPluginsToRemove != null)
        {
            foreach (var emptyPlugin in emptyPluginsToRemove)
            {
                mainWindow.Plugin.Configuration.ExternalPluginKeybinds.Remove(emptyPlugin);
            }
            mainWindow.Plugin.Configuration.Save();
        }
    }
}
