using System.Numerics;
using AEAssist.CombatRoutine;
using AEAssist.Helper;
using Dalamud.Bindings.ImGui;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace ElliotZ.ModernJobViewFramework.HotKey;

public static class HotKeyConfig {
  //public static Dictionary<string, uint> SpellList = HotKeySpellConfig.List;
  public static Dictionary<int, HotKeyTarget> TargetList = HotKeyTargetConfig.List;

  private static string _selectSpellName = "闪灼";
  private static uint _selectSpell = 25859u;
  private static string? _selectTargetName = "自己";
  private static int _targetKey = 1;
  private static HotKeyTarget _selectTarget = new("自己", SpellTargetType.Self);

  public static void DrawHotKeyConfigView(HotkeyWindow hotkeyWindow,
                                          ref Dictionary<string, HotKeySpell> hotkeyConfig,
                                          Dictionary<string, uint> spell,
                                          Action save) {
    if (!GlobalSetting.Instance.HotKey配置窗口) return;
    ImGui.SetNextWindowSize(new Vector2(375f, 350f), ImGuiCond.FirstUseEver);
    ImGui.SetNextWindowSizeConstraints(new Vector2(50f, 50f),
                                       new Vector2(float.MaxValue, float.MaxValue));
    ImGui.Begin("HotkeyConfig", ref GlobalSetting.Instance.HotKey配置窗口);
    ImGui.Indent();

    if (hotkeyConfig.Count > 0) {
      foreach ((string key, HotKeySpell value) in hotkeyConfig) {
        ImGui.Text($"使用技能:[{value.Spell.GetSpell().Name}]");
        ImGui.SameLine();
        ImGui.Text($"对[{TargetList[value.Target].Name}]释放。");

        if (ImGui.Button($"删除-{key}")) {
          hotkeyWindow.RemoveHotKey(key);
          hotkeyConfig.Remove(key);
          save();
        }
      }
    }

    ImGui.Unindent();
    ImGui.Separator();

    ImGui.Text("使用:");
    ImGui.SetNextItemWidth(200f);
    ImGui.SameLine();

    if (ImGui.BeginCombo("###选择技能", _selectSpellName)) {
      foreach (var kvs
               in spell.Where(kvs => 
                                  ImGui.Selectable(kvs.Key))) {
        _selectSpellName = kvs.Key;
        _selectSpell = kvs.Value;
      }

      ImGui.EndCombo();
    }

    ImGui.Text("对");
    ImGui.SameLine();
    ImGui.SetNextItemWidth(200f);

    if (ImGui.BeginCombo("###选择目标", _selectTargetName)) {
      foreach (var kvs
               in TargetList.Where(kvs => 
                                       ImGui.Selectable(kvs.Value.Name))) {
        _targetKey = kvs.Key;
        _selectTargetName = kvs.Value.Name;
        _selectTarget = kvs.Value;
      }

      ImGui.EndCombo();
    }

    ImGui.SameLine();
    ImGui.Text("释放。");

    string newHotkeyName = $"{_selectSpellName}{_selectTargetName}";

    if (hotkeyConfig.ContainsKey(newHotkeyName)) {
      ImGui.Text("该技能组合已存在!");
    } else {
      if (ImGui.Button("新增HOTKEY")) {
        hotkeyWindow.AddHotkey(newHotkeyName,
                               new HotKeyResolver(_selectSpell.GetSpell(), _selectTarget));
        hotkeyConfig.Add(newHotkeyName, new HotKeySpell(newHotkeyName, _selectSpell, _targetKey));
        save();
      }
    }

    ImGui.End();
  }
}
