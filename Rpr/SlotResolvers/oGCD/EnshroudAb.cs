using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.oGCD;

public class EnshroudAb : ISlotResolver {
  private IBattleChara? _target { get; set; }

  public int Check() {
    _target = SpellsDef.LemuresScythe.OptimalAOETarget(3, 
                                                       180, 
                                                       Qt.Instance.GetQt("智能AOE"));

    if (Core.Me.HasAura(AurasDef.Enshrouded) is false) return -3; // -3 for Unmet Prereq Conditions

    if (_target is null
     && !SpellsDef.LemuresSlice.GetSpell().IsReadyWithCanCast()) {
      return -99;
    }

    if (_target is not null
     && !SpellsDef.LemuresScythe.GetSpell(_target).IsReadyWithCanCast()) {
      return -4;
    }

    //if (RprHelper.PurpOrb < 2) return -3;
    if (GCDHelper.GetGCDCooldown() < RprSettings.Instance.AnimLock) return -89;
    return 0;
  }

  private Spell Solve() {
    if (Qt.Instance.GetQt("AOE")
     && _target is not null
     && SpellsDef.LemuresScythe.GetSpell(_target).IsReadyWithCanCast()) {
      return SpellsDef.LemuresScythe.GetSpell(_target);
    }

    return SpellsDef.LemuresSlice.GetSpell();
  }

  public void Build(Slot slot) {
    slot.Add(Solve());
  }
}
