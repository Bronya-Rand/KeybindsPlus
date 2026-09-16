using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using KeybindsPlus.Models;

namespace KeybindsPlus.Windows.Components
{
    public sealed class KeybindEditorModal
    {
        private bool shouldOpenModal;
        private bool isOpen;
        private bool isCreating;
        private KeybindEntry? editingEntry;

        #region Editor Fields
        private string editingName = string.Empty;
        private string editingDescription = string.Empty;
        private KeybindActionType editingActionType = KeybindActionType.Command;
        private string editingCommand = string.Empty;
        private string editingMacroText = string.Empty;
        #endregion

        public Action<KeybindEntry>? OnSave { get; set; }

        private string? GetValidationMessage()
        {
            if (string.IsNullOrWhiteSpace(editingName))
                return "Keybind Name is required.";
            if (editingActionType == KeybindActionType.Command && string.IsNullOrWhiteSpace(editingCommand))
                return "Command cannot be empty (e.g. /gpose).";
            if (editingActionType == KeybindActionType.Macro && string.IsNullOrWhiteSpace(editingMacroText))
                return "Macro text cannot be empty.";
            return null;
        }

        private bool canSave => GetValidationMessage() == null;

        public void Open(KeybindEntry? entry = null)
        {
            editingEntry = entry;

            if (entry == null)
            {
                // Reset fields for new keybind
                editingName = string.Empty;
                editingDescription = string.Empty;
                editingActionType = KeybindActionType.Command;
                editingCommand = string.Empty;
                editingMacroText = string.Empty;
                isCreating = true;
            }
            else
            {
                // Populate fields with existing keybind data
                editingName = entry.Name;
                editingDescription = entry.Description;
                editingActionType = entry.ActionType;
                editingCommand = entry.Command;
                editingMacroText = entry.MacroText;
                isCreating = false;
            }

            isOpen = true;
            shouldOpenModal = true;
        }

        public void Draw()
        {
            if (!isOpen) return;

            var popupTitle = isCreating ? "Create Keybind" : "Edit Keybind";
            var modalId = $"{popupTitle}##KeybindEditorModal";

            if (shouldOpenModal)
            {
                ImGui.OpenPopup(modalId);
                shouldOpenModal = false;
            }

            var center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSizeConstraints(new Vector2(400, 260), new Vector2(float.MaxValue, float.MaxValue));

            using var modal = ImRaii.PopupModal(modalId, ref isOpen, ImGuiWindowFlags.AlwaysAutoResize);
            if (modal)
                DrawContent();
        }

        private void DrawContent()
        {
            // Keybind Name
            ImGui.Text("Keybind Name:");
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("###KeybindName", "e.g. Quick GPose", ref editingName, 100);

            ImGui.Spacing();

            // Keybind Description
            ImGui.Text("Description (Optional):");
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("###KeybindDescription", "e.g. Opens group pose mode", ref editingDescription, 200);

            ImGui.Spacing();

            // Action Type
            DrawActionTypeDropdown();
            ImGui.Spacing();

            // Action-specific fields
            DrawActionSpecificFields();
            ImGui.Spacing();

            ImGui.Separator();
            ImGui.Spacing();

            // Save and Cancel Buttons
            var validationMsg = GetValidationMessage();
            using (ImRaii.Disabled(!canSave))
            {
                if (ImGui.Button("Save", new Vector2(100, 0)))
                {
                    if (isCreating || editingEntry == null)
                    {
                        var newEntry = new KeybindEntry
                        {
                            Name = editingName.Trim(),
                            Description = editingDescription.Trim(),
                            ActionType = editingActionType,
                            Command = editingCommand.Trim(),
                            MacroText = editingMacroText.Trim(),
                        };
                        OnSave?.Invoke(newEntry);
                    }
                    else
                    {
                        // Mutate existing entry to preserve keys and enabled status
                        editingEntry.Name = editingName.Trim();
                        editingEntry.Description = editingDescription.Trim();
                        editingEntry.ActionType = editingActionType;
                        editingEntry.Command = editingCommand.Trim();
                        editingEntry.MacroText = editingMacroText.Trim();
                        OnSave?.Invoke(editingEntry);
                    }
                    isOpen = false;
                }
            }

            // Validation Tooltip on disabled Save button
            if (!canSave && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                using var tooltip = ImRaii.Tooltip();
                ImGui.TextColored(ImGuiColors.DalamudYellow, validationMsg ?? "Cannot save.");
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(100, 0)))
                isOpen = false;
        }

        private void DrawActionTypeDropdown()
        {
            ImGui.Text("Action Type:");
            ImGui.SetNextItemWidth(-1);

            var previewText = editingActionType.ToFriendlyString();

            using var combo = ImRaii.Combo("###ActionTypeCombo", previewText);
            if (combo)
            {
                foreach (var actionType in Enum.GetValues<KeybindActionType>())
                {
                    var isSelected = editingActionType == actionType;
                    if (ImGui.Selectable(actionType.ToFriendlyString(), isSelected))
                    {
                        editingActionType = actionType;
                    }
                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
            }
        }

        private void DrawActionSpecificFields()
        {
            switch (editingActionType)
            {
                case KeybindActionType.Command:
                    ImGui.Text("Command:");
                    ImGui.SetNextItemWidth(-1);
                    ImGui.InputTextWithHint("###CommandInput", "e.g. /gpose or /bm", ref editingCommand, 200);
                    break;

                case KeybindActionType.Macro:
                    var lineCount = string.IsNullOrEmpty(editingMacroText)
                        ? 0
                        : editingMacroText.Split('\n').Length;

                    ImGui.Text("Macro Text:");
                    ImGui.SameLine();
                    ImGui.TextDisabled($"({lineCount} line{(lineCount == 1 ? "" : "s")})");

                    ImGui.SetNextItemWidth(-1);
                    ImGui.InputTextMultiline("###MacroInput", ref editingMacroText, 1000, new Vector2(-1, 120));
                    break;
            }
        }
    }
}
