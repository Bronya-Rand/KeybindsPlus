using System;
using System.Linq;
using Dalamud.Game.ClientState.Keys;
using KeybindsPlus;
using KeybindsPlus.Executors;
using KeybindsPlus.Extensions;
using KeybindsPlus.Interop;
using KeybindsPlus.Ipc;
using KeybindsPlus.Models;
using QuickBind.Executors;

namespace KeybindsPlus.Services
{
    public sealed class KeybindDispatchService : IDisposable
    {
        private readonly Configuration configuration;

        private readonly ChatCommandExecutor chatCommandExecutor;
        private readonly ChatChannelExecutor chatChannelExecutor;
        private readonly NativeActionExecutor oneShotExecutor;
        private readonly TargetingExecutor targetingExecutor;
        private readonly IpcService? ipcService;

        internal KeybindDispatchService(
            Configuration configuration,
            IpcService? ipcService = null)
        {
            this.chatCommandExecutor = new ChatCommandExecutor();
            this.chatChannelExecutor = new ChatChannelExecutor();
            this.oneShotExecutor = new NativeActionExecutor();
            this.targetingExecutor = new TargetingExecutor();
            this.configuration = configuration;
            this.ipcService = ipcService;
        }

        public bool HandleKeyEvent(VirtualKey key, bool isDown)
        {
            if (!Plugin.ClientState.IsLoggedIn) return false;
            if (!isDown) return false;
            if (ChatInterop.IsGameTextInputActive()) return false;

            var ctrl = Plugin.KeyState[VirtualKey.CONTROL];
            var shift = Plugin.KeyState[VirtualKey.SHIFT];
            var alt = Plugin.KeyState[VirtualKey.MENU];

            // Check Custom Keybinds
            var customKeybind = configuration.Keybinds.FirstOrDefault(k => k.Matches(key, ctrl, alt, shift));
            if (customKeybind != null)
            {
                DispatchCustomKeybind(customKeybind);
                return true;
            }

            // Check External Plugin Keybinds
            foreach (var (pluginName, actions) in configuration.ExternalPluginKeybinds)
            {
                foreach (var (actionId, assignment) in actions)
                {
                    if (!assignment.Enabled) continue;
                    if (assignment.Matches(key, ctrl, alt, shift))
                    {
                        ipcService?.NotifyActionTriggered(pluginName, actionId);
                        return true;
                    }
                }
            }

            // Check Targeting Keybinds
            foreach (var (action, assignment) in configuration.TargetingKeybinds)
            {
                if (!action.GetMetadata().IsSupported) continue;
                if (assignment.Matches(key, ctrl, alt, shift))
                {
                    targetingExecutor.Execute(action);
                    return true;
                }
            }

            // Check Shortcut Keybinds
            foreach (var (agent, assignment) in configuration.ShortcutKeybinds)
            {
                if (!agent.GetMetadata().IsSupported) continue;
                if (assignment.Matches(key, ctrl, alt, shift))
                {
                    var (agentIds, _) = agent.GetInfo();
                    oneShotExecutor.ExecuteWindow(agentIds);
                    return true;
                }
            }

            // Check Chat Keybinds
            foreach (var (channel, assignment) in configuration.ChatKeybinds)
            {
                if (!channel.GetMetadata().IsSupported) continue;
                if (assignment.Matches(key, ctrl, alt, shift))
                {
                    DispatchChatKeybind(channel);
                    return true;
                }
            }

            // Check Hotbar Keybinds
            foreach (var (hotbar, assignment) in configuration.HotbarKeybinds)
            {
                if (!hotbar.GetMetadata().IsSupported) continue;
                if (assignment.Matches(key, ctrl, alt, shift))
                {
                    var (hotbarId, slotId, _) = hotbar.GetInfo();
                    switch (hotbar)
                    {
                        case >= HotbarKeybinds.PetHotbar_1 and <= HotbarKeybinds.PetHotbar_12:
                            oneShotExecutor.ExecutePetHotbarSlot(slotId);
                            break;
                        case HotbarKeybinds.DutyAction_1:
                        case HotbarKeybinds.DutyAction_2:
                            oneShotExecutor.ExecuteDutyAction(slotId);
                            break;
                        default:
                            oneShotExecutor.ExecuteHotbarSlot(hotbarId, slotId);
                            break;
                    }
                    return true;
                }
            }

            return false;
        }
        private void DispatchCustomKeybind(KeybindEntry entry)
        {
            switch (entry.ActionType)
            {
                case KeybindActionType.Command:
                    ChatCommandExecutor.Execute(entry.Command);
                    break;
                case KeybindActionType.Macro:
                    chatCommandExecutor.ExecuteMacro(entry.MacroText);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown action type: {entry.ActionType}");
            }
        }
        private void DispatchChatKeybind(ChatChannels channel)
        {
            switch (channel)
            {
                //case ChatChannels.ReplyForward:
                //    chatChannelExecutor.ReplyTell();
                //    break;
                //case ChatChannels.NextCrossworldLinkshell:
                //    chatChannelExecutor.RotateCrossworldLinkshell(RotateMode.Forward);
                //    break;
                //case ChatChannels.PreviousCrossworldLinkshell:
                //    chatChannelExecutor.RotateCrossworldLinkshell(RotateMode.Back);
                //    break;
                //case ChatChannels.NextLinkshell:
                //    chatChannelExecutor.RotateLinkshell(RotateMode.Forward);
                //    break;
                //case ChatChannels.PreviousLinkshell:
                //    chatChannelExecutor.RotateLinkshell(RotateMode.Back);
                //    break;
                case ChatChannels.ReplyForward:
                case ChatChannels.ReplyBack:
                case ChatChannels.NextCrossworldLinkshell:
                case ChatChannels.PreviousCrossworldLinkshell:
                    Plugin.ChatGui.PrintError($"This action is not supported due to limitations in the game.");
                    break;
                default:
                    var (channelType, linkshellIdx, isPermanent) = channel.GetExecutionData();
                    chatChannelExecutor.SetChatChannel(channelType, linkshellIdx, isPermanent);
                    break;
            }
        }
        public void Dispose()
        {
            chatCommandExecutor.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
