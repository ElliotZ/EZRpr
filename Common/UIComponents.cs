using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace ElliotZ;

public static class UIComponents {
  private static readonly uint _toggleSliderColor = 
      ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1f));
  public static bool ToggleButton(string id, ref bool v) {
    var colors = ImGui.GetStyle().Colors;
    Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
    ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
    float frameHeight = ImGui.GetFrameHeight();
    float toggleBarLowerBound = frameHeight * 0.7f;
    float barLength = frameHeight * 1.50f;
    float radius = frameHeight * 0.5f;
    bool stateChanged = false;
    // clickable area
    ImGui.InvisibleButton((ImU8String) id, new Vector2(barLength, frameHeight));
    if (ImGui.IsItemClicked()) {
      v = !v;
      stateChanged = true;
    }
    Vector2 toggleBarTopLeft = cursorScreenPos + new Vector2(0, frameHeight * 0.3f);
    // bar behind the toggle switch
    windowDrawList.AddRectFilled(toggleBarTopLeft,
                                 new Vector2(cursorScreenPos.X + barLength,
                                             cursorScreenPos.Y + toggleBarLowerBound),
                                 // decide color according to whether button is hovered
                                 ImGui.IsItemHovered()
                                     ? ImGui.GetColorU32(
                                         !v   // color according to whether toggle is active
                                             ? colors[23] 
                                             : new Vector4(0.48f, 0.92f, 0.48f, 1f))  // bright green
                                     : ImGui.GetColorU32(
                                         !v
                                             ? colors[21] * 0.6f
                                             : new Vector4(0.20f, 0.70f, 0.20f, 1f)),  // green
                                 toggleBarLowerBound * 0.5f);
    // the toggle switch
    windowDrawList.AddCircleFilled(
        new Vector2((float) (cursorScreenPos.X - 3f 
                           + (double) radius 
                           + (v ? 1 : 0) * (barLength + 6f - radius * 2.0f)), 
                    cursorScreenPos.Y + radius), 
        radius - 1.5f, 
        _toggleSliderColor);
    
    ImGui.SameLine();
    float cursorPosY = ImGui.GetCursorPosY();
    ImGui.SetCursorPosY(cursorPosY + frameHeight * 0.25f);
    ImGui.Text(id);
    //ImGui.SetCursorPosY(cursorPosY);
    return stateChanged;
  }
  
  public static bool ToggleButtonLabeledInside(string id, ref bool v) {
    string label = id.Split("###").First();
    string realId = id.Split("###").Last();
    var colors = ImGui.GetStyle().Colors;
    Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
    ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
    float frameHeight = ImGui.GetFrameHeight();
    //float toggleBarLowerBound = frameHeight * 0.7f;
    float textWidth = ImGui.CalcTextSize(label).X;
    float barLength = frameHeight * 1.3f + textWidth;
    float sliderRadius = frameHeight * 0.5f;
    bool stateChanged = false;
    ImGui.InvisibleButton((ImU8String) realId, new Vector2(barLength, frameHeight));
    if (ImGui.IsItemClicked()) {
      v = !v;
      stateChanged = true;
    }

    windowDrawList.AddRectFilled(cursorScreenPos,
                                 new Vector2(cursorScreenPos.X + barLength,
                                             cursorScreenPos.Y + frameHeight),
                                 // same logic as above
                                 ImGui.IsItemHovered()
                                     ? ImGui.GetColorU32(
                                         !v 
                                             ? colors[23] 
                                             : new Vector4(0.20f, 0.70f, 0.20f, 1f))
                                     : ImGui.GetColorU32(
                                         !v
                                             ? colors[21] * 0.8f
                                             : new Vector4(0.14f, 0.62f, 0.14f, 1f)),
                                 frameHeight * 0.5f);

    windowDrawList.AddCircleFilled(
        new Vector2((float) (cursorScreenPos.X - 3f 
                           + (double) sliderRadius 
                           + (v ? 1 : 0) * (barLength + 6f - sliderRadius * 2.0f)), 
                    cursorScreenPos.Y + sliderRadius), 
        sliderRadius - 1.5f, 
        _toggleSliderColor);
    //windowDrawList.AddRectFilled();
    
    //ImGui.SameLine();
    Vector2 labelTopLeft = cursorScreenPos 
                         + new Vector2(v
                                           ? frameHeight * 0.3f 
                                           : 2f * sliderRadius,
                                       frameHeight * 0.25f);
    
    uint textColor = ImGui.GetColorU32(new Vector4(255, 255, 255, 255));
    windowDrawList.AddText(labelTopLeft, textColor, label);
    //ImGui.SetCursorPosY(cursorPosY);
    return stateChanged;
  }
  
  internal static bool CtrlButton(string label, string tooltip = "") {
    bool ctrlHeld = ImGui.GetIO() is { KeyCtrl: true };

    bool ret;

    using (ImRaii.Disabled(!ctrlHeld)) {
      ret = ImGui.Button(label) && ctrlHeld;
    }

    if (!string.IsNullOrEmpty(tooltip) 
     && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) {
      Tooltip(tooltip);
    }

    return ret;
  }
  
  internal static void Tooltip(string tooltip) {
    using (ImRaii.Tooltip())
    using (ImRaii.TextWrapPos(ImGui.GetFontSize() * 35.0f)) {
      ImGui.TextUnformatted(tooltip);
    }
  }
}
