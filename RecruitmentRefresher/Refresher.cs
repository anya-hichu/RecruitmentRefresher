using Dalamud.Plugin.Services;
using RecruitmentRefresher.Commands;
using System;

namespace RecruitmentRefresher;

public class Refresher : IDisposable
{
    private Config Config { get; init; }
    private IFramework Framework { get; init; }
    private RefreshCommand RefreshCommand { get; init; }

    private int RefreshCount { get; set; } = 0;
    private double MinsElapsed { get; set; } = 0;

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
        if (Config.Enabled && RefreshCommand.IsExecutable())
        {
            if (MinsElapsed >= Config.RefreshRate)
            {
                if (RefreshCount < Config.MaxRefresh)
                {
                    RefreshCommand.ExecuteTask();
                    RefreshCount++;
                }
                MinsElapsed = 0;
            }
            else
            {
                MinsElapsed += Framework.UpdateDelta.TotalMinutes;
            }
        }
        else
        {
            if (!RefreshCommand.IsExecutable())
            {
                RefreshCommand.ResetState();
            }
            RefreshCount = 0;
            MinsElapsed = 0;
        }
    }
}
