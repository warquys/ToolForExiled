using System.ComponentModel;
using ToolForExiled.Commands;

namespace ToolForExiled;

public class Translation : ITranslation
{
    public string NoPermission { get; set; } = "You don't have the permissions.";
    public string YourNotPlayer { get; set; } = "You are not a player.";
    public string NotPlayer { get; set; } = "That is not a player.";
    public string PlayerNotFound { get; set; } = "Impossible to find: %player%.";

    [Description("By default the vanila SCP name are handles. ")]
    public Dictionary<RoleInformation, RoleName> RoleName = new Dictionary<RoleInformation, RoleName>()
    {
        {
            new RoleInformation(RoleTypeSystem.CustomRoleExiled, 999),
            new RoleName("SCP-999", "S C P 9 9 9")
        }
    };


    [Description("Need a full restart.")]
    public string GetMapPointCommand { get; set; } = nameof(GetMapPoint);
    public string[] GetMapPointAliases { get; set; } = new[] { "MapPoint", "GetRoomPoint", "RoomPoint" };
    public string GetMapPointDescription { get; set; } = "Return the map point.";

}

public record RoleName(string NameNoScape, string NameSpaced);