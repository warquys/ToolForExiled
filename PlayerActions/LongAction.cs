using System;
using System.Text.RegularExpressions;
using MEC;

namespace ToolForExiled.PlayerActions;

public class LongAction
{
    const float NetworkOffset = 0.5f;

    /// <summary>
    /// Min 0, Max 100
    /// </summary>
    public float Progress { get; private set; }

    public int TotalSlice { get; }
    public float TimeForEachSlice { get; }
    public Player Player { get; }

    public Func<Player, bool> CheckAction { get; init; }
    public Action<Player> EndAction { get; init; }
    public IProgressActionDisplayer ProgressActionShower { get; init; }
    
    public LongAction(int totalSlice, float timeForAllAction, Player player)
    {
        Progress = 0;
        TotalSlice = totalSlice;
        TimeForEachSlice = (timeForAllAction / totalSlice) + NetworkOffset;
        Player = player;
    }

    public void StartAction()
    {
        Timing.RunCoroutine(ActionCoroutine(), ToolForExiledPlugin.RoundRest_CoroutineTag);
    }

    private IEnumerator<float> ActionCoroutine()
    {
        for (int i = 0; i < TotalSlice; i++)
        {
            Progress = i * 100 / TotalSlice;

            ProgressActionShower?.ShowLoading(Player, Progress, TimeForEachSlice);

            yield return Timing.WaitForSeconds(TimeForEachSlice);

            if (CheckAction != null && !CheckAction(Player))
            {
                ProgressActionShower?.ClearLoading(Player);
                yield break;
            }
        }

        ProgressActionShower?.ShowLoading(Player, 100, TimeForEachSlice);
    
        if (EndAction != null)
            EndAction(Player);
    }
}
