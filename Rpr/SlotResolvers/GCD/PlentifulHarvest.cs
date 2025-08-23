using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using ElliotZ.Common;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.GCD;

public class PlentifulHarvest : ISlotResolver
{
    private IBattleChara? Target { get; set; }
    public int Check()
    {
        Target = SpellsDef.PlentifulHarvest.OptimalLineAOETarget(1,
            Qt.Instance.GetQt("智能AOE"),
            4);
        if (Target is null ||
                SpellsDef.PlentifulHarvest
                    .GetSpell(Target)
                    .IsReadyWithCanCast() is false)
        {
            return -99;
        }
        if (Qt.Instance.GetQt("大丰收") is false) { return -98; }
        return 0;
    }

    public void Build(Slot slot)
    {
        slot.Add(SpellsDef.PlentifulHarvest
            .GetSpell(Target));
    }
}
