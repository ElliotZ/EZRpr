using System.Numerics;
using AEAssist.CombatRoutine.View.JobView;
using AEAssist.Helper;
using Dalamud.Bindings.ImGui;
using Dalamud.Utility;
using JobViewWindow = ElliotZ.ModernJobViewFramework.JobViewWindow;

namespace ElliotZ; // 改成你需要的Namespace，或者由IDE自动处理

/// <summary>
/// 宏命令控制Hotkey和QT的（希望算是比较易用的）轮子
/// </summary>
/// <param name="instance">QT窗口的参照</param>
/// <param name="cmdHandle">宏命令的柄，通常可以用ACR的名字，一定要用“/”开头</param>
/// <param name="qtKeys">QT的列表</param>
/// <param name="hkResolvers">Hotkeys的列表</param>
/// <param name="handleAddingQT">是否由MacroManager来帮你把QT和Hotkey加到QT窗口里</param>
public class MacroManager(JobViewWindow instance,
                          string cmdHandle,
                          List<QtInfo> qtKeys,
                          List<HotKeyInfo> hkResolvers,
                          bool handleAddingQT = false) {
  // 用Toast2提示QT状态会用到的东西
  private readonly List<string> _qtToastBuffer = [];
  private static bool _qtToastScheduled;

  private record CommandInfo(string CmdType, string CNCmd, string EnCmd);

  // 这三条会自动生成
  private readonly List<CommandInfo> _cmdList = [];
  private readonly Dictionary<string, string> _qtKeyDict = new(StringComparer.OrdinalIgnoreCase);

  private readonly Dictionary<string, IHotkeyResolver> _hotkeyDict =
      new(StringComparer.OrdinalIgnoreCase);

  // 是否用Toast2提示QT状态
  public bool UseToast2 = false;

  /// <summary>
  /// 在OnEnterRotation()里面需要用到的初始化方法
  /// </summary>
  public void Init() {
    RegisterHandle();
    BuildCommandList();
  }

  /// <summary>
  /// 在OnExitRotation()里面需要用到的方法
  /// </summary>
  public void Exit() => ECHelper.Commands.RemoveHandler(cmdHandle);

//  /// <summary>
//  /// 单个QT的添加
//  /// </summary>
//  /// <param name="name"></param>
//  /// <param name="en"></param>
//  /// <param name="defVal"></param>
//  /// <param name="tooltip"></param>
//  // ReSharper disable once MemberCanBePrivate.Global
//  public void AddQt(string name, string en, bool defVal, string tooltip) {
//    if (handleAddingQT) instance.AddQt(name, defVal, tooltip);
//
//    _qtKeyDict.TryAdd(name, name);
//    string cncmd = cmdHandle + " " + name + "_qt";
//    string encmd = "";
//
//    if (!en.IsNullOrEmpty()) {
//      _qtKeyDict.TryAdd(en.ToLower(), name);
//      encmd = cmdHandle + " " + en.ToLower() + "_qt";
//    }
//
//    _cmdList.Add(new CommandInfo("QT", cncmd, encmd));
//  }

  /// <summary>
  /// 单个QT的添加，支持Action callback
  /// </summary>
  /// <param name="item">包含QT信息的QtInfo类型</param>
  // ReSharper disable once MemberCanBePrivate.Global
  public void AddQt(QtInfo item) {
    if (handleAddingQT) {
      if (item.Callback is not null) {
        instance.AddQt(item.Name, item.DefVal, item.Callback);
      } else {
        instance.AddQt(item.Name, item.DefVal);
      }
      instance.SetQtToolTip(item.Tooltip);
    }

    _qtKeyDict.TryAdd(item.Name, item.Name);
    string cncmd = cmdHandle + " " + item.Name + "_qt";
    string encmd = "";

    if (!item.EnName.IsNullOrEmpty()) {
      _qtKeyDict.TryAdd(item.EnName.ToLower(), item.Name);
      encmd = cmdHandle + " " + item.EnName.ToLower() + "_qt";
    }

    _cmdList.Add(new CommandInfo("QT", cncmd, encmd));
  }

  /// <summary>
  /// 单个Hotkey的添加
  /// </summary>
  /// <param name="item">包含Hotkey信息的HotkeyInfo类型</param>
  // ReSharper disable once MemberCanBePrivate.Global
  public void AddHotkey(HotKeyInfo item) {
    if (handleAddingQT) instance.AddHotkey(item.Name, item.Hkr);

    _hotkeyDict.TryAdd(item.Name, item.Hkr);
    string cncmd = cmdHandle + " " + item.Name + "_hk";
    string encmd = "";

    if (!item.EnName.IsNullOrEmpty()) {
      _hotkeyDict.TryAdd(item.EnName.ToLower(), item.Hkr);
      encmd = cmdHandle + " " + item.EnName.ToLower() + "_hk";
    }

    _cmdList.Add(new CommandInfo("Hotkey", cncmd, encmd));
  }

  private void RegisterHandle() {
    try {
      ECHelper.Commands.RemoveHandler(cmdHandle);
    } catch (Exception) {
      // ignored
    }

    ECHelper.Commands.AddHandler(cmdHandle, 
                                 new Dalamud.Game.Command.CommandInfo(CommandHandler));
  }

  private void CommandHandler(string command, string args) {
    if (string.IsNullOrWhiteSpace(args)) {
      LogHelper.PrintError(cmdHandle[1..] + " 命令无效，请提供参数");
      return;
    }

    string processed = args.Trim().ToLower();

    if (processed.EndsWith("_qt")) {
      if (_qtKeyDict.ContainsKey(processed[..^3])) {
        ToggleQtSetting(_qtKeyDict.GetValueOrDefault(processed[..^3]));
      } else {
        LogHelper.PrintError("未知QT参数：" + args);
      }

      return;
    }

    if (processed.EndsWith("_hk")) {
      if (_hotkeyDict.ContainsKey(processed[..^3])) {
        ExecuteHotkey(_hotkeyDict.GetValueOrDefault(processed[..^3]));
      } else {
        LogHelper.PrintError("未知Hotkey参数：" + args);
      }

      return;
    }

    if (processed == "hello") {
      LogHelper.Print("Hello World!");
    } else {
      LogHelper.PrintError("未知参数：" + args);
    }
  }

  private static void ExecuteHotkey(IHotkeyResolver? hkr) {
    if (hkr is null) {
      LogHelper.PrintError("HotkeyResolver未初始化");
    } else if (hkr.Check() >= 0) {
      hkr.Run();
    } else {
      LogHelper.Print("无法执行Hotkey，可能条件不满足或技能不可用。");
    }
  }

  private void ToggleQtSetting(string? qtName) {
    if (!string.IsNullOrEmpty(qtName)) {
      if (instance.ReverseQt(qtName)) {
        string successNote = $"QT\"{qtName}\"已设置为 {instance.GetQt(qtName)}。";
        LogHelper.Print(successNote);

        if (!UseToast2) return;
        _qtToastBuffer.Add(successNote);

        if (_qtToastScheduled) return;
        _qtToastScheduled = true;
        Task.Delay(50).ContinueWith(delegate {
          string msg = string.Join("\n", _qtToastBuffer);
          Helper.SendTips(msg, 1, 1000);
          _qtToastBuffer.Clear();
          _qtToastScheduled = false;
        });
      } else {
        LogHelper.PrintError("Failed to Toggle QT");
      }
    } else {
      LogHelper.PrintError("Empty QT name");
    }
  }

  public void BuildCommandList() {
    if (_cmdList.Count > 0) return; // protect against multiple calls

    foreach (QtInfo item in qtKeys) {
      AddQt(item);
    }

    foreach (HotKeyInfo item in hkResolvers) {
      AddHotkey(item);
    }
  }

  /// <summary>
  /// 描绘一个宏命令的展示窗口，一般会在QT窗口的UpdateAction里面调用
  /// </summary>
  /// <param name="windowOpenSettings">ACR的Settings里面需要建立一个控制bool
  /// 来控制这个窗口是否打开</param>
  public void DrawCommandWindow(ref bool windowOpenSettings) {
    if (!windowOpenSettings) return;

    ImGuiViewportPtr mainViewport = ImGui.GetMainViewport();
    ImGui.SetNextWindowSize(new Vector2(mainViewport.Size.X / 2f, mainViewport.Size.Y / 1.5f),
                            ImGuiCond.Always);
    ImGui.Begin(cmdHandle[1..] + " 宏命令帮助", ref windowOpenSettings);
    ImGui.TextWrapped("通过 " + cmdHandle + " 使用快捷指令。"
                    + "结合游戏内宏使用可以方便手柄用户的操作。");
    ImGui.Separator();
    ImGui.Columns(3, "CommandColumns");
    ImGui.SetColumnWidth(0, mainViewport.Size.X / 10f);
    ImGui.SetColumnWidth(1, mainViewport.Size.X / 5f);
    ImGui.SetColumnWidth(2, mainViewport.Size.X / 5f);
    ImGui.Text("指令类型");
    ImGui.NextColumn();
    ImGui.Text("中文指令");
    ImGui.NextColumn();
    ImGui.Text("英文指令");
    ImGui.NextColumn();
    ImGui.Separator();

    foreach ((string cmdType, string cnCmd, string enCmd) in _cmdList) {
      ImGui.Text(cmdType);
      ImGui.NextColumn();
      if (ImGui.Button("复制##" + cnCmd)) ImGui.SetClipboardText(cnCmd);
      ImGui.SameLine();
      ImGui.Text(cnCmd);
      ImGui.NextColumn();
      if (ImGui.Button("复制##" + enCmd)) ImGui.SetClipboardText(enCmd);
      ImGui.SameLine();
      ImGui.Text(enCmd);
      ImGui.NextColumn();
    }

    ImGui.Columns();
    ImGui.Separator();
    if (ImGui.Button("关闭")) windowOpenSettings = false;
    ImGui.End();
  }
}
