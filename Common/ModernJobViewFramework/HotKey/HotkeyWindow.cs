using System.Numerics;
using AEAssist;
using AEAssist.CombatRoutine.View.JobView;
using AEAssist.Define.HotKey;
using AEAssist.GUI;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Bindings.ImGui;
using Keys = AEAssist.Define.HotKey.Keys;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace ElliotZ.ModernJobViewFramework.HotKey;

/// 快捷键窗口类
public class HotkeyWindow(JobViewSave save, string name) : IDisposable {
  private bool _disposed;
  public JobViewSave Save = save;

  /// 用于储存所有hotkey控件的字典
  private Dictionary<string, HotkeyControl> _hotkeyDict = new();

  /// 处于激活状态的hotkey列表
  public List<string> ActiveList = [];

  /// 动态按顺序储存hotkey名称的list，用于排序显示hotkey
  public List<string> HotkeyNameList => Save.HotkeyNameList;

  /// 记录控件名称和对应快捷键的字典
  public Dictionary<string, HotkeyConfig> HotkeyConfig => Save.HotkeyConfig;

  ///窗口拖动
  public bool LockWindow {
    get => Save.LockHotkeyWindow;
    set => Save.LockHotkeyWindow = value;
  }

  private Dictionary<string, long> _lastActiveTime = new();

  //public Dictionary<string, HotKeySpell> HotKeyConfigs = config;

  /// 隐藏的hotkey列表
  public List<string> HotkeyUnVisibleList => Save.HotkeyUnVisibleList;

  ///hotkey按钮一行有几个
  public int HotkeyLineCount {
    get => Save.HotkeyLineCount;
    set => Save.HotkeyLineCount = value;
  }

  //public void CreateHotkey()
  //{
  //    if (HotKeyConfigs.Count <= 0) return;
  //    foreach (var kvs in HotKeyConfigs)
  //    {
  //        AddHotkey(kvs.Value.Name,
  //            new HotKeyResolver(kvs.Value.spell.GetSpell(), new HotKeyTarget(
  //                HotKeyTargetConfig.List[kvs.Value.target].Name,
  //                HotKeyTargetConfig.List[kvs.Value.target].SpellTargetType)
  //                //HotKeyTargetConfig.List[kvs.Value.target].Func,
  //                //HotKeyTargetConfig.List[kvs.Value.target].CharacterAgent)
  //            ));
  //    }
  //}

  /// <summary>
  /// 添加新的Hotkey控件
  /// </summary>
  public void AddHotkey(string hotkeyName, IHotkeyResolver slot) {
    if (_hotkeyDict.ContainsKey(hotkeyName)) {
      return;
    }

    var hotkey = new HotkeyControl(hotkeyName, slot);
    _hotkeyDict.Add(hotkeyName, hotkey);
    _hotkeyDict[hotkeyName]._slot = slot;
    if (!HotkeyNameList.Contains(hotkeyName)) HotkeyNameList.Add(hotkeyName);
  }

  public void RemoveHotKey(string hotkeyName) {
    _hotkeyDict.Remove(hotkeyName);
    HotkeyNameList.Remove(hotkeyName);
    HotkeyConfig.Remove(hotkeyName);
  }

  /// 设置上一次add添加的hotkey的toolTip
  public void SetHotkeyToolTip(string toolTip) {
    _hotkeyDict[_hotkeyDict.Keys.ToArray()[^1]]._toolTip = toolTip;
  }

  /// 返回包含当前所有hotkey名字的数组
  public string[] GetHotkeyArray() {
    return _hotkeyDict.Keys.ToArray();
  }

  /// 获取Hotkey控件
  public HotkeyControl? GetHotkey(string hkName) {
    return _hotkeyDict.GetValueOrDefault(hkName);
  }

  /// 激活单个快捷键,mo无效
  public void SetHotkey(string hotkeyName) {
    if (!ActiveList.Contains(hotkeyName)
     && _hotkeyDict.TryGetValue(hotkeyName, out HotkeyControl? value))
        //激活按钮
    {
      RunSlot(value);
    }
  }

  public void DrawHotkeyWindow(QtStyle style) {
    //HotKeyConfig.DrawHotKeyConfigView(this, ref HotKeyConfigs, spell,saveAction);

    if (!Save.ShowHotkey) {
      return;
    }

    int num = HotkeyNameList.Count - HotkeyUnVisibleList.Count;

    if (num <= 0) {
      return;
    }

    long now = TimeHelper.Now();

    //更新按钮是否激活的状态
    for (int i = ActiveList.Count - 1; i >= 0; i--) {
      string hotkeyName = ActiveList[i];

      if (!_hotkeyDict.ContainsKey(hotkeyName)) {
        continue;
      }

      if (!_lastActiveTime.TryGetValue(hotkeyName, out long lastActive)) {
        continue;
      }

      // cd暂定0.5秒,也就是0.5秒内的重复点击无效
      if (now - lastActive >= 500) ActiveList.RemoveAt(i);
    }

    style.SetStyle();
    int line = num / HotkeyLineCount;

    if (num % HotkeyLineCount != 0) {
      line++;
    }

    int row = num < HotkeyLineCount ? num : HotkeyLineCount;
    Vector2 hotKeySize = QtStyle.OverlayScale * Save.QtHotkeySize;
    float width = (row - 1) * 6 + 16 + hotKeySize.X * row;
    float height = (line - 1) * 6 + 16 + hotKeySize.Y * line;
    // 设置窗口大小和位置
    ImGui.SetNextWindowSize(new Vector2(width, (int)height));
    ImGuiWindowFlags flag = LockWindow
                                ? QtStyle.QtWindowFlag | ImGuiWindowFlags.NoMove
                                : QtStyle.QtWindowFlag;
    ImGui.Begin($"###Hotkey_Window{name}", flag);

    int index = 0;

    //画出控件
    for (int i = 0; i < HotkeyNameList.Count; i++) {
      string hotkeyName = HotkeyNameList[i];

      if (!_hotkeyDict.ContainsKey(hotkeyName)) {
        continue;
      }

      if (HotkeyUnVisibleList.Contains(hotkeyName)) {
        continue;
      }

      HotkeyControl hotkey = _hotkeyDict[hotkeyName];
      ImGui.BeginChild($"###Hotkey_HotkeyControl{name + i}",
                       hotKeySize,
                       false,
                       QtStyle.QtWindowFlag);
      hotkey._slot.Draw(hotKeySize);

      //鼠标点击
      if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) {
        ImGui.BeginTooltip();
        ImGui.Text($"{hotkeyName}");

        if (hotkey._toolTip != "") {
          ImGui.Text($"{hotkey._toolTip}");
        }

        ImGui.EndTooltip();
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left)) RunSlot(hotkey);
      }

      hotkey._slot.DrawExternal(hotKeySize, ActiveList.Contains(hotkeyName));

      if (HotkeyConfig.TryGetValue(hotkeyName, out HotkeyConfig? hotkeyConfig)
       && (hotkeyConfig.Keys != Keys.None)) {
        //显示快捷键
        string text = HotkeySplice(hotkeyConfig.ModifierKey, hotkeyConfig.Keys);
        ImGui.SetCursorPos(new Vector2(0, 0));
        ImGui.Text(text);
      }

      ImGui.EndChild();

      if (index % HotkeyLineCount != HotkeyLineCount - 1) ImGui.SameLine();
      index++;
    }

    style.EndStyle();
    ImGui.End();
  }

  /// 用于draw一个更改qt排序显示等设置的视图
  public void HotkeySettingView() {
    ImGui.Checkbox("显示热键栏", ref Save.ShowHotkey);
    ImGui.TextDisabled("   *左键拖动改变hotkey顺序，右键点击显示更多");

    for (int i = 0; i < HotkeyNameList.Count; i++) {
      string item = HotkeyNameList[i];
      string visible = !HotkeyUnVisibleList.Contains(item) ? "显示" : "隐藏";

      if (visible == "隐藏") {
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.6f, 0, 0, 1));
      }

      ImGui.Selectable($"   {visible}        {item}");

      if (visible == "隐藏") {
        ImGui.PopStyleColor(1);
      }

      //排序        
      if (ImGui.IsItemActive() && !ImGui.IsItemHovered()) {
        int nNext = i + (ImGui.GetMouseDragDelta(ImGuiMouseButton.Left).Y < 0f ? -1 : 1);

        if ((nNext < 0) || (nNext >= HotkeyNameList.Count)) {
          continue;
        }

        HotkeyNameList[i] = HotkeyNameList[nNext];
        HotkeyNameList[nNext] = item;
        ImGui.ResetMouseDragDelta();
      }

      //右键
      ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 1);
      ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.2f, 0.2f, 0.2f, 1));

      if (ImGuiHelper.IsRightMouseClicked()) {
        ImGui.OpenPopup($"###hotkeyPopup{name + i}");
      }

      if (ImGui.BeginPopup($"###hotkeyPopup{name + i}")) {
        //显示隐藏
        bool vis = !HotkeyUnVisibleList.Contains(item);

        if (ImGui.Checkbox("显示", ref vis)) {
          if (!vis) {
            HotkeyUnVisibleList.Add(item);
          } else {
            HotkeyUnVisibleList.Remove(item);
          }
        }

        if (!HotkeyConfig.TryGetValue(item, out HotkeyConfig? hotkeyConfig)) {
          hotkeyConfig = new HotkeyConfig();
          HotkeyConfig[item] = hotkeyConfig;
        }

        //快捷键设置
        ImGuiHelper.KeyInput("快捷键设置", ref hotkeyConfig.Keys);
        ImGuiHelper.DrawEnum("组合键", ref hotkeyConfig.ModifierKey);

        if (ImGui.Button("重置")) HotkeyConfig.Remove(item);

        ImGui.EndPopup();
      }

      ImGui.PopStyleColor(1);
      ImGui.PopStyleVar(1);
    }
  }

  ///快捷键显示格式化
  private static string HotkeyFormat(Keys key) {
    return key switch {
        Keys.Space => "SPC",
        Keys.Oemtilde => "`",
        Keys.D1 => "1",
        Keys.D2 => "2",
        Keys.D3 => "3",
        Keys.D4 => "4",
        Keys.D5 => "5",
        Keys.D6 => "6",
        Keys.D7 => "7",
        Keys.D8 => "8",
        Keys.D9 => "9",
        Keys.D0 => "0",
        Keys.OemMinus => "-",
        Keys.Oemplus => "+",
        Keys.OemOpenBrackets => "[",
        Keys.Oem6 => "]",
        Keys.Oem5 => "\\",
        Keys.Oem1 => ";",
        Keys.Oem7 => "'",
        Keys.LShiftKey => "s",
        Keys.Oemcomma => ",",
        Keys.OemPeriod => ".",
        Keys.OemQuestion => "/",
        Keys.RShiftKey => "s",
        Keys.LControlKey => "c",
        Keys.LMenu => "a",
        Keys.RMenu => "a",
        Keys.RControlKey => "c",
        _ => $"{key}",
    };
  }

  public static string HotkeySplice(ModifierKey modifierKey, Keys key) {
    string md = modifierKey switch {
        ModifierKey.Alt => "Alt+",
        ModifierKey.Control => "Ctrl+",
        ModifierKey.Shift => "Shift+",
        _ => "",
    };
    return $"{md}{HotkeyFormat(key)}";
  }

  private void RunSlot(HotkeyControl hotkey) {
    if (hotkey._slot.Check() < 0) {
      return;
    }

    if (!ActiveList.Contains(hotkey._name)) {
      ActiveList.Add(hotkey._name);
      _lastActiveTime[hotkey._name] = TimeHelper.Now();
      hotkey._slot.Run();
    }
  }

  /// <summary>
  /// 运行键盘快捷键模块,一般放在update中
  /// </summary>
  public void RunHotkey() {
    foreach (var hotkey
             in HotkeyConfig.Where(hotkey => 
                                       hotkey.Value.Keys != Keys.None)
                            .Where(hotkey => 
                                       Core.Resolve<MemApiHotkey>().CheckState(
                                           hotkey.Value.ModifierKey, 
                                           hotkey.Value.Keys))) { //激活按钮
      RunSlot(_hotkeyDict[hotkey.Key]);
    }
  }

  public void Dispose() {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected void Dispose(bool disposing) {
    if (_disposed) return;

    if (disposing) {
      _hotkeyDict = null;
      ActiveList = null;
      _lastActiveTime = null;
    }

    _disposed = true;
  }
}
