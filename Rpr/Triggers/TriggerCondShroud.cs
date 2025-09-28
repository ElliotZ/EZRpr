using AEAssist.CombatRoutine.Trigger;
using AEAssist.GUI;

namespace ElliotZ.Rpr.Triggers;

public class TriggerCondShroud : ITriggerCond {
  [LabelName("检查蓝条是否大于等于特定值")] public int Shroud { get; set; }

  public string DisplayName => "Reaper/魂衣量谱";
  public string Remark { get; set; } = "";

  public bool Draw() {
    return false;
  }

  public bool Handle(ITriggerCondParams triggerCondParams) {
    return RprHelper.Shroud >= Shroud;
  }
}
