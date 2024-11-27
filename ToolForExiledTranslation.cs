using System.ComponentModel;
using ToolForExiled.Commands;

namespace ToolForExiled;

public class ToolForExiledTranslation : ITranslation
{
    public const string player_Pattern = "%player%";
    public const string time_Pattern = "%time%";
    public const string role_Pattern = "%role%";
    public const string item_Pattern = "%item%";
    public const string roles_Pattern = "%roles%";
    public const string items_Pattern = "%items%";

    public string NoPermission { get; set; } = "You don't have the permissions.";
    public string YourNotPlayer { get; set; } = "You are not a player.";
    public string NotPlayer { get; set; } = "That is not a player.";
    public string PlayerNotFound { get; set; } = $"Impossible to find: {player_Pattern}.";
    public string CoolDown { get; set; } = $"In cooldown, you need to wait {time_Pattern} seconds.";
    public string AllowedRoles { get; set; } = $"You can't do this... Allowed role(s) are: {roles_Pattern}.";

    [Description("By default the vanila SCP name are handles. ")]
    public List<RoleName> RoleName { get; set; } = new List<RoleName>()
    {
        new RoleName(new RoleInformation(RoleTypeSystem.CustomRoleExiled, 999), "999", "9 9 9")
    };


    [Description("Need a full restart.")]
    public string GetMapPointCommand { get; set; } = nameof(GetMapPoint);
    public string[] GetMapPointAliases { get; set; } = new[] { "MapPoint", "GetRoomPoint", "RoomPoint" };
    public string GetMapPointDescription { get; set; } = "Return the map point. 'me' for player position and not looking at, 'surface' out off room.";

}

public class RoleName
{
    public RoleInformation Role { get; set; }
    public string NameNoSpace { get; set; }
    [Description("Used for Cassie spelling.")]
    public string NameSpaced { get; set; }

    public RoleName()
    {
        Role = new RoleInformation();
        NameSpaced = "";
        NameSpaced = "";
    }

    public RoleName(RoleInformation role, string nameNoSpace, string nameSpaced)
    {
        Role = role;
        NameNoSpace = nameNoSpace;
        NameSpaced = nameSpaced;
    }

    public void Deconstruct(out string nameSpaced, out string nameNoSpace)
    {
        nameNoSpace = NameNoSpace;
        nameSpaced = NameSpaced;
    }
}