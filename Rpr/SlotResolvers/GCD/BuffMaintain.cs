using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.Define;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.JobApi;
using ElliotZ.Common;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.GCD;

public class BuffMaintain : ISlotResolver
{
    private static int GluttonyCD => (int)Math.Floor(SpellsDef.Gluttony.GetSpell().Cooldown.TotalMilliseconds);
    public int Check()
    {
        if (SpellsDef.ShadowOfDeath.GetSpell().IsReadyWithCanCast() is false) { return -99; }  // -99 for not usable
        if (Core.Me.Distance(Core.Me.GetCurrTarget()!) 
            > Helper.GlblSettings.AttackRange)
        {
            return -2;  // -2 for not in range
        }
        if (Qt.MobMan.Holding) return -3;

        if (SpellsDef.WhorlOfDeath.RecentlyUsed(5000)) { return -5; }  // -5 for Avoiding Spam

        if (!Qt.Instance.GetQt("印记")) return -98;
        
        if (Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign, GCDHelper.GetGCDDuration(), false))
        {
            return 1;  // 1 for buff maintain within a GCD
        }
        if (Qt.Instance.GetQt("暴食") && GluttonyCD < 10000 &&
            Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign, GluttonyCD + 7500) &&
            Helper.TgtAuraTimerMoreThan(AurasDef.DeathsDesign, GluttonyCD + 2500))
        {
            return 2;  // 2 for pre gluttony, earlier use because Gib/Gallows must be covered
        }
        if (Qt.Instance.GetQt("单魂衣") &&
            Core.Me.HasAura(AurasDef.Enshrouded) &&
            Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign, 10000))
        {
            return 3;
        }
        if (Core.Me.HasAura(AurasDef.Enshrouded) &&
            //SpellsDef.ArcaneCircle.GetSpell().Cooldown.TotalMilliseconds <= 5000 &&
            Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign, 30000, false))
        {
            return 3;  // 3 for burst prep
        }
        if (Core.Resolve<JobApi_Reaper>().SoulGauge == 100 &&
            !SpellsDef.Perfectio.GetSpell().IsReadyWithCanCast() &&
            !SpellsDef.PlentifulHarvest.GetSpell().IsReadyWithCanCast() &&
            Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign, 30000, false))
        {
            return 5;
        }
        //if (Qt.Instance.GetQt("神秘环") && 
        //        SpellsDef.ArcaneCircle.GetSpell().Cooldown.TotalMilliseconds < 11000 && 
        //        Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign, 11000 + GCDHelper.GetGCDDuration(), false))
        //{
        //    return 3;
        //}
        if (SpellsDef.WhorlOfDeath.IsUnlock() && AOEAuraCheck()) { return 4; }

        //if (Core.Resolve<JobApi_Reaper>().ShroudGauge >= 50 &&)
        return -1;  // -1 for general unmatch
    }

    /// <summary>
    /// Checks for Deaths Design on all enemies 
    /// </summary>
    /// <returns>true if less than half enemies around have the debuff, false otherwise</returns>
    public static bool AOEAuraCheck()
    {
        var enemyCount = TargetHelper.GetNearbyEnemyCount(5);
        var enemyList = TargetMgr.Instance.EnemysIn12;
        var noDebuffEnemyCount = enemyList.Count(v =>
                Core.Me.Distance(v.Value,
                    DistanceMode.IgnoreTargetHitbox | DistanceMode.IgnoreHeight) < 5 &&
                Helper.GetAuraTimeLeft(v.Value, 
                    AurasDef.DeathsDesign) <= BattleData.Instance.GcdDuration);
        if (RprSettings.Instance.Debug)
        {
            LogHelper.Print("BuffMaintain.AOEAuraCheck() Internals");
            LogHelper.Print($"{noDebuffEnemyCount}/{enemyCount}={(noDebuffEnemyCount / (double)enemyCount)}");
        }
        return (noDebuffEnemyCount / (double)enemyCount) > 0.5;
    }

    public static uint Solve()
    {
        var enemyCount = TargetHelper.GetNearbyEnemyCount(5);
        if (Qt.Instance.GetQt("AOE") && SpellsDef.WhorlOfDeath
                .GetSpell()
                .IsReadyWithCanCast()
                                     && enemyCount >= 3)
        {
            return SpellsDef.WhorlOfDeath;
        }
        return SpellsDef.ShadowOfDeath;
    }

    public void Build(Slot slot)
    {
        slot.Add(Solve().GetSpell());
    }
}
