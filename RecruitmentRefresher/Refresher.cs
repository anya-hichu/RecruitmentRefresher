using Dalamud.Plugin.Services;
using RecruitmentRefresher.Commands;
using System;

namespace RecruitmentRefresher;

public class Refresher : IDisposable
{
    private Config Config { get; init; }
    private IFramework Framework { get; init; }
    private RefreshCommand RefreshCommand { get; init; }

    private double SecsElapsed { get; set; } = 0;

    public Refresher(Config config, IFramework framework, RefreshCommand refreshCommand)
    {
        Config = config;
        Framework = framework;
        RefreshCommand = refreshCommand;

        Framework.Update += OnFrameworkUpdate;
    }

    public void Dispose()
    {
        Framework.Update -= OnFrameworkUpdate;
    }

    public void OnFrameworkUpdate(IFramework _)
    {
        if (Config.Enabled)
        {
            if (SecsElapsed >= Config.RefreshRate * 60)
            {
                RefreshCommand.Execute();
                SecsElapsed = 0;
            }
            else
            {
                SecsElapsed += Framework.UpdateDelta.TotalSeconds;
            }
        } 
        else
        {
            SecsElapsed = 0;
        }
       
    }
}
