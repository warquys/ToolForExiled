using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.Events.Features;
using PlayerRoles;
using PlayerRoles.Visibility;
using ToolForExiled.Patch;

namespace ToolForExiled;

public static class Extension
{
    private static Dictionary<Player, ExtensionData> _extensionData = new();
    private static Event<SendDataPlayerEventArgs> _sendPlayerEventArgs = new Event<SendDataPlayerEventArgs>();

    private static ExtensionData GetData(Player player)
    {
        if (player == null) throw new NullReferenceException();
        if (!_extensionData.TryGetValue(player, out var data))
        {
            _extensionData[player] = data = new ExtensionData();
        }

        return data;
    }

    extension (Player player)
    {
        public bool IsValid()
        {
            if (player == null) return false;
            if (player.ReferenceHub == null) return false;
            if (player.Role.Type == RoleTypeId.None) return false;
            if (!player.IsConnected) return false;
            return true;
        }

        public InvisibleMode InvisibleMode
        { 
            get => GetData(player).invisibilityFlags; 
            set => GetData(player).invisibilityFlags = value; 
        }    
    }

    extension (PlayerEvents)
    {
        public static Event<SendDataPlayerEventArgs> SendPlayerData
        { 
            get => _sendPlayerEventArgs; 
            set => _sendPlayerEventArgs = value; 
        } 
    }

    private class ExtensionData
    {
        public InvisibleMode invisibilityFlags;
    }
}
