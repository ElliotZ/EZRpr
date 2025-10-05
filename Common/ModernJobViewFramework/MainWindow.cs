using System.Diagnostics;
using System.Numerics;
using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.GUI;
using AEAssist.MemoryApi;
using Dalamud.Bindings.ImGui;

// ReSharper disable FieldCanBeMadeReadOnly.Local
#pragma warning disable CS9113 // 参数未读。

namespace ElliotZ.ModernJobViewFramework;

public class MainWindow {
  private bool _smallWindow;
  private QtStyle _style;
  private Vector2 _originalSize;
  private ModernTheme? _theme;
  private float _animationProgress;
  private bool _showSaveSuccess;
  private long _saveAnimationTime;

  /// 初始化主题
  public MainWindow(ref QtStyle style) {
    _style = style;
    // 根据当前设置初始化主题
    UpdateTheme();
    // 从保存的设置中恢复窗口状态
    RestoreWindowState();
  }

  /// <summary>
  /// 保存窗口状态到JobViewSave
  /// </summary>
  private void SaveWindowState() {
    // 保存小窗口状态
    _style.Save.SmallWindow = _smallWindow;
    // 保存原始窗口大小
    _style.Save.OriginalWindowSize = _originalSize;
  }

  /// <summary>
  /// 从JobViewSave恢复窗口状态
  /// </summary>
  private void RestoreWindowState() {
    // 恢复小窗口状态
    _smallWindow = _style.Save.SmallWindow;
    // 恢复原始窗口大小
    _originalSize = _style.Save.OriginalWindowSize;
  }

  /// <summary>
  /// 更新主题设置
  /// </summary>
  private void UpdateTheme() {
    // 从QtStyle获取当前主题
    _theme = new ModernTheme(_style.CurrentTheme);
    // 确保主题设置被保存
    if (_style.Save is not null) _style.Save.CurrentTheme = _style.CurrentTheme;
  }

  /// <summary>
  /// 获取当前主题
  /// </summary>
  private ModernTheme GetCurrentTheme() {
    // 确保主题是最新的
    if ((_theme == null) || (_theme.Colors.Primary.X == 0)) UpdateTheme();

    // 检查主题是否发生了变化
    if (_style.CurrentTheme
     != _theme.GetType()
              .GetField("CurrentPreset")?
              .GetValue(_theme) as ModernTheme.ThemePreset?) {
      UpdateTheme();
    }

    return _theme;
  }

  /// <summary>
  /// 强制刷新主题
  /// </summary>
  private void ForceRefreshTheme() {
    UpdateTheme();
  }

  /// <summary>
  /// 用于开关自动输出的控件组合
  /// </summary>
  /// <param name="buttonValue">主开关</param>
  /// <param name="stopButton">传入控制停手的变量</param>
  /// <param name="save">保存方法</param>
  public void MainControlView(ref bool buttonValue, ref bool stopButton, Action save) {
    // 创建主控制区域
    ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(15, 10));
    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(10, 8));

    // 绘制顶部控制栏
    DrawTopControlBar(ref buttonValue, ref stopButton, save);

    // 小窗口模式只显示基本控件
    if (_smallWindow) {
      ImGui.PopStyleVar(2);
      return;
    }

    // 绘制信息区域
    DrawInfoSection();
    ImGui.PopStyleVar(2);
  }

  /// <summary>
  /// 绘制顶部控制栏
  /// </summary>
  private void DrawTopControlBar(ref bool buttonValue, ref bool stopButton, Action save) {
    float windowWidth = ImGui.GetWindowWidth();

    // 主控制按钮
    DrawMainControlButton(ref buttonValue, ref stopButton);

    // 调整右侧控制按钮的垂直位置，与左侧主按钮中心对齐
    // 左侧主按钮高度是45，右侧按钮高度是35  <-- 都改成了随比例缩放的36
    // 左侧按钮中心位置：45/2 = 22.5，右侧按钮中心位置：35/2 = 17.5
    // 需要向下偏移：22.5 - 17.5 = 5像素，让中心对齐
    // 但为了更好的视觉效果，增加偏移量到8像素
    float currentY = ImGui.GetCursorPosY();
    ImGui.SetCursorPosY(currentY - 8);

    // 右侧控制按钮组
    ImGui.SameLine(windowWidth - 96 * QtStyle.OverlayScale);
    DrawControlButtons(save);

    // 分隔线
    if (!_smallWindow) {
      ImGui.Spacing();
      DrawSeparatorLine();
      ImGui.Spacing();
    }
  }

  /// <summary>
  /// 绘制主控制按钮
  /// </summary>
  private void DrawMainControlButton(ref bool buttonValue, ref bool stopButton) {
    // 更新动画时间
    _animationProgress += ImGui.GetIO().DeltaTime;

    string label = GetStatusLabel(buttonValue, stopButton);
    Vector2 buttonSize = new Vector2(100, 36) * QtStyle.OverlayScale;
    ImDrawListPtr drawList = ImGui.GetWindowDrawList();
    Vector2 buttonPos = ImGui.GetCursorScreenPos();

    // 绘制高级动画背景效果
    if (buttonValue && !stopButton) { // 运行状态 - 绿色主题动画
      DrawRunningButtonEffects(drawList, buttonPos, buttonSize);
    } else if (stopButton) { // 停手模式 - 警告主题动画
      DrawStopModeButtonEffects(drawList, buttonPos, buttonSize);
    } else { // 暂停状态 - 简单灰色效果
      DrawPausedButtonEffects(drawList, buttonPos, buttonSize);
    }

    // 设置按钮样式
    // var buttonColor = GetButtonColor(buttonValue, stopButton);
    ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 22f);
    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0)); // 透明背景，让自定义绘制显示
    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(1, 1, 1, 0.05f));
    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(1, 1, 1, 0.1f));
    ImGui.PushStyleColor(ImGuiCol.Text, GetTextColor(buttonValue, stopButton));
    ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);

    // 主按钮
    if (ImGui.Button(label, buttonSize)) {
      if (stopButton) {
        stopButton = false;
      } else {
        buttonValue = !buttonValue;
        if ((GlobalSetting.Instance != null) && !GlobalSetting.Instance.关闭动效) TriggerAnimation();
      }
    }

    // 右键直接切换停手模式
    if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
      if (buttonValue) {
        stopButton = !stopButton;
      }
    }

    ImGui.PopStyleVar(2);
    ImGui.PopStyleColor(4);

    // 显示提示
    if (!ImGui.IsItemHovered()) return;

    if (buttonValue) {
      ImGui.SetTooltip(stopButton ? "左键:恢复正常 | 右键:退出停手" : "左键:暂停 | 右键:切换停手");
    } else {
      ImGui.SetTooltip("左键:启动");
    }
  }

  /// <summary>
  /// 绘制运行状态的按钮特效
  /// </summary>
  private void DrawRunningButtonEffects(ImDrawListPtr drawList, Vector2 pos, Vector2 size) {
    // 检查是否关闭动效
    if ((GlobalSetting.Instance != null) && GlobalSetting.Instance.关闭动效) {
      // 关闭动效时只渲染基本的按钮框架和纯色
      // 1. 纯色背景
      drawList.AddRectFilled(pos,
                             pos + size,
                             ImGui.GetColorU32(new Vector4(0.15f, 0.6f, 0.2f, 0.9f)),
                             22f);

      // 2. 简单边框
      drawList.AddRect(pos,
                       pos + size,
                       ImGui.GetColorU32(new Vector4(0.3f, 1f, 0.4f, 0.8f)),
                       22f,
                       ImDrawFlags.None,
                       2f);

      // 3. 简单的暂停图标（静态）
      float centerX = pos.X + size.X / 2;
      float centerY = pos.Y + size.Y / 2;
      const float barWidth = 4f;
      const float barHeight = 20f;
      const float barSpacing = 8f;

      // 左竖线
      drawList.AddRectFilled(new Vector2(centerX - barSpacing - barWidth, centerY - barHeight / 2),
                             new Vector2(centerX - barSpacing, centerY + barHeight / 2),
                             ImGui.GetColorU32(new Vector4(0.6f, 0.6f, 0.7f, 0.6f)),
                             2f);

      // 右竖线
      drawList.AddRectFilled(new Vector2(centerX + barSpacing - barWidth, centerY - barHeight / 2),
                             new Vector2(centerX + barSpacing, centerY + barHeight / 2),
                             ImGui.GetColorU32(new Vector4(0.6f, 0.6f, 0.7f, 0.6f)),
                             2f);
    } else {
      // 原有的动画效果代码
      float time = _animationProgress;
      float centerX = pos.X + size.X / 2;
      float centerY = pos.Y + size.Y / 2;

      // 1. 动态渐变背景
      float gradientPhase = (float)(Math.Sin(time * 2) * 0.5 + 0.5);
      var color1 = new Vector4(0.1f, 0.4f, 0.15f, 0.9f);
      var color2 = new Vector4(0.15f, 0.6f, 0.2f, 0.9f);
      Vector4 currentColor = Vector4.Lerp(color1, color2, gradientPhase);
      drawList.AddRectFilled(pos, pos + size, ImGui.GetColorU32(currentColor), 22f);

      // 2. 能量波纹效果 - 沿边框扩散
      for (int i = 0; i < 2; i++) {
        float waveProgress = (time * 0.8f + i * 0.5f) % 1.0f;
        float waveExpansion = waveProgress * 8; // 扩散幅度从30改为8
        float waveAlpha = (1.0f - waveProgress) * 0.4f;

        // 绘制圆角矩形波纹，贴合按钮形状
        drawList.AddRect(pos - new Vector2(waveExpansion, waveExpansion),
                         pos + size + new Vector2(waveExpansion, waveExpansion),
                         ImGui.GetColorU32(new Vector4(0.3f, 1f, 0.4f, waveAlpha)),
                         22f + waveExpansion, // 圆角也随之扩大
                         ImDrawFlags.None,
                         1.5f);
      }

      // 3. 粒子效果
      const int particleCount = 5;

      for (int i = 0; i < particleCount; i++) {
        double angle = (time * 2 + i * (2 * Math.PI / particleCount)) % (2 * Math.PI);
        const float radius = 25f;
        float particleX = centerX + (float)(Math.Cos(angle) * radius);
        float particleY = centerY + (float)(Math.Sin(angle) * radius * 0.5f);
        float particleAlpha = (float)(Math.Sin(time * 4 + i) * 0.3 + 0.5);
        drawList.AddCircleFilled(new Vector2(particleX, particleY),
                                 3f,
                                 ImGui.GetColorU32(new Vector4(0.5f, 1f, 0.6f, particleAlpha)));
      }

      // 5. 边框呼吸效果
      float borderAlpha = (float)(Math.Sin(time * 3) * 0.3 + 0.5);
      drawList.AddRect(pos,
                       pos + size,
                       ImGui.GetColorU32(new Vector4(0.3f, 1f, 0.4f, borderAlpha)),
                       22f,
                       ImDrawFlags.None,
                       2f);

      // 6. 内部光晕
      float glowRadius = (float)(Math.Sin(time * 2.5) * 5 + 20);

      for (int i = 3; i > 0; i--) {
        float alpha = 0.15f / i;
        drawList.AddCircleFilled(new Vector2(centerX, centerY),
                                 glowRadius * i / 3,
                                 ImGui.GetColorU32(new Vector4(0.5f, 1f, 0.5f, alpha)));
      }
    }
  }

  /// <summary>
  /// 绘制停手模式的按钮特效
  /// </summary>
  private void DrawStopModeButtonEffects(ImDrawListPtr drawList, Vector2 pos, Vector2 size) {
    // 检查是否关闭动效
    if ((GlobalSetting.Instance != null) && GlobalSetting.Instance.关闭动效) {
      // 关闭动效时只渲染基本的按钮框架和纯色
      // 1. 纯色警告背景
      drawList.AddRectFilled(pos,
                             pos + size,
                             ImGui.GetColorU32(new Vector4(0.5f, 0.3f, 0.05f, 0.9f)),
                             22f);

      // 2. 简单边框
      drawList.AddRect(pos,
                       pos + size,
                       ImGui.GetColorU32(new Vector4(1f, 0.6f, 0f, 0.8f)),
                       22f,
                       ImDrawFlags.None,
                       2f);

      // 3. 静态警告图标
      float centerX = pos.X + size.X / 2;
      float centerY = pos.Y + size.Y / 2;
      const float iconSize = 15f;
      drawList.AddTriangleFilled(new Vector2(centerX, centerY - iconSize / 2),
                                 new Vector2(centerX - iconSize / 2, centerY + iconSize / 2),
                                 new Vector2(centerX + iconSize / 2, centerY + iconSize / 2),
                                 ImGui.GetColorU32(new Vector4(1f, 0.8f, 0f, 0.6f)));
    } else {
      // 原有的动画效果代码
      float time = _animationProgress;
      float centerX = pos.X + size.X / 2;
      float centerY = pos.Y + size.Y / 2;

      // 1. 警告背景
      float warningPhase = (float)(Math.Sin(time * 4) * 0.1 + 0.9);
      drawList.AddRectFilled(pos,
                             pos + size,
                             ImGui.GetColorU32(new Vector4(0.5f * warningPhase,
                                                           0.3f * warningPhase,
                                                           0.05f,
                                                           0.9f)),
                             22f);

      // 2. 警告条纹
      for (int i = 0; i < 8; i++) {
        float stripeX = pos.X + i * 20 - time * 20 % 20;

        if (stripeX < pos.X + size.X) {
          var points = new[] {
              new Vector2(stripeX, pos.Y),
              new Vector2(stripeX + 10, pos.Y),
              new Vector2(stripeX - 10, pos.Y + size.Y),
              new Vector2(stripeX - 20, pos.Y + size.Y),
          };

          // 裁剪到按钮范围内
          if ((stripeX > pos.X) && (stripeX - 20 < pos.X + size.X)) {
            drawList.AddQuadFilled(points[0],
                                   points[1],
                                   points[2],
                                   points[3],
                                   ImGui.GetColorU32(new Vector4(0, 0, 0, 0.2f)));
          }
        }
      }

      // 3. 闪烁边框
      float flashAlpha = (float)(Math.Sin(time * 8) * 0.5 + 0.5);
      drawList.AddRect(pos,
                       pos + size,
                       ImGui.GetColorU32(new Vector4(1f, 0.6f, 0f, flashAlpha)),
                       22f,
                       ImDrawFlags.None,
                       2f);

      // 4. 警告图标脉冲
      float iconPulse = (float)(Math.Sin(time * 5) * 0.1 + 1.0);
      float iconSize = 15f * iconPulse;
      drawList.AddTriangleFilled(new Vector2(centerX, centerY - iconSize / 2),
                                 new Vector2(centerX - iconSize / 2, centerY + iconSize / 2),
                                 new Vector2(centerX + iconSize / 2, centerY + iconSize / 2),
                                 ImGui.GetColorU32(new Vector4(1f, 0.8f, 0f, 0.3f)));
    }
  }

  /// <summary>
  /// 绘制暂停状态的按钮特效
  /// </summary>
  private void DrawPausedButtonEffects(ImDrawListPtr drawList, Vector2 pos, Vector2 size) {
    // 检查是否关闭动效
    Debug.Assert(GlobalSetting.Instance != null);

    if (GlobalSetting.Instance.关闭动效) {
      // 关闭动效时只渲染基本的按钮框架和纯色
      // 1. 纯色背景
      drawList.AddRectFilled(pos,
                             pos + size,
                             ImGui.GetColorU32(new Vector4(0.2f, 0.2f, 0.25f, 0.85f)),
                             22f);

      // 2. 简单边框
      drawList.AddRect(pos,
                       pos + size,
                       ImGui.GetColorU32(new Vector4(0.4f, 0.4f, 0.5f, 0.8f)),
                       22f,
                       ImDrawFlags.None,
                       1.5f);

      // 3. 静态暂停图标
      float centerX = pos.X + size.X / 2;
      float centerY = pos.Y + size.Y / 2;
      const float barWidth = 4f;
      const float barHeight = 20f;
      const float barSpacing = 8f;

      // 左竖线
      drawList.AddRectFilled(new Vector2(centerX - barSpacing - barWidth, centerY - barHeight / 2),
                             new Vector2(centerX - barSpacing, centerY + barHeight / 2),
                             ImGui.GetColorU32(new Vector4(0.6f, 0.6f, 0.7f, 0.6f)),
                             2f);

      // 右竖线
      drawList.AddRectFilled(new Vector2(centerX + barSpacing - barWidth, centerY - barHeight / 2),
                             new Vector2(centerX + barSpacing, centerY + barHeight / 2),
                             ImGui.GetColorU32(new Vector4(0.6f, 0.6f, 0.7f, 0.6f)),
                             2f);
    } else {
      // 原有的动画效果代码
      float time = _animationProgress;
      float centerX = pos.X + size.X / 2;
      float centerY = pos.Y + size.Y / 2;

      // 1. 动态渐变背景 - 缓慢的颜色变化
      float bgPhase = (float)(Math.Sin(time * 0.8) * 0.5 + 0.5);
      var color1 = new Vector4(0.15f, 0.15f, 0.2f, 0.85f);
      var color2 = new Vector4(0.2f, 0.2f, 0.25f, 0.85f);
      Vector4 currentBgColor = Vector4.Lerp(color1, color2, bgPhase);
      drawList.AddRectFilled(pos, pos + size, ImGui.GetColorU32(currentBgColor), 22f);

      // 2. 暂停图标动画 - 双竖线脉冲效果
      float iconPulse = (float)(Math.Sin(time * 2) * 0.15 + 1.0);
      float barWidth = 4f * iconPulse;
      float barHeight = 20f * iconPulse;
      const float barSpacing = 8f;

      // 左竖线
      drawList.AddRectFilled(new Vector2(centerX - barSpacing - barWidth, centerY - barHeight / 2),
                             new Vector2(centerX - barSpacing, centerY + barHeight / 2),
                             ImGui.GetColorU32(new Vector4(0.6f, 0.6f, 0.7f, 0.6f)),
                             2f);

      // 右竖线
      drawList.AddRectFilled(new Vector2(centerX + barSpacing - barWidth, centerY - barHeight / 2),
                             new Vector2(centerX + barSpacing, centerY + barHeight / 2),
                             ImGui.GetColorU32(new Vector4(0.6f, 0.6f, 0.7f, 0.6f)),
                             2f);

      // // 3. 环形等待动画 - 围绕按钮缓慢旋转
      // var ringRadius = Math.Min(size.X, size.Y) * 0.4f;
      // var ringSegments = 8;
      // for (int i = 0; i < ringSegments; i++)
      // {
      //     var angle = (time * 0.5f + i * (2 * Math.PI / ringSegments)) % (2 * Math.PI);
      //     var dotX = centerX + (float)(Math.Cos(angle) * ringRadius);
      //     var dotY = centerY + (float)(Math.Sin(angle) * ringRadius * 0.3f);
      //     var alpha = (float)(Math.Sin(time * 2 + i * 0.5f) * 0.2 + 0.3);
      //     
      //     drawList.AddCircleFilled(
      //         new Vector2(dotX, dotY),
      //         2f,
      //         ImGui.GetColorU32(new Vector4(0.5f, 0.5f, 0.6f, alpha))
      //     );
      // }

      // 4. 呼吸边框效果
      float borderBreath = (float)(Math.Sin(time * 1.5) * 0.3 + 0.5);
      drawList.AddRect(pos,
                       pos + size,
                       ImGui.GetColorU32(new Vector4(0.4f, 0.4f, 0.5f, borderBreath)),
                       22f,
                       ImDrawFlags.None,
                       1.5f);

      // // 5. 内部微光效果 - 缓慢的光晕
      // var glowPhase = (float)(Math.Sin(time * 1.2) * 0.5 + 0.5);
      // var glowRadius = 30f + glowPhase * 10f;
      // for (int i = 3; i > 0; i--)
      // {
      //     var alpha = 0.08f / i;
      //     drawList.AddCircleFilled(
      //         new Vector2(centerX, centerY),
      //         glowRadius * i / 3,
      //         ImGui.GetColorU32(new Vector4(0.4f, 0.4f, 0.5f, alpha))
      //     );
      // }

      // 6. 角落装饰动画
      const float cornerSize = 10f;
      float cornerAlpha = (float)(Math.Sin(time * 2.5) * 0.2 + 0.4);

      // 左上角
      drawList.AddLine(pos,
                       pos + new Vector2(cornerSize, 0),
                       ImGui.GetColorU32(new Vector4(0.5f, 0.5f, 0.6f, cornerAlpha)),
                       2f);
      drawList.AddLine(pos,
                       pos + new Vector2(0, cornerSize),
                       ImGui.GetColorU32(new Vector4(0.5f, 0.5f, 0.6f, cornerAlpha)),
                       2f);

      // 右下角
      drawList.AddLine(pos + size,
                       pos + size - new Vector2(cornerSize, 0),
                       ImGui.GetColorU32(new Vector4(0.5f, 0.5f, 0.6f, cornerAlpha)),
                       2f);
      drawList.AddLine(pos + size,
                       pos + size - new Vector2(0, cornerSize),
                       ImGui.GetColorU32(new Vector4(0.5f, 0.5f, 0.6f, cornerAlpha)),
                       2f);

      // // 7. 待机波纹效果
      // for (int i = 0; i < 2; i++)
      // {
      //     var waveProgress = ((time * 0.3f + i * 0.5f) % 1.0f);
      //     var waveExpansion = waveProgress * 15;
      //     var waveAlpha = (1.0f - waveProgress) * 0.15f;
      //     
      //     drawList.AddRect(
      //         pos - new Vector2(waveExpansion, waveExpansion),
      //         pos + size + new Vector2(waveExpansion, waveExpansion),
      //         ImGui.GetColorU32(new Vector4(0.4f, 0.4f, 0.5f, waveAlpha)),
      //         22f + waveExpansion,
      //         ImDrawFlags.None,
      //         1f
      //     );
      // }
    }
  }

  /// <summary>
  /// 绘制控制按钮组
  /// </summary>
  private void DrawControlButtons(Action save) {
    Debug.Assert(GlobalSetting.Instance != null);
    // 强制刷新主题，确保获取到最新设置
    ForceRefreshTheme();

    Vector2 buttonSize = new Vector2(36, 36) * QtStyle.OverlayScale;
    // 根据主题智能选择按钮颜色
    (Vector4 buttonColor, Vector4 hoverColor, Vector4 activeColor, Vector4 textColor) =
        GetControlButtonColors();

    ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 17f);
    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5, 0));
    ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);
    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, hoverColor);
    ImGui.PushStyleColor(ImGuiCol.ButtonActive, activeColor);
    ImGui.PushStyleColor(ImGuiCol.Text, textColor);

    // 保存按钮 
    Vector2 saveButtonPos = ImGui.GetCursorScreenPos();

    if (ImGui.Button("S", buttonSize)) {
      // 保存窗口状态
      SaveWindowState();
      save();
      _showSaveSuccess = true;
      _saveAnimationTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    // 为亮色主题添加微妙的阴影效果
    if (IsLightTheme()) DrawLightThemeButtonShadow(saveButtonPos, buttonSize);

    if (ImGui.IsItemHovered()) {
      ImGui.SetTooltip("保存设置");
    }

    ImGui.SameLine();
    // 缩放按钮
    string icon = _smallWindow ? "▼" : "▲";
    Vector2 scaleButtonPos = ImGui.GetCursorScreenPos();

    if (ImGui.Button(icon, buttonSize)) {
      if (!_smallWindow) _originalSize = ImGui.GetWindowSize();
      _smallWindow = !_smallWindow;

      if (!_smallWindow) {
        ImGui.SetWindowSize(_originalSize);
        GlobalSetting.Instance.TempQtShow = true;
        GlobalSetting.Instance.TempHotShow = true;
      } else {
        Vector2 smallSize = GlobalSetting.Instance.缩放后窗口大小;
        ImGui.SetWindowSize(smallSize * QtStyle.OverlayScale);

        if (GlobalSetting.Instance.缩放同时隐藏qt) {
          GlobalSetting.Instance.TempQtShow = false;
          GlobalSetting.Instance.Save();
        }

        if (GlobalSetting.Instance.缩放同时隐藏Hotkey) {
          GlobalSetting.Instance.TempHotShow = false;
          GlobalSetting.Instance.Save();
        }
      }

      // 保存窗口状态
      SaveWindowState();
      save();
    }

    // 为亮色主题添加微妙的阴影效果
    if (IsLightTheme()) DrawLightThemeButtonShadow(scaleButtonPos, buttonSize);

    if (ImGuiHelper.IsRightMouseClicked()) {
      ImGui.OpenPopup($"###iconPopup{icon}");
    }

    if (ImGui.BeginPopup($"###iconPopup{icon}")) {
      // 第一个选项：缩放同时隐藏QT
      bool hideQt = GlobalSetting.Instance.缩放同时隐藏qt;

      if (ImGui.Checkbox("缩放同时隐藏QT", ref hideQt)) {
        GlobalSetting.Instance.缩放同时隐藏qt = hideQt;
        GlobalSetting.Instance.Save();
      }

      // 第二个选项：缩放同时隐藏Hotkey
      bool hideHotkey = GlobalSetting.Instance.缩放同时隐藏Hotkey;

      if (ImGui.Checkbox("缩放同时隐藏Hotkey", ref hideHotkey)) {
        GlobalSetting.Instance.缩放同时隐藏Hotkey = hideHotkey;
        GlobalSetting.Instance.Save();
      }

      ImGui.EndPopup();
    }

    if (ImGui.IsItemHovered()) {
      ImGui.SetTooltip(_smallWindow ? "展开窗口(右键设置)" : "收起窗口(右键设置)");
    }

    ImGui.PopStyleColor(4);
    ImGui.PopStyleVar(2);
    // 显示保存成功动画
    if (_showSaveSuccess) DrawSaveSuccessAnimation();
  }

  /// <summary>
  /// 绘制信息区域
  /// </summary>
  private void DrawInfoSection() {
    // 标题和描述
    //ImGui.PushFont(ImGui.GetIO().Fonts.Fonts[0]);
    TriggerLine? currTriggerLine = AI.Instance.TriggerlineData.CurrTriggerLine;
    // var notice = "当前未加载时间轴.";
    // if (currTriggerLine != null)
    // {
    //     notice = $"当前时间轴 : [{currTriggerLine.Author}]{currTriggerLine.Name}";
    // }
    //
    // ImGui.TextColored(GetCurrentTheme().Colors.Text, notice);
    // //ImGui.PopFont();
    //
    // if (!string.IsNullOrEmpty(GlobalSetting.desc))
    // {
    //     ImGui.TextColored(GetCurrentTheme().Colors.Accent, GlobalSetting.desc);
    // }

    // ImGui.Spacing();
    // 提示信息卡片
    DrawTipCard(currTriggerLine);
  }

  /// <summary>
  /// 绘制提示卡片
  /// </summary>
  private void DrawTipCard(TriggerLine? currTriggerLine = null) {
    Vector4 tipBgColor = GetCurrentTheme().Colors.Primary with { W = 0.08f };
    Vector4 tipBorderColor = GetCurrentTheme().Colors.Primary with { W = 0.2f };

    ImGui.PushStyleColor(ImGuiCol.ChildBg, tipBgColor);
    ImGui.PushStyleColor(ImGuiCol.Border, tipBorderColor);
    ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 8f);
    ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1f);
    ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(12, 8));

    //var height = GlobalSetting.Instance.configHeight;
    ImGui.BeginChild("##TipsCard", new Vector2(-1, 112.5f * QtStyle.OverlayScale), true);

    string notice = "无";
    ImGui.TextColored(GetCurrentTheme().Colors.Primary, "当前时间轴:");
    if (currTriggerLine != null) notice = $"[{currTriggerLine.Author}]{currTriggerLine.Name}";

    ImGui.SameLine();
    ImGui.TextColored(GetCurrentTheme().Colors.Text, notice);
    ImGui.SameLine();

    if (currTriggerLine != null) {
      if (ImGui.Button("卸载时间轴")) {
        AI.Instance.TriggerlineData.Clear();
      }
    }

    float rightAlignWidth = currTriggerLine is null
                                ? 30f
                                : 30f + 23f * QtStyle.OverlayScale + ImGui.GetItemRectSize().X;
    ImGui.SameLine(ImGui.GetContentRegionAvail().X - rightAlignWidth);
    ImGui.SetNextItemWidth(80f);

    if (ImGui.BeginCombo("职能设置",
                         AI.Instance.PartyRole,
                         ImGuiComboFlags.HeightLargest)) {
      if (ImGui.Selectable("MT", AI.Instance.PartyRole == "MT")) {
        AI.Instance.PartyRole = "MT";
      }

      if (ImGui.Selectable("ST", AI.Instance.PartyRole == "ST")) {
        AI.Instance.PartyRole = "ST";
      }

      if (ImGui.Selectable("H1", AI.Instance.PartyRole == "H1")) {
        AI.Instance.PartyRole = "H1";
      }

      if (ImGui.Selectable("H2", AI.Instance.PartyRole == "H2")) {
        AI.Instance.PartyRole = "H2";
      }

      if (ImGui.Selectable("D1", AI.Instance.PartyRole == "D1")) {
        AI.Instance.PartyRole = "D1";
      }

      if (ImGui.Selectable("D2", AI.Instance.PartyRole == "D2")) {
        AI.Instance.PartyRole = "D2";
      }

      if (ImGui.Selectable("D3", AI.Instance.PartyRole == "D3")) {
        AI.Instance.PartyRole = "D3";
      }

      if (ImGui.Selectable("D4", AI.Instance.PartyRole == "D4")) {
        AI.Instance.PartyRole = "D4";
      }

      ImGui.EndCombo();
    }

    ImGui.Text($"当前地图ID: {Core.Resolve<MemApiZoneInfo>().GetCurrTerrId()} ");
    ImGui.Text($"当前天气ID: {Core.Resolve<MemApiZoneInfo>().GetWeatherId()} ");

    ImGui.EndChild();

    ImGui.PopStyleVar(3);
    ImGui.PopStyleColor(2);
  }

  /// <summary>
  /// 绘制分隔线
  /// </summary>
  private void DrawSeparatorLine() {
    ImDrawListPtr drawList = ImGui.GetWindowDrawList();
    Vector2 pos = ImGui.GetCursorScreenPos();
    float width = ImGui.GetWindowWidth() - 30;
    drawList.AddLine(pos,
                     pos + new Vector2(width, 0),
                     ImGui.GetColorU32(GetCurrentTheme().Colors.Border),
                     1f);
    ImGui.Dummy(new Vector2(0, 1));
  }

  /// <summary>
  /// 绘制保存成功动画
  /// </summary>
  private void DrawSaveSuccessAnimation() {
    long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    long elapsed = currentTime - _saveAnimationTime;

    if (elapsed > 2000) {
      _showSaveSuccess = false;
      return;
    }

    float progress = Math.Min(1f, elapsed / 2000f);
    float alpha = 1f - progress;
    float yOffset = progress * 20f;

    Vector2 pos = ImGui.GetCursorScreenPos();
    ImDrawListPtr drawList = ImGui.GetWindowDrawList();

    const string text = "✓ 设置已保存";
    Vector2 textSize = ImGui.CalcTextSize(text);
    var textPos = new Vector2(pos.X + (ImGui.GetWindowWidth() - textSize.X) / 2,
                              pos.Y - 30 - yOffset);

    // 背景
    Vector4 bgColor = GetCurrentTheme().Colors.Success with { W = alpha * 0.9f };
    drawList.AddRectFilled(textPos - new Vector2(10, 5),
                           textPos + textSize + new Vector2(10, 5),
                           ImGui.GetColorU32(bgColor),
                           8f);

    // 文字
    drawList.AddText(textPos, ImGui.GetColorU32(new Vector4(1, 1, 1, alpha)), text);
  }

  /// <summary>
  /// 触发动画
  /// </summary>
  private void TriggerAnimation() {
    _animationProgress = 0f;
  }

  /// <summary>
  /// 获取状态标签
  /// </summary>
  private static string GetStatusLabel(bool buttonValue, bool stopButton) {
    return stopButton ? "停手模式" : buttonValue ? "运行中" : "已暂停";
  }

  /// <summary>
  /// 获取文本颜色
  /// </summary>
  private Vector4 GetTextColor(bool buttonValue, bool stopButton) {
    if (stopButton || buttonValue) return new Vector4(1f, 1f, 1f, 1f);
    return GetCurrentTheme().Colors.Text;
  }

  /// <summary>
  /// 获取控制按钮的智能颜色方案
  /// </summary>
  private (Vector4 buttonColor, Vector4 hoverColor, Vector4 activeColor, Vector4 textColor)
      GetControlButtonColors() {
    ModernTheme currentTheme = GetCurrentTheme();

    // 检测是否为亮色主题
    bool isLightTheme = IsLightTheme();

    if (isLightTheme) {
      // 亮色主题 - 使用浅色背景和深色文字
      var buttonColor = new Vector4(0.95f, 0.95f, 0.95f, 0.9f); // 浅灰白色背景
      var hoverColor = new Vector4(0.9f, 0.9f, 0.9f, 0.95f); // 稍深的悬停色
      var activeColor = new Vector4(0.85f, 0.85f, 0.85f, 1f); // 更深的激活色
      var textColor = new Vector4(0.3f, 0.3f, 0.3f, 1f); // 深灰色文字

      // 为不同的亮色主题提供精确的颜色匹配
      if (currentTheme.Colors.Primary is { X: > 0.8f, Y: > 0.5f }) {
        // 樱花粉等暖色调主题
        Vector4 tint = currentTheme.Colors.Primary with { W = 0.15f };
        buttonColor = Vector4.Lerp(buttonColor, tint, 0.4f);
        hoverColor = Vector4.Lerp(hoverColor, tint, 0.5f);
        activeColor = Vector4.Lerp(activeColor, tint, 0.6f);

        // 调整文字颜色以保持对比度
        textColor = new Vector4(0.25f, 0.2f, 0.25f, 1f);
      } else if (currentTheme.Colors.Primary.Z > 0.7f) {
        // 蓝色系亮色主题
        Vector4 tint = currentTheme.Colors.Primary with { W = 0.1f };
        buttonColor = Vector4.Lerp(buttonColor, tint, 0.25f);
        hoverColor = Vector4.Lerp(hoverColor, tint, 0.35f);
        activeColor = Vector4.Lerp(activeColor, tint, 0.45f);
      } else if (currentTheme.Colors.Primary is { Y: > 0.6f, X: < 0.4f }) {
        // 绿色系亮色主题
        Vector4 tint = currentTheme.Colors.Primary with { W = 0.1f };
        buttonColor = Vector4.Lerp(buttonColor, tint, 0.2f);
        hoverColor = Vector4.Lerp(hoverColor, tint, 0.3f);
        activeColor = Vector4.Lerp(activeColor, tint, 0.4f);
      }

      return (buttonColor, hoverColor, activeColor, textColor);
    } else {
      // 深色主题 - 保持原有的深色风格
      Vector4 buttonColor = currentTheme.Colors.Surface;
      Vector4 hoverColor = currentTheme.Colors.Primary with { W = 0.2f };
      Vector4 activeColor = currentTheme.Colors.Primary with { W = 0.3f };
      Vector4 textColor = currentTheme.Colors.Text;

      return (buttonColor, hoverColor, activeColor, textColor);
    }
  }

  /// <summary>
  /// 检测是否为亮色主题
  /// </summary>
  private bool IsLightTheme() {
    ModernTheme currentTheme = GetCurrentTheme();

    // 直接检查主题预设类型
    ModernTheme.ThemePreset themeType = _style.CurrentTheme;

    if (themeType is ModernTheme.ThemePreset.浅色模式 or ModernTheme.ThemePreset.樱花粉) {
      return true;
    }

    // 通过背景色的亮度来判断是否为亮色主题
    float bgLuminance = GetLuminance(currentTheme.Colors.Background);
    float surfaceLuminance = GetLuminance(currentTheme.Colors.Surface);

    // 更精确的亮色主题检测
    // 1. 如果背景色很亮，直接认为是亮色主题
    if (bgLuminance > 0.7f) return true;

    // 2. 如果表面色很亮，也认为是亮色主题
    if (surfaceLuminance > 0.8f) return true;

    // 3. 如果背景和表面色都比较亮，认为是亮色主题
    if ((bgLuminance > 0.5f) && (surfaceLuminance > 0.6f)) return true;

    // 4. 特殊检测：樱花粉等暖色调主题
    float primaryLuminance = GetLuminance(currentTheme.Colors.Primary);
    return (primaryLuminance > 0.6f) && (currentTheme.Colors.Primary.X > 0.7f);
  }

  /// <summary>
  /// 计算颜色的亮度值
  /// </summary>
  private static float GetLuminance(Vector4 color) {
    // 使用标准的亮度计算公式
    return 0.299f * color.X + 0.587f * color.Y + 0.114f * color.Z;
  }

  /// <summary>
  /// 为亮色主题按钮绘制微妙的阴影效果
  /// </summary>
  private void DrawLightThemeButtonShadow(Vector2 pos, Vector2 size) {
    ImDrawListPtr drawList = ImGui.GetWindowDrawList();

    // 检测是否为樱花粉等暖色调主题
    bool isWarmTheme = (GetCurrentTheme().Colors.Primary.X > 0.8f)
                    && (GetCurrentTheme().Colors.Primary.Y > 0.5f);

    // 根据主题调整阴影颜色
    Vector4 shadowColor;

    if (isWarmTheme) { // 暖色调主题使用带色彩的阴影
      shadowColor = new Vector4(GetCurrentTheme().Colors.Primary.X * 0.3f,
                                GetCurrentTheme().Colors.Primary.Y * 0.3f,
                                GetCurrentTheme().Colors.Primary.Z * 0.3f,
                                0.15f);
    } else { // 标准亮色主题使用中性灰色阴影
      shadowColor = new Vector4(0.2f, 0.2f, 0.2f, 0.12f);
    }

    // 绘制多层微妙的阴影
    for (int i = 1; i <= 3; i++) {
      float offset = i * 1.5f;
      float alpha = shadowColor.W * (1f - i * 0.3f);
      Vector4 currentShadowColor = shadowColor with { W = alpha };
      drawList.AddRectFilled(pos + new Vector2(offset, offset),
                             pos + size + new Vector2(offset, offset),
                             ImGui.GetColorU32(currentShadowColor),
                             17f); // 与按钮圆角保持一致
    }

    // 添加顶部高光效果
    var highlightColor = new Vector4(1f, 1f, 1f, 0.3f);
    float highlightHeight = size.Y * 0.3f;
    drawList.AddRectFilledMultiColor(pos,
                                     pos + size with { Y = highlightHeight },
                                     ImGui.GetColorU32(highlightColor),
                                     ImGui.GetColorU32(highlightColor),
                                     ImGui.GetColorU32(highlightColor with { W = 0f }),
                                     ImGui.GetColorU32(highlightColor with { W = 0f }));

    // 添加微妙的边框效果
    Vector4 borderColor = isWarmTheme
                              ? new Vector4(GetCurrentTheme().Colors.Primary.X * 0.4f,
                                            GetCurrentTheme().Colors.Primary.Y * 0.4f,
                                            GetCurrentTheme().Colors.Primary.Z * 0.4f,
                                            0.3f)
                              : new Vector4(0.3f, 0.3f, 0.3f, 0.2f);
    drawList.AddRect(pos,
                     pos + size,
                     ImGui.GetColorU32(borderColor),
                     17f,
                     ImDrawFlags.None,
                     0.8f);
  }
}
