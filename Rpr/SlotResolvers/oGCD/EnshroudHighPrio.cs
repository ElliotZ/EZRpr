using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.oGCD;

public class EnshroudHighPrio : ISlotResolver {
  public int Check() {
    if (SpellsDef.Enshroud.GetSpell().IsReadyWithCanCast() is false) return -99;
    if (Qt.Instance.GetQt("魂衣") is false) return -98;
    
    if (Helper.AuraTimerLessThan(AurasDef.IdealHost, 1500)
     && (GCDHelper.GetGCDCooldown() >= RprSettings.Instance.AnimLock)) {
      return 1;
    }

    return -8; // -8 for exiting high prio state
  }

  public void Build(Slot slot) {
    slot.Add(SpellsDef.Enshroud.GetSpell());
  }
}
