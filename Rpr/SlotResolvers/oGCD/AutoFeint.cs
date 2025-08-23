using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using ElliotZ.Common;

namespace ElliotZ.Rpr.SlotResolvers.oGCD;

public class AutoFeint : ISlotResolver
{
    public int Check()
    {
        if (RprSettings.Instance.AutoFeint is false) { return -1; }
        if (SpellsDef.Feint.GetSpell().IsReadyWithCanCast() is false) { return -99; }
        if (Core.Me.GetCurrTarget() is null ||
                !TargetHelper.targetCastingIsDeathSentenceWithTime(Core.Me.GetCurrTarget(), 3000))
        {
            return -3;
        }
        if (GCDHelper.GetGCDCooldown() < RprSettings.Instance.AnimLock) { return -89; }
        return 0;
    }

    public void Build(Slot slot)
    {
        slot.Add(SpellsDef.Feint.GetSpell());
    }
}
