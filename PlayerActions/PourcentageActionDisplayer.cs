
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace ToolForExiled.PlayerActions;

public class PourcentageActionDisplayer : IProgressActionDisplayer
{
    [Description("If false, hint will be use.")]
    public bool UseBroadcast { get; set; } = true;

    public string ProgressMessage { get; set; } = "%progress%";

    public void ClearLoading(Player player)
    {
        if (UseBroadcast)
        {
            player.ClearBroadcasts();
        }
        else
        {
            player.ShowHint(string.Empty, 0.1f);
        }
    }

    public void ShowLoading(Player player, float progress, float duration)
    {
        string message = Regex.Replace(ProgressMessage, "%progress%", Math.Ceiling(progress).ToString(), RegexOptions.IgnoreCase);

        if (UseBroadcast)
        {
            player.Broadcast(
                (ushort)Math.Min(Math.Ceiling(duration), ushort.MaxValue),
                message,
                shouldClearPrevious: true
                );
        }
        else
        {
            player.ShowHint(
                message,
                (ushort)Math.Min(Math.Ceiling(duration), ushort.MaxValue));
        }
    }
}
