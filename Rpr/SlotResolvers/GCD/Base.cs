using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using ElliotZ.Common;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.GCD;

public class Base : ISlotResolver
{
    //private static uint PrevCombo => Core.Resolve<MemApiSpell>().GetLastComboSpellId();
    private const uint 
        St1 = SpellsDef.Slice,
        St2 = SpellsDef.WaxingSlice,
        St3 = SpellsDef.InfernalSlice,
        AOE1 = SpellsDef.SpinningScythe,
        AOE2 = SpellsDef.NightmareScythe;

    public int Check()
    {
        if (Core.Me.Distance(Core.Me.GetCurrTarget()) 
            > Helper.GlblSettings.AttackRange)
        {
            return -2;  // -2 for not in range
        }
        if (Helper.GetActionChange(SpellsDef.BloodStalk).RecentlyUsed() ||
                    SpellsDef.Gluttony.RecentlyUsed())
        {
            return -10;
        }
        return 0;
    }

    private static uint Solve()
    {
        var enemyCount = TargetHelper.GetNearbyEnemyCount(5);

        if (Qt.Instance.GetQt("AOE") &&
            enemyCount >= 3 &&
            AOE1.GetSpell().IsReadyWithCanCast() &&
            RprHelper.PrevCombo != St2 &&
            RprHelper.PrevCombo != St1)
        {
            if (AOE2.GetSpell().IsReadyWithCanCast() && RprHelper.PrevCombo == AOE1) { return AOE2; }
            return AOE1;
        }

        if (St3.GetSpell().IsReadyWithCanCast() && RprHelper.PrevCombo == St2) { return St3; }
        if (St2.GetSpell().IsReadyWithCanCast() && RprHelper.PrevCombo == St1) { return St2; }
        return St1;
    }

    public void Build(Slot slot)
    {
        slot.Add(Solve().GetSpell());
    }
}
