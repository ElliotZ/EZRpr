using AEAssist.CombatRoutine.Trigger;
using ImGuiNET;

namespace ElliotZ.Rpr.Triggers;

public class TriggerCondSoul : ITriggerCond
{
    public int Soul { get; set; }

    public string DisplayName => "灵魂量谱";
    public string Remark { get; set; }

    public bool Draw()
    {
        ImGui.Text("检查红条是否大于等于特定值");
        return true;
    }

    public bool Handle(ITriggerCondParams triggerCondParams)
    {
        return RprHelper.Soul >= Soul;
    }
}