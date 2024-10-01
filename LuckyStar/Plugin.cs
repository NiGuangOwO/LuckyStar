using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons;
using ECommons.DalamudServices;
using LuckyStar.Windows;

namespace LuckyStar;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/luckystar";
    public static Configuration Configuration;
    public readonly WindowSystem WindowSystem = new("LuckyStar");
    private MainWindow MainWindow { get; init; }

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        ECommonsMain.Init(pluginInterface, this);

        Configuration = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(Svc.PluginInterface);

        MainWindow = new MainWindow();
        WindowSystem.AddWindow(MainWindow);

        Svc.Commands.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "打开设置"
        });

        Svc.PluginInterface.UiBuilder.Draw += DrawUI;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += ToggleMainUI;
        Svc.PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        MainWindow.Dispose();
        Svc.Commands.RemoveHandler(CommandName);
        ECommonsMain.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleMainUI() => MainWindow.Toggle();
}
