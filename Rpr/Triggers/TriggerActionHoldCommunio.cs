using AEAssist.CombatRoutine.Trigger;
using AEAssist.Helper;
using Dalamud.Bindings.ImGui;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace ElliotZ.Rpr.Triggers;

public class TriggerActionHoldCommunio : ITriggerAction {
  public string DisplayName => "Reaper/保留团契";
  public int Value;
  public string Remark { get; set; } = "";

  public bool Draw() {
    string type = Value switch {
        0 => "不保留",
        1 => "死亡之影填充",
        2 => "勾刃填充",
        _ => "",
    };
    ImGui.SetNextItemWidth(120f);

    if (ImGui.BeginCombo("选择团契保留类型", type)) {
      if (ImGui.Selectable("不保留", Value == 0)) {
        Value = 0;
      }

      if (ImGui.Selectable("死亡之影填充", Value == 1)) {
        Value = 1;
      }

      if (ImGui.Selectable("勾刃填充", Value == 2)) {
        Value = 2;
      }

      ImGui.EndCombo();
    }

    return true;
  }

  public bool Handle() {
    BattleData.Instance.HoldCommunio = Value;
    if (RprSettings.Instance.TimelineDebug) {
      string state = Value switch {
          0 => "不保留团契",
          1 => "保留团契并用死亡之影填充",
          2 => "保留团契并用勾刃填充",
          _ => "错误",
      };
      LogHelper.Print("轴控", state);
    }

    return true;
  }
}
