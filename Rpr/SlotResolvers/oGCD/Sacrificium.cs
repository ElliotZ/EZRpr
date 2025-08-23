using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using ElliotZ.Common;
using ElliotZ.Rpr.QtUI;

namespace ElliotZ.Rpr.SlotResolvers.oGCD;

public class Sacrificum : ISlotResolver
{
    private IBattleChara? Target { get; set; }
    public int Check()
    {
        Target = SpellsDef.Sacrificium.OptimalAOETarget(1,
            Qt.Instance.GetQt("智能AOE"),
            5);
        if (Target is null ||
                SpellsDef.Sacrificium
                    .GetSpell(Target)
                    .IsReadyWithCanCast() is false)
        {
            return -99;
        }

        if (Qt.Instance.GetQt("神秘环") &&
                ((!Core.Me.HasAura(AurasDef.ArcaneCircle) &&
                 SpellsDef.ArcaneCircle.GetSpell().Cooldown.TotalMilliseconds <= 10000) ||
                 BattleData.Instance.JustCastAC) &&
                 !Qt.Instance.GetQt("倾泻资源"))
        {
            return -6;  // -6 for delaying for burst prep
        }

        // add QT
        // might need to check for death's design
        if (GCDHelper.GetGCDCooldown() < RprSettings.Instance.AnimLock) return -89;
        return 0;
    }

    public void Build(Slot slot)
    {
        slot.Add(SpellsDef.Sacrificium.GetSpell(Target));
    }
}
