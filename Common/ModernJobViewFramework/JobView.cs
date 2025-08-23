using System.Diagnostics;
using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.View;
using AEAssist.Helper;
using Dalamud.Interface.Utility.Raii;
using ElliotZ.Common.ModernJobViewFramework.HotKey;
using ImGuiNET;
using System.Numerics;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnusedMember.Global
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

namespace ElliotZ.Common.ModernJobViewFramework;

public class JobViewWindow : IRotationUI
{
    private Action _saveSetting;
    private QtWindow _qtWindow;
    private HotkeyWindow _hotkeyWindow;
    private MainWindow _mainWindow;
    private QtStyle _style;
    //private float userFontGlobalScale = 1.17f;
    // 运行状态动画相关
    //private float statusAnimationTime = 0f;
    
    public Dictionary<string, Action<JobViewWindow>> ExternalTab = new();

    public Action? UpdateAction;

    /// <summary>
    /// 在当前职业循环插件中创建一个gui视图
    /// </summary>
    public JobViewWindow(JobViewSave jobViewSave, 
        Action save, 
        string name
        //ref Dictionary<string, HotKeySpell> Config,
        //Dictionary<string, uint> Spell
        )
    {
        _style = new QtStyle(jobViewSave);
        _saveSetting = save;
        _qtWindow = new QtWindow(jobViewSave, name);
        _hotkeyWindow = new HotkeyWindow(jobViewSave, name + " hotkey");
        _mainWindow = new MainWindow(ref _style);
    }

    //public void CreateHotKey()
    //{
    //    hotkeyWindow.CreateHotkey();
    //}

    /// <summary>
    /// 初始化主窗口风格
    /// </summary>
    public void SetMainStyle()
    {
        _style.SetMainStyle();
    }

    /// <summary>
    /// 注销主窗口风格
    /// </summary>
    public void EndMainStyle()
    {
        _style.EndMainStyle();
    }

    /// <summary>
    /// 增加一栏说明
    /// </summary>
    /// <param name="tabName"></param>
    /// <param name="draw"></param>
    public void AddTab(string tabName, Action<JobViewWindow> draw)
    {
        ExternalTab.Add(tabName, draw);
    }

    /// <summary>
    /// 设置UI上的Update处理
    /// </summary>
    /// <param name="updateAction"></param>
    public void SetUpdateAction(Action updateAction)
    {
        UpdateAction = updateAction;
    }

    /// <summary>
    /// 添加新的qt控件
    /// </summary>
    /// <param name="name">qt的名称</param>
    /// <param name="qtValueDefault">qt的bool默认值</param>
    public void AddQt(string name, bool qtValueDefault)
    {
        _qtWindow.AddQt(name, qtValueDefault);
    }


    /// <summary>
    /// 添加新的qt控件，并且自定义方法
    /// </summary>
    /// <param name="name">qt的名称</param>
    /// <param name="qtValueDefault">qt的bool默认值</param>
    /// <param name="action">按下时触发的方法</param>
    public void AddQt(string name, bool qtValueDefault, Action<bool> action)
    {
        _qtWindow.AddQt(name, qtValueDefault, action);
    }

    public void AddQt(string name, bool qtValueDefault, string toolTip)
    {
        _qtWindow.AddQt(name, qtValueDefault, toolTip);
    }

    public void AddQt(string name, bool qtValueDefault, Action<bool> action, Vector4 color)
    {
        _qtWindow.AddQt(name, qtValueDefault, action, color);
    }

    public void RemoveAllQt()
    {
        _qtWindow.RemoveAllQt();
    }

    // 设置每行按钮个数
    public void SetLineCount(int count)
    {
        if (count < 1)
            count = 1;
        _qtWindow.QtLineCount = count;
    }


    /// 设置上一次add添加的hotkey的toolTip
    public void SetQtToolTip(string toolTip)
    {
        _qtWindow.SetQtToolTip(toolTip);
    }

    /// 画一个新的Qt窗口
    public void DrawQtWindow()
    {
        try
        {
            ModernQtWindow.DrawModernQtWindow(_qtWindow, _style, _qtWindow.Save);
        }
        catch (Exception e)
        {
            LogHelper.Error($"err: -- {e}");
        }
        
    }

    /// 创建一个更改qt排序显示等设置的视图
    public void QtSettingView()
    {
        _qtWindow.QtSettingView();
    }

    /// 获取指定名称qt的bool值
    public bool GetQt(string qtName)
    {
        return _qtWindow.GetQt(qtName).QtValue;
    }

    /// 设置指定qt的值
    public void SetQt(string qtName, bool qtValue)
    {
        _qtWindow.SetQt(qtName, qtValue);
    }

    /// 反转指定qt的值
    /// <returns>成功返回true，否则返回false</returns>
    public bool ReverseQt(string qtName)
    {
        return _qtWindow.ReverseQt(qtName);
    }

    /// 重置所有qt为默认值
    public void Reset()
    {
        _qtWindow.Reset();
    }

    /// 给指定qt设置新的默认值
    public void NewDefault(string qtName, bool newDefault)
    {
        _qtWindow.NewDefault(qtName, newDefault);
    }

    /// 将当前所有Qt状态记录为新的默认值
    public void SetDefaultFromNow()
    {
        _qtWindow.SetDefaultFromNow();
    }

    /// 返回包含当前所有qt名字的数组 不要在update里调用
    public string[] GetQtArray()
    {
        return _qtWindow.GetQtArray();
    }

    /// 画一个新的hotkey窗口
    public void DrawHotkeyWindow()
    {
        // 使用现代化Hotkey窗口
        _hotkeyWindow.DrawHotkeyWindow(_style);
    }

    /// <summary>
    /// 添加新的qt控件
    /// </summary>
    public void AddHotkey(string name, AEAssist.CombatRoutine.View.JobView.IHotkeyResolver slot)
    {
        _hotkeyWindow.AddHotkey(name, slot);
    }

    /// <summary>
    /// 获取当前激活的hotkey列表
    /// </summary>
    /// <returns></returns>
    public List<string> GetActiveList() => _hotkeyWindow.ActiveList;

    /// 设置上一次add添加的hotkey的toolTip
    public void SetHotkeyToolTip(string toolTip)
    {
        _hotkeyWindow.SetHotkeyToolTip(toolTip);
    }

    /// 激活单个快捷键,mo无效
    public void SetHotkey(string name)
    {
        _hotkeyWindow.SetHotkey(name);
    }

    /// 取消激活单个快捷键
    public void CancelHotkey(string name)
    {
        GetActiveList().Remove(name);
    }

    /// 返回包含当前所有hotkey名字的数组
    public string[] GetHotkeyArray()
    {
        return _hotkeyWindow.GetHotkeyArray();
    }

    /// 用于draw一个更改hotkey排序显示等设置的视图
    public void HotkeySettingView()
    {
        _hotkeyWindow.HotkeySettingView();
    }

    /// <summary>
    /// 运行键盘快捷键模块,一般放在update中
    /// </summary>
    public void RunHotkey()
    {
        _hotkeyWindow.RunHotkey();
        _qtWindow.RunHotkey();
    }

    public void Update()
    {
        RunHotkey();
    }


    /// <summary>
    /// 用于开关自动输出的控件组合
    /// </summary>
    public void MainControlView(ref bool buttonValue, ref bool stopButton)
    {
        _mainWindow.MainControlView(ref buttonValue, ref stopButton, _saveSetting);
    }

    ///风格设置控件
    public void ChangeStyleView()
    {
        // 现代主题选择
        ImGui.Text("选择主题预设:");
        ImGui.Separator();
        
        var themes = Enum.GetValues<ModernTheme.ThemePreset>();
        foreach (var theme in themes)
        {
            var isSelected = _style.CurrentTheme == theme;
            if (isSelected)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, _style.ModernTheme.Colors.Primary);
            }
            
            if (ImGui.Button(theme.ToString(), new Vector2(120, 30)))
            {
                _style.CurrentTheme = theme;
                _saveSetting();
            }
            
            if (isSelected)
            {
                ImGui.PopStyleColor();
            }
            
            if (((int)theme+1) % 2 != 0)
                ImGui.SameLine();
        }
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        Debug.Assert(GlobalSetting.Instance != null);
        if (ImGui.Checkbox("QT和快捷栏随主界面隐藏", ref GlobalSetting.Instance.Qt快捷栏随主界面隐藏))
        {
            GlobalSetting.Instance.Save();
        }

        if (ImGui.Checkbox("关闭UI动态效果", ref GlobalSetting.Instance.关闭动效))
        {
            GlobalSetting.Instance.Save();
        }

        if (ImGui.Button("显示/隐藏QT"))
        {
            GlobalSetting.Instance.QtShow = !GlobalSetting.Instance.QtShow;
            GlobalSetting.Instance.Save();
        }

        ImGui.SameLine();
        if (ImGui.Button("显示/隐藏快捷栏"))
        {
            GlobalSetting.Instance.HotKeyShow = !GlobalSetting.Instance.HotKeyShow;
            GlobalSetting.Instance.Save();
        }


        ImGui.Dummy(new Vector2(1, 3));

        //QT按钮一行个数
        var input = _qtWindow.QtLineCount;
        if (ImGui.InputInt("Qt按钮每行个数", ref input))
        {
            _qtWindow.QtLineCount = input < 1 ? 1 : input;
        }

        //hotkey按钮一行个数
        input = _hotkeyWindow.HotkeyLineCount;
        if (ImGui.InputInt("快捷键每行个数", ref input))
        {
            _hotkeyWindow.HotkeyLineCount = input < 1 ? 1 : input;
        }

        //QT透明度
        var qtBackGroundAlpha = _style.QtWindowBgAlpha;
        if (ImGui.SliderFloat("背景透明度", ref qtBackGroundAlpha, 0f, 1f, "%.1f"))
        {
            _style.QtWindowBgAlpha = qtBackGroundAlpha;
        }

        var smallWindowSize = GlobalSetting.Instance.缩放后窗口大小;
        if (ImGui.InputFloat2("缩放后窗口大小", ref smallWindowSize))
        {
            GlobalSetting.Instance.缩放后窗口大小 = smallWindowSize;
            GlobalSetting.Instance.Save();
        }

        // 按钮大小
        var buttonSize = _style.QtButtonSizeOrigin;
        if (ImGui.InputFloat2("按钮大小", ref buttonSize))
        {
            _style.QtButtonSizeOrigin = buttonSize;
        }

        // 热键大小
        // 按钮大小
        var hotKeySize = _style.HotkeySizeOrigin;
        if (ImGui.InputFloat2("热键大小", ref hotKeySize))
        {
            _style.HotkeySizeOrigin = hotKeySize;
        }


        ImGui.Dummy(new Vector2(1, 3));

        var lockWindow = _hotkeyWindow.LockWindow;
        if (ImGui.Checkbox("Hotkey窗口不可拖动", ref lockWindow))
        {
            _hotkeyWindow.LockWindow = lockWindow;
        }

        var lockQtWindow = _qtWindow.LockWindow;
        if (ImGui.Checkbox("Qt窗口不可拖动", ref lockQtWindow))
        {
            _qtWindow.LockWindow = lockQtWindow;
        }


        //重置按钮
        if (ImGui.Button("重置风格 ###重置"))
        {
            _style.Reset();
        }
    }

    public bool IsCustomMain()
    {
        return true;
    }

    public void OnDrawUI()
    {
        SetMainStyle();
        try
        {
            #region 加载UI

            var mainWindowCollapsed = false;
            
            var triggerlineName = "";
            if (AI.Instance.TriggerlineData.CurrTriggerLine != null)
            {
                triggerlineName =
                    $"| {AI.Instance.TriggerlineData.CurrTriggerLine.Author}-{AI.Instance.TriggerlineData.CurrTriggerLine.Name}";
            }

            var title =
                $"{GlobalSetting.Title} | {AI.Instance.BattleData.CurrBattleTimeInSec} {triggerlineName} ###aeassist";

            if (OverlayManager.Instance.Visible)
            {
                // 根据小窗口状态设置窗口大小约束
                if (_style.Save.SmallWindow)
                {
                    // 小窗口模式：锁定窗口大小
                    if (GlobalSetting.Instance != null)
                    {
                        var smallSize = GlobalSetting.Instance.缩放后窗口大小 * QtStyle.OverlayScale;
                        ImGui.SetNextWindowSizeConstraints(smallSize, smallSize);
                    }
                }
                else
                {
                    // 正常模式：允许调整大小
                    ImGui.SetNextWindowSizeConstraints(new Vector2(0, 0), new Vector2(float.MaxValue, float.MaxValue));
                }
                
                //标题栏风格 无滚动条 不会通过鼠标滚轮滚动内容
                if (ImGui.Begin(title, ref OverlayManager.Instance.Visible,
                        ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
                {
                    mainWindowCollapsed = false;
                    ImGui.SetWindowFontScale(SettingMgr.GetSetting<GeneralSettings>().OverlayScale);

                    // 绘制顶部运行状态栏
                    //DrawTopStatusBar(Share.CombatRun, PlayerOptions.Instance.Stop);
                    
                    MainControlView(ref Share.CombatRun, ref PlayerOptions.Instance.Stop);
                    UpdateAction?.Invoke();
                    //tab标签页
                    ImGui.Dummy(new Vector2(0, 5));
                    using (var bar = ImRaii.TabBar("###tab"))
                    {
                        if (bar.Success)
                        {
                            foreach (var v 
                                     in ExternalTab.Where(v 
                                         => v.Key != "Dev"))
                            {
                                using var item = ImRaii.TabItem(v.Key);
                                if (!item.Success) continue;
                                using var child = ImRaii.Child($"###tab{v.Key}");
                                if (child.Success)
                                    v.Value.Invoke(this);
                            }

                            using (var item = ImRaii.TabItem("Qt"))
                            {
                                if (item.Success)
                                {
                                    using var child = ImRaii.Child($"###Qt");
                                    if (child.Success)
                                        QtSettingView();
                                }
                            }

                            using (var item = ImRaii.TabItem("Hotkey"))
                            {
                                if (item.Success)
                                {
                                    using var child = ImRaii.Child($"###Hotkey");
                                    if (child.Success)
                                        HotkeySettingView();
                                }
                            }

                            using (var item = ImRaii.TabItem("风格"))
                            {
                                if (item.Success)
                                {
                                    using var child = ImRaii.Child($"###风格");
                                    if (child.Success)
                                        ChangeStyleView();
                                }
                            }

                            if (ExternalTab.ContainsKey("Dev"))
                            {
                                using var item = ImRaii.TabItem("Dev");
                                if (item.Success)
                                {
                                    using var child = ImRaii.Child($"###tabDev");
                                    if (child.Success)
                                        ExternalTab["Dev"].Invoke(this);
                                }
                            }
                        }
                    }
                    ImGui.End();
                }
                else
                {
                    mainWindowCollapsed = true;
                }
            }

            if (GlobalSetting.Instance is not null 
                && GlobalSetting.Instance.QtShow 
                && GlobalSetting.Instance.TempQtShow
                && (OverlayManager.Instance.Visible && !mainWindowCollapsed 
                    || !GlobalSetting.Instance.Qt快捷栏随主界面隐藏)
                )
            {
                DrawQtWindow();
            }

            if (GlobalSetting.Instance is not null 
                && GlobalSetting.Instance.HotKeyShow 
                && GlobalSetting.Instance.TempHotShow
                && (OverlayManager.Instance.Visible && !mainWindowCollapsed 
                    || !GlobalSetting.Instance.Qt快捷栏随主界面隐藏)
                )
            {
                DrawHotkeyWindow();
            }

            #endregion
        }
        catch (Exception e)
        {
            LogHelper.Error(e.Message);
        }
        finally
        {
            EndMainStyle();
        }
    }
}