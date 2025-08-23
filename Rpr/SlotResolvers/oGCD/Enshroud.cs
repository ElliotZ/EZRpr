using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.JobApi;
using AEAssist.MemoryApi;
using ElliotZ.Common;
using ElliotZ.Rpr.QtUI;
using ElliotZ.Rpr.SlotResolvers.FixedSeq;

namespace ElliotZ.Rpr.SlotResolvers.oGCD;

public class Enshroud : ISlotResolver
{
    private static bool DeadZoneCheck()
    {
        var accd = SpellsDef.ArcaneCircle.GetSpell().Cooldown.TotalMilliseconds;
        var neededShroud = (Core.Me.HasAura(AurasDef.IdealHost) ? 50 : 100) -
                              Core.Resolve<JobApi_Reaper>().ShroudGauge;
        var neededSoul = neededShroud * 5 - Core.Resolve<JobApi_Reaper>().SoulGauge;
        var soulSliceCharge = SpellsDef.SoulSlice.GetSpell().Charges;
        var gluttonyPossible = SpellsDef.Gluttony.GetSpell().Cooldown.TotalMilliseconds <
                               (accd - DblEnshPrep.PreAcEnshTimer - GCDHelper.GetGCDDuration() * 3);
        var ddTime = Core.Resolve<MemApiBuff>()
            .GetAuraTimeleft(Core.Me.GetCurrTarget() 
                             ?? throw new NullReferenceException(),
                             AurasDef.DeathsDesign, true);
        var totalTimeBeforeAC = accd -
                                GCDHelper.GetGCDDuration() * 2 -
                                6000 -
                                GCDHelper.GetGCDCooldown() -
                                DblEnshPrep.PreAcEnshTimer;
        var totalNumGCDsBeforeAC = (int)Math.Ceiling(totalTimeBeforeAC / GCDHelper.GetGCDDuration() + 0.5);
        if (gluttonyPossible) neededSoul -= 50;
        var numGcdForShroud = neededShroud / 10;
        var numGcdForSoul = neededSoul / 10;
        var numGcdForDd = (int)Math.Ceiling((accd - DblEnshPrep.PreAcEnshTimer - ddTime) / 30000);
        var maxPossibleSoulSliceUses = Math.Floor((accd - DblEnshPrep.PreAcEnshTimer) / 30000 + soulSliceCharge);
        numGcdForSoul -= (int)maxPossibleSoulSliceUses * 4;
        return totalNumGCDsBeforeAC >= numGcdForSoul + numGcdForShroud + numGcdForDd;
    }
    public int Check()
    {
        if (SpellsDef.Enshroud.GetSpell().IsReadyWithCanCast() is false) { return -99; }
        if (Qt.Instance.GetQt("魂衣") is false) { return -98; }
        if (Core.Me.Distance(Core.Me.GetCurrTarget()!) 
            > Helper.GlblSettings.AttackRange + 2)
        {
            return -2;  // -2 for not in range
        }

        if (Qt.MobMan.Holding) return -3;

        // delay for ideal host when entering combat with gauge, perfectio can be fit into opener burst this way
        if (Core.Me.HasAura(AurasDef.BloodsownCircle)
                || Core.Me.HasAura(AurasDef.ImmortalSacrifice)
                    && !Core.Me.HasAura(AurasDef.IdealHost))
        {
            return -8;
        }
        if (!Qt.Instance.GetQt("倾泻资源") && !Core.Me.HasAura(AurasDef.IdealHost))  // ignore all if dump qt is set
        {
            if (!Qt.Instance.GetQt("单魂衣") && RprHelper.Shroud < 100)
            {
                if (!DeadZoneCheck())
                {
                    return -6;
                }
                if (Qt.Instance.GetQt("暴食") &&
                        !Core.Me.HasAura(AurasDef.ArcaneCircle) &&
                        SpellsDef.Gluttony.GetSpell().Cooldown.TotalMilliseconds <= 20000 &&
                        SpellsDef.ArcaneCircle.GetSpell().Cooldown.TotalMilliseconds >= 55000)
                {
                    return -7;
                }
            }
            if (RprHelper.Shroud == 100 && RprHelper.Soul <= 90
                && SpellsDef.ArcaneCircle.RdyInGCDs(RprHelper.GcdsToSoulOvercap())) 
            { 
                return -9; 
            }
            if (SpellsDef.SoulSlice.GetSpell().Charges > 1.6f && 
                    Core.Resolve<JobApi_Reaper>().ShroudGauge < 90)
            {
                return -11;  // SoulSlice overcap protection
            }
            //if (Helper.AoeTtkCheck() && TTKHelper.IsTargetTTK(Core.Me.GetCurrTarget()))
            //{
            //    return -16;  // delay for next pack
            //}
        }
        if (Core.Me.HasAura(AurasDef.SoulReaver) || Core.Me.HasAura(AurasDef.Executioner))
        {
            return -10;  // protect Gib/Gallows
        }
        if (GCDHelper.GetGCDCooldown() < RprSettings.Instance.AnimLock) return -89;
        //if (Core.Resolve<JobApi_Reaper>().ShroudGauge < 50 && !Core.Me.HasAura(AurasDef.IdealHost)) return -1;
        return 0;
    }

    public void Build(Slot slot)
    {
        slot.Add(SpellsDef.Enshroud.GetSpell());
    }
}