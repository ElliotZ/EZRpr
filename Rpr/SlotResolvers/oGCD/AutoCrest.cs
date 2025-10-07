using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;

namespace ElliotZ.Rpr.SlotResolvers.oGCD;

public class AutoCrest : ISlotResolver {
  public int Check() {
    float crestThreshold = Core.Me.MaxHp * RprSettings.Instance.CrestPercent;

    if (RprSettings.Instance.AutoCrest is false) return -1;
    if (SpellsDef.ArcaneCrest.GetSpell().IsReadyWithCanCast() is false) return -99;
    if (Core.Me.CurrentHp > crestThreshold) return -4;
    if (Core.Me.HasAura(AurasDef.Enshrouded) && RprHelper.PurpOrb >= 2) return -5;

    if (Core.Me.GetCurrTarget() is null
     || !TargetHelper.targetCastingIsBossAOE(Core.Me.GetCurrTarget(), 2000)) {
      return -3;
    }

    if (GCDHelper.GetGCDCooldown() < RprSettings.Instance.AnimLock) return -89;
    return 0;
  }

  public void Build(Slot slot) {
    slot.Add(SpellsDef.ArcaneCrest.GetSpell());
  }
}
