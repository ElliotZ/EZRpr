using AEAssist.CombatRoutine.Trigger;
using AEAssist.Helper;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Components;
// ReSharper disable MemberCanBePrivate.Global

namespace ElliotZ.Rpr.Triggers;

public class TriggerActionAcrModeSettings : ITriggerAction {
  public string DisplayName { get; } = "Reaper/Acr模式";
  public string Remark { get; set; } = "";
  public bool Value;

  public bool Draw() {
    ImGuiComponents.ToggleButton("Mode", ref Value);
    ImGui.SameLine();
    ImGui.Text("右侧表示高难模式");
    return true;
  }

  public bool Handle() {
    RprSettings.Instance.IsHardCoreMode = Value;

    if (Value) {
      RprHelper.HardCoreMode();
    } else {
      RprHelper.CasualMode();
    }

    BattleData.IsChanged = true;

    if (RprSettings.Instance.TimelineDebug) {
      LogHelper.Print("轴控", "设置" + (Value ? "高难模式" : "日随模式"));
    }

    return true;
  }
}
