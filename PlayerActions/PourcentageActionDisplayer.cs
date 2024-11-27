
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace ToolForExiled.PlayerActions;

public class PourcentageActionDisplayer : IProgressActionDisplayer
{
    public const string progress_Pattern = "%progress%";

    [Description("If false, hint will be use.")]
    public bool UseBroadcast { get; set; } = true;

    public string ProgressMessage { get; set; } = $"{progress_Pattern}";

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
        string message = ProgressMessage;
        ToolForExiledPlugin.Instance.IgnoreCaseReplaces(ref message, progress_Pattern, progress.ToString());
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
