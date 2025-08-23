using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using ElliotZ.Common;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.GCD;

public class GibGall : ISlotResolver
{
    private IBattleChara? Target { get; set; }
    public int Check()
    {
        Target = SpellsDef.Guillotine.OptimalAOETarget(3,
            180,
            Qt.Instance.GetQt("智能AOE"));

        if (SpellsDef.Gluttony.GetSpell().RecentlyUsed()) { return 9; }  // 9 for server acq ignore
        if (Core.Me.HasAura(AurasDef.Enshrouded)) { return -14; }
        if (Target is null &&
                Helper.GetActionChange(SpellsDef.Gibbet)
                    .GetSpell()
                    .IsReadyWithCanCast() is false)
        {
            return -99;
        }
        if (Target is not null && SpellsDef.Guillotine
                .GetSpell(Target)
                .IsReadyWithCanCast() is false)
        {
            return -99;
        }

        return 0;
    }

    private Spell Solve()
    {
        //var enemyCount = TargetHelper.GetEnemyCountInsideSector(Core.Me, Core.Me.GetCurrTarget(), 8, 180);

        if (Qt.Instance.GetQt("AOE") && (Target is not null))
        {
            return Helper.GetActionChange(SpellsDef.Guillotine)
                .GetSpell(Target);
        }
        if (Core.Me.HasAura(AurasDef.EnhancedGallows))
        {
            return Helper.GetActionChange(SpellsDef.Gallows)
                .GetSpell();
        }
        if (Core.Me.HasAura(AurasDef.EnhancedGibbet))
        {
            return Helper.GetActionChange(SpellsDef.Gibbet)
                .GetSpell();
        }
        return Helper.AtRear ? 
            Helper.GetActionChange(SpellsDef.Gallows)
                .GetSpell() : 
            Helper.GetActionChange(SpellsDef.Gibbet)
                .GetSpell();
    }

    public void Build(Slot slot)
    {
        slot.Add(Solve());
    }
}
