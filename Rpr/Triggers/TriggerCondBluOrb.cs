using AEAssist.CombatRoutine.Trigger;
using AEAssist.GUI;
using ImGuiNET;

namespace ElliotZ.Rpr.Triggers;

public class TriggerCondBluOrb : ITriggerCond
{
    [LabelName("检查蓝豆子是否大于等于特定值")]
    public int BluOrb { get; set; }

    public string DisplayName => "Reaper/夜游魂量谱";
    public string Remark { get; set; }

    public bool Draw()
    {
        return false;
    }

    public bool Handle(ITriggerCondParams triggerCondParams)
    {
        return RprHelper.BlueOrb >= BluOrb;
    }
}