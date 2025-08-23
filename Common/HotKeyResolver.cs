using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.View.JobView;
using AEAssist.Extension;
using AEAssist.Helper;
using ElliotZ.Common.ModernJobViewFramework.HotKey;
using System.Numerics;

namespace ElliotZ.Common;

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
                                bool waitCoolDown = true) : IHotkeyResolver
{
    protected uint SpellId = spellId;
    protected readonly SpellTargetType TargetType = targetType;
    protected readonly bool UseHighPrioritySlot = useHighPrioritySlot;
    protected readonly bool WaitCoolDown = waitCoolDown;

    public HotKeyResolver(Spell spell, HotKeyTarget target) : this(spell.Id, target.SpellTargetType)
    {
    }

    public virtual void Draw(Vector2 size)
    {
        HotkeyHelper.DrawSpellImage(size, Helper.GetActionChange(SpellId));
    }

    public virtual void DrawExternal(Vector2 size, bool isActive)
    {
        var targetSpellId = Helper.GetActionChange(SpellId);
        var spell = targetSpellId.GetSpell(TargetType);

        if (WaitCoolDown && spell.IsUnlockWithRoleSkills())
        {
            if (spell.Cooldown.TotalMilliseconds <= 5000.0)
            {
                if (isActive)
                {
                    HotkeyHelper.DrawActiveState(size);
                }
                else
                {
                    HotkeyHelper.DrawGeneralState(size);
                }
            }
            else
            {
                HotkeyHelper.DrawDisabledState(size);
            }

            HotkeyHelper.DrawCooldownText(spell, size);
            HotkeyHelper.DrawChargeText(spell, size);
        }
        else
        {
            SpellHelper.DrawSpellInfo(spell, size, isActive);
        }
    }

    public virtual int Check()
    {
        var s = Helper.GetActionChange(SpellId).GetSpell(TargetType);
        if (WaitCoolDown && !s.IsUnlockWithRoleSkills()) return -1;
        if (UseHighPrioritySlot && s.CheckInHPQueueTop()) return -3;
        var isReady = WaitCoolDown ? s.Cooldown.TotalMilliseconds <= 5000 : s.IsReadyWithCanCast();
        return isReady ? 0 : -2;
    }

    public virtual void Run()
    {
        var targetSpellId = Helper.GetActionChange(SpellId);
        var spell = targetSpellId.GetSpell(TargetType);
        var cooldown = spell.Cooldown.TotalMilliseconds;

        if (WaitCoolDown && cooldown > 0)
        {
            _ = Run1(spell, (int)cooldown);
        }
        else
        {
            _ = Run1(spell);
        }
    }

    protected virtual async Task Run1(Spell spell, int delay = 0)
    {
        if (delay > 0) await Coroutine.Instance.WaitAsync(delay);

        if (UseHighPrioritySlot &&
                Core.Me.GetCurrTarget() is not null &&
                Core.Me.GetCurrTarget()!.CanAttack() &&
                Core.Me.InCombat())
        {
            var slot = new Slot();
            slot.Add(spell);
            if (spell.IsAbility())
            {
                AI.Instance.BattleData.HighPrioritySlots_OffGCD.Enqueue(slot);
            }
            else
            {
                AI.Instance.BattleData.HighPrioritySlots_GCD.Enqueue(slot);
            }

        }
        else
        {
            AI.Instance.BattleData.AddSpell2NextSlot(spell);
        }
    }
}
