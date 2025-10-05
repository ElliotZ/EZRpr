using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace ElliotZ;

public static class UIComponents {
  public static bool ToggleButton(string id, ref bool v)
  {
    var colors = ImGui.GetStyle().Colors;
    Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
    ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
    float frameHeight = ImGui.GetFrameHeight();
    float toggleBarLowerBound = frameHeight * 0.7f;
    float x = frameHeight * 1.50f;
    float num = frameHeight * 0.5f;
    bool stateChanged = false;
    ImGui.InvisibleButton((ImU8String) id, new Vector2(x, frameHeight));
    if (ImGui.IsItemClicked())
    {
      v = !v;
      stateChanged = true;
    }
    Vector2 toggleBarTopLeft = cursorScreenPos + new Vector2(0, frameHeight * 0.3f);
    if (ImGui.IsItemHovered()) {
      windowDrawList.AddRectFilled(toggleBarTopLeft, new Vector2(cursorScreenPos.X + x, cursorScreenPos.Y + toggleBarLowerBound), ImGui.GetColorU32(!v ? colors[23] : new Vector4(0.48f, 0.92f, 0.48f, 1f)), toggleBarLowerBound * 0.5f);
    } else {
      windowDrawList.AddRectFilled(toggleBarTopLeft, new Vector2(cursorScreenPos.X + x, cursorScreenPos.Y + toggleBarLowerBound), ImGui.GetColorU32(!v ? colors[21] * 0.6f : new Vector4(0.20f, 0.70f, 0.20f, 1f)), toggleBarLowerBound * 0.5f);
    }

    windowDrawList.AddCircleFilled(new Vector2((float) (cursorScreenPos.X - 3f + (double) num + (v ? 1 : 0) * (x + 6f - num * 2.0f)), cursorScreenPos.Y + num), num - 1.5f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)));
    
    ImGui.SameLine();
    var cursorPosY = ImGui.GetCursorPosY();
    ImGui.SetCursorPosY(cursorPosY + frameHeight * 0.25f);
    ImGui.Text(id);
    //ImGui.SetCursorPosY(cursorPosY);
    return stateChanged;
  }
  
  public static bool ToggleButtonLabeledInside(string id, ref bool v)
  {
    string label = id.Split("###").First();
    string realId = id.Split("###").Last();
    var colors = ImGui.GetStyle().Colors;
    Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
    ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
    float frameHeight = ImGui.GetFrameHeight();
    //float toggleBarLowerBound = frameHeight * 0.7f;
    float textWidth = ImGui.CalcTextSize(label).X;
    float x = frameHeight * 1.3f + textWidth;
    float sliderRadius = frameHeight * 0.5f;
    bool stateChanged = false;
    ImGui.InvisibleButton((ImU8String) realId, new Vector2(x, frameHeight));
    if (ImGui.IsItemClicked())
    {
      v = !v;
      stateChanged = true;
    }

    if (ImGui.IsItemHovered()) {
      windowDrawList.AddRectFilled(cursorScreenPos, 
                                   new Vector2(cursorScreenPos.X + x, cursorScreenPos.Y + frameHeight), 
                                   ImGui.GetColorU32(!v ? colors[23] : new Vector4(0.20f, 0.70f, 0.20f, 1f)), 
                                   frameHeight * 0.5f);
    } else {
      windowDrawList.AddRectFilled(cursorScreenPos, 
                                   new Vector2(cursorScreenPos.X + x, cursorScreenPos.Y + frameHeight), 
                                   ImGui.GetColorU32(!v ? colors[21] * 0.8f : new Vector4(0.14f, 0.62f, 0.14f, 1f)), 
                                   frameHeight * 0.5f);
    }

    windowDrawList.AddCircleFilled(new Vector2((float) (cursorScreenPos.X - 3f + (double) sliderRadius + (v ? 1 : 0) * (x + 6f - sliderRadius * 2.0f)), cursorScreenPos.Y + sliderRadius), 
                                   sliderRadius - 1.5f, 
                                   ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1f)));
    //windowDrawList.AddRectFilled();
    
    //ImGui.SameLine();
    Vector2 labelTopLeft = cursorScreenPos + new Vector2(v? frameHeight * 0.3f : 2f * sliderRadius, frameHeight * 0.25f);
    uint imCol = ImGui.GetColorU32(new Vector4(255, 255, 255, 255));
    //ImGui.SetCursorPosY(cursorPosY + frameHeight * 0.25f);
    //ImGui.SetCursorPosX(cursorPosX + x);
    windowDrawList.AddText(labelTopLeft, imCol, label);
    //ImGui.SetCursorPosY(cursorPosY);
    return stateChanged;
  }
  
  internal static bool CtrlButton(string label, string tooltip = "")
  {
    var ctrlHeld = ImGui.GetIO() is { KeyCtrl: true };

    bool ret;
    using (ImRaii.Disabled(!ctrlHeld))
      ret = ImGui.Button(label) && ctrlHeld;

    if (!string.IsNullOrEmpty(tooltip) && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
      Tooltip(tooltip);

    return ret;
  }
  
  internal static void Tooltip(string tooltip)
  {
    using (ImRaii.Tooltip())
    using (ImRaii.TextWrapPos(ImGui.GetFontSize() * 35.0f))
    {
      ImGui.TextUnformatted(tooltip);
    }
  }
}
