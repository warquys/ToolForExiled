#if DEBUG
using System.Xml.Linq;
using CommandSystem;
using PlayerRoles;

namespace ToolForExiled.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class SetRoleTest : ICommand
{
    public string Command => "SetRoleTest";

    public string[] Aliases => new string[0];

    public string Description => "testSR";

    public bool SanitizeResponse => true;

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        new RoleInformation(PlayerRoles.RoleTypeId.Scp096).SetRole(
            Player.Get(sender),
            default(RoleSpawnFlags),
            default(SpawnReason));
        response = "Set class";
        return true;
    }
}
#endif