using System;
using System.Numerics;
using System.Text;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using KeybindsPlus.Models;
using QuickBind.Interop;

namespace KeybindsPlus.Windows.Components;

public class KeybindSetModal
{
    private const string ModalTitle = "Set Keybind##QB_KeybindSetModal";

    private bool shouldOpenModal;
    private bool isOpen;
    private bool isListening;

    private string targetTitle = "Keybind";
    private int slotNumber = 1;
    private KeyChord workingChord = new();
    private Action<KeyChord>? onChordSaved;

    #region Conflict Info
    private KeyConflictInfo? currentConflict;
    private Func<KeyChord, KeyConflictInfo?>? onConflict;
    #endregion

    public bool IsListening => isOpen && isListening;

    public void Open(
        string title,
        KeyChord sourceChord,
        int slot,
        Func<KeyChord, KeyConflictInfo?>? checkConflict,
        Action<KeyChord> onSave)
    {
        targetTitle = title;
        slotNumber = slot;
        onChordSaved = onSave;
        onConflict = checkConflict;
        workingChord = new KeyChord(sourceChord.Key, sourceChord.Ctrl, sourceChord.Shift, sourceChord.Alt);

        currentConflict = null;
        shouldOpenModal = true;
        isOpen = true;
        isListening = false;
    }

    public void Open(KeybindEntry entry,
        int slot,
        Func<KeyChord, KeyConflictInfo?>? checkConflict = null,
        Action? onSave = null)
    {
        var sourceChord = slot == 1 ? entry.PrimaryKey : entry.SecondaryKey;
        Open(entry.Name, sourceChord, slot,
        checkConflict,
        updated =>
        {
            if (slot == 1)
                entry.PrimaryKey = updated;
            else
                entry.SecondaryKey = updated;

            onSave?.Invoke();
        });
    }

    /// <summary>
    /// Feeds incoming key events to the modal when actively listening.
    /// </summary>
    public bool HandleKeyEvent(VirtualKey key, bool isDown)
    {
        if (!isOpen || !isListening || !isDown) return false;

        // Ignore standalone modifier keypresses while waiting for the main key
        if (key is VirtualKey.CONTROL or VirtualKey.LCONTROL or VirtualKey.RCONTROL
            or VirtualKey.SHIFT or VirtualKey.LSHIFT or VirtualKey.RSHIFT
            or VirtualKey.MENU or VirtualKey.LMENU or VirtualKey.RMENU)
        {
            return true;
        }

        // Grab current modifier states from the OS directly.
        // Plugin.KeyState relies on the game processing WM_KEYDOWN, but during
        // recording mode InputInterceptorService consumes those messages before
        // they reach the game, so Plugin.KeyState is stale for modifiers.
        var ctrl = NativeMethods.IsKeyDown(0x11);  // VK_CONTROL
        var shift = NativeMethods.IsKeyDown(0x10); // VK_SHIFT
        var alt = NativeMethods.IsKeyDown(0x12);   // VK_MENU

        // Assign the recorded chord
        workingChord = new KeyChord(key, ctrl, shift, alt);
        isListening = false; // Finished listening

        // Check for conflicts after recording
        currentConflict = onConflict?.Invoke(workingChord);
        return true;
    }

    public void Draw()
    {
        if (shouldOpenModal)
        {
            ImGui.OpenPopup(ModalTitle);
            shouldOpenModal = false;
        }

        if (!isOpen) return;

        // Center modal on screen
        var center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(350, 0), ImGuiCond.Appearing);

        const ImGuiWindowFlags modalFlags = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse;
        using var popup = ImRaii.PopupModal(ModalTitle, ref isOpen, modalFlags);
        if (!popup.Success)
        {
            isListening = false;
            return;
        }

        // Header Text
        ImGui.TextUnformatted($"Assign Inputs To: {targetTitle} (Keybind {slotNumber})");
        ImGui.Separator();
        ImGui.Spacing();

        // Input Box Row
        DrawInputRow();

        ImGui.Spacing();

        // Status / Helper hint
        if (isListening)
        {
            var isCtrl = NativeMethods.IsKeyDown(0x11);  // VK_CONTROL
            var isShift = NativeMethods.IsKeyDown(0x10); // VK_SHIFT
            var isAlt = NativeMethods.IsKeyDown(0x12);   // VK_MENU

            if (isCtrl || isShift || isAlt)
                ImGui.TextColored(new Vector4(1.0f, 0.8f, 0.2f, 1.0f), "Holding modifier... Press your primary key (e.g. F13, P, Mouse 4).");
            else
                ImGui.TextColored(new Vector4(0.4f, 0.8f, 1.0f, 1.0f), "Listening for keypress... (Press any key or key combo)");
        }
        else
        {
            ImGui.TextDisabled("Click the box above to record a new key combination.");
        }

        // Conflict Info
        if (currentConflict != null && !isListening)
        {
            ImGui.Spacing();
            if (currentConflict.IsNativeGameBind)
                ImGui.TextColored(ImGuiColors.DalamudYellow, $"'{workingChord.GetDisplayString()}' is already bound to a native game action.\nApplying this keybind will override the game's keybind.");
            else
                ImGui.TextColored(ImGuiColors.DalamudRed, $"{workingChord.GetDisplayString()} is already bound to \'{currentConflict.OwnerName}\' (Keybind {currentConflict.Slot}).");
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Save and Cancel Buttons
        DrawFooterButtons();
    }

    private void DrawInputRow()
    {
        var displayString = GetLiveDisplayString();

        // Highlight box with active border when listening
        using var borderCol = isListening
            ? ImRaii.PushColor(ImGuiCol.Border, new Vector4(1.0f, 0.8f, 0.2f, 1.0f))
            : null;
        using var borderStyle = isListening
            ? ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 2.0f)
            : null;

        // Keybind Button
        var buttonWidth = ImGui.GetContentRegionAvail().X - 110.0f;
        if (ImGui.Button($"{displayString}##RecordedKeySlot", new Vector2(buttonWidth, 32.0f)))
        {
            isListening = !isListening;
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(isListening ? "Listening... Press any key or click to stop." : "Click to record a new key combination.");
        }

        ImGui.SameLine();

        // Unbind Button
        using (ImRaii.PushColor(ImGuiCol.Button, new Vector4(0.6f, 0.2f, 0.2f, 0.7f), !workingChord.IsEmpty))
        {
            if (ImGui.Button("Unbind##UnbindKeySlot", new Vector2(100.0f, 32.0f)))
            {
                workingChord.Clear();
                currentConflict = null;
                isListening = false;
            }
        }
    }

    private void DrawFooterButtons()
    {
        using (ImRaii.Disabled(isListening))
        {
            var saveButtonLabel = currentConflict != null && !currentConflict.IsNativeGameBind
                ? "Unbind and Save" : "Save";

            if (ImGui.Button(saveButtonLabel))
            {
                currentConflict?.UnbindAction?.Invoke(); // Unbind the conflicting keybind
                onChordSaved?.Invoke(workingChord);
                isOpen = false;
                isListening = false;
                ImGui.CloseCurrentPopup();
            }
        }

        ImGui.SameLine();

        using (ImRaii.Disabled(isListening))
        {
            if (ImGui.Button("Cancel"))
            {
                isOpen = false;
                isListening = false;
                ImGui.CloseCurrentPopup();
            }
        }
    }

    private string GetLiveDisplayString()
    {
        if (!isListening)
        {
            return workingChord.IsEmpty ? "Not Bound" : workingChord.GetDisplayString();
        }

        var isCtrl = NativeMethods.IsKeyDown(0x11);  // VK_CONTROL
        var isShift = NativeMethods.IsKeyDown(0x10); // VK_SHIFT
        var isAlt = NativeMethods.IsKeyDown(0x12);   // VK_MENU

        if (isCtrl || isShift || isAlt)
        {
            var sb = new StringBuilder();
            if (isCtrl) sb.Append("Ctrl+");
            if (isAlt) sb.Append("Alt+");
            if (isShift) sb.Append("Shift+");
            sb.Append("...");
            return sb.ToString();
        }

        return workingChord.IsEmpty ? "< Press Any Key... >" : $"{workingChord.GetDisplayString()} (Press new key...)";
    }
}
