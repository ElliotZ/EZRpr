using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.JobApi;
using Dalamud.Game.ClientState.Objects.Types;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.oGCD;

public class Gluttony : ISlotResolver {
  private IBattleChara? _target { get; set; }

  public int Check() {
    _target = SpellsDef.Gluttony.OptimalAOETarget(1, 
                                                  Qt.Instance.GetQt("智能AOE"), 
                                                  5);

    if (_target is null 
     || SpellsDef.Gluttony
                 .GetSpell(_target)
                 .IsReadyWithCanCast() is false) {
      return -99;
    }

    if (Qt.Instance.GetQt("暴食") is false) return -98;

    if (Core.Me.Distance(Core.Me.GetCurrTarget()) > Helper.GlobalSettings.AttackRange) {
      return -2; // -2 for not in range
    }

    if (Qt.MobMan.Holding) return -3;

    if (Core.Me.HasAura(AurasDef.Enshrouded)) return -97; // wrong door go use sacrificium

    if (Core.Me.HasAura(AurasDef.Executioner)
     || Core.Me.HasAura(AurasDef.SoulReaver)
     || (Qt.Instance.GetQt("魂衣") && (Core.Resolve<JobApi_Reaper>().ShroudGauge > 80))) {
      return -4; // -4 for Overcapped Resources
    }

    if (!Qt.Instance.GetQt("单魂衣")
     && SpellsDef.Enshroud.IsUnlock()
     && Core.Me.HasAura(AurasDef.ArcaneCircle)) {
      return -12; // delay for burst window
    }

    if ((Helper.ComboTimer < 2 * GCDHelper.GetGCDDuration() + GCDHelper.GetGCDCooldown())
     && ((RprHelper.PrevCombo == SpellsDef.Slice)
      || (RprHelper.PrevCombo == SpellsDef.WaxingSlice))) {
      return -9; // -9 for combo protection
    }

    // add QT
    // might need to check for death's design
    if (GCDHelper.GetGCDCooldown() < RprSettings.Instance.AnimLock) return -89;
    return 0;
  }

  public void Build(Slot slot) {
    slot.Add(SpellsDef.Gluttony.GetSpell(_target));
  }
}
