using System.Numerics;
using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.View.JobView;
using AEAssist.Extension;
using AEAssist.Helper;
using ElliotZ.ModernJobViewFramework.HotKey;

namespace ElliotZ;

/// <summary>
/// 
/// </summary>
/// <param name="spellId"></param>
/// <param name="targetType"></param>
/// <param name="useHighPrioritySlot">使用不卡GCD的强插</param>
/// <param name="waitCoolDown">是否允许提早5秒点HK</param>
public class HotKeyResolver(uint spellId,
                            SpellTargetType targetType = SpellTargetType.Target,
                            bool useHighPrioritySlot = true,
                            bool waitCoolDown = true) : IHotkeyResolver {
  protected uint _spellId = spellId;
  protected readonly SpellTargetType _targetType = targetType;
  protected readonly bool _useHighPrioritySlot = useHighPrioritySlot;
  protected readonly bool _waitCoolDown = waitCoolDown;

  public HotKeyResolver(Spell spell, HotKeyTarget target) :
      this(spell.Id, target.SpellTargetType) { }

  public virtual void Draw(Vector2 size) {
    HotkeyHelper.DrawSpellImage(size, _spellId.AdaptiveId());
  }

  public virtual void DrawExternal(Vector2 size, bool isActive) {
    uint targetSpellId = _spellId.AdaptiveId();
    Spell spell = targetSpellId.GetSpell(_targetType);

    if (_waitCoolDown && spell.IsUnlockWithRoleSkills()) {
      if (spell.Cooldown.TotalMilliseconds <= 5000.0) {
        if (isActive) {
          HotkeyHelper.DrawActiveState(size);
        } else {
          HotkeyHelper.DrawGeneralState(size);
        }
      } else {
        HotkeyHelper.DrawDisabledState(size);
      }

      HotkeyHelper.DrawCooldownText(spell, size);
      HotkeyHelper.DrawChargeText(spell, size);
    } else {
      SpellHelper.DrawSpellInfo(spell, size, isActive);
    }
  }

  public virtual int Check() {
    Spell s = _spellId.AdaptiveId().GetSpell(_targetType);
    if (_waitCoolDown && !s.IsUnlockWithRoleSkills()) return -1;
    if (_useHighPrioritySlot && s.CheckInHPQueueTop()) return -3;
    bool isReady = _waitCoolDown 
                       ? s.Cooldown.TotalMilliseconds <= 5000 
                       : s.IsReadyWithCanCast();
    return isReady ? 0 : -2;
  }

  public virtual void Run() {
    uint targetSpellId = _spellId.AdaptiveId();
    Spell spell = targetSpellId.GetSpell(_targetType);
    double cooldown = spell.Cooldown.TotalMilliseconds;

    if (_waitCoolDown && (cooldown > 0)) {
      _ = Run1(spell, (int)cooldown);
    } else {
      _ = Run1(spell);
    }
  }

  protected virtual async Task Run1(Spell spell, int delay = 0) {
    if (delay > 0) await Coroutine.Instance.WaitAsync(delay);

    if (_useHighPrioritySlot
     && Core.Me.GetCurrTarget() is not null
     && Core.Me.GetCurrTarget().CanAttack()
     && Core.Me.InCombat()) {
      var slot = new Slot();
      slot.Add(spell);

      if (spell.IsAbility()) {
        AI.Instance.BattleData.HighPrioritySlots_OffGCD.Enqueue(slot);
      } else {
        AI.Instance.BattleData.HighPrioritySlots_GCD.Enqueue(slot);
      }
    } else {
      AI.Instance.BattleData.AddSpell2NextSlot(spell);
    }
  }
}
