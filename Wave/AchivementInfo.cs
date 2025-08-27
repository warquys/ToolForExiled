using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerRoles;
using Respawning.Waves.Generic;
using Respawning;
using ToolForExiled;

namespace ToolForExiled.Wave;

public record struct AchievementInfo(FactionTypeSystem FactionSystem, uint FactionId, int Influence, int Token)
{
    public AchievementInfo() : this(Faction.FoundationStaff, 0, 0)
    {
        
    }

    public AchievementInfo(Faction faction, int Influence, int Token) 
        : this(FactionTypeSystem.Vanila, unchecked((uint)faction), Influence, Token)
    {

    }

    public void GiveAchievement()
    {
        if (FactionSystem == FactionTypeSystem.Vanilla)
        {
            var vanillaFaction = unchecked((Faction)FactionId);
            if (WaveManager.TryGet(vanillaFaction, out var spawnWave)
                && spawnWave is ILimitedWave limited)
            {
                switch (Influence)
                {
                    case > 0:
                        FactionInfluenceManager.Add(vanillaFaction, Influence);
                        break;
                    case < 0:
                        FactionInfluenceManager.Remove(vanillaFaction, -Influence);
                        break;
                }

                switch (Token)
                {
                    case > 0:
                        limited.RespawnTokens += Token;
                        break;
                    case < 0:
                        limited.RespawnTokens = Math.Max(limited.RespawnTokens + Token, 0);
                        break;
                }
            }
        }
        else
        {
            var factionId = FactionId;
            var wave = WaveManager.Waves.FirstOrDefault(p => 
                p is ILimitedWave limited
                && p is IVtCustomFactionWave waveCustom 
                && waveCustom.FactionId == factionId) as ILimitedWave;
            if (wave != null)
            {
                // TODO: Influance look RespawnTokensManager
                wave.RespawnTokens += Token;
            }

        }
    }
}

public enum FactionTypeSystem
{
    Vanila,
    Vanilla = Vanila,
    VtSystem
}