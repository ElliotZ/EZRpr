using AEAssist.CombatRoutine.View.JobView;

namespace ElliotZ.ModernJobViewFramework.HotKey;

public class HotkeyControl {
  internal readonly string _name;
  internal IHotkeyResolver _slot;
  internal string _toolTip = "";

  internal HotkeyControl(string name, IHotkeyResolver slot) {
    _name = name;
    _slot = slot;
  }
}
