using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using KeybindsPlus.Models;

namespace KeybindsPlus.Windows.Components;

public static class KeybindButton
{
    /// <summary>
    /// Renders a clickable keybind button that shows the current key chord,
    /// of a given keybind entry.
    /// </summary>
    /// <param name="id">A unique ImGui identifier.</param>
    /// <param name="keyChord">The key chord model.</param>
    /// <param name="entryName">Name of the parent keybind.</param>
    /// <param name="slotNumber">1 for Keybind 1, 2 for Keybind 2.</param>
    /// <param name="onSave">Callback to persist changes.</param>
    /// <param name="onClick">Optional override for left-click.</param>
    public static bool Draw(
        string id,
        KeyChord keyChord,
        string entryName,
        int slotNumber,
        Action? onSave = null,
        Action? onClick = null)
    {
        var displayString = keyChord.GetDisplayString();
        var isBound = !keyChord.IsEmpty;
        var buttonLabel = $"{displayString}##{id}";

        // Use available table column width with a minimum width
        var buttonWidth = Math.Max(120f, ImGui.GetContentRegionAvail().X);
        var buttonSize = new Vector2(buttonWidth, 0);

        // Dim text for "Not Bound" state
        using var textCol = ImRaii.PushColor(
            ImGuiCol.Text,
            isBound ? new Vector4(1.0f, 1.0f, 1.0f, 1.0f) : new Vector4(0.6f, 0.6f, 0.6f, 1.0f));

        var clicked = ImGui.Button(buttonLabel, buttonSize);

        // Left-Click Callback
        if (clicked)
            onClick?.Invoke();

        // Right-Click Callback
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            if (isBound)
            {
                keyChord.Clear();
                onSave?.Invoke();
            }
        }

        // Tooltip on Hover
        if (ImGui.IsItemHovered())
        {
            using (ImRaii.Tooltip())
            {
                ImGui.TextUnformatted($"{entryName} - Keybind {slotNumber}");
                ImGui.Separator();
                ImGui.TextColored(ImGuiColors.DalamudOrange, "Left-Click:");
                ImGui.SameLine();
                ImGui.TextUnformatted("Set / Record Keybind");
                ImGui.TextColored(ImGuiColors.DalamudYellow, "Right-Click:");
                ImGui.SameLine();
                ImGui.TextUnformatted("Clear (Set to Not Bound)");
            }
        }

        return clicked;
    }
}
