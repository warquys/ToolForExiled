namespace ToolForExiled;

public class Config : IConfig
{
    public bool IsEnabled { get => true; set { } }
    public bool Debug { get; set; }

    public string AnnoncerName { get; set; }

}