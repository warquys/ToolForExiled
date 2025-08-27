using NorthwoodLib.Pools;
using PlayerRoles;
using Respawning;
using Respawning.Waves;
using ToolForExiled.Wave;

namespace RespawnReplacer;

[HarmonyPatch]
public static class PatchWaveSync
{
    [HarmonyPatch(typeof(WaveUpdateMessage), nameof(WaveUpdateMessage.ServerSendUpdate))]
    [HarmonyPrefix]
    public static bool NoSync() => false;
   
    // Do not CALL THE FUKING EXILED EVENT for safty
    [HarmonyPatch(typeof(WaveSpawner), nameof(WaveSpawner.SpawnWave))]
    [HarmonyPrefix]
    public static bool SpawnWave(SpawnableWaveBase wave, ref List<ReferenceHub> __result)
    {
        if (wave is not IVtCustomWave vtCustomWave)
            return true;

        List<ReferenceHub> list = ListPool<ReferenceHub>.Shared.Rent();
        try
        {
            Team spawnableTeam = wave.TargetFaction.GetSpawnableTeam();
            List<ReferenceHub> availablePlayers = WaveSpawner.GetAvailablePlayers(spawnableTeam);
            int maxWaveSize = wave.MaxWaveSize;
            int num = Mathf.Min(availablePlayers.Count, maxWaveSize);
            if (num <= 0)
            {
                __result = list;
                return false;
            }
            vtCustomWave.GenerateUnit();

            IAnnouncedWave announcedWave = wave as IAnnouncedWave;
            if (announcedWave != null)
            {
                announcedWave.Announcement.PlayAnnouncement();
            }

            list.AddRange(availablePlayers.GetRange(0, num));
            vtCustomWave.SpawnPlayer(list);
        }
        catch (Exception e)
        {
            Log.Error($"Spawing {vtCustomWave.GetType().FullName}: " + e);
            return false;
        }
        __result = list;
        return false;
    }
}