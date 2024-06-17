
using System.ComponentModel;
using System.Text;

namespace ToolForExiled.PlayerActions;

public class BarActionDisplayer : IProgressActionDisplayer
{
    private readonly StringBuilder stringBuilder = new StringBuilder();

    public int BarLength { get; set; } = 20;
    
    [Description("If false, hint will be use.")] 
    public bool UseBroadcast { get; set; } = true;

    public string LoadingStarBarString { get; set; } = "<mspace=0.65em>[";
    public char LoadingHeadChar { get; set; } = '>';
    public char LoadingFullChar { get; set; } = '=';
    public char LoadingEmptyChar { get; set; } = ' ';
    public string LoadingEndBarString { get; set; } = "]</mspace>";

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
        stringBuilder.Clear();
        if (progress == 100)
        {
            stringBuilder.Clear();
            stringBuilder.Append(LoadingStarBarString);
            stringBuilder.Append(new String(LoadingFullChar, BarLength));
            stringBuilder.Append(LoadingEndBarString);
        }
        else
        {
            var fullChar = (int)Math.Ceiling((progress / 100) * BarLength);
            var emptyChar = (int)BarLength - fullChar - 1; // -1 bc the Head Char
            stringBuilder.Append(LoadingStarBarString);
            stringBuilder.Append(new String(LoadingFullChar, fullChar));
            stringBuilder.Append(LoadingHeadChar);
            stringBuilder.Append(new String(LoadingEmptyChar, emptyChar));
            stringBuilder.Append(LoadingEndBarString);
        }

        if (UseBroadcast)
        {
            player.Broadcast(
                (ushort)Math.Min(Math.Ceiling(duration), ushort.MaxValue),
                stringBuilder.ToString(),
                shouldClearPrevious: true
                );
        }
        else
        {
            player.ShowHint(
                stringBuilder.ToString(),
                (ushort)Math.Min(Math.Ceiling(duration), ushort.MaxValue));
        }

    }
}
