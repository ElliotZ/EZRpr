using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Opener;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using ElliotZ.Common;
using ElliotZ.Rpr.QtUI;
// ReSharper disable RedundantBoolCompare

namespace ElliotZ.Rpr.SlotResolvers.FixedSeq;

public class Opener100 : IOpener
{
    public int StartCheck()
    {
        if (Qt.Instance.GetQt("起手") is false) { return -98; }
        if (!Qt.Instance.GetQt("神秘环") || !Qt.Instance.GetQt("魂衣")) { return -98; }
        if (Core.Me.Level < 88) { return -99; }  // might not need this
        if (SpellsDef.SoulSlice.IsMaxChargeReady(0.0f) is false) { return -99; }
        if (SpellsDef.ArcaneCircle.GetSpell().IsReadyWithCanCast() is false) { return -99; }
        if (Core.Me.Distance(Core.Me.GetCurrTarget()!) 
                > Helper.GlblSettings.AttackRange)
        {
            return -2;  // -2 for not in range
        }
        if (SpellsDef.Gluttony.CoolDownInGCDs(3) == false) { return -6; }
        //if (RprHelper.Shroud >= 50) return -7;
        if (TargetHelper.GetNearbyEnemyCount(25) > 2)
        {
            return -13; // opener is basically only meant for single target
        }
        return 0;
    }

    public int StopCheck(int index)
    {
        return -1;
    }

    public void InitCountDown(CountDownHandler cdh)
    {
        if (RprSettings.Instance.RestoreQtSet) { Qt.LoadQtStatesNoPot(); }

        const int startTime = 15000;
        if (!Core.Me.HasAura(AurasDef.Soulsow) && SpellsDef.Soulsow.GetSpell().IsReadyWithCanCast())
        {
            cdh.AddAction(startTime, () => SpellsDef.Soulsow.GetSpell());
        }
        if (RprSettings.Instance.PrepullSprint && Spell.CreateSprint().IsReadyWithCanCast())
        {
            cdh.AddAction(startTime, Spell.CreateSprint);
        }
        cdh.AddAction(RprSettings.Instance.PrepullCastTimeHarpe,
                      () => SpellsDef.Harpe.GetSpell(SpellTargetType.Target));
        if (RprSettings.Instance.PrepullIngress && PrepullIngressCheck())
        {
            cdh.AddAction(RprSettings.Instance.PrepullCastTimeHarpe -
                              (int)SpellsDef.Harpe.GetSpell().CastTime.TotalMilliseconds,
                          PrepullIngress);
        }
    }

    private static bool PrepullIngressCheck()
    {
        if (Core.Me.GetCurrTarget() is null) return false;
        var targetRing = Core.Me.GetCurrTarget()!.HitboxRadius * 2;
        var atkRange = Helper.GlblSettings.AttackRange;

        return SpellsDef.HellsIngress.GetSpell().IsReadyWithCanCast() &&
               //Core.Me.GetCurrTarget().Distance(Core.Me) < 15 + targetRing + atkRange &&
               Core.Me.GetCurrTarget()!.Distance(Core.Me) > 15 - targetRing - atkRange;
    }

    private static Spell PrepullIngress()
    {
        Core.Resolve<MemApiMoveControl>().Stop();
        Core.Resolve<MemApiMove>().SetRot(Helper.GetRotationToTarget(Core.Me.Position,
                                                                     Core.Me.GetCurrTarget()!.Position));
        return SpellsDef.HellsIngress.GetSpell();
    }

    public List<Action<Slot>> Sequence { get; } = [Step0, Step1];

    private static void Step0(Slot slot)
    {
        slot.Add(SpellsDef.ShadowOfDeath.GetSpell());
        if (Qt.Instance.GetQt("爆发药") && !Qt.Instance.GetQt("爆发药2分") && !RprSettings.Instance.TripleWeavePot)
        {
            slot.Add(new SlotAction(SlotAction.WaitType.WaitInMs,
                                    GCDHelper.GetGCDDuration() - RprSettings.Instance.AnimLock,
                                    Spell.CreatePotion()));
        }
    }

    private static void Step1(Slot slot)
    {
        slot.Add(SpellsDef.SoulSlice.GetSpell());
        if (RprSettings.Instance.TripleWeavePot && Qt.Instance.GetQt("爆发药") && !Qt.Instance.GetQt("爆发药2分"))
        {
            slot.Add(new SlotAction(SlotAction.WaitType.WaitInMs,
                        GCDHelper.GetGCDDuration() - RprSettings.Instance.AnimLock * 3,
                        SpellsDef.ArcaneCircle.GetSpell()));
            slot.Add(Spell.CreatePotion());
            slot.Add(SpellsDef.Gluttony.GetSpell());
        }
        else
        {
            slot.Add(new SlotAction(SlotAction.WaitType.WaitInMs,
                                    GCDHelper.GetGCDDuration() - 2000,
                                    SpellsDef.ArcaneCircle.GetSpell()));
            slot.Add(SpellsDef.Gluttony.GetSpell());
            //LogHelper.Error("why does this not work");
        }
    }

    public uint Level { get; } = 88;
}
