using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Opener;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.FixedSeq;

public class OpenerCountDownOnly : IOpener {
  public int StartCheck() {
    //if (Qt.Instance.GetQt("起手") == false) { return -98; }
    //if (RprSettings.Instance.PrepullIngress && Core.Me.Level < 20) { return -99; }  // might not need this
    return 0;
  }

  public int StopCheck(int index) {
    return -1;
  }

  public void InitCountDown(CountDownHandler cdh) {
    if (RprSettings.Instance.RestoreQtSet) Qt.LoadQtStatesNoPot();

    const int startTime = 15000;

    if (!Core.Me.HasAura(AurasDef.Soulsow) && SpellsDef.Soulsow.GetSpell().IsReadyWithCanCast()) {
      cdh.AddAction(startTime, () => SpellsDef.Soulsow.GetSpell());
    }

    if (RprSettings.Instance.PrepullSprint && Spell.CreateSprint().IsReadyWithCanCast()) {
      cdh.AddAction(startTime, Spell.CreateSprint);
    }

    cdh.AddAction(RprSettings.Instance.PrepullCastTimeHarpe,
                  () => SpellsDef.Harpe.GetSpell(SpellTargetType.Target));

    if (RprSettings.Instance.PrepullIngress && PrepullIngressCheck()) {
      cdh.AddAction(RprSettings.Instance.PrepullCastTimeHarpe
                  - (int)SpellsDef.Harpe.GetSpell().CastTime.TotalMilliseconds,
                    PrepullIngress);
    }
  }

  private static bool PrepullIngressCheck() {
    if (Core.Me.GetCurrTarget() is null) return false;
    float targetRing = Core.Me.GetCurrTarget().HitboxRadius * 2;
    int atkRange = Helper.GlobalSettings.AttackRange;

    return SpellsDef.HellsIngress.GetSpell().IsReadyWithCanCast() 
        //&& Core.Me.GetCurrTarget().Distance(Core.Me) < 15 + targetRing + atkRange 
        && (Core.Me.GetCurrTarget().Distance(Core.Me) > 15 - targetRing - atkRange);
           
           
  }

  private static Spell PrepullIngress() {
    Core.Resolve<MemApiMoveControl>().Stop();
    Core.Resolve<MemApiMove>()
        .SetRot(Helper.GetRotationToTarget(Core.Me.Position, 
                                           Core.Me.GetCurrTarget().Position));
    return SpellsDef.HellsIngress.GetSpell();
  }

  public List<Action<Slot>> Sequence { get; } = [];

  public uint Level { get; } = 1;
}
