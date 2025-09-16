using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.GCD;

public class PlentifulHarvest : ISlotResolver {
  private IBattleChara? _target { get; set; }

  public int Check() {
    _target = SpellsDef.PlentifulHarvest.OptimalLineAOETarget(1, 
                                                              Qt.Instance.GetQt("智能AOE"), 
                                                              4);

    if (_target is null
     || SpellsDef.PlentifulHarvest.GetSpell(_target).IsReadyWithCanCast() is false) {
      return -99;
    }

    if (Qt.Instance.GetQt("大丰收") is false) return -98;
    return 0;
  }

  public void Build(Slot slot) {
    slot.Add(SpellsDef.PlentifulHarvest.GetSpell(_target));
  }
}
