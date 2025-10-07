using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.GCD;

public class EnshroudSk : ISlotResolver {
  private IBattleChara? _target { get; set; }
  private IBattleChara? _communioTarget { get; set; }

  public int Check() {
    int aoeExpectation = Core.Me.HasAura(AurasDef.EnhancedCrossReaping)
                      || Core.Me.HasAura(AurasDef.EnhancedVoidReaping)
                             ? 3
                             : 4;  // aoe condition depends on whether reapings are enhanced
    _target = SpellsDef.GrimReaping.OptimalAOETarget(aoeExpectation,
                                                     180f,
                                                     Qt.Instance.GetQt("智能AOE"));
    _communioTarget = SpellsDef.Communio.OptimalAOETarget(1, 
                                                          Qt.Instance.GetQt("智能AOE"), 
                                                          5);

    if (Core.Me.HasAura(AurasDef.Enshrouded) is false) return -3; // -3 for Unmet Prereq Conditions

    if ((!SpellsDef.Communio.IsUnlock() || (RprHelper.BlueOrb > 1))
     && (Core.Me.Distance(Core.Me.GetCurrTarget()) > Helper.GlobalSettings.AttackRange)) {
      return -2; // -2 for not in range
    }

    return 0;
  }

  private static bool DeathsDesignMaintainCheck() {
//    if (Qt.Instance.GetQt("单魂衣")
//     && Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign, 10000, false)) {
//      return true;
//    }

    // pause enshroud slashes for buff maintaining
    if ((!Core.Me.HasAura(AurasDef.ArcaneCircle)
     || Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign,
                                     Helper.GetAuraTimeLeft(AurasDef.ArcaneCircle),
                                     false))
      && Core.Me.HasAura(AurasDef.Enshrouded, 10000)
//      && !SpellsDef.ShadowOfDeath.GetSpell().RecentlyUsed(8500)
//      && !SpellsDef.WhorlOfDeath.GetSpell().RecentlyUsed(8500)
      && !Qt.MobMan.Holding
//      && !Core.Me.HasAura(AurasDef.PerfectioOculta)
       ) {
      if ((!Qt.Instance.GetQt("AOE") || TargetHelper.GetNearbyEnemyCount(5) <= 2)
       && Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign,
                                      10000,
                                      false)) {
        return true;
      }

      if (Qt.Instance.GetQt("AOE") 
       && TargetHelper.GetNearbyEnemyCount(5) > 2 
       && BuffMaintain.AOEAuraCheck()) {
        return true;
      }
    }

    return false;
  }
  
  private Spell Solve() {
    if (DeathsDesignMaintainCheck()) {
      return BuffMaintain.Solve().GetSpell();
    }
    
    if (_communioTarget is not null
     && SpellsDef.Communio.GetSpell().IsReadyWithCanCast()
     && (RprHelper.BlueOrb < 2)) {
      return BattleData.Instance.HoldCommunio switch {
          1 => BuffMaintain.Solve().GetSpell(),
          2 => SpellsDef.Harpe.GetSpell(),
          _ => new Spell(SpellsDef.Communio, _communioTarget) { DontUseGcdOpt = true },
      };
    }

    if (Qt.Instance.GetQt("AOE") && _target is not null) {
      return SpellsDef.GrimReaping.GetSpell(_target);
    }

    return Core.Me.HasAura(AurasDef.EnhancedCrossReaping)
               ? SpellsDef.CrossReaping.GetSpell()
               : SpellsDef.VoidReaping.GetSpell();
  }

  public void Build(Slot slot) {
    if (SpellsDef.ArcaneCircle.GetSpell().RecentlyUsed()) {
      Spell ac = SpellsDef.ArcaneCircle.GetSpell();
      int wait = 650 - (int) (ac.RecastTimeElapsed * 1000);
      if (RprSettings.Instance.Debug) LogHelper.Print($"wait: {wait}");
      slot.Add(new SlotAction(SlotAction.WaitType.WaitInMs,
                              wait,
                              Solve()));
    } else {
      slot.Add(Solve());
    }
  }
}
