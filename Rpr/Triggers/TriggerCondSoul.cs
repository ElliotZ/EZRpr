using AEAssist.CombatRoutine.Trigger;
using AEAssist.GUI;
using ImGuiNET;

namespace ElliotZ.Rpr.Triggers;

public class TriggerCondSoul : ITriggerCond
{
    [LabelName("检查红条是否大于等于特定值")]
    public int Soul { get; set; }

    public string DisplayName => "Reaper/灵魂量谱";
    public string Remark { get; set; }

    public bool Draw()
    {
        return false;
    }

    public bool Handle(ITriggerCondParams triggerCondParams)
    {
        return RprHelper.Soul >= Soul;
    }
}