using AEAssist.CombatRoutine.View.JobView;

namespace ElliotZ.Common.ModernJobViewFramework.HotKey;

public class HotkeyControl
{
    internal readonly string Name;
    internal IHotkeyResolver Slot;
    internal string ToolTip = "";

    internal HotkeyControl(string name, IHotkeyResolver slot)
    {
        Name = name;
        Slot = slot;
    }
}