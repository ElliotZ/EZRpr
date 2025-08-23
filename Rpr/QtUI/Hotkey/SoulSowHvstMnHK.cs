using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.Extension;
using AEAssist.Helper;
using ElliotZ.Common;

namespace ElliotZ.Rpr.QtUI.Hotkey;

public class SoulSowHvstMnHK() :
             HotKeyResolver(SpellsDef.Soulsow, SpellTargetType.Target, false, false)
{
    public override int Check()
    {
        var targetSpellId = Helper.GetActionChange(SpellId);
        var s = targetSpellId == SpellsDef.HarvestMoon ?
                        targetSpellId.GetSpell(TargetType) :
                        targetSpellId.GetSpell();
        if (UseHighPrioritySlot && s.CheckInHPQueueTop()) return -3;
        if (Core.Me.CastActionId == SpellsDef.Soulsow) return -4;  // what the fuck are you doing LMAO
        var isReady = !Core.Me.HasAura(AurasDef.Soulsow) || s.IsReadyWithCanCast();
        return isReady ? 0 : -2;
    }

    public override void Run()
    {
        var targetSpellId = Helper.GetActionChange(SpellId);
        var spell = targetSpellId == SpellsDef.HarvestMoon ? 
                        targetSpellId.GetSpell(TargetType) : 
                        targetSpellId.GetSpell();
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
}
