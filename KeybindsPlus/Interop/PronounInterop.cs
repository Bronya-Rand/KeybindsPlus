using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using KeybindsPlus.Helpers;

namespace KeybindsPlus.Interop
{
    internal static unsafe class PronounInterop
    {
        public static IGameObject? GetByPlaceholder(string placeholder)
        {
            var pronounModule = PronounModule.Instance();
            if (pronounModule == null)
            {
                LogHelper.LogError("PronounModule.Instance() returned null, cannot get pronoun placeholder.");
                return null;
            }
            var pronounObject = pronounModule->ResolvePlaceholder(placeholder, 0, 0);
            var pronounObjectPtr = (nint)pronounObject;
            return Plugin.ObjectTable.CreateObjectReference(pronounObjectPtr);
        }
    }
}
