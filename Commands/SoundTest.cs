#if DEBUG && SOUND_API_SUPPORTED
using CommandSystem;
using Exiled.Permissions.Extensions;
using RemoteAdmin.Communication;
using SCPSLAudioApi.AudioCore;

namespace ToolForExiled.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
[CommandHandler(typeof(GameConsoleCommandHandler))]
public class SoundTest : ICommand
{
    public string Command => "SoundTest";

    public string[] Aliases => new string[0];

    public string Description => "test";

    public bool SanitizeResponse => true;
    
    public string SoundPath = "C:\\Users\\Pc\\AppData\\Roaming\\EXILED\\Femur.ogg";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        ToolForExiledPlugin.Instance.TryCassie("", "", SoundPath);
        response = "";
        return true;
    }
}
#endif