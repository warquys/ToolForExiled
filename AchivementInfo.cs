using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerRoles;
using Respawning.Waves.Generic;
using Respawning;

namespace ToolForExiled;

public record struct AchievementInfo(Faction Faction, int Influence, int Token)
{
    public AchievementInfo() : this(Faction.FoundationStaff, 0, 0)
    {
        
    }

    public void GiveAchievement()
    {
        if (WaveManager.TryGet(Faction, out var spawnWave)
            && spawnWave is ILimitedWave limited)
        {
            switch (Influence)
            {
                case > 0:
                    FactionInfluenceManager.Add(Faction, Influence);
                    break;
                case < 0: 
                    FactionInfluenceManager.Remove(Faction, -Influence);
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
}
