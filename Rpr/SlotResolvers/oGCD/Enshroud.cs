using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.JobApi;
using AEAssist.MemoryApi;
using ElliotZ.Rpr.QtUI;
using ElliotZ.Rpr.SlotResolvers.FixedSeq;

namespace ElliotZ.Rpr.SlotResolvers.oGCD;

public class Enshroud : ISlotResolver {
  /// <summary>
  /// 计算是否需要为下一窗口的双附体攒蓝条。计算原理大约是通过计算下一个神秘环前的总共GCD数，以及通过当前的红蓝条的状态、技能CD、目标带有的印记时长等状态，估算攒出附体所需的50蓝条所需要的GCD数，并以此判断现在是否需要保留附体。
  /// </summary>
  /// <returns>如果需要保留附体的话返回true</returns>
  private static bool DeadZoneCheck() {
    double accd = SpellsDef.ArcaneCircle.GetSpell().Cooldown.TotalMilliseconds;
    int neededShroud = (Core.Me.HasAura(AurasDef.IdealHost) ? 50 : 100)
                     - Core.Resolve<JobApi_Reaper>().ShroudGauge;
    int neededSoul = neededShroud * 5 - Core.Resolve<JobApi_Reaper>().SoulGauge;
    float soulSliceCharge = SpellsDef.SoulSlice.GetSpell().Charges;
    bool gluttonyPossible = SpellsDef.Gluttony.GetSpell().Cooldown.TotalMilliseconds
                          < accd - DblEnshPrep.PreAcEnshTimer - GCDHelper.GetGCDDuration() * 3;
    int ddTime = Core.Resolve<MemApiBuff>().GetAuraTimeleft(
        Core.Me.GetCurrTarget(),
        AurasDef.DeathsDesign,
        true);
    double totalTimeBeforeAC = accd
                             - GCDHelper.GetGCDDuration() * 2
                             - 6000
                             - GCDHelper.GetGCDCooldown()
                             - DblEnshPrep.PreAcEnshTimer;
    int totalNumGCDsBeforeAC =
        (int)Math.Ceiling(totalTimeBeforeAC / GCDHelper.GetGCDDuration() + 0.5);
    if (gluttonyPossible) neededSoul -= 50;
    int numGcdForShroud = neededShroud / 10;
    int numGcdForSoul = neededSoul / 10;
    int numGcdForDd = (int)Math.Ceiling((accd - DblEnshPrep.PreAcEnshTimer - ddTime) / 30000);
    double maxPossibleSoulSliceUses = Math.Floor((accd - DblEnshPrep.PreAcEnshTimer) / 30000
                                               + soulSliceCharge);
    numGcdForSoul -= (int)maxPossibleSoulSliceUses * 4;
    return totalNumGCDsBeforeAC >= numGcdForSoul + numGcdForShroud + numGcdForDd;
  }

  public int Check() {
    if (SpellsDef.Enshroud.GetSpell().IsReadyWithCanCast() is false) return -99;
    if (Qt.Instance.GetQt("魂衣") is false) return -98;

    if (Core.Me.Distance(Core.Me.GetCurrTarget()) > Helper.GlobalSettings.AttackRange + 2) {
      return -2; // -2 for not in range
    }

    if (Qt.MobMan.Holding) return -3;

    // delay for ideal host when entering combat with gauge
    // perfectio can be fit into opener burst this way
    if (Core.Me.HasAura(AurasDef.BloodsownCircle)
     || (Core.Me.HasAura(AurasDef.ImmortalSacrifice) && !Core.Me.HasAura(AurasDef.IdealHost))) {
      return -8;
    }

    // perfectio gets eaten if not dumped before enshroud
    if (Core.Me.HasAura(AurasDef.PerfectioParata)) return -12;

    // ignore all if dump qt is set
    if (!Qt.Instance.GetQt("倾泻资源") && !Core.Me.HasAura(AurasDef.IdealHost)) {
      if (!Qt.Instance.GetQt("单魂衣") && (RprHelper.Shroud < 100)) {
        if (!DeadZoneCheck()) return -6;

        if (Qt.Instance.GetQt("暴食")
         && !Core.Me.HasAura(AurasDef.ArcaneCircle)
         && (SpellsDef.Gluttony.GetSpell().Cooldown.TotalMilliseconds <= 20000)
         && (SpellsDef.ArcaneCircle.GetSpell().Cooldown.TotalMilliseconds >= 55000)) {
          return -7;
        }
      }

      if ((RprHelper.Shroud == 100)
       && (RprHelper.Soul <= 90)
       && SpellsDef.ArcaneCircle.RdyInGCDs(RprHelper.GcdsToSoulOvercap())) {
        return -9;
      }

      if ((SpellsDef.SoulSlice.GetSpell().Charges > 1.6f)
       && (Core.Resolve<JobApi_Reaper>().ShroudGauge < 90)) {
        return -11; // SoulSlice overcap protection
      }
    }

    if (Core.Me.HasAura(AurasDef.SoulReaver) || Core.Me.HasAura(AurasDef.Executioner)) {
      return -10; // protect Gib/Gallows
    }

    if (GCDHelper.GetGCDCooldown() < RprSettings.Instance.AnimLock) return -89;
    //if (Core.Resolve<JobApi_Reaper>().ShroudGauge < 50 && !Core.Me.HasAura(AurasDef.IdealHost)) return -1;
    return 0;
  }

  public void Build(Slot slot) {
    slot.Add(SpellsDef.Enshroud.GetSpell());
  }
}
