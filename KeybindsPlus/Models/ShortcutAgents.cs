using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace KeybindsPlus.Models
{
    public enum ShortcutAgents : uint
    {
        Inventory, // 6u (AgentId.Inventory)
        Emotes, // 21u (AgentId.Emote)
        ActionMenu, // 34u (AgentId.ActionMenu)
        Character, // 39u (AgentId.Status)
        Maps, //40u (AgentId.Map)
        Social, //56u (AgentId.Social)
        PartyMembers, // Fires AgentId.Social and AgentId.PartyMember
        ArmouryChest, // 90u (AgentId.ArmouryBoard)
        GearSets, // 98u (AgentId.GearSet)
        Achievement, // 103u (AgentId.Achievement)
        Companion, // 104u (AgentId.Buddy)
        PvpProfile, // 135u (AgentId.PvpProfile)
        MinionGuide, // 159u (AgentId.MinionNotebook)
        MountGuide, // 160u (AgentId.MountNotebook)
        GoldSaucer, // 185u (AgentId.GoldSaucer)
        Currency, // 203u (AgentId.Currency)
        ChocoboSaddlebag, // 300u (AgentId.InventoryBuddy)
        BlueMageSpellbook, // 325u (AgentId.AozNotebook)
        Collection, // 375u (AgentId.McGuffin)
        FashionAccessories, // 399u (AgentId.OrnamentNoteBook)
        Portraits, // 407u (AgentId.BannerList)
        AdventurePlate, // 413u (AgentId.CharaCard)
        Facewear, // 460u (AgentId.Glasses)
        Fellowship, // 354u (Agent.CircleList)
        FriendList, // Fires AgentId.Social and AgentId.Friendlist
        ContactList, // 268u (AgentId.ContactList)
        Blacklist, // 57u (AgentId.Blacklist)
        MuteList, // 58u (AgentId.Mutelist)
        TermFilter, // 461u (AgentId.TermFilter)
        //NoviceNetworkList, // TODO: Identify AgentId for Novice Network List
        PlayerSearch, // Fires AgentId.Social and AgentId.Search
        FellowshipFinder, // 360u (AgentId.CircleFinder)
        PartyFinder, // 130u (AgentId.LookingForGroup)
        ReadyCheck, // TODO: Check if AgentId.ReadyCheck is correct (137u)
        RecordReadyCheck, // 309u (AgentId.RecordReadyCheck)
        Countdown, // 251u (AgentId.CountDownSettingDialog)
        FreeCompany, // 79u (AgentId.FreeCompany)
        Housing, // 120u (AgentId.Housing)
        PvpTeam, // 283u (AgentId.PvpTeam)
        Linkshell, // 60u (AgentId.Linkshell)
        CrossWorldLinkshell, // 318u (AgentId.CrossWorldLinkShell)
        DutyFinder, // 54u (AgentId.ContentsFinder)
        RaidFinder, // 234u (AgentId.RaidFinder)
        VCDungeonFinder, // 440u (AgentId.VVDFinder)
        DutySupport, // 347u (AgentId.DawnStory)
        Trust, // 346u (AgentId.Dawn)
        Timers, // 106u (AgentId.ContentsTimer)
        Journal, // 33u (AgentId.QuestJournal)
        DutyRecorder, // 302u (AgentId.ContentsReplaySetting)
        NewGamePlus, // 351u (AgentId.QuestRedo)
        HallOfTheNovice, // 218u (AgentId.BeginnersMansionProblem)
        CraftingLog, // 25u (AgentId.RecipeNote)
        GatheringLog, // 24u (AgentId.GatheringNote)
        HuntingLog, // 74u (AgentId.MonsterNote)
        FishingLog, // 29u (AgentId.FishingNote)
        FishGuide, // 30u (AgentId.FishGuide)
        SightseeingLog, // 157u (AgentId.AdventureNotebook)
        ChallengeLog, // 136u (AgentId.ContentsNote)
        OrchestrionList, // 223u (AgentId.Orchestrion)
        Signs, // 35u (AgentId.Marker)
        Waymarks, // 138u (AgentId.FieldMarker)
        StrategyBoard, // 442u (AgentId.TofuList)
        Return, // 71u (AgentId.Return)
        Teleport, // 52u (AgentId.Teleport)
        //SystemMenu, // TODO: This doesn't use a AgentId but fires a addon to on AgentHud
        AetherCurrent, // 201u (AgentId.AetherCurrent)
        MountSpeed, // 275u (AgentId.MountSpeed)
        SharedFate, // 77u (AgentId.FateProgress)
        ActiveHelp, // 91u (AgentId.HowtoList)
        Recommendations, // 109u (AgentId.RecommendList)
        UserMacros, // 22u (AgentId.Macro)
        SupportDesk, // 99u (AgentId.SupportMain)
        OfficialSites, // 221u (AgentId.WebLauncher)
        Playguide, // 220u (AgentId.PlayGuide)
        CharacterConfiguration, // 16u (AgentId.ConfigCharacter)
        SystemConfiguration, // 12u (AgentId.Config)
        LogColors, // 14u (AgentId.ConfigLogColor)
        ButtonConfiguration, // 17u (AgentId.ConfigPadcustomize)
        SubcommandCustomization, // 217u (AgentId.ItemContextCustomize)
        Alarm, // 321u (AgentId.Alarm)
        CommandPanel, // 491u (AgentId.QuickPanel)
        MastersBestiary, // 500u (AgentId.XBMMonsterNotebook)
    }
    public static class ShortcutAgentsExtensions
    {
        public static ActionMetadata GetMetadata(this ShortcutAgents agent)
        {
            var (_, name) = agent.GetInfo();
            return new(name, ActionCategory.Shortcuts);
        }

        /// <summary>
        /// Returns the AgentId and friendly name for the given ShortcutAgents enum value.
        /// </summary>
        /// <remarks>
        /// Some actions require firing multiple agents, requiring a list of AgentIds to be returned.
        /// </remarks>
        /// <param name="agent"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static (List<AgentId> agentId, string friendlyName) GetInfo(this ShortcutAgents agent) => agent switch
        {
            ShortcutAgents.Inventory => ([AgentId.Inventory], "Inventory"),
            ShortcutAgents.Emotes => ([AgentId.Emote], "Emotes"),
            ShortcutAgents.ActionMenu => ([AgentId.ActionMenu], "Actions & Traits"),
            ShortcutAgents.Character => ([AgentId.Status], "Character"),
            ShortcutAgents.Maps => ([AgentId.Map], "Maps"),
            ShortcutAgents.Social => ([AgentId.Social], "Social"),
            ShortcutAgents.PartyMembers => ([AgentId.Social, AgentId.PartyMember], "Party Members"),
            ShortcutAgents.ArmouryChest => ([AgentId.ArmouryBoard], "Armoury Chest"),
            ShortcutAgents.GearSets => ([AgentId.GearSet], "Gear Sets"),
            ShortcutAgents.Achievement => ([AgentId.Achievement], "Achievements"),
            ShortcutAgents.Companion => ([AgentId.Buddy], "Companion"),
            ShortcutAgents.PvpProfile => ([AgentId.PvpProfile], "PvP Profile"),
            ShortcutAgents.MinionGuide => ([AgentId.MinionNotebook], "Minion Guide"),
            ShortcutAgents.MountGuide => ([AgentId.MountNotebook], "Mount Guide"),
            ShortcutAgents.GoldSaucer => ([AgentId.GoldSaucer], "Gold Saucer"),
            ShortcutAgents.Currency => ([AgentId.Currency], "Currency"),
            ShortcutAgents.ChocoboSaddlebag => ([AgentId.InventoryBuddy], "Chocobo Saddlebag"),
            ShortcutAgents.BlueMageSpellbook => ([AgentId.AozNotebook], "Blue Mage Spellbook"),
            ShortcutAgents.Collection => ([AgentId.McGuffin], "Collection"),
            ShortcutAgents.FashionAccessories => ([AgentId.OrnamentNoteBook], "Fashion Accessories"),
            ShortcutAgents.Portraits => ([AgentId.BannerList], "Portraits"),
            ShortcutAgents.AdventurePlate => ([AgentId.CharaCard], "Adventure Plate"),
            ShortcutAgents.Facewear => ([AgentId.Glasses], "Facewear"),
            ShortcutAgents.Fellowship => ([AgentId.CircleList], "Fellowship"),
            ShortcutAgents.FriendList => ([AgentId.Social, AgentId.Friendlist], "Friend List"),
            ShortcutAgents.ContactList => ([AgentId.ContactList], "Contact List"),
            ShortcutAgents.Blacklist => ([AgentId.Blacklist], "Blacklist"),
            ShortcutAgents.MuteList => ([AgentId.Mutelist], "Mute List"),
            ShortcutAgents.TermFilter => ([AgentId.TermFilter], "Term Filter"),
            ShortcutAgents.PlayerSearch => ([AgentId.Social, AgentId.Search], "Player Search"),
            ShortcutAgents.FellowshipFinder => ([AgentId.CircleFinder], "Fellowship Finder"),
            ShortcutAgents.PartyFinder => ([AgentId.LookingForGroup], "Party Finder"),
            ShortcutAgents.ReadyCheck => ([AgentId.ReadyCheck], "Ready Check"),
            ShortcutAgents.RecordReadyCheck => ([AgentId.RecordReadyCheck], "Record Ready Check"),
            ShortcutAgents.Countdown => ([AgentId.CountDownSettingDialog], "Countdown"),
            ShortcutAgents.FreeCompany => ([AgentId.FreeCompany], "Free Company"),
            ShortcutAgents.Housing => ([AgentId.Housing], "Housing"),
            ShortcutAgents.PvpTeam => ([AgentId.PvpTeam], "PvP Team"),
            ShortcutAgents.Linkshell => ([AgentId.Linkshell], "Linkshell"),
            ShortcutAgents.CrossWorldLinkshell => ([AgentId.CrossWorldLinkShell], "Cross-World Linkshell"),
            ShortcutAgents.DutyFinder => ([AgentId.ContentsFinder], "Duty Finder"),
            ShortcutAgents.RaidFinder => ([AgentId.RaidFinder], "Raid Finder"),
            ShortcutAgents.VCDungeonFinder => ([AgentId.VVDFinder], "V&C Dungeon Finder"),
            ShortcutAgents.DutySupport => ([AgentId.DawnStory], "Duty Support"),
            ShortcutAgents.Trust => ([AgentId.Dawn], "Trust"),
            ShortcutAgents.Timers => ([AgentId.ContentsTimer], "Timers"),
            ShortcutAgents.Journal => ([AgentId.QuestJournal], "Journal"),
            ShortcutAgents.DutyRecorder => ([AgentId.ContentsReplaySetting], "Duty Recorder"),
            ShortcutAgents.NewGamePlus => ([AgentId.QuestRedo], "New Game+"),
            ShortcutAgents.HallOfTheNovice => ([AgentId.BeginnersMansionProblem], "Hall of the Novice"),
            ShortcutAgents.CraftingLog => ([AgentId.RecipeNote], "Crafting Log"),
            ShortcutAgents.GatheringLog => ([AgentId.GatheringNote], "Gathering Log"),
            ShortcutAgents.HuntingLog => ([AgentId.MonsterNote], "Hunting Log"),
            ShortcutAgents.FishingLog => ([AgentId.FishingNote], "Fishing Log"),
            ShortcutAgents.FishGuide => ([AgentId.FishGuide], "Fish Guide"),
            ShortcutAgents.SightseeingLog => ([AgentId.AdventureNotebook], "Sightseeing Log"),
            ShortcutAgents.ChallengeLog => ([AgentId.ContentsNote], "Challenge Log"),
            ShortcutAgents.OrchestrionList => ([AgentId.Orchestrion], "Orchestrion"),
            ShortcutAgents.Signs => ([AgentId.Marker], "Signs"),
            ShortcutAgents.Waymarks => ([AgentId.FieldMarker], "Waymarks"),
            ShortcutAgents.StrategyBoard => ([AgentId.TofuList], "Strategy Board"),
            ShortcutAgents.Return => ([AgentId.Return], "Return"),
            ShortcutAgents.Teleport => ([AgentId.Teleport], "Teleport"),
            ShortcutAgents.AetherCurrent => ([AgentId.AetherCurrent], "Aether Currents"),
            ShortcutAgents.MountSpeed => ([AgentId.MountSpeed], "Mount Speed"),
            ShortcutAgents.SharedFate => ([AgentId.FateProgress], "Shared Fate"),
            ShortcutAgents.ActiveHelp => ([AgentId.HowtoList], "Active Help"),
            ShortcutAgents.Recommendations => ([AgentId.RecommendList], "Recommendations"),
            ShortcutAgents.UserMacros => ([AgentId.Macro], "User Macros"),
            ShortcutAgents.SupportDesk => ([AgentId.SupportMain], "Support Desk"),
            ShortcutAgents.OfficialSites => ([AgentId.WebLauncher], "Official Sites"),
            ShortcutAgents.Playguide => ([AgentId.PlayGuide], "Playguide"),
            ShortcutAgents.CharacterConfiguration => ([AgentId.ConfigCharacter], "Character Configuration"),
            ShortcutAgents.SystemConfiguration => ([AgentId.Config], "System Configuration"),
            ShortcutAgents.LogColors => ([AgentId.ConfigLogColor], "Log Colors"),
            ShortcutAgents.ButtonConfiguration => ([AgentId.ConfigPadcustomize], "Button Configuration"),
            ShortcutAgents.SubcommandCustomization => ([AgentId.ItemContextCustomize], "Subcommand Customization"),
            ShortcutAgents.Alarm => ([AgentId.Alarm], "Alarm"),
            ShortcutAgents.CommandPanel => ([AgentId.QuickPanel], "Command Panel"),
            ShortcutAgents.MastersBestiary => ([AgentId.XBMMonsterNotebook], "Master's Bestiary"),
            _ => throw new ArgumentOutOfRangeException(nameof(agent), agent, null)
        };
    }
}
