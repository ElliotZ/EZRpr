using AEAssist.CombatRoutine.Trigger;
using AEAssist.GUI;

namespace ElliotZ.Rpr.Triggers;

public class TriggerCondPurpOrb : ITriggerCond {
  [LabelName("检查紫豆子是否大于等于特定值")] public int PurpOrb { get; set; }

  public string DisplayName => "Reaper/虚无魂量谱";
  public string Remark { get; set; }

  public bool Draw() {
    return false;
  }

  public bool Handle(ITriggerCondParams triggerCondParams) {
    return RprHelper.PurpOrb >= PurpOrb;
  }
}
