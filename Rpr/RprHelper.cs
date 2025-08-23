using AEAssist;
using AEAssist.JobApi;
using AEAssist.MemoryApi;
using ElliotZ.Common;

namespace ElliotZ.Rpr;

public static class RprHelper
{
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
    public static bool AuraInGCDs(uint buffId, int gcd)
    {
        var timeLeft = Helper.GetAuraTimeLeft(buffId);
        if (timeLeft <= 0) return false;
        if (GetGcdDuration <= 0) return false;

        return timeLeft / GetGcdDuration < gcd;
    }

    public static int GcdsToSoulOvercap()
    {
        var res = (100 - Core.Resolve<JobApi_Reaper>().SoulGauge) / 10;
        if (Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign,
                                            BattleData.Instance.GcdDuration * (res + 3),
                                            false) ||
            Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign,
                                            30000 + BattleData.Instance.GcdDuration * res,
                                            false))
        {
            res++;
        }
        return res;
    }
}
