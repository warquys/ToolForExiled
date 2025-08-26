using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Extensions;
using Exiled.Events.Features;
using Mirror;
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


        public void SendFakeEffectIntensity(EffectType effect, byte intensity = 1)
            => SendNetworkMessage(player, MirrorService.GetCustomVarMessage(player.ReferenceHub.playerEffectsController, writer =>
            {
                var effectId = player.GetEffect(effect);
                writer.WriteUInt(1); //Which SyncObject will be updated

                //SyncList Specific
                writer.WriteUInt(1); //The amount of changes
                writer.WriteByte((byte)SyncList<byte>.Operation.OP_SET);
                writer.WriteUInt((uint)effect); //effect id/index
                writer.WriteByte(intensity); // Intensity
            }, false));

        public void SendNetworkMessage<TNetworkMessage>(TNetworkMessage msg, int channel = 0)
                where TNetworkMessage : struct, NetworkMessage =>
                player.Connection?.Send(msg, channel);
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
