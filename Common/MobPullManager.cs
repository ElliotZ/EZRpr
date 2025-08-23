using AEAssist;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility;
using ElliotZ.Common.ModernJobViewFramework;
using System.Numerics;

namespace ElliotZ.Common;

public class MobPullManager(JobViewWindow qtInstance, string holdQT = "")
{
    private IBattleChara? _currTank;

    private Vector3 _currentPosition = Vector3.Zero;
    private Vector3 _lastPosition = Vector3.Zero;
    private long _lastCheckTime;
    private bool _tankMoving;

    /// <summary>
    /// 主控QT的名字，默认空
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public string HoldQtName { get; set; } = holdQT;

    private bool _holding;
    /// <summary>
    /// 如果不使用主控QT可以通过Holding判断是否施放技能
    /// </summary>
    public bool Holding 
    { 
        get => HoldQtName.IsNullOrEmpty() ? _holding : qtInstance.GetQt(HoldQtName);
        // ReSharper disable once MemberCanBePrivate.Global
        set 
        {
            if (HoldQtName.IsNullOrEmpty())
                _holding = value;
            else
                qtInstance.SetQt(HoldQtName, !value);
        } 
    }

    /// <summary>
    /// 需要自行添加需要控制的QT
    /// </summary>
    public readonly List<string> BurstQTs = [];

    /// <summary>
    /// 当前场景是否适用留爆发功能，以及是否存在Tank的判定
    /// </summary>
    /// <returns></returns>
    private bool CheckTank()
    {
        if (Core.Resolve<MemApiDuty>().InMission && 
            Core.Resolve<MemApiDuty>().DutyMembersNumber() == 4)
        {
            _currTank = PartyHelper.CastableTanks.FirstOrDefault();
        }
        else _currTank = null;

        return _currTank is not null;
    }

    private static uint GetTerritoryId => Core.Resolve<MemApiMap>().GetCurrTerrId();


    // ReSharper disable once UnusedMember.Global
    public void 重置() => Reset();
    /// <summary>
    /// 重置状态，需要在OnPreCombat和OnResetBattle中调用
    /// </summary>
    public void Reset()
    {
        _lastCheckTime = 0L;
        _currentPosition = Vector3.Zero;
        _lastPosition = Vector3.Zero;
        Holding = CheckTank();
    }
    
    // ReSharper disable once UnusedMember.Global
    public void 拉怪中留爆发(int 当前时间, float 集中度) => HoldBurstIfPulling(当前时间, 集中度);
    /// <summary>
    /// 在坦克拉怪过程中留爆发的控制逻辑
    /// </summary>
    /// <param name="currTime">当前战斗持续时间，可以直接把OnBattleUpdate的currTime参数填入</param>
    /// <param name="concentrationThreshold">设定的小怪集中度</param>
    public void HoldBurstIfPulling(int currTime, float concentrationThreshold)
    {
        if (_currTank is null) return;
        
        if (currTime - _lastCheckTime >= 1000)
        {
            CheckTankPosition();
            _lastCheckTime = currTime;
        }

        if (!_tankMoving)
        {
            if (CheckEnemiesAroundTank() > concentrationThreshold)
            {
                SetAllQTs(true);  // also sets HoldingQT to false if possible
            }
        }
        if (Holding && Core.Me.GetCurrTarget() is not null && Core.Me.GetCurrTarget().IsBoss())
        {
            SetAllQTs(true);
        }
    }
    
    // ReSharper disable once UnusedMember.Global
    public void 小怪死亡留爆发(int 当前时间, float 血量阈值, int 死亡时间阈值) 
        => HoldBurstIfMobsDying(当前时间, 血量阈值, 死亡时间阈值);
    /// <summary>
    /// 在一波小怪接近死亡时留爆发的控制逻辑
    /// </summary>
    /// <param name="currTime">当前战斗持续时间，可以直接把OnBattleUpdate的currTime参数填入</param>
    /// <param name="mobHPThreshold">设定的小怪血量阈值</param>
    /// <param name="minTTK">设定的小怪平均死亡时间阈值，用ms计算</param>
    public void HoldBurstIfMobsDying(int currTime, float mobHPThreshold, int minTTK)
    {
        // exclude boss battles, msq ultima wep, and 8 man duties in general
        if (Core.Resolve<MemApiDuty>().InMission &&
               Core.Resolve<MemApiDuty>().DutyMembersNumber() != 8 &&
               !Core.Resolve<MemApiDuty>().InBossBattle &&  
               //!Core.Me.GetCurrTarget().IsDummy() &&
               GetTerritoryId != 1048 &&
               currTime > 10000 &&
               (GetTotalHealthPercentageOfNearbyEnemies() < mobHPThreshold ||
                GetAverageTTKOfNearbyEnemies() < minTTK))
        {
            SetAllQTs(false);
        }
    }

    /// <summary>
    /// 设置所有BurstQTs里面记录的QT。如果设置了总控holdQT则只会设置总控QT。
    /// </summary>
    /// <param name="val"></param>
    private void SetAllQTs(bool val)
    {
        if (HoldQtName.IsNullOrEmpty())
        {
            foreach (var item in BurstQTs) { qtInstance.SetQt(item, val); }
        }
        Holding = !val;
    }

    /// <summary>
    /// 判断Tank周围的敌人密度是否大于设定阈值
    /// </summary>
    private float CheckEnemiesAroundTank()
    {
        var visibleEnemiesIn25 = 0;
        var visibleEnemiesIn5 = 0;
        if (_currTank == null)
        {
            return 0f;
        }

        var enemies = TargetMgr.Instance.Enemys;
        foreach (var item in enemies)
        {
            if (_currTank.Distance(item.Value) <= 25f)
            {
                visibleEnemiesIn25++;
            }

            if (_currTank.Distance(item.Value) <= 5f)
            {
                visibleEnemiesIn5++;
            }
        }

        return ((visibleEnemiesIn25 > 0) ?
                             (visibleEnemiesIn5 / (float)visibleEnemiesIn25) : 0f);
    }

    /// <summary>
    /// 维护TankMoving状态，如果1秒内队伍tank位移超过1.5米则TankMoving为True
    /// </summary>
    private void CheckTankPosition()
    {
        if (_currTank == null) return;
        
        _currentPosition = _currTank.Position;
        var num = Vector3.Distance(_currentPosition, _lastPosition);
        _tankMoving = !(num < 1.5f);
        _lastPosition = _currentPosition;
    }

    /// <summary>
    /// 求25米内敌人的总HP百分比
    /// </summary>
    /// <returns></returns>
    public static float 附近敌人总血量比例() => GetTotalHealthPercentageOfNearbyEnemies();
    public static float GetTotalHealthPercentageOfNearbyEnemies()
    {
        var enemiesIn = TargetMgr.Instance.EnemysIn25;
        var totalMobCurrHp = 0f;
        var totalMobMaxHp = 0f;
        var mobCount = 0;
        foreach (var item 
                 in enemiesIn.Where(item 
                     => !item.Value.IsBoss()))
        {
            totalMobCurrHp += item.Value.CurrentHp;
            totalMobMaxHp += item.Value.MaxHp;
            mobCount++;
        }

        if (mobCount == 0)
        {
            return 0f;
        }

        return totalMobCurrHp / totalMobMaxHp;
    }

    /// <summary>
    /// 求25米内敌人的平均死亡时间
    /// </summary>
    /// <returns></returns>
    public static float 附近敌人平均死亡时间() => GetAverageTTKOfNearbyEnemies();
    public static float GetAverageTTKOfNearbyEnemies()
    {
        var enemiesIn = TargetMgr.Instance.EnemysIn25;
        List<float> ttkList = [];
        var mobCount = 0;
        // 遍历25米内敌人，根据敌人的EntityID把所有大于0的DeathPrediction加起来，跳过boss
        foreach (var value 
                 in enemiesIn.Select(item 
                     => item.Value))
        {
            if (TargetHelper.IsBoss(value) ||
                !TargetMgr.Instance.TargetStats.TryGetValue(value.EntityId, out var value2) ||
                value2.DeathPrediction <= 0) continue;
            ttkList.Add(value2.DeathPrediction);
            mobCount++;
        }

        if (mobCount == 0)
        {
            return 0f;
        }

        // 如果TTKList总数大于5则掐头去尾取平均
        if (ttkList.Count > 4)
        {
            ttkList.Sort();
            ttkList.RemoveAt(0);
            ttkList.RemoveAt(ttkList.Count - 1);
        }

        return ttkList.Average();
    }
}
