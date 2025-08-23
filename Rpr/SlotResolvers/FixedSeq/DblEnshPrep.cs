using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.JobApi;
using AEAssist.MemoryApi;
using ElliotZ.Common;
using ElliotZ.Rpr.QtUI;
// ReSharper disable RedundantBoolCompare

namespace ElliotZ.Rpr.SlotResolvers.FixedSeq;

public class DblEnshPrep : ISlotSequence
{
    //private static bool needShadow(int t) => Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign, t, false);
    public static double PreAcEnshTimer => GCDHelper.GetGCDDuration() * 2.5 + 800;

    public int StartCheck()
    {
        if (Core.Me.Level < 88) { return -99; }
        if (SpellsDef.ArcaneCircle.GetSpell().Cooldown.TotalMilliseconds > PreAcEnshTimer)
        {
            return -6;
        }

        // only weave in 1st weaving window
        if (GCDHelper.GetGCDCooldown() < 1000) return -8;

        if (SpellsDef.Enshroud.GetSpell().IsReadyWithCanCast() is false) { return -99; }
        if (!Qt.Instance.GetQt("神秘环") || !Qt.Instance.GetQt("魂衣")) { return -98; }
        if (Qt.Instance.GetQt("单魂衣")) return -98;
        if (Core.Resolve<MemApiDuty>().InMission &&
                Core.Resolve<MemApiDuty>().DutyMembersNumber() is 4 or 24 &&
                !Core.Resolve<MemApiDuty>().InBossBattle)
        {
            return -98;
        }
        if (Core.Me.Distance(Core.Me.GetCurrTarget()!) 
                > Helper.GlblSettings.AttackRange)
        {
            return -2;  // -2 for not in range
        }
        if (Core.Resolve<JobApi_Reaper>().ShroudGauge < 50) { return -1; }
        if (Core.Me.HasAura(AurasDef.SoulReaver) || Core.Me.HasAura(AurasDef.Executioner)) { return -10; }
        return 0;
    }

    public int StopCheck(int index)
    {
        return -1;
    }

    public List<Action<Slot>> Sequence { get; } =
    [
        Step0,
        Step1,
        Step2,
    ];

    private static void Step0(Slot slot)
    {
        slot.Add(SpellsDef.Enshroud.GetSpell());
        slot.Add(GCD.BuffMaintain.Solve().GetSpell());
    }

    private static void Step1(Slot slot)
    {
        slot.Add(SpellsDef.VoidReaping.GetSpell());

    }
    private static void Step2(Slot slot)
    {
        if (Helper.TgtAuraTimerMoreThan(AurasDef.DeathsDesign, 30000) &&
            SpellsDef.HarvestMoon.GetSpell().IsReadyWithCanCast() && Qt.Instance.GetQt("收获月"))
            slot.Add(SpellsDef.HarvestMoon.GetSpell());
        else
            slot.Add(GCD.BuffMaintain.Solve().GetSpell());
        if (BattleData.Instance.NumBurstPhases == 0)
        {
            if (Qt.Instance.GetQt("爆发药") && ItemHelper.CheckCurrJobPotion())
            {
                slot.Add(new SlotAction(SlotAction.WaitType.None, 0, Spell.CreatePotion()));
                slot.Add(SpellsDef.ArcaneCircle.GetSpell());
            }
            else
            {
                slot.Add(new SlotAction(SlotAction.WaitType.WaitForSndHalfWindow,
                    0,
                    SpellsDef.ArcaneCircle.GetSpell()));
            }
        }
        else
        {
            slot.Add(SpellsDef.ArcaneCircle.GetSpell());
            slot.Add(new SlotAction(SlotAction.WaitType.None, 0, Spell.CreatePotion()));
        }
        BattleData.Instance.NumBurstPhases++;
    }
}
