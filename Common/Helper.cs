using System.Numerics;
using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.Define;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;

// ReSharper disable UnusedMember.Local

namespace ElliotZ;

public static class Helper {
  public const string AuthorName = "ElliotZ";

  // some reference urls if needed

  //private static int _GCDDuration = 0;

  /// <summary>
  /// 获取自身buff的剩余时间
  /// </summary>
  /// <param name="buffId"></param>
  /// <returns></returns>
  public static int GetAuraTimeLeft(uint buffId) 
      => Core.Resolve<MemApiBuff>().GetAuraTimeleft(Core.Me, buffId, true);

  public static int GetAuraTimeLeft(IBattleChara c, uint buffId) 
      => Core.Resolve<MemApiBuff>().GetAuraTimeleft(c, buffId, true);

  public static bool TargetIsBoss => Core.Me.GetCurrTarget().IsBoss();

  public static bool TargetIsDummy => Core.Me.GetCurrTarget().IsDummy();

  public static bool TargetIsBossOrDummy => TargetIsBoss || TargetIsDummy;

  /// <summary>显示一个文本提示，用于在游戏中显示简短的消息。</summary>
  /// <param name="msg">要显示的消息文本。</param>
  /// <param name="s">文本提示的样式。支持蓝色提示（1）和红色提示（2）两种</param>
  /// <param name="time">文本提示显示的时间（单位毫秒）。如显示3秒，填写3000即可</param>
  public static void SendTips(string msg, int s = 1, int time = 3000) {
    Core.Resolve<MemApiChatMessage>().Toast2(msg, s, time);
  }

  public static bool IsMoving => MoveHelper.IsMoving();

  /// <summary>
  /// 全局设置
  /// </summary>
  public static GeneralSettings GlobalSettings => SettingMgr.GetSetting<GeneralSettings>();

  /// <summary>
  /// 当前地图id
  /// </summary>
  public static uint GetTerritoryId => Core.Resolve<MemApiMap>().GetCurrTerrId();

  /// <summary>
  /// 返回可变技能的当前id
  /// </summary>
  public static uint AdaptiveId(this uint spellId) => Core.Resolve<MemApiSpell>()
                                                          .CheckActionChange(spellId);

  /// <summary>
  /// 高优先级插入条件检测函数
  /// </summary>
  public static int HighPrioritySlotCheckFunc(SlotMode mode, Slot slot) {
    if (mode != SlotMode.OffGcd) return 1;
    //限制高优先能力技插入，只能在g窗口前半和后半打
    if (GCDHelper.GetGCDCooldown() is > 750 and < 1500) return -1;
    //连续的两个高优先能力技插入，在gcd前半窗口打，以免卡gcd
    if ((slot.Actions.Count > 1) && (GCDHelper.GetGCDCooldown() < 1500)) return -1;
    return 1;
  }

  public static double ComboTimer =>
      Core.Resolve<MemApiSpell>().GetComboTimeLeft().TotalMilliseconds;

  public static bool AtRear => Core.Resolve<MemApiTarget>().IsBehind;
  public static bool AtFlank => Core.Resolve<MemApiTarget>().IsFlanking;

  /// <summary>
  /// 充能技能还有多少冷却时间(ms)才可用
  /// </summary>
  /// <param name="skillId">技能id</param>
  /// <returns></returns>
  public static int ChargeSkillTimer(uint skillId) {
    Spell spell = skillId.GetSpell();
    return (int)(spell.Cooldown.TotalMilliseconds
               - spell.RecastTime.TotalMilliseconds / spell.MaxCharges * (spell.MaxCharges - 1));
  }

  public static bool AnyAuraTimerLessThan(List<uint> auras, int timeLeft) {
    return Core.Me.StatusList.Any(aura => (aura.StatusId != 0)
                                       && (Math.Abs(aura.RemainingTime) * 1000.0 <= timeLeft)
                                       && auras.Contains(aura.StatusId));
  }

  /// <summary>
  /// 自身有buff且时间小于
  /// </summary>
  public static bool AuraTimerLessThan(uint buffId, int timeLeft) {
    if (!Core.Me.HasAura(buffId)) return false;
    return GetAuraTimeLeft(buffId) <= timeLeft;
  }

  public static bool AuraTimerMoreThan(uint buffId, int timeLeft) {
    if (!Core.Me.HasAura(buffId)) return false;
    return GetAuraTimeLeft(buffId) > timeLeft;
  }

  /// <summary>
  /// 目标有buff且时间小于等于，有buff参数如果为false，则当目标没有玩家的buff是也返回true
  /// 以毫秒计算
  /// </summary>
  public static bool TgtAuraTimerLessThan(uint buffId, int timeLeft, bool hasBuff = true) {
    IBattleChara? target = Core.Me.GetCurrTarget();
    if (target is null) return false;

    if (hasBuff) {
      if (!target.HasLocalPlayerAura(buffId)) return false;
    } else {
      if (!target.HasLocalPlayerAura(buffId)) return true;
    }

    int time = Core.Resolve<MemApiBuff>().GetAuraTimeleft(target, buffId, true);
    return time <= timeLeft;
  }

  /// <summary>
  /// 目标有buff且时间大于，有buff参数如果为false，则当目标没有玩家的buff且timeLeft为0也返回true
  /// 以毫秒计算
  /// </summary>
  public static bool TgtAuraTimerMoreThan(uint buffId, int timeLeft, bool hasBuff = true) {
    IBattleChara? target = Core.Me.GetCurrTarget();
    if (target is null) return false;

    if (hasBuff) {
      if (!target.HasLocalPlayerAura(buffId)) return false;
    } else {
      if (!target.HasLocalPlayerAura(buffId) && (timeLeft == 0)) return true;
    }

    int time = Core.Resolve<MemApiBuff>().GetAuraTimeleft(target, buffId, true);
    return time > timeLeft;
  }

  /// <summary>
  /// 周围8米内目标如果有超过三分之二会在设定TTK内死亡，则返回True
  /// </summary>
  /// <returns></returns>
  [Obsolete("not needed since introduction of MobPullManager, use that instead")]
  public static bool AoeTTKCheck() {
    int enemyCount = TargetHelper.GetNearbyEnemyCount(8);
    var enemyList = TargetMgr.Instance.EnemysIn12;
    int lowHpCount = 
        enemyList.Count(v => {
          float dist = Core.Me.Distance(v.Value, 
                                        DistanceMode.IgnoreTargetHitbox 
                                      | DistanceMode.IgnoreHeight);
          return dist <= 8 && TTKHelper.IsTargetTTK(v.Value);
        });
    return lowHpCount / (double)enemyCount > 0.667;
  }

  [Obsolete("not needed since introduction of MobPullManager, use that instead")]
  public static bool AoeTTKCheck(int time) {
    int enemyCount = TargetHelper.GetNearbyEnemyCount(8);
    var enemyList = TargetMgr.Instance.EnemysIn12;
    int lowHpCount = 
        enemyList.Count(v => {
          float dist = Core.Me.Distance(v.Value, 
                                        DistanceMode.IgnoreTargetHitbox 
                                      | DistanceMode.IgnoreHeight);
          return dist <= 8 && TTKHelper.IsTargetTTK(v.Value, time, false);
        });
    return lowHpCount / (double)enemyCount > 0.667;
  }

  public static bool InCasualDutyNonBoss =>
      Core.Resolve<MemApiDuty>().InMission
   && Core.Resolve<MemApiDuty>().DutyMembersNumber() is 4 or 24
   && !TargetIsBossOrDummy
   && !Core.Resolve<MemApiDuty>().InBossBattle;

  /// <summary>
  /// 在list中添加一个唯一的元素
  /// </summary>
  public static bool AddUnique<T>(this List<T> list, T item) {
    if (list.Contains(item)) return false;
    list.Add(item);
    return true;
  }

  public static bool RdyInGCDs(this uint spellId, int numOfGCDs) {
    double gcd = GCDHelper.GetGCDDuration() > 0 ? GCDHelper.GetGCDDuration() : 2500;
    int cdInMilliSecs = (int)Core.Resolve<MemApiSpell>().GetCooldown(spellId).TotalMilliseconds;

    if ((spellId.GetSpell().MaxCharges > 1) && (spellId.GetSpell().Charges >= 1.0f)) {
      cdInMilliSecs = 0;
    }

    return numOfGCDs >= (int)Math.Ceiling(cdInMilliSecs / gcd);
  }

  public static bool TgtHasAuraFromMe(List<uint> buffs) {
    return buffs.Any(buff => Core.Me.GetCurrTarget().HasLocalPlayerAura(buff));
  }

  /// <summary>
  /// 带开关的智能AOE选择器
  /// </summary>
  /// <param name="spellId"></param>
  /// <param name="count"></param>
  /// <param name="toggle">这里可以输入控制智能AOE的QT</param>
  /// <param name="dmgRange">如果toggle有可能为false的话一定要填这个，不然就等死吧</param>
  /// <returns></returns>
  public static IBattleChara? OptimalAOETarget(this uint spellId,
                                               int count,
                                               bool toggle = true,
                                               int dmgRange = 0) {
    if (toggle) return TargetHelper.GetMostCanTargetObjects(spellId, count);
    //var spellDmgRange = Core.Resolve<MemApiSpell>().
    int enemyCount = TargetHelper.GetNearbyEnemyCount(Core.Me.GetCurrTarget(),
                                                      (int)spellId.GetSpell().ActionRange,
                                                      dmgRange);
    return count <= enemyCount ? Core.Me.GetCurrTarget() : null;
  }

  /// <summary>
  /// 带开关的智能AOE选择器，扇形
  /// </summary>
  /// <param name="spellId"></param>
  /// <param name="count"></param>
  /// <param name="angle"></param>
  /// <param name="toggle">这里可以输入控制智能AOE的QT</param>
  /// <returns></returns>
  public static IBattleChara? OptimalAOETarget(this uint spellId,
                                               int count,
                                               float angle,
                                               bool toggle = true) {
    if (toggle) return TargetHelper.GetMostCanTargetObjects(spellId, count, angle);
    int enemyCount = TargetHelper.GetEnemyCountInsideSector(
        Core.Me,
        Core.Me.GetCurrTarget(),
        (int)spellId.GetSpell().ActionRange,
        angle);
    return count <= enemyCount ? Core.Me.GetCurrTarget() : null;
  }

  /// <summary>
  /// 带开关的智能AOE选择器，矩形
  /// </summary>
  /// <param name="spellId"></param>
  /// <param name="count"></param>
  /// <param name="width">如果toggle有可能为false的话一定要填这个，不然就等死吧</param>
  /// <param name="toggle">这里可以输入控制智能AOE的QT</param>
  /// <returns></returns>
  public static IBattleChara? OptimalLineAOETarget(this uint spellId,
                                                   int count,
                                                   bool toggle = true,
                                                   int width = 0) {
    if (toggle) return TargetHelper.GetMostCanTargetObjects(spellId, count);
    int enemyCount = TargetHelper.GetEnemyCountInsideRect(
        Core.Me,
        Core.Me.GetCurrTarget(),
        (int)spellId.GetSpell().ActionRange,
        width);
    return count <= enemyCount ? Core.Me.GetCurrTarget() : null;
  }

  public static float GetRotationToTarget(Vector3 from, Vector3 to) {
    float y = to.X - from.X;
    float x = to.Z - from.Z;
    return MathF.Atan2(y, x);
  }

  public static bool InAnyRaidBuff() {
    //检测目标团辅
    List<uint> tgtDebuff = [_背刺, _连环计];
    //检测自身团辅
    List<uint> selfBuff =
        [_灼热之光, _星空, _占卜, _义结金兰, _战斗连祷, _大舞, _战斗之声, _鼓励, _神秘环];
    return tgtDebuff.Any(buff => AuraTimerLessThan(buff, 15000))
        || selfBuff.Any(buff => TgtAuraTimerLessThan(buff, 15000));
  }

  public static bool IsUnlockWithRoleSkills(this Spell spell) {
    // dirty fix for now; need better ways to detect if a role skill is unlocked
    return SpellsDef.RoleSkills.Contains(spell.Id)
         || spell.IsUnlock();
  }

  /// <summary>
  /// check if hp queue contains spell
  /// </summary>
  /// <param name="spell"></param>
  /// <returns>True if spell is in HP queue, false otherwise</returns>
  public static bool CheckInHPQueue(this Spell spell) {
    if (spell.IsAbility()) {
      var all = HPQueueToStrList(AI.Instance.BattleData.HighPrioritySlots_OffGCD);
      return all.Contains(spell.Name);
    } else {
      var all = HPQueueToStrList(AI.Instance.BattleData.HighPrioritySlots_GCD);
      return all.Contains(spell.Name);
    }
  }

  /// <summary>
  /// check for HP queue top
  /// </summary>
  /// <param name="spell"></param>
  /// <returns>True if spell is at the top of HP queue, false otherwise</returns>
  public static bool CheckInHPQueueTop(this Spell spell) {
    if (spell.IsAbility()) {
      var all = HPQueueToStrList(AI.Instance.BattleData.HighPrioritySlots_OffGCD);
      return all.Count > 0 && all[0] == spell.Name;
    } else {
      var all = HPQueueToStrList(AI.Instance.BattleData.HighPrioritySlots_GCD);
      return all.Count > 0 && all[0] == spell.Name;
    }
  }

  private static List<string> HPQueueToStrList(Queue<Slot> src) {
    List<string> result = [];
    foreach (SlotAction item in src.SelectMany(slot => slot.Actions)) {
      result.Add(item.Spell.Name);
    }
    return result;
  }

  public static string Test() {
    TriggerlineData dat = AI.Instance.TriggerlineData;
    AfterSpellCondParams c1 = dat.CreateAfterSpellParams(SpellsDef.Harpe);
    BattleStartCondParams c2 = dat.CreateBattleStartParams();
    BattleEndCondParams c3 = dat.CreateBattleEndParams();
    AddStatusCondParams c4 = dat.CreateAddStatusParams(AurasDef.Soulsow);
    TargetAbleCondParams c5 = dat.CreateTargetAbleParams(Core.Me, true);
    dat.TriggerCustomCondParams(c1);
    dat.TriggerCustomCondParams(c2);
    dat.TriggerCustomCondParams(c3);
    dat.TriggerCustomCondParams(c4);
    dat.TriggerCustomCondParams(c5);
    return c5.ToString();
  }
  
  private const uint _背刺 = 3849,
                     _强化药 = 49,
                     _灼热之光 = 2703,
                     _星空 = 3685,
                     _占卜 = 1878,
                     _义结金兰 = 1185,
                     _战斗连祷 = 786,
                     _大舞 = 1822,
                     _战斗之声 = 141,
                     _鼓励 = 1239,
                     _神秘环 = 2599,
                     _连环计 = 2617;
}
