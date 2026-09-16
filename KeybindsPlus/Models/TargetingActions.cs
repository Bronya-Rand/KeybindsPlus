using System;

namespace KeybindsPlus.Models
{
    public enum TargetingActions
    {
        // Party Member Targetting
        TargetPartyMember1, TargetPartyMember2, TargetPartyMember3, TargetPartyMember4,
        TargetPartyMember5, TargetPartyMember6, TargetPartyMember7, TargetPartyMember8,

        // General / Assist Targetting
        TargetAssist, // Target of Target
        TargetLastTarget,
        TargetLastEnemy,
        TargetEnemyAttackingYou,
        TargetNearestEnemy,
        TargetNearestPC,
        TargetNearestNPC,
        TargetPet,
        TargetOwnCompanion,
        SetFocusTarget,
        TargetFocusTarget,
        FaceTarget,

        // Enemy / Enmity Cycling
        CycleEnemiesNearToFar,
        CycleEnemiesFarToNear,
        CycleEnmityListUp,
        CycleEnmityListDown,

        // Alliance Targetting
        TargetAlliance1_Member1, TargetAlliance1_Member2, TargetAlliance1_Member3, TargetAlliance1_Member4,
        TargetAlliance1_Member5, TargetAlliance1_Member6, TargetAlliance1_Member7, TargetAlliance1_Member8,

        TargetAlliance2_Member1, TargetAlliance2_Member2, TargetAlliance2_Member3, TargetAlliance2_Member4,
        TargetAlliance2_Member5, TargetAlliance2_Member6, TargetAlliance2_Member7, TargetAlliance2_Member8,

        // Alliance Cycling
        CycleAllianceListNearToFar,
        CycleAllianceListFarToNear,
        CycleAllianceList1_NearToFar, CycleAllianceList1_FarToNear,
        CycleAllianceList2_NearToFar, CycleAllianceList2_FarToNear,

        // PvP Targetting
        TargetEnemyPartyMember1,
        TargetEnemyPartyMember2,
        TargetEnemyPartyMember3,
        TargetEnemyPartyMember4,
        TargetEnemyPartyMember5,

        // Targeting Filters / Modes
        SetTargetModeAll,
        SetTargetModePCOnly,
        SetTargetModePartyOnly,
        SetTargetModeEnemyOnly,
    }
    public static class TargetingActionsExtensions
    {
        public static ActionMetadata GetMetadata(this TargetingActions action) =>
            action switch
            {
                TargetingActions.CycleEnemiesNearToFar => new("Cycle through Enemies (Nearest to Farthest)", ActionCategory.Targeting),
                TargetingActions.CycleEnemiesFarToNear => new("Cycle through Enemies (Farthest to Nearest)", ActionCategory.Targeting),

                TargetingActions.TargetPartyMember1 => new("Target Member 1 in Party List", ActionCategory.Targeting),
                TargetingActions.TargetPartyMember2 => new("Target Member 2 in Party List", ActionCategory.Targeting),
                TargetingActions.TargetPartyMember3 => new("Target Member 3 in Party List", ActionCategory.Targeting),
                TargetingActions.TargetPartyMember4 => new("Target Member 4 in Party List", ActionCategory.Targeting),
                TargetingActions.TargetPartyMember5 => new("Target Member 5 in Party List", ActionCategory.Targeting),
                TargetingActions.TargetPartyMember6 => new("Target Member 6 in Party List", ActionCategory.Targeting),
                TargetingActions.TargetPartyMember7 => new("Target Member 7 in Party List", ActionCategory.Targeting),
                TargetingActions.TargetPartyMember8 => new("Target Member 8 in Party List", ActionCategory.Targeting),

                TargetingActions.TargetPet => new("Target Pet/Familiar", ActionCategory.Targeting),
                TargetingActions.TargetOwnCompanion => new("Target Own Companion", ActionCategory.Targeting),
                TargetingActions.TargetAssist => new("Target the Target of Your Current Target", ActionCategory.Targeting),
                TargetingActions.TargetLastTarget => new("Target Last Target", ActionCategory.Targeting),
                TargetingActions.TargetLastEnemy => new("Target Last Enemy", ActionCategory.Targeting),
                TargetingActions.TargetEnemyAttackingYou => new("Target Enemy Attacking You", ActionCategory.Targeting),
                TargetingActions.TargetNearestEnemy => new("Target Nearest Enemy", ActionCategory.Targeting),
                TargetingActions.TargetNearestPC => new("Target Nearest PC", ActionCategory.Targeting),
                TargetingActions.TargetNearestNPC => new("Target Nearest NPC or Object", ActionCategory.Targeting),

                TargetingActions.SetFocusTarget => new("Set Focus Target", ActionCategory.Targeting),
                TargetingActions.TargetFocusTarget => new("Target Current Focus Target", ActionCategory.Targeting),
                TargetingActions.FaceTarget => new("Face Target", ActionCategory.Targeting),

                TargetingActions.SetTargetModeAll => new("Set Targeting Mode to All", ActionCategory.Targeting, false, "This action is currently not exposed by Dalamud nor ClientStructs"),
                TargetingActions.SetTargetModePCOnly => new("Set Targeting Mode to PC Only", ActionCategory.Targeting, false, "This action is currently not exposed by Dalamud nor ClientStructs"),
                TargetingActions.SetTargetModePartyOnly => new("Set Targeting Mode to Party Only", ActionCategory.Targeting, false, "This action is currently not exposed by Dalamud nor ClientStructs"),
                TargetingActions.SetTargetModeEnemyOnly => new("Set Targeting Mode to Enemy Only", ActionCategory.Targeting, false, "This action is currently not exposed by Dalamud nor ClientStructs"),

                TargetingActions.CycleEnmityListUp => new("Cycle Up through Enmity List", ActionCategory.Targeting),
                TargetingActions.CycleEnmityListDown => new("Cycle Down through Enmity List", ActionCategory.Targeting),

                TargetingActions.CycleAllianceListNearToFar => new("Cycle through Alliance List (Nearest to Farthest)", ActionCategory.Targeting),
                TargetingActions.CycleAllianceListFarToNear => new("Cycle through Alliance List (Farthest to Nearest)", ActionCategory.Targeting),
                TargetingActions.CycleAllianceList1_NearToFar => new("Cycle through Alliance List 1 (Nearest to Farthest)", ActionCategory.Targeting),
                TargetingActions.CycleAllianceList1_FarToNear => new("Cycle through Alliance List 1 (Farthest to Nearest)", ActionCategory.Targeting),
                TargetingActions.CycleAllianceList2_NearToFar => new("Cycle through Alliance List 2 (Nearest to Farthest)", ActionCategory.Targeting),
                TargetingActions.CycleAllianceList2_FarToNear => new("Cycle through Alliance List 2 (Farthest to Nearest)", ActionCategory.Targeting),

                TargetingActions.TargetAlliance1_Member1 => new("Target Alliance 1, Member 1", ActionCategory.Targeting),
                TargetingActions.TargetAlliance1_Member2 => new("Target Alliance 1, Member 2", ActionCategory.Targeting),
                TargetingActions.TargetAlliance1_Member3 => new("Target Alliance 1, Member 3", ActionCategory.Targeting),
                TargetingActions.TargetAlliance1_Member4 => new("Target Alliance 1, Member 4", ActionCategory.Targeting),
                TargetingActions.TargetAlliance1_Member5 => new("Target Alliance 1, Member 5", ActionCategory.Targeting),
                TargetingActions.TargetAlliance1_Member6 => new("Target Alliance 1, Member 6", ActionCategory.Targeting),
                TargetingActions.TargetAlliance1_Member7 => new("Target Alliance 1, Member 7", ActionCategory.Targeting),
                TargetingActions.TargetAlliance1_Member8 => new("Target Alliance 1, Member 8", ActionCategory.Targeting),
                TargetingActions.TargetAlliance2_Member1 => new("Target Alliance 2, Member 1", ActionCategory.Targeting),
                TargetingActions.TargetAlliance2_Member2 => new("Target Alliance 2, Member 2", ActionCategory.Targeting),
                TargetingActions.TargetAlliance2_Member3 => new("Target Alliance 2, Member 3", ActionCategory.Targeting),
                TargetingActions.TargetAlliance2_Member4 => new("Target Alliance 2, Member 4", ActionCategory.Targeting),
                TargetingActions.TargetAlliance2_Member5 => new("Target Alliance 2, Member 5", ActionCategory.Targeting),
                TargetingActions.TargetAlliance2_Member6 => new("Target Alliance 2, Member 6", ActionCategory.Targeting),
                TargetingActions.TargetAlliance2_Member7 => new("Target Alliance 2, Member 7", ActionCategory.Targeting),
                TargetingActions.TargetAlliance2_Member8 => new("Target Alliance 2, Member 8", ActionCategory.Targeting),

                TargetingActions.TargetEnemyPartyMember1 => new("Crystalline Conflict: Target Enemy Party, Member 1", ActionCategory.Targeting),
                TargetingActions.TargetEnemyPartyMember2 => new("Crystalline Conflict: Target Enemy Party, Member 2", ActionCategory.Targeting),
                TargetingActions.TargetEnemyPartyMember3 => new("Crystalline Conflict: Target Enemy Party, Member 3", ActionCategory.Targeting),
                TargetingActions.TargetEnemyPartyMember4 => new("Crystalline Conflict: Target Enemy Party, Member 4", ActionCategory.Targeting),
                TargetingActions.TargetEnemyPartyMember5 => new("Crystalline Conflict: Target Enemy Party, Member 5", ActionCategory.Targeting),
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };
    }
}
