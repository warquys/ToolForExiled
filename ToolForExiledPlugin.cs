#if SOUND_API_SUPPORTED
using SCPSLAudioApi.AudioCore;
#endif
using System.Text.RegularExpressions;
using Exiled.API.Extensions;
using MEC;
using PlayerRoles;
using UserSettings.ServerSpecific;
using static NineTailedFoxAnnouncer;

namespace ToolForExiled;

public class ToolForExiledPlugin : Plugin<ToolForExiledConfig, ToolForExiledTranslation>
{
    public const int IdForAudio = 840;

    public const string CommandConfigPermission = "ConfigHelp";

    public const string RoundRest_CoroutineTag = "kill_at_rest_(waitinforplayer)";

    public static ToolForExiledPlugin Instance 
    { 
        get; 
        private set; 
    }

    public Harmony Harmony { get; private set; }

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
        Harmony = new Harmony(PluginInfo.PLUGIN_GUID);
    }

    public List<IRestable> Restables { get; } = [];

    public override void OnEnabled()
    {
        ServerEvents.WaitingForPlayers.Subscribe(Rest);
        Harmony.DEBUG = Config.Debug;
        Harmony.PatchAll();
        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        ServerEvents.WaitingForPlayers.Unsubscribe(Rest);
        Harmony.UnpatchAll(Harmony.Id);
        base.OnDisabled();
    }

    public void Rest()
    {
        Log.Debug("Rest, base on Waiting For Players");
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
