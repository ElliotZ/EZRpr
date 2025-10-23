using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using System.Numerics;

namespace ElliotZ.Rpr.QtUI.Hotkey;

public class EgressHK(int hkType)
    : HotKeyResolver(SpellsDef.HellsEgress, SpellTargetType.Self, false, false) {
  public override void Draw(Vector2 size) {
    if (Core.Me.HasAura(AurasDef.RegressReady)) {
      HotkeyHelper.DrawSpellImage(size, _spellId.AdaptiveId());
    } else {
      switch (hkType) {
        case IngressHK.FaceTarget:
          HotkeyHelper.DrawSpellImage(size,
                                      Path.Combine(Share.CurrentDirectory,
                                                   @"Resources/ElliotZ/egress_t.png"));
          break;

        case IngressHK.FaceCam:
          HotkeyHelper.DrawSpellImage(size,
                                      Path.Combine(Share.CurrentDirectory,
                                                   @"Resources/ElliotZ/egress_cam.png"));
          break;

        default:
          HotkeyHelper.DrawSpellImage(size, _spellId);
          break;
      }
    }
  }
  
  public override void DrawExternal(Vector2 size, bool isActive) {
    Spell spell = _spellId.AdaptiveId().GetSpell(_targetType);

    if (_spellId.AdaptiveId() == SpellsDef.Regress) {
      if (isActive) {
        HotkeyHelper.DrawActiveState(size);
      } else {
        HotkeyHelper.DrawGeneralState(size);
      }

      HotkeyHelper.DrawCooldownText(spell, size);
      HotkeyHelper.DrawChargeText(spell, size);
    } else {
      SpellHelper.DrawSpellInfo(spell, size, isActive);
    }
  }

  public override int Check() {
    //if (HkType == IngressHK.FaceTarget && Core.Me.GetCurrTarget() is null) return -9;
    if (_spellId.AdaptiveId() != SpellsDef.Regress) return base.Check();
    return IngressHK.RegressPosition().Equals(Vector3.Zero) ? -8 : 0;
  }

  protected override async Task Run1(Spell spell, int delay = 0) {
    if (delay > 0) await Coroutine.Instance.WaitAsync(delay);

    if (Core.Me.HasAura(AurasDef.RegressReady)) {
      AI.Instance.BattleData.AddSpell2NextSlot(new Spell(SpellsDef.Regress,
                                                         IngressHK.RegressPosition()));
    } else {
      switch (hkType) {
        case 2:
          if (Core.Me.GetCurrTarget() is not null) {
            Core.Resolve<MemApiMoveControl>().Stop();
            Core.Resolve<MemApiMove>()
                .SetRot(Helper.GetRotationToTarget(Core.Me.Position,
                                                   Core.Me.GetCurrTarget().Position));
          }

          break;

        case 3:
          Core.Resolve<MemApiMoveControl>().Stop();
          Core.Resolve<MemApiMove>().SetRot(CameraHelper.GetCameraRotation());
          break;
      }

      AI.Instance.BattleData.AddSpell2NextSlot(SpellsDef.HellsEgress.GetSpell());
    }
  }
}
