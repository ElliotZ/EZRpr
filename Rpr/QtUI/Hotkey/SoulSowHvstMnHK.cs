using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.Extension;
using AEAssist.Helper;

namespace ElliotZ.Rpr.QtUI.Hotkey;

public class SoulSowHvstMnHK()
    : HotKeyResolver(SpellsDef.Soulsow) {
  public override int Check() {
    if (Core.Me.CastActionId == SpellsDef.Soulsow) {
      return -4; // what the fuck are you doing LMAO
    }

    return base.Check();
  }

  public override void Run() {
    uint targetSpellId = _spellId.AdaptiveId();
    Spell spell = targetSpellId == SpellsDef.HarvestMoon
                      ? targetSpellId.GetSpell(_targetType)
                      : targetSpellId.GetSpell();
    double cooldown = spell.Cooldown.TotalMilliseconds - 500;

    if (_waitCoolDown && (cooldown > 0)) {
      _ = Run1(spell, (int)cooldown);
    } else {
      _ = Run1(spell);
    }
  }
}
