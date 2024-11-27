namespace ToolForExiled;

public class ToolForExiledConfig : IConfig
{
    public bool IsEnabled { get => true; set { } }
    public bool Debug { get; set; }

    public string AnnoncerName { get; set; } = "Cassie";

}