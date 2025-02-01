using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UserSettings.ServerSpecific;

namespace ToolForExiled;

public static class UserSettingFactory
{
    public static SSTwoButtonsSetting CreateYesNo(int? id, string label, bool defaultEnable, string hint) =>
        new SSTwoButtonsSetting(id, label,
            optionA: ToolForExiledPlugin.Instance.Translation.ButtonDisable,
            optionB: ToolForExiledPlugin.Instance.Translation.ButtonEnable,
            defaultEnable, hint);


}
