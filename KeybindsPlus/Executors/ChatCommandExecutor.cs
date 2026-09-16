using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using KeybindsPlus.Helpers;
using KeybindsPlus.Interop;

namespace KeybindsPlus.Executors
{
    public sealed partial class ChatCommandExecutor : IDisposable
    {
        private static readonly Regex WaitSuffixRegex = XIVWaitMacro();
        private CancellationTokenSource? currentMacroCts;
        private readonly Lock macroLock = new();

        public static void Execute(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;

            var trimmed = command.Trim();
            if (!trimmed.StartsWith('/'))
                trimmed = "/" + trimmed;

            Plugin.Framework.RunOnFrameworkThread(() =>
            {
                var dalamudCmdResult = Plugin.CommandManager.ProcessCommand(trimmed);
                if (!dalamudCmdResult)
                    ChatInterop.SendMessage(trimmed);
            });
        }

        public void ExecuteMacro(string macroText)
        {
            if (string.IsNullOrWhiteSpace(macroText)) return;

            CancellationToken token;
            lock (macroLock)
            {
                // In FFXIV, executing a new macro cancels the running one
                currentMacroCts?.Cancel();
                currentMacroCts?.Dispose();
                currentMacroCts = new CancellationTokenSource();
                token = currentMacroCts.Token;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    var lines = macroText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
                    foreach (var rawLine in lines)
                    {
                        if (token.IsCancellationRequested) break;

                        var line = rawLine.Trim();
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        if (line.StartsWith("//") || line.StartsWith('#')) continue;

                        // Check for standalone /wait <seconds>
                        if (line.StartsWith("/wait", StringComparison.OrdinalIgnoreCase))
                        {
                            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            var waitSec = 1.0f; // Default /wait is 1 second in FFXIV
                            if (parts.Length > 1)
                            {
                                var numStr = parts[1].Replace(',', '.');
                                float.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out waitSec);
                            }

                            if (waitSec > 0)
                            {
                                await Task.Delay((int)(waitSec * 1000), token);
                            }
                            continue;
                        }

                        // Check for suffix <wait.<seconds>>
                        var delayMs = 0;
                        var match = WaitSuffixRegex.Match(line);
                        if (match.Success)
                        {
                            var numStr = match.Groups[1].Value.Replace(',', '.');
                            if (float.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var suffixSec) && suffixSec > 0)
                            {
                                delayMs = (int)(suffixSec * 1000);
                            }
                            line = WaitSuffixRegex.Replace(line, "").Trim();
                        }

                        // If the line was only <wait.X>, delay and continue
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            if (delayMs > 0)
                                await Task.Delay(delayMs, token);
                            continue;
                        }

                        // Execute on Framework Thread
                        var finalCmd = line;
                        await Plugin.Framework.RunOnFrameworkThread(() =>
                        {
                            if (token.IsCancellationRequested) return;

                            if (finalCmd.StartsWith('/'))
                            {
                                var dalamudCmdResult = Plugin.CommandManager.ProcessCommand(finalCmd);
                                if (!dalamudCmdResult)
                                    ChatInterop.SendMessage(finalCmd);
                            }
                            else
                            {
                                // Plain text message output to current chat channel (e.g. "Hello party!")
                                ChatInterop.SendMessage(finalCmd);
                            }
                        });

                        // If line had a suffix delay, wait before processing next line
                        if (delayMs > 0)
                        {
                            await Task.Delay(delayMs, token);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    LogHelper.LogDebug("Macro execution was cancelled.");
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex, "Error during macro execution.");
                }
            }, token);
        }

        public void CancelMacro()
        {
            lock (macroLock)
            {
                if (currentMacroCts != null)
                {
                    LogHelper.LogDebug("Cancelling active macro.");
                    currentMacroCts.Cancel();
                    currentMacroCts.Dispose();
                    currentMacroCts = null;
                }
            }
        }

        public void Dispose()
        {
            CancelMacro();
            GC.SuppressFinalize(this);
        }

        [GeneratedRegex(@"<wait\.(\d+(?:[\.,]\d+)?)>", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
        private static partial Regex XIVWaitMacro();
    }
}
