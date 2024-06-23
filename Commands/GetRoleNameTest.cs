#if DEBUG
using System.Xml.Linq;
using CommandSystem;

namespace ToolForExiled.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
[CommandHandler(typeof(GameConsoleCommandHandler))]
public class GetRoleNameTest : ICommand
{
    public string Command => "GetRoleNameTest";

    public string[] Aliases => new string[0];

    public string Description => "testRN";

    public bool SanitizeResponse => true;

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Contains("lst"))
        {
            response = "\n";
            response += string.Join("\n", ToolForExiledPlugin.Instance.Translation.RoleName
                .Select(p => $"NameNoSpace: {p.NameNoSpace}, NameSpaced: {p.NameSpaced}, {p.Role}"));
        }
        else
        {
            var player = Player.Get(sender);
            ToolForExiledPlugin.Instance.CassieConvertRoleName(player, out var withoutSpace, out var withSpace);
            response = $"withoutSpace {withoutSpace}, withSpace {withSpace}";
        }
        return true;
    }
}
#endif