using System.ComponentModel;

namespace ToolForExiled;

public class ToolForExiledConfig : IConfig
{
    public bool IsEnabled { get => true; set { } }
    public bool Debug { get; set; }

    public string AnnoncerName { get; set; } = "Cassie";

    [Description("Use to avoid the rate limite of hint and broadcast")]
    public float MinTimeBetweenHint { get; set; } = 1f;
}