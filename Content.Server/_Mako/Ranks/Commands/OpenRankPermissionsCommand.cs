using Content.Server._Mako.Ranks;
using Content.Server.Administration;
using Content.Server.EUI;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Player;

namespace Content.Server.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class OpenRankPermissionsCommand : LocalizedCommands
{
    [Dependency] private readonly EuiManager _euiManager = null!;

    public override string Command => "rankpermissions";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not ICommonSession player)
        {
            shell.WriteError("This command can only be used by players.");
            return;
        }

        var eui = new RankPermissionsEui();
        _euiManager.OpenEui(eui, player);
    }
}
