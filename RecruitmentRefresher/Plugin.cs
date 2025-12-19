using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using RecruitmentRefresher.Windows;
using ECommons;
using Dalamud.Game;
using RecruitmentRefresher.Commands;

namespace RecruitmentRefresher;

public sealed class Plugin : IDalamudPlugin
{
    public static readonly string NAMESPACE = "RecruitmentRefresher";

    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static ICondition Condition { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;

    public Config Config { get; init; }

    public readonly WindowSystem WindowSystem = new(NAMESPACE);
    private ConfigWindow ConfigWindow { get; init; }

    private RefreshCommand RefreshCommand { get; init; }
    private Refresher Refresher { get; init; }

    public Plugin()
    {
        ECommonsMain.Init(PluginInterface, this);

        Config = PluginInterface.GetPluginConfig() as Config ?? new Config();

        ConfigWindow = new(Config);
        WindowSystem.AddWindow(ConfigWindow);
        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleConfigUI;

        RefreshCommand = new(ChatGui, PlayerState, CommandManager, Condition, Config, SigScanner);
        Refresher = new(Config, Framework, RefreshCommand);
    }

    public void Dispose()
    {
        Refresher.Dispose();
        RefreshCommand.Dispose();
        WindowSystem.RemoveAllWindows();
        ECommonsMain.Dispose();
    }

    private void DrawUI() => WindowSystem.Draw();
    public void ToggleConfigUI() => ConfigWindow.Toggle();
}
