using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace RecruitmentRefresher.Commands;

public unsafe class RefreshCommand : IDisposable
{
    private static readonly string COMMAND_NAME = "/rr";
    private static readonly string COMMAND_HELP_MESSAGE = "Refresh party finder recruitment";

    private static readonly int MAX_RETRY = 50;
    private static readonly int RETRY_DELAY = 50; //ms

    private static readonly uint RECRUITING_PARTY_MEMBER_ID = 26;

    private static readonly string OPEN_PARTY_FINDER_SIGNATURE = "40 53 48 83 EC 20 48 8B D9 E8 ?? ?? ?? ?? 84 C0 74 07 C6 83 ?? ?? ?? ?? ?? 48 83 C4 20 5B C3 CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC 40 53";
   
    private delegate void OpenPartyFinderDelegate(void* agentLfg, ulong contentId);

    private IChatGui ChatGui { get; init; }
    private IClientState ClientState { get; init; }
    private ICommandManager CommandManager { get; init; }
    private ICondition Condition { get; init; }
    private Config Config { get; init; }
    private OpenPartyFinderDelegate OpenPartyFinder { get; init; }

    public RefreshCommand(IChatGui chatGui, IClientState clientState, ICommandManager commandManager, ICondition condition, Config config, ISigScanner sigScanner)
    {
        ChatGui = chatGui;
        ClientState = clientState;
        CommandManager = commandManager;
        Condition = condition;
        Config = config;

        commandManager.AddHandler(COMMAND_NAME, new CommandInfo(OnCommand)
        {
            HelpMessage = COMMAND_HELP_MESSAGE
        });

        var openPartyFinderPtr = sigScanner.ScanText(OPEN_PARTY_FINDER_SIGNATURE);
        OpenPartyFinder = Marshal.GetDelegateForFunctionPointer<OpenPartyFinderDelegate>(openPartyFinderPtr);
    }

    public void Dispose()
    {
        CommandManager.RemoveHandler(COMMAND_NAME);
    }

    private void OnCommand(string command, string args)
    {
        Execute();
    }

    public void Execute()
    {
        if (ClientState.LocalPlayer?.OnlineStatus.ValueNullable?.RowId == RECRUITING_PARTY_MEMBER_ID)
        {
            Task.Run(() =>
            {
                OpenPartyFinder(AgentLookingForGroup.Instance(), ClientState.LocalContentId);
                if(!TryWaitFor<AddonMaster.LookingForGroupDetail>(out var groupDetail))
                {
                    ChatGui.PrintError("Failed to capture group detail window");
                    return;
                }
                groupDetail.JoinEdit();
                if (!TryWaitFor<AddonMaster.LookingForGroupCondition>(out var groupCondition))
                {
                    ChatGui.PrintError("Failed to capture group condition window");
                    return;
                };
                if (!TryRecruit(groupCondition))
                {
                    ChatGui.PrintError("Failed to click on recruit button");
                    return;
                }
            });
        }
    }

    private static bool TryWaitFor<T>(out T addonMaster) where T : IAddonMasterBase
    {
        for (var i = 0; !GenericHelpers.TryGetAddonMaster(out addonMaster) || !addonMaster.IsAddonReady; i++)
        {
            Thread.Sleep(RETRY_DELAY);
            if (i > MAX_RETRY)
            {
                return false;
            }
        }
        return true;
    }

    private static bool TryRecruit(AddonMaster.LookingForGroupCondition addon)
    {
        for (var i = 0; !addon.RecruitButton->IsEnabled; i++)
        {
            Thread.Sleep(RETRY_DELAY);
            if (i > MAX_RETRY)
            {
                return false;
            }
        }
        addon.Recruit();
        return true;
    }
}
