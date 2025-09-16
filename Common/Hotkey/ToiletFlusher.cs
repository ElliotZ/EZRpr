using AEAssist.CombatRoutine.Module;

namespace ElliotZ.Hotkey;

public class ToiletFlusher() : HotKeyResolver(36988u,
                                              useHighPrioritySlot: false,
                                              waitCoolDown: false) {
  public override int Check() {
    return 0;
  }

  public override void Run() {
    AI.Instance.BattleData.HighPrioritySlots_OffGCD.Clear();
    AI.Instance.BattleData.HighPrioritySlots_GCD.Clear();
  }
}
