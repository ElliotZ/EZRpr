using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.oGCD;

public class Sacrificum : ISlotResolver {
  private IBattleChara? _target { get; set; }

  public int Check() {
    _target = SpellsDef.Sacrificium.OptimalAOETarget(1, 
                                                     Qt.Instance.GetQt("智能AOE"), 
                                                     5);

    if (_target is null 
     || SpellsDef.Sacrificium
                 .GetSpell(_target)
                 .IsReadyWithCanCast() is false) {
      return -99;
    }

    if (Qt.Instance.GetQt("祭牲") is false) return -98;

    if (Qt.Instance.GetQt("神秘环") && !Qt.Instance.GetQt("倾泻资源")) {
      if (!Core.Me.HasAura(AurasDef.ArcaneCircle)
       && (SpellsDef.ArcaneCircle.GetSpell().Cooldown.TotalMilliseconds <= 10000)) {
        return -6; // delaying for burst prep
      }

      if (Core.Resolve<MemApiSpellCastSuccess>().LastSpell == SpellsDef.ArcaneCircle) return -7;
    }

    // might need to check for death's design
    if (GCDHelper.GetGCDCooldown() < RprSettings.Instance.AnimLock) return -89;
    return 0;
  }

  public void Build(Slot slot) {
    slot.Add(SpellsDef.Sacrificium.GetSpell(_target));
  }
}
