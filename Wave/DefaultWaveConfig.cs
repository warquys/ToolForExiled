using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Respawning.Config;

namespace ToolForExiled.Wave;

public class DefaultWaveConfig : IWaveConfig
{
    public bool IsEnabled { get; set; } = true;
}
