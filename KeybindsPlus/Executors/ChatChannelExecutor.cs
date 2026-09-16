using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using KeybindsPlus.Helpers;

namespace KeybindsPlus.Executors
{
    public enum RotateMode : int
    {
        Back = -1,
        None = 0,
        Forward = 1
    }
    public sealed unsafe class ChatChannelExecutor
    {
        public void SetChatChannel(int channelIdx, uint? linkshellIdx, bool isPermanent = true)
        {
            Plugin.Framework.RunOnFrameworkThread(() =>
            {
                var chatModule = RaptureShellModule.Instance();
                if (chatModule == null)
                {
                    LogHelper.LogError("RaptureShellModule.Instance() returned null, cannot set chat channel.");
                    LogHelper.PrintUnavailable();
                    return;
                }

                var target = Utf8String.FromString("");
                chatModule->ChangeChatChannel(channelIdx, linkshellIdx ?? 0, target, isPermanent);
                target->Dtor(true);
            });
        }
        public void ReplyTell()
        {
            // TODO: Implement Chat Swap To Tell
        }
        public void RotateLinkshell(RotateMode mode) =>
            Plugin.Framework.RunOnFrameworkThread(() =>
            {
                var module = UIModule.Instance();
                if (module == null)
                {
                    LogHelper.LogError("UIModule.Instance() returned null, cannot rotate linkshell.");
                    LogHelper.PrintUnavailable();
                    return;
                }
                module->RotateLinkshellHistory((int)mode);
            });
        public void RotateCrossworldLinkshell(RotateMode mode) =>
            Plugin.Framework.RunOnFrameworkThread(() =>
            {
                var module = UIModule.Instance();
                if (module == null)
                {
                    LogHelper.LogError("UIModule.Instance() returned null, cannot rotate cross-world linkshell.");
                    LogHelper.PrintUnavailable();
                    return;
                }
                module->RotateCrossLinkshellHistory((int)mode);
            });
    }
}
