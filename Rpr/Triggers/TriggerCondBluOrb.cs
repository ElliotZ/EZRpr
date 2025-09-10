using AEAssist.CombatRoutine.Trigger;
using ImGuiNET;

namespace ElliotZ.Rpr.Triggers;

public class TriggerCondBluOrb : ITriggerCond
{
    public int BluOrb { get; set; }

    public string DisplayName => "夜游魂量谱";
    public string Remark { get; set; }

    public bool Draw()
    {
        ImGui.Text("检查蓝豆子是否大于等于特定值");
        return true;
    }

    public bool Handle(ITriggerCondParams triggerCondParams)
    {
        return RprHelper.BlueOrb >= BluOrb;
    }
}