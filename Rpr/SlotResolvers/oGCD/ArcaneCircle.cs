using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.oGCD;

public class ArcaneCircle : ISlotResolver {
  public int Check() {
    if (SpellsDef.ArcaneCircle.GetSpell().IsReadyWithCanCast() is false) return -99;
    if (Qt.Instance.GetQt("神秘环") is false) return -98;

    if (Qt.MobMan.Holding) return -3;
    if (Core.Me.Distance(Core.Me.GetCurrTarget()) > Helper.GlobalSettings.AttackRange + 2) {
      return -2; // -2 for not in range
    }

    if ((AI.Instance.BattleData.CurrBattleTimeInMs < 5000)
     && (Math.Abs(SpellsDef.SoulScythe.GetSpell().Charges - 2) < 0.000005)) {
      return -11;
    }

    if (GCDHelper.GetGCDCooldown() < RprSettings.Instance.AnimLock) return -89;
    
    return 0;
  }

  public void Build(Slot slot) {
    BattleData.Instance.NumBurstPhases++;
    slot.Add(SpellsDef.ArcaneCircle.GetSpell());
  }
}
