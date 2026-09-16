using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Keys;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Client.UI;
using KeybindsPlus;
using KeybindsPlus.Ipc;
using KeybindsPlus.Models;

namespace KeybindsPlus.Services
{
    internal unsafe class KeybindConflictService(Configuration configuration, ExternalActionRegistry? registry = null)
    {
        private readonly Configuration configuration = configuration;
        private readonly ExternalActionRegistry? registry = registry;

        public KeyConflictInfo? FindConflict(KeyChord chord, object currentItem, int currentSlot)
        {
            if (chord.IsEmpty) return null;

            // 1. Check Custom Keybinds
            foreach (var entry in configuration.Keybinds)
            {
                if (ReferenceEquals(currentItem, entry))
                {
                    // Check across slots within the same entry
                    if (currentSlot == 1 && !entry.SecondaryKey.IsEmpty && entry.SecondaryKey.Equals(chord))
                        return new KeyConflictInfo
                        {
                            OwnerName = entry.Name,
                            Slot = 2,
                            IsNativeGameBind = false,
                            UnbindAction = () => entry.SecondaryKey.Clear()
                        };
                    if (currentSlot == 2 && !entry.PrimaryKey.IsEmpty && entry.PrimaryKey.Equals(chord))
                        return new KeyConflictInfo
                        {
                            OwnerName = entry.Name,
                            Slot = 1,
                            IsNativeGameBind = false,
                            UnbindAction = () => entry.PrimaryKey.Clear()
                        };
                    continue;
                }

                if (!entry.PrimaryKey.IsEmpty && entry.PrimaryKey.Equals(chord))
                    return new KeyConflictInfo
                    {
                        OwnerName = entry.Name,
                        Slot = 1,
                        IsNativeGameBind = false,
                        UnbindAction = () => entry.PrimaryKey.Clear()
                    };
                if (!entry.SecondaryKey.IsEmpty && entry.SecondaryKey.Equals(chord))
                    return new KeyConflictInfo
                    {
                        OwnerName = entry.Name,
                        Slot = 2,
                        IsNativeGameBind = false,
                        UnbindAction = () => entry.SecondaryKey.Clear()
                    };
            }

            // 2. Check Plugin Game Keybinds (Shortcuts, Chat, Hotbar, Targeting)
            var shortcutConflict = CheckAssignmentConflict(
                configuration.ShortcutKeybinds,
                agent => agent.GetMetadata(),
                chord, currentItem, currentSlot);
            if (shortcutConflict != null) return shortcutConflict;

            var chatConflict = CheckAssignmentConflict(
                configuration.ChatKeybinds,
                channel => channel.GetMetadata(),
                chord, currentItem, currentSlot);
            if (chatConflict != null) return chatConflict;

            var hotbarConflict = CheckAssignmentConflict(
                configuration.HotbarKeybinds,
                bind => bind.GetMetadata(),
                chord, currentItem, currentSlot);
            if (hotbarConflict != null) return hotbarConflict;

            var targetingConflict = CheckAssignmentConflict(
                configuration.TargetingKeybinds,
                action => action.GetMetadata(),
                chord, currentItem, currentSlot);
            if (targetingConflict != null) return targetingConflict;

            // 3. Check External Plugin Keybinds
            foreach (var (pluginName, actions) in configuration.ExternalPluginKeybinds)
            {
                foreach (var (actionId, assignment) in actions)
                {
                    var actionName = registry?.GetDisplayName(pluginName, actionId) ?? actionId;
                    var friendlyName = $"[{pluginName}] {actionName}";

                    if (ReferenceEquals(currentItem, assignment))
                    {
                        if (currentSlot == 1 && !assignment.SecondaryKey.IsEmpty && assignment.SecondaryKey.Equals(chord))
                            return new KeyConflictInfo
                            {
                                OwnerName = friendlyName,
                                Slot = 2,
                                IsNativeGameBind = false,
                                UnbindAction = () => assignment.SecondaryKey.Clear()
                            };
                        if (currentSlot == 2 && !assignment.PrimaryKey.IsEmpty && assignment.PrimaryKey.Equals(chord))
                            return new KeyConflictInfo
                            {
                                OwnerName = friendlyName,
                                Slot = 1,
                                IsNativeGameBind = false,
                                UnbindAction = () => assignment.PrimaryKey.Clear()
                            };
                        continue;
                    }

                    if (!assignment.PrimaryKey.IsEmpty && assignment.PrimaryKey.Equals(chord))
                        return new KeyConflictInfo
                        {
                            OwnerName = friendlyName,
                            Slot = 1,
                            IsNativeGameBind = false,
                            UnbindAction = () => assignment.PrimaryKey.Clear()
                        };
                    if (!assignment.SecondaryKey.IsEmpty && assignment.SecondaryKey.Equals(chord))
                        return new KeyConflictInfo
                        {
                            OwnerName = friendlyName,
                            Slot = 2,
                            IsNativeGameBind = false,
                            UnbindAction = () => assignment.SecondaryKey.Clear()
                        };
                }
            }

            // 4. Check for conflicts with in-game keybind system (UIInputData)
            return CheckGameKeybindConflict(chord);
        }

        private static KeyConflictInfo? CheckGameKeybindConflict(KeyChord chord)
        {
            if (chord.IsEmpty) return null;

            var inputData = UIInputData.Instance();
            if (inputData == null) return null;

            var gameKeybindSpan = inputData->GetKeybindSpan();
            foreach (var idx in Enumerable.Range(0, gameKeybindSpan.Length))
            {
                ref var keybind = ref gameKeybindSpan[idx];
                var keybindOne = keybind.KeySettings[0];
                var keybindTwo = keybind.KeySettings[1];

                if (IsKeybindConflict(chord, keybindOne))
                    return new KeyConflictInfo
                    {
                        OwnerName = $"Game Keybind {idx + 1}",
                        Slot = 1,
                        IsNativeGameBind = true,
                        UnbindAction = () => { }
                    };
                if (IsKeybindConflict(chord, keybindTwo))
                    return new KeyConflictInfo
                    {
                        OwnerName = $"Game Keybind {idx + 1}",
                        Slot = 2,
                        IsNativeGameBind = true,
                        UnbindAction = () => { }
                    };
            }
            return null;
        }
        public static bool IsGameKeybindConflict(KeyChord chord) => CheckGameKeybindConflict(chord) != null;

        private static KeyConflictInfo? CheckAssignmentConflict<TKey>(
            Dictionary<TKey, KeybindAssignment> dictionary,
            Func<TKey, ActionMetadata> getMetadata,
            KeyChord chord,
            object currentItem,
            int currentSlot) where TKey : notnull
        {
            foreach (var (key, assignment) in dictionary)
            {
                var friendlyName = getMetadata(key).FriendlyName;

                if (ReferenceEquals(currentItem, assignment) || Equals(currentItem, key))
                {
                    if (currentSlot == 1 && !assignment.SecondaryKey.IsEmpty && assignment.SecondaryKey.Equals(chord))
                        return new KeyConflictInfo
                        {
                            OwnerName = friendlyName,
                            Slot = 2,
                            IsNativeGameBind = false,
                            UnbindAction = () => assignment.SecondaryKey.Clear()
                        };
                    if (currentSlot == 2 && !assignment.PrimaryKey.IsEmpty && assignment.PrimaryKey.Equals(chord))
                        return new KeyConflictInfo
                        {
                            OwnerName = friendlyName,
                            Slot = 1,
                            IsNativeGameBind = false,
                            UnbindAction = () => assignment.PrimaryKey.Clear()
                        };
                    continue;
                }

                if (!assignment.PrimaryKey.IsEmpty && assignment.PrimaryKey.Equals(chord))
                    return new KeyConflictInfo
                    {
                        OwnerName = friendlyName,
                        Slot = 1,
                        IsNativeGameBind = false,
                        UnbindAction = () => assignment.PrimaryKey.Clear()
                    };
                if (!assignment.SecondaryKey.IsEmpty && assignment.SecondaryKey.Equals(chord))
                    return new KeyConflictInfo
                    {
                        OwnerName = friendlyName,
                        Slot = 2,
                        IsNativeGameBind = false,
                        UnbindAction = () => assignment.SecondaryKey.Clear()
                    };
            }

            return null;
        }

        private static bool IsKeybindConflict(KeyChord chord, KeySetting xivKeybind)
        {
            if (chord.IsEmpty) return false;
            if (xivKeybind.Key == SeVirtualKey.NO_KEY) return false;

            var xivKeybindCtrl = xivKeybind.KeyModifier.HasFlag(KeyModifierFlag.Ctrl);
            var xivKeybindShift = xivKeybind.KeyModifier.HasFlag(KeyModifierFlag.Shift);
            var xivKeybindAlt = xivKeybind.KeyModifier.HasFlag(KeyModifierFlag.Alt);
            var xivKeybindKey = (VirtualKey)xivKeybind.Key;

            return chord.Key == xivKeybindKey &&
                   chord.Ctrl == xivKeybindCtrl &&
                   chord.Shift == xivKeybindShift &&
                   chord.Alt == xivKeybindAlt;
        }
    }
}
