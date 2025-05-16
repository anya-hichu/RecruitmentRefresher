using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace RecruitmentRefresher.Windows;

public class ConfigWindow : Window
{
    private Config Config { get; init; }

    public ConfigWindow(Config config) : base("Recruitment Refresher - Config##configWindow")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new(360, 120),
            MaximumSize = new(360, 120)
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

        var verbose = Config.Verbose;
        if (ImGui.Checkbox("Verbose###verbose", ref verbose))
        {
            Config.Verbose = verbose;
            Config.Save();
        }
        ImGuiComponents.HelpMarker("Display message when recruitment comment is reapplied (client clears it sometimes)");
    }
}
