using System.Numerics;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.Helper;
using Dalamud.Bindings.ImGui;
using ElliotZ.Rpr.QtUI;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace ElliotZ.Rpr.Triggers;

public class TriggerActionQt : ITriggerAction {
  public string DisplayName { get; } = "Reaper/QT";
  public string Remark { get; set; } = "";

  public Dictionary<string, bool> QTValues = new();

  private readonly string[] _qtArray;
  
  public TriggerActionQt() {
    _qtArray = Qt.Instance.GetQtArray();
  }

  public bool Draw() {
    ImGui.NewLine();
    ImGui.Separator();
    ImGui.Text("点击按钮在三种状态间切换：未添加 / 已关闭 / 已启用");
    ImGui.NewLine();
    const int columns = 5;
    int count = 0;

    foreach (string qt in _qtArray) {
      ImGui.PushID(qt);

      if (QTValues.TryGetValue(qt, out bool isEnabled)) {
        ImGui.PushStyleColor(ImGuiCol.Text,
                             isEnabled
                                 ? new Vector4(0f, 1f, 0f, 1f) // ✅ 启用：绿色
                                 : new Vector4(1.0f, 0.4f, 0.7f, 1.0f) // ❌ 未启用：粉红色
        );
      } else {
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 1f, 1f)); // 🆕 未添加：默认白
      }

      if (ImGui.Button(qt)) {
        if (QTValues.TryAdd(qt, false)) { } // 🆕 → ❌
        else if (!QTValues[qt]) {
          QTValues[qt] = true; // ❌ → ✅
        } else {
          QTValues.Remove(qt); // ✅ → 🆕
        }
      }

      ImGui.PopStyleColor();
      ImGui.PopID();

      if (++count % columns != 0) {
        ImGui.SameLine();
      }
    }

    ImGui.NewLine();
    ImGui.Separator();

    if (QTValues.Count is 0) return true;

    List<string> toRemove = [];

    foreach (var kvp in QTValues) {
      string qt = kvp.Key;
      bool val = kvp.Value;

      ImGui.PushID(qt);
      if (ImGui.Checkbox(" ", ref val)) QTValues[qt] = val;

      ImGui.SameLine();
      ImGui.Text(qt);
      ImGui.SameLine();

      Vector4 color = val ? new Vector4(0f, 1f, 0f, 1f) : new Vector4(1f, 0f, 0f, 1f);
      string status = val ? "（已启用）" : "（已关闭）";
      ImGui.TextColored(color, status);

      ImGui.SameLine();
      if (ImGui.Button("删除")) toRemove.Add(qt);

      ImGui.PopID();
    }

    // 删除被标记的项
    foreach (string qt in toRemove) QTValues.Remove(qt);

    ImGui.Separator();

    if (ImGui.Button("全部启用")) {
      foreach (string key in QTValues.Keys.ToList()) {
        QTValues[key] = true;
      }
    }

    ImGui.SameLine();

    if (ImGui.Button("全部关闭")) {
      foreach (string key in QTValues.Keys.ToList()) {
        QTValues[key] = false;
      }
    }

    ImGui.SameLine();

    if (ImGui.Button("清除所有")) QTValues.Clear();

    return true;
  }

  public bool Handle() {
    foreach (var kvp in QTValues) {
      if (RprSettings.Instance.TimelineDebug) {
        LogHelper.Print("轴控",$"设置Qt{kvp.Key} => {kvp.Value}");
      }
      Qt.Instance.SetQt(kvp.Key, kvp.Value);
    }

    return true;
  }
}
