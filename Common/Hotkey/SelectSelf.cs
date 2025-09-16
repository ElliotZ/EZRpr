using AEAssist;
using AEAssist.Extension;
using AEAssist.Helper;

namespace ElliotZ.Hotkey;

public class SelectSelf() : HotKeyResolver(7385u,
                                           useHighPrioritySlot: false,
                                           waitCoolDown: false) {
  public override int Check() {
    return 0;
  }

  public override void Run() {
    Core.Me.SetTarget(Core.Me);
    LogHelper.Print("已选中自己。");
  }
}
