using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.GCD;

public class SoulSow : ISlotResolver {
  public int Check() {
    IBattleChara? t = Core.Me.GetCurrTarget();
    return t is not null
        && Qt.Instance.GetQt("播魂种")
        && !Core.Me.HasAura(AurasDef.Soulsow)
        && t.HasAnyAura(StopHelper.Invulns,
                        (int)SpellsDef.Soulsow.GetSpell().CastTime.TotalMilliseconds)
               ? 1
               : -1;
  }

  public void Build(Slot slot) {
    slot.Add(SpellsDef.Soulsow.GetSpell());
  }
}
