using System;
using PlayerRoles;
using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;
using ExCustomRole = Exiled.CustomRoles.API.Features.CustomRole;
using Exiled.CustomRoles;

#if UNCOMPLICATED_ROLE_SUPPORTED
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;
using UnCustomRole = UncomplicatedCustomRoles.API.Features.CustomRole;
using UnPlayerExtension = UncomplicatedCustomRoles.Extensions.PlayerExtension;
#endif

namespace ToolForExiled;

public record struct RoleInformation(RoleTypeSystem RoleSystem, uint RoleId)
{
    public const uint NONE_ID = uint.MaxValue;

    // WTF! YOU DO NOT USE EXILED CUSTOM ROLES AND YOU WHANT TO ADD YOUR CUSTOM ROLES MANAGEMENT !!!
    // LUCK

    public RoleInformation() : this(RoleTypeSystem.Vanilla, NONE_ID) { }

    public RoleInformation(RoleTypeId roleTypeId) : this(RoleTypeSystem.Vanilla, unchecked((uint)roleTypeId)) { }

    public void SetRole(Player player, params object[] complementaryInfo)
    {
        try
        {
            if (!player.IsValid()) return;

            var extractor = new Extractor<object>();
            var customRoles = player.GetCustomRoles();
            foreach (var exiledCustomRole in customRoles)
                exiledCustomRole.RemoveRole(player);

            switch (RoleSystem)
            {
                case RoleTypeSystem.Vanilla:
                    extractor.AddSource(complementaryInfo)
                        .AddExtraction<RoleSpawnFlags>(out var roleSpawnFlag, RoleSpawnFlags.All)
                        .AddExtraction<SpawnReason>(out var spawnReason, SpawnReason.ForceClass)
                        .Execute();
                    
                    player.Role.Set(unchecked((RoleTypeId)RoleId), spawnReason, roleSpawnFlag);

                    break;

                case RoleTypeSystem.CustomRolesExiled:
                    if (!ExCustomRole.TryGet(RoleId, out var role) || role == null)
                    {
                        Log.Warn($"Role {RoleId} not found.");
                        return;
                    }
                    role.AddRole(player);
                    break;

#if UNCOMPLICATED_ROLE_SUPPORTED
            case RoleTypeSystem.UncomplicatedCustomRole:
                player.SetCustomRole(unchecked((int)RoleId));
                break;
#endif

                // where add you code to spawn the role...

                default:
                    break;
            }
        }
        catch (Exception)
        {
            Log.Error($@"Exception raised when trying to set a role:
Type {RoleSystem}
Id {RoleId}");
            throw;
        }
       
    }

    public bool IsValid(Player player, bool? hasCustomRole = null)
    {
        if (!player.IsValid()) return false;

        switch (RoleSystem)
        {
            case RoleTypeSystem.Vanila:

                if (hasCustomRole == true)
                    return false; 

                if (hasCustomRole == null && player.HasCustomRole())
                    return false;
                
                if (player.Role.Type < 0) return false;
                return unchecked((uint)player.Role.Type) == RoleId;

            case RoleTypeSystem.CustomRolesExiled when hasCustomRole ?? true:
                if (ExCustomRole.TryGet(RoleId, out var role) && role != null)
                    return role.Check(player);
                goto default;

#if UNCOMPLICATED_ROLE_SUPPORTED
            case RoleTypeSystem.UncomplicatedCustomRole when hasCustomRole ?? true:
                if (UnCustomRole.Alive.TryGetValue(player.Id, out int id))
                    return unchecked((uint)id) == RoleId;
                goto default;
#endif

            // where add you code to check if the role is use or not...

            default:
                return false;
        }
    }

    public override string ToString()
    {
        return $"{RoleSystem}, {RoleId}";
    }

    enum KeepPosition
    {
        No = 0,
        Yes = 1
    }
}