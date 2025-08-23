using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using ElliotZ.Common;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.GCD;

public class EnshroudSk : ISlotResolver
{
    private IBattleChara? Target { get; set; }
    private IBattleChara? CommunioTarget { get; set; }

    public int Check()
    {
        var enhancedReapingCheck = (Core.Me.HasAura(AurasDef.EnhancedCrossReaping) ||
                                        Core.Me.HasAura(AurasDef.EnhancedVoidReaping)) ?
                                    3 : 4;
        Target = SpellsDef.GrimReaping.OptimalAOETarget(enhancedReapingCheck,
                                                            180f,
                                                            Qt.Instance.GetQt("智能AOE"));
        CommunioTarget = SpellsDef.Communio.OptimalAOETarget(1, Qt.Instance.GetQt("智能AOE"), 5);

        if (Core.Me.HasAura(AurasDef.Enshrouded) is false)
        {
            return -3;  // -3 for Unmet Prereq Conditions
        }
        if ((!SpellsDef.Communio.IsUnlock() || RprHelper.BlueOrb > 1) &&
                Core.Me.Distance(Core.Me.GetCurrTarget()!) 
                > Helper.GlblSettings.AttackRange)
        {
            return -2;  // -2 for not in range
        }

        if (Qt.Instance.GetQt("单魂衣") && 
                Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign,
                                           10000,
                                           false))
        {
            return -6;
        }
        if (!Core.Me.HasAura(AurasDef.ArcaneCircle)
                || Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign, 
                                               Helper.GetAuraTimeLeft(AurasDef.ArcaneCircle), 
                                               false)
                && Core.Me.HasAura(AurasDef.Enshrouded, 8500)
                // && !Core.Me.HasAura(AurasDef.PerfectioOculta)
            )
        {
            if (TargetMgr.Instance.EnemysIn20.Count <= 2 &&
                Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign, 30000, false))
            {
                return -6;
            }
            if (TargetMgr.Instance.EnemysIn20.Count > 2 && BuffMaintain.AOEAuraCheck())
            {
                return -7;
            }
        }
        return 0;
    }

    private Spell Solve()
    {
        //var purpOrb = Core.Resolve<JobApi_Reaper>().VoidShroud;
        //var enemyCount = TargetHelper.GetEnemyCountInsideSector(Core.Me, Core.Me.GetCurrTarget(), 8, 180);

        if (CommunioTarget is not null &&
                SpellsDef.Communio.GetSpell().IsReadyWithCanCast() &&
                RprHelper.BlueOrb < 2)
        {
            return SpellsDef.Communio.GetSpell(CommunioTarget!);
        }
        if (Qt.Instance.GetQt("AOE") && Target is not null)
        {
            return SpellsDef.GrimReaping.GetSpell(Target!);
        }
        if (Core.Me.HasAura(AurasDef.EnhancedCrossReaping))
        {
            return SpellsDef.CrossReaping.GetSpell();
        }
        //if (SpellsDef.ArcaneCircle.GetSpell().Cooldown.TotalMilliseconds <= 5000 &&
        //        Helper.TgtAuraTimerLessThan(AurasDef.DeathsDesign, 30000))  // TODO: Add Burst QT
        //{
        //    return SpellsDef.ShadowOfDeath;
        //}
        return SpellsDef.VoidReaping.GetSpell();
    }

    public void Build(Slot slot)
    {
        slot.Add(Solve());
    }
}
