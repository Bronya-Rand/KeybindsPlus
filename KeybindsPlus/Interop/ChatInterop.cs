using System;
using System.Text;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace KeybindsPlus.Interop
{
    public unsafe class ChatInterop
    {
        public static void SendMessageUnsafe(byte[] message)
        {
            var mes = Utf8String.FromSequence(message.NullTerminate());
            UIModule.Instance()->ProcessChatBoxEntry(mes);
            mes->Dtor(true);
        }
        public static void SendMessage(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            if (bytes.Length == 0)
                throw new ArgumentException("Message cannot be empty.", nameof(message));
            if (bytes.Length > 500)
                throw new ArgumentException("Message is too long.", nameof(message));
            if (message.Length != SanitizeText(message).Length)
                throw new ArgumentException("Message contains invalid characters.", nameof(message));

            SendMessageUnsafe(bytes);
        }
        public static bool IsGameTextInputActive()
        {
            try
            {
                // Check AtkInputManager for active text input
                var stage = AtkStage.Instance();
                if (stage != null && stage->AtkInputManager != null)
                {
                    if (stage->AtkInputManager->IsTextInputActive)
                        return true;
                }

                // Check UIModule for active text input if AtkInputManager doesn't indicate active input
                var uiModule = UIModule.Instance();
                if (uiModule != null)
                {
                    var raptureAtk = uiModule->GetRaptureAtkModule();
                    if (raptureAtk != null && raptureAtk->IsTextInputActive())
                        return true;
                }
            }
            catch { }
            return false;
        }
        private static string SanitizeText(string text)
        {
            var csText = Utf8String.FromString(text);
            csText->SanitizeString((AllowedEntities)0x27F); // Borrowed from ChatTwo
            var sanitized = csText->ToString();
            csText->Dtor(true);

            return sanitized;
        }
    }
}
