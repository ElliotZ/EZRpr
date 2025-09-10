using AEAssist.CombatRoutine.Trigger;
using ImGuiNET;

namespace ElliotZ.Rpr.Triggers;

public class TriggerCondPurpOrb : ITriggerCond
{
    public int PurpOrb { get; set; }

    public string DisplayName => "虚无魂量谱";
    public string Remark { get; set; }

    public bool Draw()
    {
        ImGui.Text("检查紫豆子是否大于等于特定值");
        return true;
    }

    public bool Handle(ITriggerCondParams triggerCondParams)
    {
        return RprHelper.PurpOrb >= PurpOrb;
    }
}