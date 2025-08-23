using AEAssist.CombatRoutine.Trigger;
using AEAssist.GUI;
using ElliotZ.Rpr.QtUI;
using ImGuiNET;

namespace ElliotZ.Rpr.Triggers;

public class TriggerActionQt : ITriggerAction
{
    public string DisplayName { get; } = "Reaper/QT";
    public string Remark { get; set; } = "";

    public string Key = "";
    public bool Value;

    // 辅助数据 因为是private 所以不存档
    private int _selectIndex;
    private readonly string[] _qtArray = Qt.Instance.GetQtArray();

    public bool Draw()
    {
        _selectIndex = Array.IndexOf(_qtArray, Key);
        if (_selectIndex == -1)
        {
            _selectIndex = 0;
        }
        ImGuiHelper.LeftCombo("选择Key", ref _selectIndex, _qtArray);
        Key = _qtArray[_selectIndex];
        ImGui.SameLine();
        using (new GroupWrapper())
        {
            ImGui.Checkbox("", ref Value);
        }
        return true;
    }

    public bool Handle()
    {
        Qt.Instance.SetQt(Key, Value);
        //if (RprSettings.Instance.TimeLinesDebug) LogHelper.Print("时间轴", $"{Key}QT => {Value}");
        return true;
    }
}
