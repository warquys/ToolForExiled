namespace ToolForExiled;

public enum RoleTypeSystem
{
    Vanila,
    // lol i forget an l
    Vanilla = Vanila,
    CustomRoleExiled,
    // lol i forget an s
    CustomRolesExiled = CustomRoleExiled,
#if UNCOMPLICATED_ROLE_SUPPORTED
    UncomplicatedCustomRole,
#endif

    // the system that you use
}