#if SOUND_API_SUPPORTED
using SCPSLAudioApi.AudioCore;
#endif
using AdminToys;
using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
using Exiled.API.Extensions;
using Exiled.API.Features.Roles;
using MEC;
using PlayerRoles;
using Utils.NonAllocLINQ;
using static NineTailedFoxAnnouncer;
using System.Text.RegularExpressions;

namespace ToolForExiled;

public class ToolForExiledPlugin : Plugin<Config, Translation>
{
    public const int IdForAudio = 840;

    public const string CommandConfigPermission = "ConfigHelp";

    public const string RoundRest_CoroutineTag = "kill_at_rest_(waitinforplayer)";

#if SOUND_API_SUPPORTED

    private Npc _soundAnnoncer;

    public Npc SoundAnnoncer
    {
        get 
        { 
            if (_soundAnnoncer == null)
            {
                _soundAnnoncer = Npc.Spawn(Config.AnnoncerName, RoleTypeId.Spectator, userId: "Annoncer@ToolForExiled");
                Player.Dictionary.Remove(_soundAnnoncer.GameObject);
                ReferenceHub.AllHubs.Remove(_soundAnnoncer.ReferenceHub);
                _soundAnnoncer.RemoteAdminPermissions = PlayerPermissions.AFKImmunity;
                var audio = AudioPlayerBase.Get(SoundAnnoncer.ReferenceHub);
                audio.BroadcastChannel = VoiceChat.VoiceChatChannel.Intercom;
                _soundAnnoncer.RankName = "Cassie";
                _soundAnnoncer.RankColor = "light_green";
            }

            return _soundAnnoncer; 
        }
        set { _soundAnnoncer = value; }
    }


#endif

    public static ToolForExiledPlugin Instance 
    { 
        get; 
        private set; 
    }

    #region Plugin Info
    public override string Author => PluginInfo.PLUGIN_AUTHORS;
    public override string Name => PluginInfo.PLUGIN_NAME;
    public override string Prefix => PluginInfo.PLUGIN_GUID.ToSnakeCase();
    public override Version Version => new Version(PluginInfo.PLUGIN_VERSION);
    public override PluginPriority Priority => PluginPriority.Highest;
    #endregion

    public ToolForExiledPlugin()
    {
        Assembly = typeof(ToolForExiledPlugin).Assembly;
        Instance = this;
        InternalTranslation = new Translation();
    }

    public List<IRestable> Restables { get; } = [];

    public override void OnEnabled()
    {
        ServerEvents.WaitingForPlayers.Subscribe(Rest);
        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        ServerEvents.WaitingForPlayers.Unsubscribe(Rest);
        base.OnDisabled();
    }

    public void Rest()
    {
        Log.Debug("Rest, base on Waiting For Players");
#if SOUND_API_SUPPORTED
        SoundAnnoncer = null;
#endif
        Timing.KillCoroutines(RoundRest_CoroutineTag);
        Restables.ForEach(p => p.Reset());
    }

    public void Replaces(ref string str, params (string patern, object replacement)[] replacements)
    {
        foreach ((string pattern, object replacement) in replacements)
        {
            str = str.Replace(pattern, replacement.ToString());
        }
    }

    public void IgnoreCaseReplaces(ref string str, params (string pattern, object replacement)[] replacements)
    {
        foreach ((string pattern, object replacement) in replacements)
        {
            str = Regex.Replace(str, pattern, replacement.ToString(), RegexOptions.IgnoreCase);
        }
    }

    public void TryCassie(string message, string translation, string soundMessage)
    {
        try
        {
#if SOUND_API_SUPPORTED
            if (!string.IsNullOrEmpty(soundMessage))
            {
                var audio = AudioPlayerBase.Get(SoundAnnoncer.ReferenceHub);
                audio.AudioToPlay.Add(soundMessage);
                if (audio.CurrentPlay == null || audio.IsFinished)
                    audio.Play(0);
            }
#endif
            if (!string.IsNullOrEmpty(message))
            {
                if (!string.IsNullOrEmpty(translation))
                    Cassie.MessageTranslated(message, translation);
                else
                    Cassie.Message(message);
            }
        }
        catch (Exception e)
        {
            Log.Error(e);
#if DEBUG
            throw;
#endif
        }
        finally
        {
#if SOUND_API_SUPPORTED
            if (!string.IsNullOrEmpty(soundMessage) && SoundAnnoncer.ReferenceHub == null)
            {
                var audio = AudioPlayerBase.Get(SoundAnnoncer.ReferenceHub);
                var lastIndex = audio.AudioToPlay.Count - 1;
                if (lastIndex >= 0 && audio.AudioToPlay[lastIndex] == soundMessage)
                    audio.AudioToPlay.RemoveAt(lastIndex);
                if (lastIndex == 0)
                    audio.Stoptrack(false);
            }
#endif
        }
    }

    public void CassieConvertRoleName(RoleInformation role, out string withoutSpace, out string withSpace)
    {
        if (role.RoleSystem == RoleTypeSystem.Vanila)
        {
            var roleTypeId = (RoleTypeId)role.RoleId;
            if (RoleExtensions.GetTeam(roleTypeId) == Team.SCPs)
            {
                ConvertSCP(roleTypeId, out withoutSpace, out withSpace);
                return;
            }
        }

        if (Translation.RoleName.TryGetValue(role, out var name))
        {
            name.Deconstruct(out withSpace, out withoutSpace);
            return;
        }

        withoutSpace = "";
        withSpace = "";
        return;

        // Other Role not convertible by default
        /* switch (player.Role.Type)
        {
            
        }*/
    }

    public void CassieConvertRoleName(Player player, out string withoutSpace, out string withSpace)
    {
        var hasCustomRole = player.HasCustomRole();
        if (hasCustomRole) 
        {
            var name = Translation.RoleName.FirstOrDefault(p => p.Key.IsValid(player, hasCustomRole)).Value;
            if (name != null)
            {
                name.Deconstruct(out withSpace, out withoutSpace);
                return;
            }

            if (player.IsScp)
            {
                ConvertSCP(player.Role.Type, out withoutSpace, out withSpace);
                return;
            }
        }

        withoutSpace = "";
        withSpace = "";
        return;
    }
}

public interface IRestable
{
    void Reset();
}
