// Synapse MIT license

using CommandSystem;
using Exiled.Permissions.Extensions;
using RemoteAdmin.Communication;

namespace ToolForExiled.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class GetMapPoint : ICommand
{
    public string Command => ToolForExiledPlugin.Instance.Translation.GetMapPointCommand;

    public string[] Aliases => ToolForExiledPlugin.Instance.Translation.GetMapPointAliases;

    public string Description => ToolForExiledPlugin.Instance.Translation.GetMapPointDescription;

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var player = Player.Get(sender);
        if (player == null)
        {
            response = ToolForExiledPlugin.Instance.Translation.YourNotPlayer;
            return false;
        }

        if (!player.CheckPermission(ToolForExiledPlugin.CommandConfigPermission))
        {
            response = ToolForExiledPlugin.Instance.Translation.NoPermission;
            return false;
        }

        RoomPoint point;
        if (arguments.Count != 0)
        {
            point = new RoomPoint(player.Position, player.Rotation);
        }
        else
        {
            var cameraTransform = player.CameraTransform;
            Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit recastHit, 100f);
            point = new RoomPoint(recastHit.point + Vector3.up * 0.1f, Quaternion.identity);
        }
        response = $"\nThe position {(arguments.Count != 0 ? "where you stand" : "you are looking at")} as RoomPoint (change , to . in the syml config):" +
                        $"\n  room: {point.RoomType}" +
                        $"\n  x: {point.position.x}" +
                        $"\n  y: {point.position.y}" +
                        $"\n  z: {point.position.z}";

        Log.Info($"{point.RoomType}, {point.position.x}, {point.position.y}, {point.position.z}");

        if (sender is CommandSender commandSender)
            RaClipboard.Send(commandSender, RaClipboard.RaClipBoardType.PlayerId, $"{point.RoomType} {point.position.x} {point.position.y} {point.position.z}");
        return true;
    }
}