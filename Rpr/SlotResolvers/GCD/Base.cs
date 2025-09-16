using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.GCD;

public class Base : ISlotResolver {
  //private static uint PrevCombo => Core.Resolve<MemApiSpell>().GetLastComboSpellId();
  private const uint _st1 = SpellsDef.Slice;
  private const uint _st2 = SpellsDef.WaxingSlice;
  private const uint _st3 = SpellsDef.InfernalSlice;
  private const uint _aoe1 = SpellsDef.SpinningScythe;
  private const uint _aoe2 = SpellsDef.NightmareScythe;

  public int Check() {
    if (Core.Me.Distance(Core.Me.GetCurrTarget()) > Helper.GlobalSettings.AttackRange) {
      return -2; // -2 for not in range
    }

    if (SpellsDef.BloodStalk.AdaptiveId().RecentlyUsed()
     || SpellsDef.Gluttony.RecentlyUsed()) {
      return -10;
    }

    return 0;
  }

  private static uint Solve() {
    int enemyCount = TargetHelper.GetNearbyEnemyCount(5);

    if (Qt.Instance.GetQt("AOE")
     && (enemyCount >= 3)
     && _aoe1.GetSpell().IsReadyWithCanCast()
     && (RprHelper.PrevCombo != _st2)
     && (RprHelper.PrevCombo != _st1)) {
      if (_aoe2.GetSpell().IsReadyWithCanCast() && (RprHelper.PrevCombo == _aoe1)) {
        return _aoe2;
      }

      return _aoe1;
    }

    if (_st3.GetSpell().IsReadyWithCanCast() && (RprHelper.PrevCombo == _st2)) return _st3;
    if (_st2.GetSpell().IsReadyWithCanCast() && (RprHelper.PrevCombo == _st1)) return _st2;
    return _st1;
  }

  public void Build(Slot slot) {
    slot.Add(Solve().GetSpell());
  }
}
