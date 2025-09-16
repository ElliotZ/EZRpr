using System.Numerics;
using AEAssist.CombatRoutine.Trigger;
using Dalamud.Bindings.ImGui;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.Triggers;

public class TriggerActionQt : ITriggerAction {
  public string DisplayName { get; } = "Reaper/QT";
  public string Remark { get; set; } = "";

  private readonly Dictionary<string, bool> _qtValues = new();

  private readonly string[] _qtArray = Qt.Instance.GetQtArray();

  public bool Draw() {
    ImGui.NewLine();
    ImGui.Separator();
    ImGui.Text("点击按钮在三种状态间切换：未添加 / 已关闭 / 已启用");
    ImGui.NewLine();
    const int columns = 5;
    int count = 0;

    foreach (string qt in _qtArray) {
      ImGui.PushID(qt);

      if (_qtValues.TryGetValue(qt, out bool isEnabled)) {
        ImGui.PushStyleColor(ImGuiCol.Text,
                             isEnabled
                                 ? new Vector4(0f, 1f, 0f, 1f) // ✅ 启用：绿色
                                 : new Vector4(1.0f, 0.4f, 0.7f, 1.0f) // ❌ 未启用：粉红色
        );
      } else {
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 1f, 1f)); // 🆕 未添加：默认白
      }

      if (ImGui.Button(qt)) {
        if (_qtValues.TryAdd(qt, false)) { } // 🆕 → ❌
        else if (!_qtValues[qt]) {
          _qtValues[qt] = true; // ❌ → ✅
        } else {
          _qtValues.Remove(qt); // ✅ → 🆕
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

    if (_qtValues.Count is 0) return true;

    List<string> toRemove = [];

    foreach (var kvp in _qtValues) {
      string qt = kvp.Key;
      bool val = kvp.Value;

      ImGui.PushID(qt);
      if (ImGui.Checkbox(" ", ref val)) _qtValues[qt] = val;

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
    foreach (string qt in toRemove) _qtValues.Remove(qt);

    ImGui.Separator();

    if (ImGui.Button("全部启用")) {
      foreach (string key in _qtValues.Keys.ToList()) {
        _qtValues[key] = true;
      }
    }

    ImGui.SameLine();

    if (ImGui.Button("全部关闭")) {
      foreach (string key in _qtValues.Keys.ToList()) {
        _qtValues[key] = false;
      }
    }

    ImGui.SameLine();

    if (ImGui.Button("清除所有")) _qtValues.Clear();

    return true;
  }

  public bool Handle() {
    foreach (var kvp in _qtValues) Qt.Instance.SetQt(kvp.Key, kvp.Value);
    return true;
  }
}
