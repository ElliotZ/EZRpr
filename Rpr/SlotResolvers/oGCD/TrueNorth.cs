using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using ElliotZ.Common;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.oGCD;

public class TrueNorth : ISlotResolver
{
    //private static uint currGibbet => Helper.GetActionChange(SpellsDef.Gibbet);
    //private static uint currGallows => Helper.GetActionChange(SpellsDef.Gallows);

    public int Check()
    {
        if (SpellsDef.TrueNorth
                .GetSpell()
                .IsReadyWithCanCast() is false) { return -99; }
        if (Qt.Instance.GetQt("真北") is false) { return -98; }

        if (Core.Me.HasAura(AurasDef.TrueNorth)) { return -5; }  // -5 for avoiding spam
        if (Qt.Instance.GetQt("AOE") 
            && TargetHelper.GetNearbyEnemyCount(8) >= 3)
        {
            return -3;
        }

        if (Core.Me.GetCurrTarget() is null ||
            !Core.Me.GetCurrTarget().HasPositional() ||
            GCDHelper.GetGCDCooldown() >= RprSettings.Instance.AnimLock + 100 ||
            GCDHelper.GetGCDCooldown() < RprSettings.Instance.AnimLock ||
            (!Core.Me.HasAura(AurasDef.SoulReaver) &&
             !Core.Me.HasAura(AurasDef.Executioner))) return -1;
        
        if (Core.Me.HasAura(AurasDef.EnhancedGallows) && !Helper.AtRear) { return 0; }
        if (Core.Me.HasAura(AurasDef.EnhancedGibbet) && !Helper.AtFlank) { return 0; }
        return -1;
    }

    public void Build(Slot slot)
    {
        slot.Add(SpellsDef.TrueNorth.GetSpell());
    }
}
