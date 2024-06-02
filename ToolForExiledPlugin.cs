#if SOUND_API_SUPPORTED
using AudioPlayer.API;
#endif
using CommandSystem;
using Exiled.API.Extensions;
using Exiled.API.Features.Roles;
using MEC;
using PlayerRoles;
using Utils.NonAllocLINQ;
using static NineTailedFoxAnnouncer;

namespace ToolForExiled;

public class ToolForExiledPlugin : Plugin<Config, Translation>
{
    public const int IdForAudio = 840;

    public const string CommandConfigPermission = "ConfigHelp";

    public const string RoundRest_CoroutineTag = "kill_at_rest_(waitinforplayer)";

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
        Timing.KillCoroutines(RoundRest_CoroutineTag);
        Restables.ForEach(p => p.Reset());
    }

    public void TryCassie(string message, string translation, string soungMessage)
    {
#if SOUND_API_SUPPORTED
        if (!string.IsNullOrEmpty(soungMessage))
            AudioController.PlayAudioFromFile(soungMessage, id: IdForAudio);
#endif

        if (!string.IsNullOrEmpty(message))
        {
            if (!string.IsNullOrEmpty(translation))
                Cassie.MessageTranslated(message, translation);
            else
                Cassie.Message(message);
        }
    }

    public void CassieConvertRoleName(RoleInformation role, out string withoutSpace, out string withSpace)
    {
        if (role.RoleType == RoleType.Vanila)
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
