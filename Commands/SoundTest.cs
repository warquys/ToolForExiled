#if DEBUG && SOUND_API_SUPPORTED
using CommandSystem;
using Exiled.Permissions.Extensions;
using RemoteAdmin.Communication;
using SCPSLAudioApi.AudioCore;

namespace ToolForExiled.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class SoundTest : ICommand
{
    public string Command => "SoundTest";

    public string[] Aliases => new string[0];

    public string Description => "test";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var tool = ToolForExiledPlugin.Instance;
        var audio = AudioPlayerBase.Get(tool.SoundAnnoncer.ReferenceHub);
        audio.AudioToPlay.Add("C:\\Users\\Pc\\AppData\\Roaming\\EXILED\\Femur.ogg");
        audio.LogDebug = true;
        audio.LogInfo = true;
        if (audio.CurrentPlay == null || audio.IsFinished)
            audio.Play(0);
        response = "";
        return true;
    }
}
#endif