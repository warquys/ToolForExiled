using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;
using PlayerRoles;
#if UNCOMPLICATED_ROLE_SUPPORTED
using UncomplicatedCustomRoles.API.Features;
#endif

namespace ToolForExiled;

public class RoleInformation(RoleType roleType, uint roleId) : IEquatable<RoleInformation>
{
    public const uint NONE_ID = uint.MaxValue;

    // WTF! YOU DO NOT USE EXILED AND YOU WHANT TO ADD YOUR CUSTOM ROLE MANAGEMENT !!!
    // LUCK

    public RoleType RoleType { get; set; } = roleType;
    public uint RoleId { get; set; } = roleId;

    public RoleInformation() : this(RoleType.Vanila, 0) { }

    public RoleInformation(RoleTypeId roleTypeId) : this(RoleType.Vanila, unchecked((uint)roleTypeId)) { }

    public void SetRole(Player player)
    {
        var customRoles = player.GetCustomRoles();
        
        foreach (var exiledCustomRole in customRoles)
            exiledCustomRole.RemoveRole(player);

        switch (RoleType)
        {
            case RoleType.Vanila:
                player.Role.Set((RoleTypeId)RoleId, RoleSpawnFlags.All);
                break;

            case RoleType.CustomRoleExiled:
                if (!CustomRole.TryGet(RoleId, out var role) || role == null)
                {
                    Log.Warn($"Role {RoleId} not found.");
                    return;
                }

                role.AddRole(player);
                break;

#if UNCOMPLICATED_ROLE_SUPPORTED
            case RoleType.UncomplicatedCustomRole:
                Manager.Summon(player, unchecked((int)RoleId));
                break;
#endif

            // where add you code to spwn the role...

            default:
                break;
        }
    }

    public bool IsValid(Player player, bool? hasCustomRole = null)
    {
        switch (RoleType)
        {
            case RoleType.Vanila when !(hasCustomRole ?? player.HasCustomRole()):
                if (player.Role.Type < 0) return false;
                return ((uint)player.Role.Type) == RoleId;

            case RoleType.CustomRoleExiled when hasCustomRole ?? true:
                if (CustomRole.TryGet(RoleId, out var role) && role != null)
                    return role.Check(player);
                goto default;

#if UNCOMPLICATED_ROLE_SUPPORTED
            case RoleType.UncomplicatedCustomRole when hasCustomRole ?? true:
                if (Manager.GetAlive().TryGetValue(player.Id, out int id))
                    return unchecked((uint)id) == RoleId;
                goto default;
#endif

            // where add you code to check if the role is use or not...

            default:
                return false;
        }
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as RoleInformation);
    }

    public bool Equals(RoleInformation other)
    {
        return other is not null &&
               RoleType == other.RoleType &&
               RoleId == other.RoleId;
    }

    public override int GetHashCode()
    {
        int hashCode = -1723842667;
        hashCode = hashCode * -1521134295 + RoleType.GetHashCode();
        hashCode = hashCode * -1521134295 + RoleId.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(RoleInformation left, RoleInformation right)
    {
        return EqualityComparer<RoleInformation>.Default.Equals(left, right);
    }

    public static bool operator !=(RoleInformation left, RoleInformation right)
    {
        return !(left == right);
    }
}

public static class CustomRoleExtension
{

    // JUSTE EDIT THIS TO SAY IF YES OR NO THE PLAYER AVE A CUSTOME ROLE.
    public static bool HasCustomRole(this Player player)
    {
        var hasExile = CustomRole.Registered.Any(p => p.Check(player));
        if (hasExile) return true;

#if UNCOMPLICATED_ROLE_SUPPORTED
        var hasUncompicated = Manager.HasCustomRole(player);
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
        return role != null;
    }

}


public enum RoleType
{
    Vanila,
    CustomRoleExiled,
#if UNCOMPLICATED_ROLE_SUPPORTED
    UncomplicatedCustomRole,
#endif

    // where the system that you use
}