// CODE OF SYNAPSE, MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.Events.Features;
using LabApi.Events.Arguments.PlayerEvents;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.Visibility;
using RelativePositioning;

namespace ToolForExiled.Patch;

[HarmonyPatch]
public static class SendPlayerDataPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FpcServerPositionDistributor), nameof(FpcServerPositionDistributor.GetNewSyncData))]
    public static bool GetNewData(ReferenceHub receiver, ReferenceHub target, FirstPersonMovementModule fpmm,
        bool isInvisible, out FpcSyncData __result)
    {
        try
        {
            var player = Player.Get(receiver);
            var targetPlayer = Player.Get(target);
            __result = GetSyncData(player, targetPlayer, fpmm, isInvisible, out _);
            return false;
        }
        catch (Exception ex)
        {
            Log.Error("Player SendPlayerData Patch failed\n" + ex);
            __result = default;
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FpcServerPositionDistributor), nameof(FpcServerPositionDistributor.WriteAll))]
    public static bool WriteAll(ReferenceHub receiver, NetworkWriter writer)
    {
        try
        {
            ushort num = 0;
            bool asVisiblityControler;
            VisibilityController visibilityController;
            if (receiver.roleManager.CurrentRole is ICustomVisibilityRole customVisibilityRole)
            {
                asVisiblityControler = true;
                visibilityController = customVisibilityRole.VisibilityController;
            }
            else
            {
                asVisiblityControler = false;
                visibilityController = null;
            }
            var playerReceiver = Player.Get(receiver);
            foreach (var hub in ReferenceHub.AllHubs)
            {
                if (hub.isLocalPlayer) continue;
                var targetPlayer = Player.Get(hub);
                if (targetPlayer.Role.Base is not IFpcRole fpcRole) continue;
                if (targetPlayer.NetId != receiver.netId)
                {
                    var isInvisible = asVisiblityControler && !visibilityController.ValidateVisibility(hub);
                    PlayerValidatedVisibilityEventArgs playerValidatedVisibilityEventArgs = new PlayerValidatedVisibilityEventArgs(receiver, hub, !isInvisible);
                    LabApi.Events.Handlers.PlayerEvents.OnValidatedVisibility(playerValidatedVisibilityEventArgs);
                    isInvisible = !playerValidatedVisibilityEventArgs.IsVisible;
                    var newSyncData = GetSyncData(playerReceiver, targetPlayer, fpcRole.FpcModule, isInvisible, out var canSee);
                    if (!isInvisible && canSee)
                    {
                        FpcServerPositionDistributor._bufferPlayerIDs[num] = hub.PlayerId;
                        FpcServerPositionDistributor._bufferSyncData[num] = newSyncData;
                        num++;
                    }

                    RoleTypeId role = FpcServerPositionDistributor.GetVisibleRole(receiver, hub);
                    RoleTypeId? overiedRole = FpcServerPositionDistributor._roleSyncEvent?.Invoke(hub, receiver, role);
                    if (overiedRole.HasValue)
                    {
                        role = overiedRole.Value;
                    }

                    if (!hub.roleManager.PreviouslySentRole.TryGetValue(receiver.netId, out var value) || value != role)
                    {
                        FpcServerPositionDistributor.SendRole(receiver, hub, role);
                    }
                }
                //else
                //{
                //    if (!player.refreshHorizontalRotation && !player.refreshVerticalRotation) continue;
                //    var newSyncData = GetSelfPlayerData(player, fpcRole.FpcModule);
                //    FpcServerPositionDistributor._bufferPlayerIDs[num] = player.PlayerId;
                //    FpcServerPositionDistributor._bufferSyncData[num] = newSyncData;
                //    num++;
                //}
            }
            writer.WriteUShort(num);
            for (int i = 0; i < num; i++)
            {
                writer.WriteRecyclablePlayerId(new RecyclablePlayerId(FpcServerPositionDistributor._bufferPlayerIDs[i]));
                FpcServerPositionDistributor._bufferSyncData[i].Write(writer);
            }
        }
        catch (Exception ex)
        {
            Log.Error("Send Player Data Patch failed\n" + ex);
        }

        return false;
    }

    private static FpcSyncData GetSelfPlayerData(Player player, FirstPersonMovementModule firtstPersonModule)
    {
        //if (player.refreshHorizontalRotation)
        //{
        //    firtstPersonModule.MouseLook.CurrentHorizontal = player.horizontalRotation;
        //    player.refreshHorizontalRotation = false;
        //}
        //if (player.refreshVerticalRotation)
        //{
        //    firtstPersonModule.MouseLook.CurrentVertical = player.verticalRotation;
        //    player.refreshVerticalRotation = false;
        //}


        var data = new FpcSyncData(default,
        firtstPersonModule.SyncMovementState,
        firtstPersonModule.IsGrounded,
        new RelativePosition(Vector3.zero),
        firtstPersonModule.MouseLook);
        return data;
    }

    private static FpcSyncData GetSyncData(Player receiver, Player target,
        FirstPersonMovementModule firstPersonModule, bool isInvisible, out bool canSee)
    {
        var prevSyncData = FpcServerPositionDistributor.GetPrevSyncData(receiver.ReferenceHub, target.ReferenceHub);
        if (receiver == null || target == null)
        {
            canSee = false;
            return default;
        }
        var ev = new SendDataPlayerEventArgs(receiver, target)
        {
            Position = target.Position,
            IsGrounded = firstPersonModule.IsGrounded,
            IsInvisible = isInvisible,
            MovementState = firstPersonModule.SyncMovementState
        };

        switch (target.InvisibleMode)
        {
            case InvisibleMode.Full:
            case InvisibleMode.Alive or InvisibleMode.Ghost when receiver.IsAlive:
            // case InvisibleMode.Admin when !player.HasPermission("synapse.see.invisible"):
            case InvisibleMode.Visual when receiver.Role.Type is not RoleTypeId.Scp079 and not RoleTypeId.Scp939
                and not RoleTypeId.Scp096 and not RoleTypeId.Spectator:
                ev.IsInvisible = true;
                break;
        }

        PlayerEvents.SendPlayerData.InvokeSafely(ev);

        var syncData = ev.IsInvisible
            ? default
            : new FpcSyncData(prevSyncData, ev.MovementState, ev.IsGrounded,
                new RelativePosition(ev.Position), firstPersonModule.MouseLook);

        FpcServerPositionDistributor.PreviouslySent[receiver.NetId][target.NetId] = syncData;
        canSee = !ev.IsInvisible;
        return syncData;
    }
}

public enum InvisibleMode
{
    /// <summary>
    /// The Player is Visible for everyone
    /// </summary>
    None,
    /// <summary>
    /// The Player is only visual Invisible but SCP-079, SCP-096 and SCP-939 are still able to scan/feel/hear them
    /// </summary>
    Visual,
    /// <summary>
    /// The Player is invisible for every living being but Spectator can see them
    /// </summary>
    Alive,
    /// <summary>
    /// The Player can only be seen by Spectators and can't trigger SCP-173/SCP-096 or teslas
    /// </summary>
    Ghost,
    /// <summary>
    /// The Player is invisible for all that doesn't have the synapse.see.invisible permission
    /// </summary>
    [Obsolete("NOT SUPOORTED FOR NOW")]
    Admin,
    /// <summary>
    /// No one can see the Player except the Player himself
    /// </summary>
    Full
}
public class SendDataPlayerEventArgs : IPlayerEvent, IExiledEvent, IDeniableEvent
{
    public SendDataPlayerEventArgs(Player player, Player playerToSee)
    {
        Player = player;
        PlayerToSee = playerToSee;
    }

    /// <summary>
    /// The player looking.
    /// </summary>
    public Player Player { get; }

    bool IDeniableEvent.IsAllowed { get => IsInvisible; set => IsInvisible = value; }

    public Player PlayerToSee { get; }

    public PlayerMovementState MovementState { get; set; }

    public bool IsGrounded { get; set; }

    public bool IsInvisible { get; set; }

    public Vector3 Position { get; set; }
}
