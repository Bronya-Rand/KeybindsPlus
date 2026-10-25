using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using KeybindsPlus.Ipc;
using KeybindsPlus.Services;
using KeybindsPlus.Windows;

namespace KeybindsPlus;

public sealed class Plugin : IAsyncDalamudPlugin
{
    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    internal static IClientState ClientState { get; private set; } = null!;

    [PluginService]
    internal static IPluginLog Log { get; private set; } = null!;

    [PluginService]
    internal static IKeyState KeyState { get; private set; } = null!;

    [PluginService]
    internal static IChatGui ChatGui { get; private set; } = null!;

    [PluginService]
    internal static IFramework Framework { get; private set; } = null!;

    [PluginService]
    internal static ITargetManager TargetManager { get; private set; } = null!;

    [PluginService]
    internal static IPartyList PartyList { get; private set; } = null!;

    [PluginService]
    internal static IObjectTable ObjectTable { get; private set; } = null!;

    private const string CommandName = "/kp";

    public Configuration Configuration { get; private set; } = null!;

    public readonly WindowSystem WindowSystem = new(Constants.PluginName);
    private ConfigWindow ConfigWindow { get; set; } = null!;
    private MainWindow MainWindow { get; set; } = null!;

    public ExternalActionRegistry ExternalActionRegistry { get; private set; } = null!;
    internal IpcService IpcService { get; private set; } = null!;

    private InputInterceptorService inputInterceptorService = null!;
    private KeybindDispatchService keybindDispatchService = null!;

    public Task LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ExternalActionRegistry = new ExternalActionRegistry();
        var conflictService = new KeybindConflictService(Configuration, ExternalActionRegistry);
        IpcService = new IpcService(Configuration, ExternalActionRegistry, conflictService);

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        inputInterceptorService = new InputInterceptorService(Framework);
        inputInterceptorService.IsRecordingPredicate = () =>
            MainWindow.KeybindSetModal.IsListening;
        inputInterceptorService.OnKeyEvent += HandleKeyDown;

        keybindDispatchService = new KeybindDispatchService(Configuration, IpcService);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = $"Opens the {Constants.PluginName} window."
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        IpcService.Dispose();
        keybindDispatchService.Dispose();
        inputInterceptorService.OnKeyEvent -= HandleKeyDown;
        inputInterceptorService.Dispose();

        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;

        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);

        return ValueTask.CompletedTask;
    }

    private bool HandleKeyDown(VirtualKey vkCode, bool isDown) =>
        // If modal is actively listening, feed key event to it
        MainWindow.KeybindSetModal.HandleKeyEvent(vkCode, isDown) ||
        keybindDispatchService.HandleKeyEvent(vkCode, isDown);

    private void OnCommand(string command, string args) => MainWindow.Toggle();
    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
}
