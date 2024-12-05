using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace RecruitmentRefresher.Windows;

public class ConfigWindow : Window
{
    private Config Config { get; init; }

    public ConfigWindow(Config config) : base("Recruitment Refresher - Config###configWindow")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new(360, 100),
            MaximumSize = new(float.MaxValue, float.MaxValue)
        };

        Config = config;
    }

    public override void Draw()
    {
        var enabled = Config.Enabled;
        if (ImGui.Checkbox("Enabled###enabled", ref enabled))
        {
            Config.Enabled = enabled;
            Config.Save();
        }

        var refreshRate = Config.RefreshRate;
        if (ImGui.DragInt("Refresh Rate (mins)###refreshRate", ref refreshRate, 1, 1, 59, "%d", ImGuiSliderFlags.AlwaysClamp))
        {
            Config.RefreshRate = refreshRate;
            Config.Save();
        }
    }
}
