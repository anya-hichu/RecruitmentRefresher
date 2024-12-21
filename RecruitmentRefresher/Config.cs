using Dalamud.Configuration;
using System;

namespace RecruitmentRefresher;

[Serializable]
public class Config : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool Enabled { get; set; } = true;
    public int RefreshRate { get; set; } = 30; // mins
    public bool Verbose { get; set; } = false;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
