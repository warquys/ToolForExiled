using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerRoles;

namespace ToolForExiled;

public static class Extension
{
    public static bool IsValid(this Player player)
    {
        if (player == null) return false;
        if (player.ReferenceHub == null) return false;
        if (player.Role.Type == RoleTypeId.None) return false;
        if (!player.IsConnected) return false;
        return true;
    }
}
