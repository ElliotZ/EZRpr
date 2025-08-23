using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using ElliotZ.Common;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.oGCD;

public class ArcaneCircle : ISlotResolver
{
    public int Check()
    {
        if (SpellsDef.ArcaneCircle.GetSpell().IsReadyWithCanCast() is false) { return -99; }
        if (Qt.Instance.GetQt("神秘环") is false) { return -98; }

        // mostly unused
        if (Core.Me.GetCurrTarget() is null 
            || Helper.AoeTTKCheck() 
            && TTKHelper.IsTargetTTK(Core.Me.GetCurrTarget()))
        {
            return -16;  // delay for next pack
        }
        if (Qt.MobMan.Holding) return -3;

        if (AI.Instance.BattleData.CurrBattleTimeInMs < 5000 &&
                Math.Abs(SpellsDef.SoulScythe.GetSpell().Charges - 2) < 0.000005)
        {
            return -11;
        }
        if (GCDHelper.GetGCDCooldown() < RprSettings.Instance.AnimLock) return -89;
        return 0;
    }

    public void Build(Slot slot)
    {
        slot.Add(SpellsDef.ArcaneCircle.GetSpell());
    }
}
