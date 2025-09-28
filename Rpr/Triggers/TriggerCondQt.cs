using System.Drawing;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.GUI;
using Dalamud.Bindings.ImGui;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.Triggers;

public class TriggerCondQt : ITriggerCond {
  public string DisplayName => "Reaper/QT检测";
  public string Remark { get; set; } = "";

  public string Key = "";
  public bool Value;

  private int _selectIndex;
  private readonly string[] _qtArray = Qt.Instance.GetQtArray();

  public bool Draw() {
    _selectIndex = Array.IndexOf(_qtArray, Key);
    if (_selectIndex == -1) _selectIndex = 0;
    ImGuiHelper.LeftCombo("选择Key", ref _selectIndex, _qtArray);
    Key = _qtArray[_selectIndex];
    ImGui.SameLine();

    using (new GroupWrapper()) {
      ImGui.Checkbox("", ref Value);
    }

    ImGuiHelper.TextColor(Color.Orange, "判断该qt的状态");
    return true;
  }

  public bool Handle(ITriggerCondParams triggerCondParams) {
    return Qt.Instance.GetQt(Key) == Value;
  }
}
