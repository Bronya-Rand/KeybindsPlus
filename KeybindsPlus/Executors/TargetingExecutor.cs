using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.UI.Arrays;
using KeybindsPlus;
using KeybindsPlus.Helpers;
using KeybindsPlus.Interop;
using KeybindsPlus.Models;
using static FFXIVClientStructs.FFXIV.Client.UI.Arrays.AllianceListNumberArray;
using static FFXIVClientStructs.FFXIV.Client.UI.Arrays.EnemyListNumberArray;

namespace KeybindsPlus.Executors
{
    public sealed unsafe class TargetingExecutor
    {
        private volatile int lastAllianceMemberIndex = -1; // For cycling through alliance members
        private volatile int lastEnmityIndex = -1; // For cycling through enmity list
        private List<uint> cachedEnemyCycleIds = []; // For cycling through enemies
        private DateTime lastEnemyCycleTime = DateTime.MinValue;
        private static readonly TimeSpan CycleResetTimeout = TimeSpan.FromSeconds(2.5f);

        public void Execute(TargetingActions action)
        {
            Plugin.Framework.RunOnFrameworkThread(() =>
            {
                switch (action)
                {
                    // Party List Targeting
                    case >= TargetingActions.TargetPartyMember1 and <= TargetingActions.TargetPartyMember8:
                        var partyIdx = (int)(action - TargetingActions.TargetPartyMember1);
                        TargetPartyMember(partyIdx);
                        break;
                    case TargetingActions.TargetAssist:
                        TargetAssist();
                        break;
                    case TargetingActions.TargetLastTarget:
                        TargetLastTarget();
                        break;
                    case TargetingActions.TargetLastEnemy:
                        ExecuteTargetPlaceholderCommand("<le>");
                        break;
                    case TargetingActions.TargetEnemyAttackingYou:
                        ExecuteTargetPlaceholderCommand("<la>");
                        break;
                    case TargetingActions.SetFocusTarget:
                        if (Plugin.TargetManager.Target != null)
                            Plugin.TargetManager.FocusTarget = Plugin.TargetManager.Target;
                        break;
                    case TargetingActions.TargetFocusTarget:
                        TargetFocusTarget();
                        break;
                    case TargetingActions.FaceTarget:
                        FaceTarget();
                        break;
                    case TargetingActions.TargetNearestEnemy:
                        TargetNearestEnemy();
                        break;
                    case TargetingActions.TargetNearestPC:
                        TargetNearestPC();
                        break;
                    case TargetingActions.TargetNearestNPC:
                        TargetNearestNPCObject();
                        break;
                    case TargetingActions.TargetPet:
                        ExecuteTargetPlaceholderCommand("<pet>");
                        break;
                    case TargetingActions.TargetOwnCompanion:
                        ExecuteTargetPlaceholderCommand("<c>");
                        break;

                    case >= TargetingActions.TargetAlliance1_Member1 and <= TargetingActions.TargetAlliance1_Member8:
                        var memberIdx1 = (int)(action - TargetingActions.TargetAlliance1_Member1);
                        TargetAllianceListMember(1, memberIdx1);
                        break;
                    case >= TargetingActions.TargetAlliance2_Member1 and <= TargetingActions.TargetAlliance2_Member8:
                        var memberIdx2 = (int)(action - TargetingActions.TargetAlliance2_Member1);
                        TargetAllianceListMember(2, memberIdx2);
                        break;
                    case >= TargetingActions.TargetEnemyPartyMember1 and <= TargetingActions.TargetEnemyPartyMember5: // Crystalline Conflict
                        var enemyPartyIdx = (int)(action - TargetingActions.TargetEnemyPartyMember1);
                        ExecuteTargetPlaceholderCommand($"<e{enemyPartyIdx + 1}>");
                        break;

                    case TargetingActions.CycleEnemiesNearToFar:
                    case TargetingActions.CycleEnemiesFarToNear:
                    case TargetingActions.CycleEnmityListUp:
                    case TargetingActions.CycleEnmityListDown:
                    case TargetingActions.CycleAllianceListNearToFar:
                    case TargetingActions.CycleAllianceListFarToNear:
                    case TargetingActions.CycleAllianceList1_NearToFar:
                    case TargetingActions.CycleAllianceList1_FarToNear:
                    case TargetingActions.CycleAllianceList2_NearToFar:
                    case TargetingActions.CycleAllianceList2_FarToNear:
                    case TargetingActions.SetTargetModeAll:
                    case TargetingActions.SetTargetModePCOnly:
                    case TargetingActions.SetTargetModePartyOnly:
                    case TargetingActions.SetTargetModeEnemyOnly:
                        ExecuteTargetingModeOrCycle(action);
                        break;
                }
            });
        }
        private static void TargetPartyMember(int partyIdx)
        {
            // Directly target the party member using the party list
            if (Plugin.PartyList.Length > partyIdx &&
                Plugin.PartyList[partyIdx] is { } targetObj)
            {
                Plugin.TargetManager.Target = targetObj.GameObject;
                return;
            }

            // Fallback to command
            ExecuteTargetPlaceholderCommand($"<{partyIdx + 1}>");
        }
        private static void TargetAssist()
        {
            if (Plugin.TargetManager.Target?.TargetObject is { } targetOfTarget)
            {
                Plugin.TargetManager.Target = targetOfTarget;
                return;
            }

            ExecuteTargetPlaceholderCommand("<tt>");
        }
        private static void TargetLastTarget()
        {
            if (Plugin.TargetManager.PreviousTarget is { } lastTarget)
            {
                Plugin.TargetManager.Target = lastTarget;
                return;
            }
            ExecuteTargetPlaceholderCommand("<lt>");
        }
        private static void TargetFocusTarget()
        {
            if (Plugin.TargetManager.FocusTarget is { } focusTarget)
            {
                Plugin.TargetManager.Target = focusTarget;
                return;
            }

            ExecuteTargetPlaceholderCommand("<f>");
        }
        private static void ExecuteTargetPlaceholderCommand(string placeholder)
        {
            var targetObj = PronounInterop.GetByPlaceholder(placeholder);
            if (targetObj != null)
                Plugin.TargetManager.Target = targetObj;
        }
        private void ExecuteTargetingModeOrCycle(TargetingActions action)
        {
            switch (action)
            {
                case TargetingActions.CycleEnemiesNearToFar:
                    CycleEnemies();
                    break;
                case TargetingActions.CycleEnemiesFarToNear:
                    CycleEnemies(true);
                    break;
                case TargetingActions.CycleEnmityListUp:
                    CycleEnmityList();
                    break;
                case TargetingActions.CycleEnmityListDown:
                    CycleEnmityList(true);
                    break;
                case TargetingActions.CycleAllianceListNearToFar:
                    CycleAllianceList(null);
                    break;
                case TargetingActions.CycleAllianceListFarToNear:
                    CycleAllianceList(null, true);
                    break;
                case TargetingActions.CycleAllianceList1_NearToFar:
                    CycleAllianceList(1);
                    break;
                case TargetingActions.CycleAllianceList1_FarToNear:
                    CycleAllianceList(1, true);
                    break;
                case TargetingActions.CycleAllianceList2_NearToFar:
                    CycleAllianceList(2);
                    break;
                case TargetingActions.CycleAllianceList2_FarToNear:
                    CycleAllianceList(2, true);
                    break;
                default:
                    LogHelper.LogError($"Unhandled targeting action: {action}");
                    break;
            }
        }
        private void TargetAllianceListMember(int allianceListNumber, int memberIdx) // 1 or 2 (or 3 for small alliance)
        {
            Plugin.Framework.RunOnFrameworkThread(() =>
            {
                var array = AllianceListNumberArray.Instance();
                if (array == null)
                {
                    LogHelper.LogError("AllianceListNumberArray.Instance() returned null, cannot get alliance list.");
                    LogHelper.PrintUnavailable();
                    return;
                }

                var groupIdx = allianceListNumber - 1; // Convert to 0-based index
                if (groupIdx < 0 || groupIdx >= array->PartyCount)
                {
                    LogHelper.LogError($"Invalid alliance list number: {allianceListNumber}. Must be 1 or {array->PartyCount}.");
                    LogHelper.PrintUnavailable();
                    return;
                }

                var group = array->Groups[groupIdx];
                if (memberIdx >= group.MemberCount)
                {
                    LogHelper.LogWarning($"Invalid member index: {memberIdx}. Alliance list {allianceListNumber} has {group.MemberCount} members.");
                    return;
                }

                // Pass member's GameObject to TargetManager
                var member = group.Members[memberIdx];
                if (member.EntityId == 0 || !member.IsTargetable) return;

                var memberGameObject = Plugin.ObjectTable.SearchByEntityId(member.EntityId);
                Plugin.TargetManager.Target = memberGameObject;
            });
        }

        /// <summary>
        /// Cycles through the members of the specified alliance list, targeting them in order.
        /// </summary>
        /// <param name="listNumber">The alliance list to cycle through, or null to cycle through both.</param>
        /// <param name="reverse">Whether to cycle between near-to-far or far-to-near.</param>
        private void CycleAllianceList(int? listNumber, bool reverse = false) // null: all alliance lists
        {
            var array = AllianceListNumberArray.Instance();
            if (array == null)
            {
                LogHelper.LogError("AllianceListNumberArray.Instance() returned null, cannot get alliance list group index.");
                LogHelper.PrintUnavailable();
                return;
            }

            int? listIdx = listNumber.HasValue ? listNumber.Value - 1 : null; // Convert to 0-based index if specified
            if (listIdx != null && (listIdx < 0 || listIdx >= array->PartyCount))
            {
                LogHelper.LogError($"Invalid alliance list number: {listNumber}. Must be 1 or {array->PartyCount}.");
                LogHelper.PrintUnavailable();
                return;
            }

            // Setup the alliance list to cycle through based on the specified list index
            var members = new List<AllianceListMemberNumberArray>();
            var groupIndices = listNumber.HasValue
                ? [listIdx!.Value]
                : new[] { 0, 1, 2 };

            foreach (var g in groupIndices)
            {
                if (g < 0 || g >= array->PartyCount) continue;
                var group = array->Groups[g];
                for (var m = 0; m < group.MemberCount; m++)
                    members.Add(group.Members[m]);
            }
            if (members.Count == 0)
            {
                LogHelper.LogWarning("No members found in the specified alliance list(s).");
                return;
            }
            if (lastAllianceMemberIndex >= members.Count)
                lastAllianceMemberIndex = -1; // Reset if the last index is out of bounds

            if (lastAllianceMemberIndex == -1)
            {
                var currentTargetId = Plugin.TargetManager.Target?.EntityId ?? 0u;

                // Get the index of the current target in the members list
                var currentIndex = members.FindIndex(m => m.EntityId == currentTargetId);
                lastAllianceMemberIndex = currentIndex != -1 ? currentIndex : -1;
            }

            var step = reverse ? -1 : 1;
            var startIdx = lastAllianceMemberIndex == -1
                ? (reverse ? members.Count - 1 : 0)
                : (((lastAllianceMemberIndex + step) % members.Count) + members.Count) % members.Count;

            for (var i = 0; i < members.Count; i++)
            {
                var candidateIdx = (((startIdx + (i * step)) % members.Count) + members.Count) % members.Count;
                var candidate = members[candidateIdx];

                if (candidate.EntityId != 0 && candidate.IsTargetable)
                {
                    var targetGameObject = Plugin.ObjectTable.SearchByEntityId(candidate.EntityId);
                    if (targetGameObject != null)
                    {
                        Plugin.TargetManager.SoftTarget = targetGameObject;
                        lastAllianceMemberIndex = candidateIdx;
                        return;
                    }
                }
            }

            LogHelper.LogWarning("No targetable members found in the specified alliance list(s).");
            lastAllianceMemberIndex = -1; // Reset if no targetable members found
            Plugin.TargetManager.Target = null;
        }
        private static void TargetNearestEnemy() =>
            TargetNearest(obj => !obj.IsDead &&
                (obj.ObjectKind == ObjectKind.BattleNpc || obj.ObjectKind == ObjectKind.Pc) &&
                TargetingInterop.CanAttack(obj));
        private static void TargetNearestPC() =>
            TargetNearest(obj => obj.ObjectKind == ObjectKind.Pc);
        private static void TargetNearestNPCObject() =>
            TargetNearest(obj => obj.ObjectKind is ObjectKind.EventNpc
                or ObjectKind.EventObj
                or ObjectKind.AreaObject
                or ObjectKind.GatheringPoint
                or ObjectKind.Aetheryte);
        private static void FaceTarget()
        {
            var player = Plugin.ObjectTable.LocalPlayer;
            var target = Plugin.TargetManager.Target;
            if (player == null || target == null) return;

            var diff = target.Position - player.Position;
            var angle = MathF.Atan2(diff.X, diff.Z);

            var playerCharacter = (Character*)player.Address;
            if (playerCharacter != null)
                playerCharacter->GameObject.Rotation = angle;
        }
        private void CycleEnemies(bool reverse = false)
        {
            var player = Plugin.ObjectTable.LocalPlayer;
            if (player == null) return;

            // Get all valid enemies
            var currentCandidates = Plugin.ObjectTable
                .Where(obj => obj.IsValid() &&
                    obj.IsTargetable &&
                    !obj.IsDead &&
                    (obj.ObjectKind == ObjectKind.BattleNpc || obj.ObjectKind == ObjectKind.Pc) &&
                    obj.EntityId != player.EntityId &&
                    TargetingInterop.WithinRange(player, obj) &&
                    TargetingInterop.CanAttack(obj) &&
                    TargetingInterop.InLineOfSight(obj))
                .ToList();

            if (currentCandidates.Count == 0)
            {
                LogHelper.LogWarning("No valid enemies found to cycle through.");
                cachedEnemyCycleIds.Clear();
                return;
            }
            var currentCandidateIds = currentCandidates.Select(obj => obj.EntityId).ToHashSet();

            // Reset cache if cache is empty or cycle timeout has passed
            var now = DateTime.UtcNow;
            if (now - lastEnemyCycleTime > CycleResetTimeout || cachedEnemyCycleIds.Count == 0)
            {
                cachedEnemyCycleIds = currentCandidates
                    .OrderBy(obj => TargetingInterop.GetDistanceToPlayer(player, obj))
                    .Select(obj => obj.EntityId)
                    .ToList();
            }
            else
            {
                // Remove any cached IDs that are no longer valid
                cachedEnemyCycleIds = cachedEnemyCycleIds
                    .Where(id => currentCandidateIds.Contains(id))
                    .ToList();

                // Add any new valid enemies to the cache
                var newEnemies = currentCandidates
                    .Where(obj => !cachedEnemyCycleIds.Contains(obj.EntityId))
                    .OrderBy(obj => TargetingInterop.GetDistanceToPlayer(player, obj))
                    .Select(obj => obj.EntityId);

                cachedEnemyCycleIds.AddRange(newEnemies);
            }
            lastEnemyCycleTime = now;
            if (cachedEnemyCycleIds.Count == 0) return;

            // Determine the current target's index in the cached list
            var currentTargetId = Plugin.TargetManager.Target?.EntityId ?? 0u;
            var currentIndex = cachedEnemyCycleIds.IndexOf(currentTargetId);

            int nextIndex;
            if (currentIndex == -1)
                nextIndex = reverse ? cachedEnemyCycleIds.Count - 1 : 0;
            else
            {
                var step = reverse ? -1 : 1;
                nextIndex = (currentIndex + step + cachedEnemyCycleIds.Count) % cachedEnemyCycleIds.Count;
            }

            // Target the next enemy in the cycle
            var nextTargetId = cachedEnemyCycleIds[nextIndex];
            var nextTarget = currentCandidates.FirstOrDefault(obj => obj.EntityId == nextTargetId) ??
                Plugin.ObjectTable.SearchByEntityId(nextTargetId);
            if (nextTarget != null)
                Plugin.TargetManager.Target = nextTarget;
        }
        private void CycleEnmityList(bool reverse = false)
        {
            var array = EnemyListNumberArray.Instance();
            if (array == null)
                return;

            var enemies = new List<EnemyListEnemyNumberArray>();
            for (var i = 0; i < array->EnemyCount; i++)
            {
                var enemy = array->Enemies[i];
                if (enemy.EntityId != 0)
                    enemies.Add(enemy);
            }
            if (lastEnmityIndex >= enemies.Count)
                lastEnmityIndex = -1; // Reset if the last index is out of bounds

            if (lastEnmityIndex == -1)
            {
                var currentTargetId = Plugin.TargetManager.Target?.EntityId ?? 0u;
                var currentIndex = enemies.FindIndex(e => e.EntityId == currentTargetId);
                lastEnmityIndex = currentIndex != -1 ? currentIndex : -1;
            }

            var step = reverse ? -1 : 1;
            var startIdx = lastEnmityIndex == -1
                ? (reverse ? enemies.Count - 1 : 0)
                : (((lastEnmityIndex + step) % enemies.Count) + enemies.Count) % enemies.Count;

            for (var i = 0; i < enemies.Count; i++)
            {
                var candidateIdx = (((startIdx + (i * step)) % enemies.Count) + enemies.Count) % enemies.Count;
                var candidate = enemies[candidateIdx];
                if (candidate.EntityId != 0)
                {
                    var targetGameObject = Plugin.ObjectTable.SearchByEntityId((uint)candidate.EntityId);
                    if (targetGameObject != null)
                    {
                        Plugin.TargetManager.SoftTarget = targetGameObject;
                        lastEnmityIndex = candidateIdx;
                        return;
                    }
                }
            }

            LogHelper.LogWarning("No targetable enemies found in the enemy list.");
            lastEnmityIndex = -1; // Reset if no targetable enemies found
            Plugin.TargetManager.Target = null;
        }
        private static void TargetNearest(Func<IGameObject, bool> filter)
        {
            var player = Plugin.ObjectTable.LocalPlayer;
            if (player == null) return;

            var nearest = Plugin.ObjectTable
                .Where(obj => obj.IsValid() && obj.IsTargetable &&
                    obj.EntityId != player.EntityId &&
                    TargetingInterop.WithinRange(player, obj) &&
                    filter(obj))
                .OrderBy(obj => TargetingInterop.GetDistanceToPlayer(player, obj))
                .FirstOrDefault();

            if (nearest != null && TargetingInterop.InLineOfSight(nearest))
                Plugin.TargetManager.Target = nearest;
        }
    }
}
