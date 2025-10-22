using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.GCD;

public class Perfectio : ISlotResolver {
  private IBattleChara? _target { get; set; }

  public int Check() {
    _target = SpellsDef.Perfectio.OptimalAOETarget(1, 
                                                   Qt.Instance.GetQt("智能AOE"), 
                                                   5);

    if (_target is null 
     || SpellsDef.Perfectio.GetSpell(_target).IsReadyWithCanCast() is false) {
      return -99;
    }

    if (Qt.Instance.GetQt("完人") is false) return -98;

    if ((Helper.ComboTimer < GCDHelper.GetGCDDuration() + 200)
     && ((RprHelper.PrevCombo == SpellsDef.Slice)
      || (RprHelper.PrevCombo == SpellsDef.WaxingSlice))) {
      return -9; // -9 for combo protection
    }

    return 0;
  }

  public void Build(Slot slot) {
    slot.Add(SpellsDef.Perfectio.GetSpell(_target));
  }
}
