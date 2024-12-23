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
using Exiled.Events.Features;
using System.Collections.Generic;
using System.Data;

namespace ToolForExiled;

public class ToolForExiledPlugin : Plugin<ToolForExiledConfig, ToolForExiledTranslation>
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
            if (_soundAnnoncer == null || _soundAnnoncer.ReferenceHub == null)
            {
                _soundAnnoncer = Npc.Spawn(Config.AnnoncerName, RoleTypeId.Spectator);
                Player.Dictionary.Remove(_soundAnnoncer.GameObject);
                ReferenceHub.AllHubs.Remove(_soundAnnoncer.ReferenceHub);
                _soundAnnoncer.RemoteAdminPermissions = PlayerPermissions.AFKImmunity;
                var audio = AudioPlayerBase.Get(SoundAnnoncer.ReferenceHub);
                audio.Continue = true;
                audio.ShouldPlay = true;
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
        Instance = this;
    }

    public List<IRestable> Restables { get; } = [];

    public override void OnEnabled()
    {
        ServerEvents.WaitingForPlayers.Subscribe(Rest);
#if SOUND_API_SUPPORTED
        AudioPlayerBase.OnFinishedTrack += OnFinishTrack;
#endif
        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        ServerEvents.WaitingForPlayers.Unsubscribe(Rest);
#if SOUND_API_SUPPORTED
        AudioPlayerBase.OnFinishedTrack -= OnFinishTrack;
#endif
        base.OnDisabled();
    }

#if SOUND_API_SUPPORTED
    public void OnFinishTrack(AudioPlayerBase playerBase, string track, bool directPlay, ref int nextQueuePos)
    {
        if (_soundAnnoncer == null || _soundAnnoncer.ReferenceHub == null)
            return;

        if (!AudioPlayerBase.AudioPlayers.TryGetValue(_soundAnnoncer.ReferenceHub, out var myBase)
            || myBase != playerBase)
            return;

        _soundAnnoncer.Destroy();
        _soundAnnoncer = null;
    }
#endif

    public void Rest()
    {
        Log.Debug("Rest, base on Waiting For Players");
#if SOUND_API_SUPPORTED
        Timing.CallDelayed(1f, () =>
        {
            try
            {
                if (_soundAnnoncer != null)
                    _soundAnnoncer.Destroy();
            }
            catch { }
        });
        _soundAnnoncer = null;
#endif
        Timing.KillCoroutines(RoundRest_CoroutineTag);
        Restables.ForEach(p => p.Reset());
    }

    public void IgnoreCaseReplaces<T>(ref string str, string pattern, T replacement)
    {
        str = Regex.Replace(str, pattern, replacement.ToString(), RegexOptions.IgnoreCase);
    }

    public void IgnoreCaseReplaces(ref string str, params (string pattern, string replacement)[] replacements)
    {
        if (string.IsNullOrWhiteSpace(str)) return;

        for (int i = 0; i < replacements.Length; i++)
        {
            IgnoreCaseReplaces(ref str, replacements[i].pattern, replacements[i].replacement);
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
#if SOUND_API_SUPPORTED
            if (!string.IsNullOrEmpty(soundMessage) && SoundAnnoncer.ReferenceHub != null) try
            {
                var audio = AudioPlayerBase.Get(SoundAnnoncer.ReferenceHub);
                var lastIndex = audio.AudioToPlay.Count - 1;
                if (lastIndex >= 0 && audio.AudioToPlay[lastIndex] == soundMessage)
                    audio.AudioToPlay.RemoveAt(lastIndex);
                if (lastIndex == -1)
                    audio.Stoptrack(false);
            }
            catch { }
#endif

#if DEBUG
            throw;
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
                withoutSpace ??= string.Empty;
                withSpace ??= string.Empty; 
                return;
            }
        }

        var roleName = Translation.RoleName.Find(p => p.Role == role);
        if (roleName != null)
        {
            roleName.Deconstruct(out withSpace, out withoutSpace);
            withoutSpace ??= string.Empty;
            withSpace ??= string.Empty; 
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
        var roleName = Translation.RoleName.Find(p => p.Role.IsValid(player, hasCustomRole));
        if (roleName != null)
        {
            roleName.Deconstruct(out withSpace, out withoutSpace);
            withoutSpace ??= string.Empty;
            withSpace ??= string.Empty; 
            return;
        }

        if (!hasCustomRole && player.IsScp)
        {
            ConvertSCP(player.Role.Type, out withoutSpace, out withSpace);
            withoutSpace ??= string.Empty;
            withSpace ??= string.Empty;
            return;
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
