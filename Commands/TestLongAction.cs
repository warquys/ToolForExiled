#if DEBUG
using CommandSystem;
using ToolForExiled.PlayerActions;

namespace ToolForExiled.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class TestLongAction : ICommand
{
    public string Command => "TestLongAction";

    public string[] Aliases => new string[0];

    public string Description => "test";

    public bool SanitizeResponse => true;

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var player = Player.Get(sender);
        var longAction = new LongAction(20, 10, player)
        {
            CheckAction = (player) => true,
            EndAction = (player) =>
            {
                var longAction = new LongAction(10, 10, player)
                {
                    CheckAction = (player) => true,
                    EndAction = (player) => Log.Info("Action ended"),
                    ProgressActionShower = new PourcentageActionDisplayer()
                };
                longAction.StartAction();
                Log.Info("Action ended");
            },
            ProgressActionShower = new BarActionDisplayer()
            {
                BarLength = 15,
            }
        };
        longAction.StartAction();
        response = "";
        return true;
    }
}
#endif