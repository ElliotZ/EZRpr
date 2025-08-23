using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using ElliotZ.Common;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.GCD;

public class Harpe : ISlotResolver
{
    public int Check()
    {
        if (SpellsDef.Harpe.GetSpell().IsReadyWithCanCast() is false) { return -99; }
        if (Helper.IsMoving && !RprSettings.Instance.ForceCast) { return -2; }
        if (Qt.Instance.GetQt("勾刃") is false) { return -98; }// Add QT

        if (Core.Me.HasAura(AurasDef.SoulReaver) || Core.Me.HasAura(AurasDef.Executioner))
        {
            return -10;  // -10 for protecting SoulReaver/Executioner
        }
        return 0;
    }

    //private static uint Solve()
    //{
    //    if (Core.Me.HasAura(AurasDef.Soulsow)) { return SpellsDef.HarvestMoon; }
    //    return SpellsDef.Harpe;
    //}

    public void Build(Slot slot)
    {
        slot.Add(SpellsDef.Harpe.GetSpell());
    }
}
