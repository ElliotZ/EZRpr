using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.oGCD;

public class BloodStalk : ISlotResolver {
  private IBattleChara? _target { get; set; }

  public int Check() {
    _target = SpellsDef.GrimSwathe.OptimalAOETarget(4, 
                                                    180, 
                                                    Qt.Instance.GetQt("智能AOE"));

    if (_target is null
     && SpellsDef.BloodStalk.AdaptiveId().GetSpell().IsReadyWithCanCast() is false) {
      return -99;
    }

    if (_target is not null 
     && SpellsDef.GrimSwathe.GetSpell(_target).IsReadyWithCanCast() is false) {
      return -99;
    }

    if (Qt.Instance.GetQt("挥割/爪") is false) return -98;
    if (Core.Me.HasAura(AurasDef.Enshrouded)) return -1; // not this slot resolver

    if ((RprHelper.Shroud == 100)
     || Core.Me.HasAura(AurasDef.SoulReaver)
     || Core.Me.HasAura(AurasDef.Executioner)) {
      return -4;
    }

    if ((Helper.ComboTimer <= GCDHelper.GetGCDDuration() + RprSettings.Instance.AnimLock * 3)
     && ((RprHelper.PrevCombo == SpellsDef.Slice)
      || (RprHelper.PrevCombo == SpellsDef.WaxingSlice))) {
      return -9; // -9 for combo protection
    }

    if (Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign,
                                    GCDHelper.GetGCDDuration() 
                                  + RprSettings.Instance.AnimLock * 3,
                                    false)) {
      return -14;
    }

    if (Core.Me.HasAura(AurasDef.ImmortalSacrifice) || SpellsDef.PlentifulHarvest.RecentlyUsed()) {
      return -15; // delay for burst window
    }

    if (Helper.AuraTimerLessThan(AurasDef.ArcaneCircle, 5000)
     && Core.Me.HasAura(AurasDef.PerfectioParata)) {
      return -16;
    }

    if (Qt.Instance.GetQt("暴食")) {
      if (SpellsDef.Gluttony.IsUnlock()
       && Qt.MobMan.Holding is false
       && (SpellsDef.Gluttony.GetSpell().Cooldown.TotalMilliseconds < GCDHelper.GetGCDDuration())) {
        return -21;
      }

      if ((RprHelper.Soul == 100)
       && (GCDHelper.GetGCDCooldown() >= RprSettings.Instance.AnimLock)) {
        return 1;
      }

      if (SpellsDef.Gluttony.IsUnlock()
       && SpellsDef.Gluttony.RdyInGCDs(RprHelper.GcdsToSoulOvercap())
       && !(SpellsDef.Gluttony.RdyInGCDs(2)
         && (SpellsDef.SoulSlice.GetSpell().Charges > 1.7f))) {
        return -22; // delay for gluttony gauge cost
      }
    } else {
      if (Qt.Instance.GetQt("神秘环")
       && RprHelper.Soul < 100
       && SpellsDef.ArcaneCircle.IsUnlock()
       && !SpellsDef.ArcaneCircle.RdyInGCDs(RprHelper.GcdsToSoulOvercap() + 3)) {
        return -31;
      }
    }

    if (!Qt.Instance.GetQt("倾泻资源")) // ignore all if dump qt is set
    {
      if (Qt.Instance.GetQt("神秘环")
       && SpellsDef.ArcaneCircle.IsUnlock()
       && SpellsDef.ArcaneCircle.RdyInGCDs(2)
       && (RprHelper.Shroud != 40)) {
        return -17; // delay for gluttony after burst window
      }

      if ((RprHelper.Soul == 100)
       && (GCDHelper.GetGCDCooldown() >= RprSettings.Instance.AnimLock)) {
        return 1;
      }

      if (Qt.Instance.GetQt("神秘环")
       && SpellsDef.ArcaneCircle.IsUnlock()
       && SpellsDef.ArcaneCircle.RdyInGCDs(Math.Min(6, RprHelper.GcdsToSoulOvercap() + 3))
       && (RprHelper.Shroud != 40)) {
        return -12; // delay for gluttony after burst window
      }

      if (!Helper.AuraTimerMoreThan(AurasDef.TrueNorth,
                                    BattleData.Instance.GcdDuration 
                                  - GCDHelper.GetGCDCooldown())
       && Qt.Instance.GetQt("真北")
       && Qt.Instance.GetQt("真北优化")
       && Core.Me.GetCurrTarget() is not null
       && Core.Me.GetCurrTarget().HasPositional()
       && !SpellsDef.TrueNorth.IsMaxChargeReady(1.8f)
       && ((Core.Me.HasAura(AurasDef.EnhancedGallows) && !Helper.AtRear)
        || (Core.Me.HasAura(AurasDef.EnhancedGibbet) && !Helper.AtFlank))) {
        return -13; // TN Optimizations perhaps
      }
    }

    if (GCDHelper.GetGCDCooldown() < RprSettings.Instance.AnimLock) return -89;
    return 0;
  }

  private Spell Solve() {
    if (Qt.Instance.GetQt("AOE")
     && _target is not null
     && SpellsDef.GrimSwathe.GetSpell(_target).IsReadyWithCanCast()) {
      return SpellsDef.GrimSwathe.GetSpell(_target);
    }

    return SpellsDef.BloodStalk.AdaptiveId().GetSpell();
  }

  public void Build(Slot slot) {
    slot.Add(Solve());
  }
}
