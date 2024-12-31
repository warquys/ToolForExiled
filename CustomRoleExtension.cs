using Exiled.API.Features.Pools;
using Exiled.API.Features.Roles;
using ExCustomRole = Exiled.CustomRoles.API.Features.CustomRole;

#if UNCOMPLICATED_ROLE_SUPPORTED
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;
using UnCustomRole = UncomplicatedCustomRoles.API.Features.CustomRole;
using UnPlayerExtension = UncomplicatedCustomRoles.Extensions.PlayerExtension;
#endif

namespace ToolForExiled;

public static class CustomRoleExtension
{
    // JUSTE EDIT THIS TO SAY IF YES OR NO THE PLAYER AVE A CUSTOME ROLE.
    public static bool HasCustomRole(this Player player)
    {
        var hasExile = ExCustomRole.Registered.Any(p => p.Check(player));
        if (hasExile) return true;

#if UNCOMPLICATED_ROLE_SUPPORTED
        var hasUncompicated = UnPlayerExtension.HasCustomRole(player);
        if (hasUncompicated) return true;
#endif

        return false;
    }
    
    public static bool IsValid(this IEnumerable<RoleInformation> roles, Player player, bool hasCustomRole)
    {
        return roles.Any(p => p.IsValid(player, hasCustomRole));
    }

    public static bool IsValid(this IEnumerable<RoleInformation> roles, Player player)
        => roles.IsValid(player, player.HasCustomRole());

    public static bool IsValid(this IEnumerable<RoleInformation> roles, Player player, bool hasCustomRole, out RoleInformation role)
    {
        role = roles.FirstOrDefault(p => p.IsValid(player, hasCustomRole));
        return role != default;
    }

    public static bool IsValid(this IEnumerable<RoleInformation> roles, Player player, out RoleInformation role)
        => roles.IsValid(player, player.HasCustomRole(), out role);

    public static bool IsValid(this IRolesRestrained restrained, Player player, bool hasCustomRole)
        => restrained.ValidRoles.IsValid(player, hasCustomRole);

    public static bool IsValid(this IRolesRestrained restrained, Player player) 
        => restrained.ValidRoles.IsValid(player);

    public static bool IsValid(this IRolesRestrained restrained, Player player, bool hasCustomRole, out RoleInformation role)
        => restrained.ValidRoles.IsValid(player, hasCustomRole, out role);

    public static bool IsValid(this IRolesRestrained restrained, Player player, out RoleInformation role)
        => restrained.ValidRoles.IsValid(player, out role);

    public static bool IsValid(this IRoleRestrained restrained, Player player, bool hasCustomRole)
    => restrained.ValidRole.IsValid(player, hasCustomRole);

    public static bool FirstRolesValid<T>(this IEnumerable<T> restraineds, Player player, bool hasCustomRole, out T valid)
        where T : IRolesRestrained
    {
        foreach (var restrained in restraineds)
        {
            if (restrained.IsValid(player, hasCustomRole))
            {
                valid = restrained;
                return true;
            }
        }
        valid = default;
        return false;
    }

    public static bool FirstRolesValid<T>(this IEnumerable<T> restraineds, Player player, out T valid)
        where T : IRolesRestrained
        => restraineds.FirstRolesValid(player, player.HasCustomRole(), out valid);

    public static bool FirstRolesValid<T>(this IEnumerable<T> restraineds, Player player, bool hasCustomRole, out T valid, out RoleInformation role)
        where T : IRolesRestrained
    {
        foreach (var restrained in restraineds)
        {
            if (restrained.IsValid(player, hasCustomRole, out role))
            {
                valid = restrained;
                return true;
            }
        }
        role = default;
        valid = default;
        return false;
    }

    public static bool FirstRolesValid<T>(this IEnumerable<T> restraineds, Player player, out T valid, out RoleInformation role)
        where T : IRolesRestrained
        => restraineds.FirstRolesValid(player, player.HasCustomRole(), out valid, out role);

    public static bool FirstRoleValid<T>(this IEnumerable<T> restraineds, Player player, bool hasCustomRole, out T valid)
        where T : IRoleRestrained
    {
        foreach (var restrained in restraineds)
        {
            if (restrained.IsValid(player, hasCustomRole))
            {
                valid = restrained;
                return true;
            }
        }
        valid = default;
        return false;
    }

    public static bool FirstRoleValid<T>(this IEnumerable<T> restraineds, Player player, out T valid)
        where T : IRoleRestrained
        => restraineds.FirstRoleValid(player, player.HasCustomRole(), out valid);

    public static bool FirstRoleValid<T>(this IEnumerable<T> restraineds, Player player, bool hasCustomRole, out T valid, out RoleInformation role)
        where T : IRoleRestrained
    {
        foreach (var restrained in restraineds)
        {
            if (restrained.IsValid(player, hasCustomRole))
            {
                role = restrained.ValidRole;
                valid = restrained;
                return true;
            }
        }
        role = default;
        valid = default;
        return false;
    }

    public static bool FirstRoleValid<T>(this IEnumerable<T> restraineds, Player player, out T valid, out RoleInformation role)
        where T : IRoleRestrained
        => restraineds.FirstRoleValid(player, player.HasCustomRole(), out valid, out role);

    public static string GetInvalidMessage(this IEnumerable<RoleInformation> validsRoles)
    {
        var message = ToolForExiledPlugin.Instance.Translation.AllowedRoles;
        var rolesNames = validsRoles.Select(GetName);

        ToolForExiledPlugin.Instance.IgnoreCaseReplaces(
            ref message,
            ToolForExiledTranslation.roles_Pattern,
            string.Join(", ", rolesNames));

        return message;

        string GetName(RoleInformation role)
        {
            var rowName = ToolForExiledPlugin.Instance.Translation.RoleName.FirstOrDefault(p => p.Role == role);
            if (rowName == null)
                return $"Config Name Not Defined ({role.RoleSystem} {role.RoleId})";
            return rowName.NameNoSpace;
        }
    }

    public static string GetInvalidMessage(this IRolesRestrained restrained)
        => restrained.ValidRoles.GetInvalidMessage();
}
