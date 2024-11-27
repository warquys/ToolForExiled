namespace ToolForExiled;

public interface IRolesRestrained
{
    IEnumerable<RoleInformation> ValidRoles { get; }
}

public interface IRoleRestrained
{
    RoleInformation ValidRole { get; }
}