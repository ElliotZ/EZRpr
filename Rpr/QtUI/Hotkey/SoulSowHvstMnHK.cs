using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.Extension;
using AEAssist.Helper;

namespace ElliotZ.Rpr.QtUI.Hotkey;

public class SoulSowHvstMnHK()
    : HotKeyResolver(SpellsDef.Soulsow, SpellTargetType.Target, false, false) {
  public override int Check() {
    uint targetSpellId = _spellId.AdaptiveId();
    Spell s = targetSpellId == SpellsDef.HarvestMoon
                  ? targetSpellId.GetSpell(_targetType)
                  : targetSpellId.GetSpell();

    if (_useHighPrioritySlot && s.CheckInHPQueueTop()) {
      return -3;
    }

    if (Core.Me.CastActionId == SpellsDef.Soulsow) {
      return -4; // what the fuck are you doing LMAO
    }

    bool isReady = !Core.Me.HasAura(AurasDef.Soulsow) || s.IsReadyWithCanCast();
    return isReady ? 0 : -2;
  }

  public override void Run() {
    uint targetSpellId = _spellId.AdaptiveId();
    Spell spell = targetSpellId == SpellsDef.HarvestMoon
                      ? targetSpellId.GetSpell(_targetType)
                      : targetSpellId.GetSpell();
    double cooldown = spell.Cooldown.TotalMilliseconds;

    if (_waitCoolDown && (cooldown > 0)) {
      _ = Run1(spell, (int)cooldown);
    } else {
      _ = Run1(spell);
    }
  }
}
