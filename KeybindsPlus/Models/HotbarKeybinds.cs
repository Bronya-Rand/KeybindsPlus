namespace KeybindsPlus.Models
{
    public enum HotbarKeybinds
    {
        Hotbar1_1, Hotbar1_2, Hotbar1_3, Hotbar1_4, Hotbar1_5, Hotbar1_6,
        Hotbar1_7, Hotbar1_8, Hotbar1_9, Hotbar1_10, Hotbar1_11, Hotbar1_12,

        Hotbar2_1, Hotbar2_2, Hotbar2_3, Hotbar2_4, Hotbar2_5, Hotbar2_6,
        Hotbar2_7, Hotbar2_8, Hotbar2_9, Hotbar2_10, Hotbar2_11, Hotbar2_12,

        Hotbar3_1, Hotbar3_2, Hotbar3_3, Hotbar3_4, Hotbar3_5, Hotbar3_6,
        Hotbar3_7, Hotbar3_8, Hotbar3_9, Hotbar3_10, Hotbar3_11, Hotbar3_12,

        Hotbar4_1, Hotbar4_2, Hotbar4_3, Hotbar4_4, Hotbar4_5, Hotbar4_6,
        Hotbar4_7, Hotbar4_8, Hotbar4_9, Hotbar4_10, Hotbar4_11, Hotbar4_12,

        Hotbar5_1, Hotbar5_2, Hotbar5_3, Hotbar5_4, Hotbar5_5, Hotbar5_6,
        Hotbar5_7, Hotbar5_8, Hotbar5_9, Hotbar5_10, Hotbar5_11, Hotbar5_12,

        Hotbar6_1, Hotbar6_2, Hotbar6_3, Hotbar6_4, Hotbar6_5, Hotbar6_6,
        Hotbar6_7, Hotbar6_8, Hotbar6_9, Hotbar6_10, Hotbar6_11, Hotbar6_12,

        Hotbar7_1, Hotbar7_2, Hotbar7_3, Hotbar7_4, Hotbar7_5, Hotbar7_6,
        Hotbar7_7, Hotbar7_8, Hotbar7_9, Hotbar7_10, Hotbar7_11, Hotbar7_12,

        Hotbar8_1, Hotbar8_2, Hotbar8_3, Hotbar8_4, Hotbar8_5, Hotbar8_6,
        Hotbar8_7, Hotbar8_8, Hotbar8_9, Hotbar8_10, Hotbar8_11, Hotbar8_12,

        Hotbar9_1, Hotbar9_2, Hotbar9_3, Hotbar9_4, Hotbar9_5, Hotbar9_6,
        Hotbar9_7, Hotbar9_8, Hotbar9_9, Hotbar9_10, Hotbar9_11, Hotbar9_12,

        // Pet Hotbar
        PetHotbar_1, PetHotbar_2, PetHotbar_3, PetHotbar_4, PetHotbar_5, PetHotbar_6,
        PetHotbar_7, PetHotbar_8, PetHotbar_9, PetHotbar_10, PetHotbar_11, PetHotbar_12,

        // Duty Actions (apparently)
        DutyAction_1, DutyAction_2
    }
    public static class HotbarKeybindsExtensions
    {
        public static ActionMetadata GetMetadata(this HotbarKeybinds bind)
        {
            var (_, _, name) = bind.GetInfo();
            return new(name, ActionCategory.Hotbar);
        }

        public static (uint hotbarId, uint slotId, string friendlyName) GetInfo(this HotbarKeybinds bind)
        {
            switch (bind)
            {
                case HotbarKeybinds.DutyAction_1:
                    return (0, 0, "Duty Actions 1");
                case HotbarKeybinds.DutyAction_2:
                    return (0, 1, "Duty Actions 2");
                case >= HotbarKeybinds.PetHotbar_1 and <= HotbarKeybinds.PetHotbar_12:
                    var petRaw = (int)bind - (int)HotbarKeybinds.PetHotbar_1;
                    var petSlotId = (uint)(petRaw + 1); // Slot 1-12
                    return (0, petSlotId - 1, $"Pet Hotbar - Slot {petSlotId}");
                default:
                    var raw = (int)bind;
                    var hotbarId = (uint)((raw / 12) + 1); // Hotbar 1-10
                    var slotId = (uint)((raw % 12) + 1); // Slot 1-12
                    return (hotbarId - 1, slotId - 1, $"Hotbar {hotbarId} - Slot {slotId}");
            }
        }
    }
}
