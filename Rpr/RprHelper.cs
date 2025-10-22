using AEAssist;
using AEAssist.JobApi;
using AEAssist.MemoryApi;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr;

public static class RprHelper {
  public static int GetGcdDuration => BattleData.Instance.GcdDuration;

  public static uint PrevCombo => Core.Resolve<MemApiSpell>().GetLastComboSpellId();

  //public static int ComboTimer => (int)Core.Resolve<MemApiSpell>().GetComboTimeLeft().TotalMilliseconds;
  public static int Soul => Core.Resolve<JobApi_Reaper>().SoulGauge;
  public static int Shroud => Core.Resolve<JobApi_Reaper>().ShroudGauge;
  public static int BlueOrb => Core.Resolve<JobApi_Reaper>().LemureShroud;
  public static int PurpOrb => Core.Resolve<JobApi_Reaper>().VoidShroud;

  /// <summary>
  /// 自身buff剩余时间是否在x个gcd内
  /// </summary>
  /// <param name="buffId"></param>
  /// <param name="gcd">Number of GCDs</param>
  /// <returns></returns>
  public static bool AuraInGCDs(uint buffId, int gcd) {
    int timeLeft = Helper.GetAuraTimeLeft(buffId);
    if (timeLeft <= 0) return false;
    if (GetGcdDuration <= 0) return false;

    return timeLeft / GetGcdDuration < gcd;
  }

  public static int GcdsToSoulOvercap() {
    int res = (100 - Core.Resolve<JobApi_Reaper>().SoulGauge) / 10;

    if (Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign,
                                    BattleData.Instance.GcdDuration * (res + 3),
                                    false)
     || Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign,
                                    30000 + BattleData.Instance.GcdDuration * res,
                                    false)) {
      res++;
    }

    return res;
  }

  public static void CasualMode() {
    RprSettings.Instance.HoldBurstAtDyingPack = true;
    RprSettings.Instance.HoldBurstWhenTankPulling = true;
    RprSettings.Instance.AutoBloodBath = true;
    RprSettings.Instance.AutoCrest = true;
    RprSettings.Instance.AutoSecondWind = true;
    RprSettings.Instance.AutoFeint = true;
    if (RprSettings.Instance.ToggleStopOnModeChange) RprSettings.Instance.HandleStopMechs = true;
    RprSettings.Instance.IsHardCoreMode = false;
    RprSettings.Instance.AutoDumpResources = true;
    RprSettings.Instance.AutoSetSingleShroudInTrashPull = true;
  }

  public static void HardCoreMode() {
    RprSettings.Instance.HoldBurstAtDyingPack = false;
    RprSettings.Instance.HoldBurstWhenTankPulling = false;
    RprSettings.Instance.AutoBloodBath = false;
    RprSettings.Instance.AutoCrest = false;
    RprSettings.Instance.AutoSecondWind = false;
    RprSettings.Instance.AutoFeint = false;
    if (RprSettings.Instance.ToggleStopOnModeChange) RprSettings.Instance.HandleStopMechs = false;
    RprSettings.Instance.IsHardCoreMode = true;
    RprSettings.Instance.AutoDumpResources = false;
    RprSettings.Instance.AutoSetSingleShroudInTrashPull = false;
  }
}
