using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.GCD;

public class GaugeGainCD : ISlotResolver {
  public int Check() {
    if (SpellsDef.SoulSlice.GetSpell().IsReadyWithCanCast() is false) return -99;
    if (Qt.Instance.GetQt("灵魂割") is false) return -98; // -98 for QT toggled off

    if (!Qt.Instance.GetQt("倾泻资源")) {
      if (RprHelper.Soul > 50) return -4; // -4 for Overcapped Resources

      if ((Core.Me.Level < 78) 
       && (RprHelper.Soul == 50) 
       && SpellsDef.Gluttony.CoolDownInGCDs(3)) {
        return -4;
      }
    }

    if (SpellsDef.Enshroud.IsUnlock()
     && Core.Me.HasAura(AurasDef.ArcaneCircle)
     && !Core.Me.HasAura(AurasDef.BloodsownCircle)) {
      return -5;
    }

    if ((Helper.ComboTimer <= GCDHelper.GetGCDDuration() * 2 + 200)
     && ((RprHelper.PrevCombo == SpellsDef.Slice)
      || (RprHelper.PrevCombo == SpellsDef.WaxingSlice))) {
      return -9; // -9 for combo protection
    }

    return 0;
  }

  private static uint Solve() {
    int enemyCount = TargetHelper.GetNearbyEnemyCount(5);

    if (Qt.Instance.GetQt("AOE")
     && SpellsDef.SoulScythe.GetSpell().IsReadyWithCanCast()
     && (enemyCount >= 3)) {
      return SpellsDef.SoulScythe;
    }

    return SpellsDef.SoulSlice;
  }

  public void Build(Slot slot) {
    slot.Add(Solve().GetSpell());
  }
}
