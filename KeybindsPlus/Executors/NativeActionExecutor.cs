using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using KeybindsPlus;
using KeybindsPlus.Helpers;

namespace KeybindsPlus.Executors
{
    public sealed unsafe class NativeActionExecutor
    {
        public void ExecuteWindow(List<AgentId> agentIds)
        {
            Plugin.Framework.RunOnFrameworkThread(() =>
            {
                var agentModule = AgentModule.Instance();
                if (agentModule == null)
                {
                    LogHelper.LogError("AgentModule.Instance() returned null, cannot execute window action.");
                    LogHelper.PrintUnavailable();
                    return;
                }

                foreach (var agentId in agentIds)
                {
                    var agent = agentModule->GetAgentByInternalId(agentId);
                    if (agent == null)
                    {
                        LogHelper.LogError($"AgentModule.GetAgentByInternalId({agentId}) returned null, cannot execute window action.");
                        LogHelper.PrintUnavailable();
                        return;
                    }

                    // Check if the agent window can be opened (progress block, duty, etc.)
                    var canOpen = agent->IsActivatable();
                    if (canOpen)
                    {
                        if (agent->IsAgentActive() || agent->IsAddonShown())
                            agent->Hide();
                        else
                            agent->Show();
                    }
                    else
                        LogHelper.PrintUnavailable();
                }
            });
        }
        public void ExecuteHotbarSlot(uint hotbarId, uint slotId)
        {
            Plugin.Framework.RunOnFrameworkThread(() =>
            {
                var hotbarModule = RaptureHotbarModule.Instance();
                if (hotbarModule == null)
                {
                    LogHelper.LogError("RaptureHotbarModule.Instance() returned null, cannot execute hotbar action.");
                    LogHelper.PrintUnavailable();
                    return;
                }

                var slot = hotbarModule->GetSlotById(hotbarId, slotId);
                if (slot == null)
                {
                    LogHelper.LogError($"RaptureHotbarModule.GetSlotById({hotbarId}, {slotId}) returned null, cannot execute hotbar action.");
                    LogHelper.PrintUnavailable();
                    return;
                }

                var isSlotUsable = slot->IsSlotUsable(slot->ApparentSlotType, slot->ApparentActionId);
                if (slot->IsEmpty)
                {
                    LogHelper.LogWarning($"Hotbar slot {hotbarId}:{slotId} is empty, cannot execute hotbar action.");
                    return;
                }
                else if (!isSlotUsable)
                {
                    LogHelper.LogError("Cannot execute action at this time. Slot is not usable.");
                    return;
                }

                hotbarModule->ExecuteSlot(slot);
            });
        }
        public void ExecutePetHotbarSlot(uint slotId)
        {
            Plugin.Framework.RunOnFrameworkThread(() =>
            {
                var hotbarModule = RaptureHotbarModule.Instance();
                if (hotbarModule == null)
                {
                    LogHelper.LogError("RaptureHotbarModule.Instance() returned null, cannot execute pet hotbar action.");
                    LogHelper.PrintUnavailable();
                    return;
                }

                var rawSlotId = (int)slotId;
                var petHotbar = &hotbarModule->PetHotbar;
                var slot = petHotbar->Slots[rawSlotId];
                if (slot.IsEmpty)
                {
                    LogHelper.LogWarning($"Pet hotbar slot {slotId} is empty, cannot execute pet hotbar action.");
                    return;
                }

                hotbarModule->ExecuteSlot(&slot);
            });
        }
        public void ExecuteDutyAction(uint actionId)
        {
            Plugin.Framework.RunOnFrameworkThread(() =>
            {
                var hotbarModule = RaptureHotbarModule.Instance();
                if (hotbarModule == null)
                {
                    LogHelper.LogError("RaptureHotbarModule.Instance() returned null, cannot execute duty action.");
                    LogHelper.PrintUnavailable();
                    return;
                }

                if (!hotbarModule->DutyActionsPresent)
                {
                    LogHelper.LogWarning("Duty actions are not present, cannot execute duty action.");
                    LogHelper.PrintUnavailable();
                    return;
                }

                hotbarModule->ExecuteDutyActionSlot(actionId);
            });
        }
    }
}
