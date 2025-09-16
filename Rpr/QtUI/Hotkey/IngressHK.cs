using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using System.Numerics;

namespace ElliotZ.Rpr.QtUI.Hotkey;

/// <summary>
/// 只使用不卡gcd的强插
/// </summary>
public class IngressHK(int hkType) : // 1 - use current direction, 2 - face target, 3 - face camera
    HotKeyResolver(SpellsDef.HellsIngress, SpellTargetType.Self, false) {
  public const int CurrDir = 1;
  public const int FaceTarget = 2;
  public const int FaceCam = 3;

  public override void Draw(Vector2 size) {
    if (Core.Me.HasAura(AurasDef.RegressReady)) {
      HotkeyHelper.DrawSpellImage(size, _spellId.AdaptiveId());
    } else {
      switch (hkType) {
        case FaceTarget:
          HotkeyHelper.DrawSpellImage(size,
                                      Path.Combine(Share.CurrentDirectory,
                                                   @"Resources/ElliotZ/ingress_t.png"));
          break;

        case FaceCam:
          HotkeyHelper.DrawSpellImage(size,
                                      Path.Combine(Share.CurrentDirectory,
                                                   @"Resources/ElliotZ/ingress_cam.png"));
          break;

        default:
          HotkeyHelper.DrawSpellImage(size, _spellId);
          break;
      }
    }
  }

  public override int Check() {
    //if (HkType == FaceTarget && Core.Me.GetCurrTarget() is null) return -9;
    if (Core.Me.HasAura(AurasDef.RegressReady) && RegressPosition().Equals(Vector3.Zero)) {
      return -8;
    }

    return base.Check();
  }

  protected override async Task Run1(Spell spell, int delay = 0) {
    if (delay > 0) await Coroutine.Instance.WaitAsync(delay);

    if (Core.Me.HasAura(AurasDef.RegressReady)) {
      AI.Instance.BattleData.AddSpell2NextSlot(new Spell(SpellsDef.Regress, RegressPosition()));
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

      AI.Instance.BattleData.AddSpell2NextSlot(SpellsDef.HellsIngress.GetSpell());
    }
  }

  public static Vector3 RegressPosition() {
    Dictionary<uint, IGameObject> dictionary = [];
    Core.Resolve<MemApiTarget>().GetNearbyGameObjects(30f, dictionary);
    return dictionary.Values
                     .Where(e => e.DataId is 1219)
                     .Select(e => e.Position)
                     .FirstOrDefault();
  }
}
