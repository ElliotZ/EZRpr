using AEAssist.Helper;
using AEAssist.IO;
using ElliotZ.ModernJobViewFramework;

// ReSharper disable FieldCanBeMadeReadOnly.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8604 // Possible null reference argument.

namespace ElliotZ.Rpr;

public class RprSettings {
  public static RprSettings Instance;

  /// <summary>
  /// 配置文件适合放一些一般不会在战斗中随时调整的开关数据
  /// 如果一些开关需要在战斗中调整 或者提供给时间轴操作 那就用QT
  /// 非开关类型的配置都放配置里 比如诗人绝峰能量配置
  /// </summary>

  #region 标准模板代码 可以直接复制后改掉类名即可

  private static string _path;

  public static void Build(string settingPath) {
    _path = Path.Combine(settingPath, $"{nameof(RprSettings)}.json");

    if (!File.Exists(_path)) {
      Instance = new RprSettings();
      Instance.Save();
      return;
    }

    try {
      Instance = JsonHelper.FromJson<RprSettings>(File.ReadAllText(_path));
    } catch (Exception e) {
      Instance = new RprSettings();
      LogHelper.Error(e.ToString());
    }
  }

  public void Save() {
    Directory.CreateDirectory(Path.GetDirectoryName(_path));
    File.WriteAllText(_path, JsonHelper.ToJson(this));
  }

  #endregion

  // General Settings
  public int AnimLock = 550;
  public bool ForceCast = false;
  public bool ForceNextSlotsOnHKs = false;
  public bool NoPosDrawInTN = false;
  public int PosDrawStyle = 2;

  public bool RestoreQtSet = true;
  public bool AutoSetCasual = true;
  public bool IsHardCoreMode = false;

  //public bool SmartAOE = true;
  public bool CommandWindowOpen = true;
  public bool ShowToast = false;
  public bool Debug = false;
  public bool TimelineDebug = false;

  // Roulette Utility Settings
  public bool HoldBurstAtDyingPack = true;
  public bool HoldBurstWhenTankPulling = true;
  public bool AutoCrest = false;
  public float CrestPercent = 0.8f;
  public bool AutoSecondWind = true;
  public float SecondWindPercent = 0.8f;
  public bool AutoBloodBath = true;
  public float BloodBathPercent = 0.6f;
  public bool AutoFeint = false;
  public float MinMobHpPercent = 0.1f;
  public float ConcentrationThreshold = 0.75f;
  public int MinTTK = 10;
  public bool HandleStopMechs = true;
  public bool ToggleStopOnModeChange = true;
  public bool AutoDumpResources = true;
  public bool AutoSetSingleShroudInTrashPull = true;

  // Opener Settings
  public bool TripleWeavePot = false;
  public int PrepullCastTimeHarpe = 1700;
  public bool PrepullSprint = true;
  public bool PrepullIngress = true;

  public Dictionary<string, bool> QtStatesHardCore;
  public Dictionary<string, bool> QtStatesCasual;

  private RprSettings() {
    ResetQtStates(true);
    ResetQtStates(false);
  }

  public void ResetQtStates(bool isHardCoreMode) {
    // QT设置存档
    if (isHardCoreMode is true) {
      QtStatesHardCore = new Dictionary<string, bool> {
          ["起手"] = true,
          ["起手药"] = false,
          ["单魂衣"] = false,
          ["爆发"] = true,
          ["神秘环"] = true,
          ["魂衣"] = true,
          ["大丰收"] = true,
          ["印记"] = true,
          ["灵魂割"] = true,
          ["挥割/爪"] = true,
          ["暴食"] = true,
          ["完人"] = true,
          ["真北"] = true,
          ["收获月"] = true,
          ["勾刃"] = true,
          ["AOE"] = false,
          ["播魂种"] = true,
          ["祭牲"] = true,
          ["倾泻资源"] = false,
          ["真北优化"] = true,
          ["智能AOE"] = false,
          ["自动突进"] = false,
          ["爆发准备"] = true,
      };
    } else {
      QtStatesCasual = new Dictionary<string, bool> {
          ["起手"] = true,
          ["起手药"] = false,
          ["单魂衣"] = false,
          ["爆发"] = true,
          ["神秘环"] = true,
          ["魂衣"] = true,
          ["大丰收"] = true,
          ["印记"] = true,
          ["灵魂割"] = true,
          ["挥割/爪"] = true,
          ["暴食"] = true,
          ["完人"] = true,
          ["真北"] = true,
          ["收获月"] = true,
          ["勾刃"] = true,
          ["AOE"] = true,
          ["播魂种"] = true,
          ["祭牲"] = true,
          ["倾泻资源"] = false,
          ["真北优化"] = true,
          ["智能AOE"] = true,
          ["自动突进"] = false,
          ["爆发准备"] = true,
      };
    }
  }

public JobViewSave JobViewSave = new() {
      CurrentTheme = ModernTheme.ThemePreset.RPR,
      QtLineCount = 3,
      QtUnVisibleList = ["挥割/爪", "暴食", "灵魂割", "祭牲", "印记", "爆发准备"],
  };
}
