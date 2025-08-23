using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.JobApi;
using AEAssist.MemoryApi;
using ElliotZ.Common;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.oGCD;

public class Ingress : ISlotResolver
{
    public int Check()
    {
        if (Core.Me.GetCurrTarget() is null) return -9;
        var targetRing = Core.Me.GetCurrTarget().HitboxRadius * 2;
        var atkRange = Helper.GlblSettings.AttackRange;

        if (SpellsDef.HellsIngress.GetSpell().IsReadyWithCanCast() &&
                (Core.Me.HasAura(AurasDef.SoulReaver) ||
                    Core.Me.HasAura(AurasDef.Executioner) ||
                    !Qt.Instance.GetQt("勾刃") ||
                    Core.Resolve<JobApi_Reaper>().LemureShroud > 2) &&
                Qt.Instance.GetQt("自动突进") &&
                //GCDHelper.GetGCDCooldown() < 1100 &&
                Core.Me.GetCurrTarget().Distance(Core.Me) < 15 + targetRing + atkRange &&
                Core.Me.GetCurrTarget().Distance(Core.Me) > 15 - targetRing - atkRange &&
                GCDHelper.GetGCDCooldown() >= RprSettings.Instance.AnimLock)
        {
            return 0;
        }
        return -1;
    }

    public void Build(Slot slot)
    {
        Core.Resolve<MemApiMoveControl>().Stop();
        Core.Resolve<MemApiMove>().SetRot(Helper.GetRotationToTarget(Core.Me.Position,
                                                                     Core.Me.GetCurrTarget().Position));
        slot.Add(SpellsDef.HellsIngress.GetSpell());
    }
}
