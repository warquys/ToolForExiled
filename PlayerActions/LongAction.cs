using System;
using System.Text.RegularExpressions;
using MEC;

namespace ToolForExiled.PlayerActions;

public class LongAction
{
    const float NetworkOffset = 0.5f;
    const float MinTimeBetweenFrame = 1f;

    private float LastDisplay = 0;

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
        TotalSlice = Math.Max(totalSlice, 1);
        TimeForEachSlice = timeForAllAction / totalSlice;
#if DEBUG
        Log.Info($"TimeForEachSlice {TimeForEachSlice}");
        Log.Info($"TotalSlice {TotalSlice}");
        Log.Info($"TotalSlice*TimeForEachSlice {TotalSlice * TimeForEachSlice}");
#endif
        Player = player;
        LastDisplay = Timing.LocalTime - (MinTimeBetweenFrame * 2);
    }

    public void StartAction()
    {
        Timing.RunCoroutine(ActionCoroutine().RerouteExceptions(ex =>
        {
            Log.Error($"Failed long action of {ex}, {Player}.");
        }), ToolForExiledPlugin.RoundRest_CoroutineTag);
    }

    private IEnumerator<float> ActionCoroutine()
    {
        for (int i = 0; i < TotalSlice; i++)
        {
            Progress = (i / (float)TotalSlice) * 100;

            if (LastDisplay + MinTimeBetweenFrame < Timing.LocalTime)
            { 
                try
                {
                    LastDisplay = Timing.LocalTime;
                    var time = Math.Max(MinTimeBetweenFrame, TimeForEachSlice) + NetworkOffset;
                    ProgressActionShower?.ShowLoading(Player, Progress, time);
                }
                catch (Exception e)
                {
                    Log.Error(e);
#if DEBUG
                Log.Error($"Progress {Progress}");
#endif
                    throw;
                }
            }

            yield return Timing.WaitForSeconds(TimeForEachSlice);

            if (CheckAction != null && !CheckAction(Player))
            {
                ProgressActionShower?.ClearLoading(Player);
                yield break;
            }
        }
#if DEBUG
        Log.Info("Action End");
#endif
        ProgressActionShower?.ShowLoading(Player, 100, TimeForEachSlice);
    
        if (EndAction != null)
        {
            EndAction(Player);
        }
    }
}
