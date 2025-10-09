using System.Numerics;
using AEAssist;
using AEAssist.GUI;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Bindings.ImGui;
using Keys = AEAssist.Define.HotKey.Keys;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace ElliotZ.ModernJobViewFramework;

/// Qt窗口类
public class QtWindow : IDisposable {
  public class QtControl {
    public bool QtValue;
    public bool QtValueDefault;
    public string Name { get; }
    public string ToolTip { get; set; } = "";
    public Action<bool> OnClick { get; }
    // 自定义按钮颜色（优先于默认颜色）
    public Vector4 Color { get; set; }
    // 是否使用自定义颜色
    public bool UseColor { get; set; }

    public QtControl(string name, bool qtValueDefault, bool useColor) {
      Name = name;
      QtValueDefault = qtValueDefault;
      OnClick = _ => { };
      Color = QtStyle.DefaultMainColor;
      UseColor = useColor;
      Reset();
    }

    public QtControl(string name, bool qtValueDefault, Action<bool> action, bool useColor) {
      Name = name;
      QtValueDefault = qtValueDefault;
      OnClick = action;
      Color = QtStyle.DefaultMainColor;
      UseColor = useColor;
      Reset();
    }

    public QtControl(string name, bool qtValueDefault, Action<bool> action, Vector4 color) {
      Name = name;
      QtValueDefault = qtValueDefault;
      OnClick = action;
      Color = color;
      UseColor = true;
      Reset();
    }

    ///重置qt状态
    public void Reset() {
      QtValue = QtValueDefault;
    }
  }

  public JobViewSave Save;
  public readonly string Name;

  /// 动态按顺序储存qt名称的list，用于排序显示qt
  public List<string> QtNameList => Save.QtNameList;
  /// 隐藏的qt列表
  public List<string> QtUnVisibleList => Save.QtUnVisibleList;
  public Dictionary<string, HotkeyConfig> QtHotkeyConfig => Save.QtHotkeyConfig;

  ///窗口拖动
  public bool LockWindow {
    get => Save.LockQtWindow;
    set => Save.LockQtWindow = value;
  }

  ///QT按钮一行有几个
  public int QtLineCount {
    get => Save.QtLineCount;
    set => Save.QtLineCount = value;
  }

  private bool _disposed;
  /// 用于储存所有qt控件的字典
  private Dictionary<string, QtControl> _qtDict = new();

  /// 构造函数中恢复窗口位置
  public QtWindow(JobViewSave save, string name) {
    Save = save;
    Name = name;

    // 恢复窗口位置
    RestoreWindowPosition();
  }

  /// <summary>
  /// 恢复窗口位置
  /// </summary>
  private void RestoreWindowPosition() {
    // 确保位置在屏幕范围内
    Vector2 displaySize = ImGui.GetIO().DisplaySize;
    Vector2 pos = Save.QtWindowPos;

    // 限制位置在屏幕范围内
    pos.X = Math.Max(0, Math.Min(pos.X, displaySize.X - 100));
    pos.Y = Math.Max(0, Math.Min(pos.Y, displaySize.Y - 100));

    Save.QtWindowPos = pos;
  }

  public void AddQt(string qtName, bool qtValueDefault) {
    if (_qtDict.ContainsKey(qtName)) {
      return;
    }

    var qt = new QtControl(qtName, qtValueDefault, false);
    _qtDict.Add(qtName, qt);
    if (!QtNameList.Contains(qtName)) QtNameList.Add(qtName);
  }

  public void AddQt(string qtName, bool qtValueDefault, string toolTip) {
    if (_qtDict.ContainsKey(qtName)) {
      return;
    }

    var qt = new QtControl(qtName, qtValueDefault, false) {
        ToolTip = toolTip,
    };
    _qtDict.Add(qtName, qt);
    if (!QtNameList.Contains(qtName)) QtNameList.Add(qtName);
  }

  public void AddQt(string qtName, bool qtValueDefault, Action<bool> action) {
    if (_qtDict.ContainsKey(qtName)) {
      return;
    }

    var qt = new QtControl(qtName, qtValueDefault, action, false);
    _qtDict.Add(qtName, qt);
    if (!QtNameList.Contains(qtName)) QtNameList.Add(qtName);
  }

  public void AddQt(string qtName, bool qtValueDefault, Action<bool> action, Vector4 color) {
    if (_qtDict.ContainsKey(qtName)) {
      return;
    }

    var qt = new QtControl(qtName, qtValueDefault, action, color);
    _qtDict.Add(qtName, qt);
    if (!QtNameList.Contains(qtName)) QtNameList.Add(qtName);
  }

  public void RemoveAllQt() {
    _qtDict.Clear();
  }

  /// 设置上一次add添加的hotkey的toolTip
  public void SetQtToolTip(string toolTip) {
    _qtDict[_qtDict.Keys.ToArray()[^1]].ToolTip = toolTip;
  }

  public void SetQtColor(string name, Vector4 color) {
    _qtDict[name].UseColor = true;
    _qtDict[name].Color = color;
  }

  /// 获取指定名称qt的控件
  public QtControl GetQt(string qtName) {
    return !_qtDict.TryGetValue(qtName, out QtControl? value)
               ? new QtControl("noExist", false, false)
               : value;
  }

  /// 设置指定qt的值
  /// <returns>成功返回true，否则返回false</returns>
  public bool SetQt(string qtName, bool qtValue) {
    if (!_qtDict.TryGetValue(qtName, out QtControl? value)) {
      return false;
    }

    value.QtValue = qtValue;
    return true;
  }

  /// 反转指定qt的值
  /// <returns>成功返回true，否则返回false</returns>
  public bool ReverseQt(string qtName) {
    if (!_qtDict.TryGetValue(qtName, out QtControl? value)) {
      return false;
    }

    value.QtValue = !value.QtValue;
    return true;
  }
  
//  public void Reset() {
//    if (!Save.AutoReset) {
//      return;
//    }
//
//    foreach (QtControl qt in _qtDict.Select(qt => qt.Value)) {
//      qt.Reset();
//      LogHelper.Info("重置所有qt为默认值");
//    }
//  }

  /// 给指定qt设置新的默认值
  public void NewDefault(string qtName, bool newDefault) {
    if (!_qtDict.TryGetValue(qtName, out QtControl? value)) {
      return;
    }

    value.QtValueDefault = newDefault;
    LogHelper.Info($"改变qt \"{value.Name}\" 默认值为 {value.QtValueDefault}");
  }

  /// 将当前所有Qt状态记录为新的默认值
  public void SetDefaultFromNow() {
    foreach (QtControl qt in _qtDict.Select(qt => qt.Value)) {
      if (qt.QtValueDefault != qt.QtValue) {
        qt.QtValueDefault = qt.QtValue;
        LogHelper.Info($"改变qt \"{qt.Name}\" 默认值为 {qt.QtValueDefault}");
      }
    }
  }

  /// 返回包含当前所有qt名字的数组
  public string[] GetQtArray() {
    return _qtDict.Keys.ToArray();
  }

  // public string GetQtTooltip(string qtName)
  // {
  //     return _qtDict[qtName].ToolTip;
  // }
  //
  // public Vector4? GetQtColor(string qtName)
  // {
  //     if (_qtDict[qtName].UseColor) return _qtDict[qtName].Color;
  //     return null;
  // }

  /// 用于draw一个更改qt排序显示等设置的视图
  public void QtSettingView() {
    ImGui.Checkbox("显示QT控件", ref Save.ShowQt);
    //ImGui.Checkbox("战斗结束qt自动重置回战斗前状态", ref save.AutoReset);

    // 添加标签页切换按钮
    ImGui.Separator();
    ImGui.TextDisabled("   *左键拖动改变qt顺序，右键点击qt显示更多操作");

    for (int i = 0; i < QtNameList.Count; i++) {
      string item = QtNameList[i];
      string visible = !QtUnVisibleList.Contains(item) ? "显示" : "隐藏";

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

        if ((nNext < 0) || (nNext >= QtNameList.Count)) {
          continue;
        }

        QtNameList[i] = QtNameList[nNext];
        QtNameList[nNext] = item;
        ImGui.ResetMouseDragDelta();
      }

      //右键
      ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 1);
      ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.2f, 0.2f, 0.2f, 1));

      if (ImGuiHelper.IsRightMouseClicked()) {
        ImGui.OpenPopup($"###hotkeyPopup{Name + i}");
      }

      if (ImGui.BeginPopup($"###hotkeyPopup{Name + i}")) {
        //显示隐藏
        bool vis = !QtUnVisibleList.Contains(item);

        if (ImGui.Checkbox("显示", ref vis)) {
          if (!vis) {
            QtUnVisibleList.Add(item);
          } else {
            QtUnVisibleList.Remove(item);
          }
        }

        if (!QtHotkeyConfig.TryGetValue(item, out HotkeyConfig? hotkeyConfig)) {
          hotkeyConfig = new HotkeyConfig();
          QtHotkeyConfig[item] = hotkeyConfig;
        }

        //快捷键设置
        ImGuiHelper.KeyInput("快捷键设置", ref hotkeyConfig.Keys);
        ImGuiHelper.DrawEnum("组合键", ref hotkeyConfig.ModifierKey);

        if (ImGui.Button("重置")) QtHotkeyConfig.Remove(item);

        ImGui.EndPopup();
      }

      ImGui.PopStyleColor(1);
      ImGui.PopStyleVar(1);
    }
  }

  public void RunHotkey() {
    foreach (QtControl control
             in QtHotkeyConfig.Where(hotkey => 
                                         hotkey.Value.Keys != Keys.None)
                              .Where(hotkey => 
                                         Core.Resolve<MemApiHotkey>().CheckState(
                                             hotkey.Value.ModifierKey,
                                             hotkey.Value.Keys))
                              .Select(hotkey => 
                                          _qtDict[hotkey.Key])) {
      control.QtValue = !control.QtValue;
    }
  }

  public void Dispose() {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected void Dispose(bool disposing) {
    if (_disposed) return;

    if (disposing) _qtDict = null;
    _disposed = true;
  }
}
