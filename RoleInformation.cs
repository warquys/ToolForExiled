﻿using System;
using PlayerRoles;
using ExCustomRole = Exiled.CustomModules.API.Features.CustomRoles.CustomRole;
using ExPlayerExtensions = Exiled.CustomModules.API.Extensions.PlayerExtensions;

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

    public RoleInformation() : this(RoleTypeSystem.Vanilla, 0) { }

    public RoleInformation(RoleTypeId roleTypeId) : this(RoleTypeSystem.Vanilla, unchecked((uint)roleTypeId)) { }

    public void SetRole(Player player, params object[] complementaryInfo)
    {
        if (player == null) return;

        var extractor = new Extractor<object>();
        var customRoles = ExPlayerExtensions.Get(player);
        
        //foreach (var exiledCustomRole in customRoles)
        //    exiledCustomRole.RemoveRole(player);

        switch (RoleSystem)
        {
            case RoleTypeSystem.Vanilla:
                player.Role.Set((RoleTypeId)RoleId, RoleSpawnFlags.All);
                break;

            case RoleTypeSystem.CustomRolesExiled:
                if (!ExCustomRole.TryGet(RoleId, out var role) || role == null)
                {
                    Log.Warn($"Role {RoleId} not found.");
                    return;
                }

                extractor.AddSource(complementaryInfo)
                    .AddExtraction<RoleSpawnFlags>(out var roleSpawnFlag, RoleSpawnFlags.All)
                    .AddExtraction<SpawnReason>(out var spawnReason, null)
                    .AddExtraction<KeepPosition>(out var shouldKeepPosition, KeepPosition.No)
                    .Extract();

                ExPlayerExtensions.Spawn(player, role, KeepPosition.Yes == shouldKeepPosition, spawnReason, roleSpawnFlag);
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

    public bool IsValid(Player player, bool? hasCustomRole = null)
    {
        if (player == null) return false;

        switch (RoleSystem)
        {
            case RoleTypeSystem.Vanila when !(hasCustomRole ?? player.HasCustomRole()):
                if (player.Role.Type < 0) return false;
                return unchecked((uint)player.Role.Type) == RoleId;

            case RoleTypeSystem.CustomRolesExiled when hasCustomRole ?? true:
                if (ExPlayerExtensions.TryGet(player, out var playerRole) && playerRole.Id == RoleId)
                    return true;
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

public static class CustomRoleExtension
{

    // JUSTE EDIT THIS TO SAY IF YES OR NO THE PLAYER AVE A CUSTOME ROLE.
    public static bool HasCustomRole(this Player player)
    {
        var hasExile = ExCustomRole.TryGet(player, out _);
        if (hasExile) return true;

#if UNCOMPLICATED_ROLE_SUPPORTED
        var hasUncompicated = UnPlayerExtension.HasCustomRole(player);
        if (hasUncompicated) return true;
#endif

        return false;
    }
    
    public static bool IsValid(this IEnumerable<RoleInformation> roles, Player player)
    {
        var hasCustomRole = player.HasCustomRole();
        return roles.Any(p => p.IsValid(player, hasCustomRole));
    }

    public static bool IsValid(this IEnumerable<RoleInformation> roles, Player player, out RoleInformation role)
    {
        var hasCustomRole = player.HasCustomRole();
        role = roles.FirstOrDefault(p => p.IsValid(player, hasCustomRole));
        return role != default;
    }

}


public enum RoleTypeSystem
{
    Vanila,
    // lol i forget an l
    Vanilla = Vanila,
    CustomRoleExiled,
    // lol i forget an s
    CustomRolesExiled = CustomRoleExiled,
#if UNCOMPLICATED_ROLE_SUPPORTED
    UncomplicatedCustomRole,
#endif

    // the system that you use
}