using System;
using Dalamud.Plugin.Ipc;
using Dalamud.Utility;
using KeybindsPlus;
using KeybindsPlus.Helpers;
using KeybindsPlus.Models;
using KeybindsPlus.Services;

namespace KeybindsPlus.Ipc;

internal sealed class IpcService : IDisposable
{
    public const int ApiVersion = 1;

    public const string ApiVersionLabel = "QuickBind.ApiVersion";
    public const string RegisterActionLabel = "QuickBind.RegisterAction";
    public const string UnregisterActionLabel = "QuickBind.UnregisterAction";
    public const string UnregisterPluginLabel = "QuickBind.UnregisterPlugin";
    public const string ActionTriggeredLabel = "QuickBind.ActionTriggered";

    private readonly ICallGateProvider<int> apiVersionProvider;
    private readonly ICallGateProvider<string, string, string, string, string?, bool> registerActionProvider;
    private readonly ICallGateProvider<string, string, bool> unregisterActionProvider;
    private readonly ICallGateProvider<string, bool> unregisterPluginProvider;
    private readonly ICallGateProvider<string, string, object?> actionTriggeredProvider;

    private readonly Configuration configuration;
    private readonly ExternalActionRegistry registry;
    private readonly KeybindConflictService conflictService;

    public IpcService(
        Configuration configuration,
        ExternalActionRegistry registry,
        KeybindConflictService conflictService)
    {
        this.configuration = configuration;
        this.registry = registry;
        this.conflictService = conflictService;

        apiVersionProvider = Plugin.PluginInterface.GetIpcProvider<int>(ApiVersionLabel);
        apiVersionProvider.RegisterFunc(() => ApiVersion);

        registerActionProvider = Plugin.PluginInterface.GetIpcProvider<string, string, string, string, string?, bool>(RegisterActionLabel);
        registerActionProvider.RegisterFunc(RegisterAction);

        unregisterActionProvider = Plugin.PluginInterface.GetIpcProvider<string, string, bool>(UnregisterActionLabel);
        unregisterActionProvider.RegisterFunc(UnregisterAction);

        unregisterPluginProvider = Plugin.PluginInterface.GetIpcProvider<string, bool>(UnregisterPluginLabel);
        unregisterPluginProvider.RegisterFunc(UnregisterPlugin);

        actionTriggeredProvider = Plugin.PluginInterface.GetIpcProvider<string, string, object?>(ActionTriggeredLabel);
    }

    public void NotifyActionTriggered(string pluginName, string actionId)
    {
        try
        {
            actionTriggeredProvider.SendMessage(pluginName, actionId);
        }
        catch (Exception ex)
        {
            LogHelper.LogError(ex, $"Failed to send ActionTriggered IPC event for [{pluginName}] {actionId}");
        }
    }

    private bool RegisterAction(
        string pluginName,
        string actionId,
        string displayName,
        string description,
        string? defaultChordString)
    {
        if (pluginName.IsNullOrEmpty() || actionId.IsNullOrEmpty())
            return false;

        KeyChord? parsedDefault = null;
        if (!defaultChordString.IsNullOrEmpty() && KeyChord.TryParse(defaultChordString, out var parsed))
            parsedDefault = parsed;

        // Check if user already has an assignment configured
        if (configuration.ExternalPluginKeybinds.TryGetValue(pluginName, out var existingActions) &&
            existingActions.TryGetValue(actionId, out _))
        {
            // Preserve user's configured keys, but update runtime metadata and suggested default
            registry.Register(pluginName, actionId, displayName, description, parsedDefault);
            return true;
        }

        // Create assignment
        var newAssignment = new KeybindAssignment { Enabled = true };

        if (parsedDefault != null && !parsedDefault.IsEmpty)
        {
            // Check for conflict
            var conflict = conflictService.FindConflict(parsedDefault, newAssignment, 1);
            if (conflict != null)
            {
                // Leave unbound to prevent clobbering existing binds
                LogHelper.LogWarning(
                    $"[IPC] Action [{pluginName}] {actionId} suggested default {parsedDefault.GetDisplayString()} conflicts with {conflict.OwnerName} (Slot {conflict.Slot}). Left unbound.");
            }
            else
            {
                newAssignment.PrimaryKey = parsedDefault;
                LogHelper.LogInfo(
                    $"[IPC] Action [{pluginName}] {actionId} assigned default keybind: {parsedDefault.GetDisplayString()}");
            }
        }

        // Save assignment to configuration
        if (!configuration.ExternalPluginKeybinds.TryGetValue(pluginName, out var pluginActions))
        {
            pluginActions = [];
            configuration.ExternalPluginKeybinds[pluginName] = pluginActions;
        }
        pluginActions[actionId] = newAssignment;
        configuration.Save();

        // Register runtime metadata
        registry.Register(pluginName, actionId, displayName, description, parsedDefault);
        return true;
    }

    private bool UnregisterAction(string pluginName, string actionId)
    {
        registry.MarkInactive(pluginName, actionId);
        return true;
    }

    private bool UnregisterPlugin(string pluginName)
    {
        registry.MarkPluginInactive(pluginName);
        return true;
    }

    public void Dispose()
    {
        apiVersionProvider.UnregisterFunc();
        registerActionProvider.UnregisterFunc();
        unregisterActionProvider.UnregisterFunc();
        unregisterPluginProvider.UnregisterFunc();
    }
}
