using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.GCD;

public class HarvestMoonHighPrio : ISlotResolver {
  private IBattleChara? _target { get; set; }

  public int Check() {
    _target = SpellsDef.HarvestMoon.OptimalAOETarget(1, 
                                                     Qt.Instance.GetQt("智能AOE"), 
                                                     5);

    if (_target is null 
     || SpellsDef.HarvestMoon.GetSpell(_target).IsReadyWithCanCast() is false) {
      return -99;
    }

    if (Qt.Instance.GetQt("收获月") is false) return -98;
    if (Qt.Instance.GetQt("倾泻资源") is false) return -1;

    if (Core.Me.HasAura(AurasDef.SoulReaver) || Core.Me.HasAura(AurasDef.Executioner)) {
      return -10; // -10 for protecting SoulReaver/Executioner
    }

    return 0;
  }

  public void Build(Slot slot) {
    slot.Add(SpellsDef.HarvestMoon.GetSpell(_target));
  }
}
