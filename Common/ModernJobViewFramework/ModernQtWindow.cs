using System.Numerics;
using AEAssist.Helper;
using ImGuiNET;

namespace ElliotZ.Common.ModernJobViewFramework;

/// <summary>
/// 现代化Qt窗口组件
/// </summary>
public static class ModernQtWindow
{
    private static ModernTheme? _theme;
    private static readonly Dictionary<string, float> ButtonAnimations = new();
    private static readonly Dictionary<string, long> ButtonAnimationTimes = new();

    /// <summary>
    /// 确保theme不为null，如果为null则使用默认主题
    /// </summary>
    private static void EnsureThemeInitialized()
    {
        if (_theme != null) return;
        LogHelper.Debug("ModernQtWindow: theme为null，使用默认主题");
        _theme = new ModernTheme();
    }

    /// <summary>
    /// 绘制现代化Qt按钮
    /// </summary>
    private static void DrawModernQtButton(string label, QtWindow.QtControl qt, Vector2 size, Vector4? customColor = null)
    {
        EnsureThemeInitialized();

        // label = label switch
        // {
        //     "GCD单体治疗" => "GCD单奶",
        //     "GCD群体治疗" => "GCD群奶",
        //     "能力技治疗" => "能力技奶",
        //     _ => label
        // };

        var buttonId = label + ImGui.GetID(label);

        // 初始化动画状态
        if (!ButtonAnimations.ContainsKey(buttonId))
        {
            ButtonAnimations[buttonId] = qt.QtValue ? 1f : 0f;
            ButtonAnimationTimes[buttonId] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        // 更新动画

        UpdateButtonAnimation(buttonId, qt.QtValue);

        var animProgress = ButtonAnimations[buttonId];
        var baseColor = customColor ?? _theme.Colors.Primary;
        var currentColor = ModernTheme.BlendColor(
            _theme.Colors.Surface,
            baseColor,
            animProgress
        );

        // 按钮样式
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, size.Y * 0.5f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0);
        ImGui.PushStyleColor(ImGuiCol.Button, currentColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, LightenColor(currentColor, 0.1f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, LightenColor(currentColor, 0.2f));

        var pos = ImGui.GetCursorScreenPos();

        // 绘制背景阴影
        if (qt.QtValue)
        {
            DrawButtonShadow(pos, size, animProgress);
        }

        // 按钮
        if (ImGui.Button(label, size))
        {
            qt.QtValue = !qt.QtValue;
            qt.OnClick(qt.QtValue);
        }

        // 绘制状态指示器
        //DrawStateIndicator(pos, size, value, animProgress);

        // 绘制发光边框
        if (qt.QtValue)
        {
            DrawGlowBorder(pos, size, baseColor, animProgress);
        }

        ImGui.PopStyleColor(3);
        ImGui.PopStyleVar(2);
    }

    /// <summary>
    /// 绘制现代化Qt窗口
    /// </summary>
    public static void DrawModernQtWindow(QtWindow qtWindow, QtStyle style, JobViewSave save)
    {
        if (!save.ShowQt)
            return;

        // 确保theme不为null
        EnsureThemeInitialized();

        // 动态获取主题，与主界面联动
        if (style.CurrentThemeChanged)
        {
            _theme = new ModernTheme(style.CurrentTheme);
            if (style.CurrentThemeChanged)
            {
                style.UpdateLastTheme();
            }
        }

        if (!IsThemeMatching(style.CurrentTheme))
        {
            _theme = new ModernTheme(style.CurrentTheme);
        }

        var qtNameList = qtWindow.QtNameList;
        var qtUnVisibleList = qtWindow.QtUnVisibleList;
        var visibleCount = qtNameList.Count - qtUnVisibleList.Count;

        if (visibleCount <= 0)
            return;

        // 计算窗口大小
        var qtLineCount = qtWindow.QtLineCount;
        var line = Math.Ceiling((float)visibleCount / qtLineCount);
        var row = visibleCount < qtLineCount ? visibleCount : qtLineCount;

        var buttonSize = style.QtButtonSize;
        var spacing = new Vector2(10, 10);
        var padding = new Vector2(15, 15);

        var windowWidth = padding.X * 2 + buttonSize.X * row + spacing.X * (row - 1);
        var windowHeight = padding.Y * 2 + buttonSize.Y * line + spacing.Y * (line - 1);

        // 设置窗口样式
        ImGui.SetNextWindowSize(new Vector2(windowWidth, (float)windowHeight));

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 12f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, padding);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, spacing);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, _theme.Colors.Background with { W = style.QtWindowBgAlpha });
        ImGui.PushStyleColor(ImGuiCol.Border, _theme.Colors.Border);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1f);

        var flag = qtWindow.LockWindow ? QtStyle.QtWindowFlag | ImGuiWindowFlags.NoMove : QtStyle.QtWindowFlag;
        ImGui.Begin($"###Qt_Window{qtWindow.Name}", flag);


        // 绘制窗口背景效果
        DrawWindowBackground();

        // 绘制Qt按钮
        var index = 0;
        foreach (var qtName in qtNameList)
        {
            if (qtUnVisibleList.Contains(qtName))
                continue;

            var qtCtrl = qtWindow.GetQt(qtName);
            var customColor = GetQtCustomColor(qtWindow, qtName);

            DrawModernQtButton(qtName, qtCtrl, buttonSize, customColor);
            // qtWindow.SetQt(qtName, qtCtrl.QtValue);
            
            // 显示工具提示
            if (ImGui.IsItemHovered())
            {
                DrawModernTooltip(qtName, GetQtTooltip(qtWindow, qtName));
            }

            if (index % qtLineCount != qtLineCount - 1)
                ImGui.SameLine();

            index++;
        }

        ImGui.End();

        ImGui.PopStyleVar(4);
        ImGui.PopStyleColor(2);
    }

    /// <summary>
    /// 更新按钮动画
    /// </summary>
    private static void UpdateButtonAnimation(string buttonId, bool targetState)
    {
        var currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var lastTime = ButtonAnimationTimes[buttonId];
        var deltaTime = (currentTime - lastTime) / 1000f;

        ButtonAnimationTimes[buttonId] = currentTime;

        var current = ButtonAnimations[buttonId];
        var target = targetState ? 1f : 0f;
        const float animSpeed = 5f; // 动画速度

        if (Math.Abs(current - target) > 0.01f)
        {
            ButtonAnimations[buttonId] = current + (target - current) * Math.Min(1f, deltaTime * animSpeed);
        }
        else
        {
            ButtonAnimations[buttonId] = target;
        }
    }

    /// <summary>
    /// 绘制按钮阴影
    /// </summary>
    private static void DrawButtonShadow(Vector2 pos, Vector2 size, float intensity)
    {
        var drawList = ImGui.GetWindowDrawList();
        var shadowColor = new Vector4(0, 0, 0, 0.3f * intensity);
        var shadowOffset = new Vector2(0, 2);
        const float shadowBlur = 4f;

        for (var i = 0; i < 3; i++)
        {
            var alpha = shadowColor.W * (1f - i / 3f);
            var offset = shadowOffset + new Vector2(0, i * shadowBlur);
            var blur = i * shadowBlur;

            drawList.AddRectFilled(
                pos + offset - new Vector2(blur),
                pos + size + offset + new Vector2(blur),
                ImGui.GetColorU32(shadowColor with { W = alpha }),
                size.Y * 0.5f + blur
            );
        }
    }

    /// <summary>
    /// 绘制发光边框
    /// </summary>
    private static void DrawGlowBorder(Vector2 pos, Vector2 size, Vector4 color, float intensity)
    {
        var drawList = ImGui.GetWindowDrawList();
        var time = DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0f;
        var pulse = (float)(Math.Sin(time * 2) * 0.2 + 0.8);

        for (var i = 0; i < 2; i++)
        {
            var alpha = intensity * pulse * (0.3f - i * 0.1f);
            var offset = i * 2f;

            drawList.AddRect(
                pos - new Vector2(offset),
                pos + size + new Vector2(offset),
                ImGui.GetColorU32(color with { W = alpha }),
                size.Y * 0.5f,
                ImDrawFlags.None,
                2f
            );
        }
    }

    /// <summary>
    /// 绘制窗口背景效果
    /// </summary>
    private static void DrawWindowBackground()
    {
        EnsureThemeInitialized();

        var drawList = ImGui.GetWindowDrawList();
        var windowPos = ImGui.GetWindowPos();
        var windowSize = ImGui.GetWindowSize();

        // 绘制微妙的渐变背景
        ModernTheme.DrawGradient(
            windowPos,
            windowSize,
            _theme.Colors.Background with { W = 0f },
            _theme.Colors.Background with { W = 0.05f }
        );

        // 绘制装饰性图案
        var patternColor = _theme.Colors.Primary with { W = 0.02f };
        for (int i = 0; i < 3; i++)
        {
            var offset = i * 50f;
            drawList.AddCircle(
                windowPos + new Vector2(windowSize.X - 30 - offset, 30 + offset),
                20f + i * 10f,
                ImGui.GetColorU32(patternColor),
                32,
                1f
            );
        }
    }

    /// <summary>
    /// 绘制现代化工具提示
    /// </summary>
    private static void DrawModernTooltip(string title, string content)
    {
        if (string.IsNullOrEmpty(content))
            return;

        EnsureThemeInitialized();

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 8f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(12, 8));
        ImGui.PushStyleColor(ImGuiCol.PopupBg, _theme.Colors.Surface);
        ImGui.PushStyleColor(ImGuiCol.Border, _theme.Colors.Border);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 1f);

        ImGui.BeginTooltip();

        ImGui.TextColored(_theme.Colors.Primary, title);
        ImGui.TextColored(_theme.Colors.TextSecondary, content);

        ImGui.EndTooltip();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(2);
    }

    /// <summary>
    /// 获取Qt按钮的自定义颜色
    /// </summary>
    private static Vector4? GetQtCustomColor(QtWindow window, string qtName)
    {
        var qt = window.GetQt(qtName);
        return qt.UseColor ? qt.Color : null;
    }

    /// <summary>
    /// 获取Qt按钮的工具提示
    /// </summary>
    private static string GetQtTooltip(QtWindow window, string qtName)
    {
        return window.GetQt(qtName).ToolTip;
    }

    /// <summary>
    /// 颜色变亮
    /// </summary>
    private static Vector4 LightenColor(Vector4 color, float amount)
    {
        return new Vector4(
            Math.Min(1f, color.X + amount),
            Math.Min(1f, color.Y + amount),
            Math.Min(1f, color.Z + amount),
            color.W
        );
    }

    /// <summary>
    /// 检查主题是否匹配
    /// </summary>
    private static bool IsThemeMatching(ModernTheme.ThemePreset targetTheme)
    {
        EnsureThemeInitialized();
        // 创建一个临时主题来比较颜色
        var tempTheme = new ModernTheme(targetTheme);
        return _theme.Colors.Primary.Equals(tempTheme.Colors.Primary);
    }
}