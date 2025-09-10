using AEAssist.CombatRoutine.Trigger;
using ImGuiNET;

namespace ElliotZ.Rpr.Triggers;

public class TriggerCondShroud : ITriggerCond
{
    public int Shroud { get; set; }

    public string DisplayName => "魂衣量谱";
    public string Remark { get; set; }

    public bool Draw()
    {
        ImGui.Text("检查蓝条是否大于等于特定值");
        return true;
    }

    public bool Handle(ITriggerCondParams triggerCondParams)
    {
        return RprHelper.Shroud >= Shroud;
    }
}