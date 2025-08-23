using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using ElliotZ.Common;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.GCD;

public class PerfectioHighPrio : ISlotResolver
{
    private IBattleChara? Target { get; set; }
    public int Check()
    {
        Target = SpellsDef.Perfectio.OptimalAOETarget(1,
            Qt.Instance.GetQt("智能AOE"),
            5);
        if (Target is null ||
                SpellsDef.Perfectio
                    .GetSpell(Target)
                    .IsReadyWithCanCast() is false)
        {
            return -99;
        }
        if (Qt.Instance.GetQt("完人") is false) { return -98; }
        if (!Qt.Instance.GetQt("倾泻资源") &&
                Helper.GetAuraTimeLeft(AurasDef.PerfectioParata) > 2500)
        {
            return -8;  // -8 for Exiting High Prio state
        }
        return 0;
    }

    public void Build(Slot slot)
    {
        slot.Add(SpellsDef.Perfectio
            .GetSpell(Target!));
    }
}
