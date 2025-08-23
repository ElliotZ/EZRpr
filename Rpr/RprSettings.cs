
using AEAssist.Helper;
using AEAssist.IO;
using ElliotZ.Common.ModernJobViewFramework;
// ReSharper disable FieldCanBeMadeReadOnly.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8604 // Possible null reference argument.

namespace ElliotZ.Rpr;

public class RprSettings
{
    public static RprSettings Instance;

    /// <summary>
    /// 配置文件适合放一些一般不会在战斗中随时调整的开关数据
    /// 如果一些开关需要在战斗中调整 或者提供给时间轴操作 那就用QT
    /// 非开关类型的配置都放配置里 比如诗人绝峰能量配置
    /// </summary>

    #region 标准模板代码 可以直接复制后改掉类名即可

    private static string _path;

    public static void Build(string settingPath)
    {
        _path = Path.Combine(settingPath, $"{nameof(RprSettings)}.json");
        if (!File.Exists(_path))
        {
            Instance = new RprSettings();
            Instance.Save();
            return;
        }

        try
        {
            Instance = JsonHelper.FromJson<RprSettings>(File.ReadAllText(_path));
        }
        catch (Exception e)
        {
            Instance = new RprSettings();
            LogHelper.Error(e.ToString());
        }
    }

    public void Save()
    {
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
    //public bool SmartAOE = true;
    public bool CommandWindowOpen = true;
    public bool ShowToast = false;
    public bool Debug = false;

    // Roulette Utility Settings
    public bool NoBurst = true;
    public bool PullingNoBurst = true;
    public bool AutoCrest = false;
    public float CrestPercent = 0.8f;
    public bool AutoSecondWind = true;
    public float SecondWindPercent = 0.8f;
    public bool AutoBloodBath = true;
    public float BloodBathPercent = 0.6f;
    public bool AutoFeint = false;
    public float MinMobHpPercent = 0.1f;
    public float ConcentrationThreshold = 0.75f;
    public int MinTTK = 15;
    public bool HandleStopMechs = true;

    // Opener Settings
    public bool TripleWeavePot = false;
    public int PrepullCastTimeHarpe = 1700;
    public bool PrepullSprint = true;
    public bool PrepullIngress = true;

    //public bool AutoUpdateTimeLines = true;
    //public bool TimeLinesDebug = false;

    // QT设置存档
    public Dictionary<string, bool> QtStates = [];
    public JobViewSave JobViewSave = new()
    {
        CurrentTheme = ModernTheme.ThemePreset.RPR,
        QtLineCount = 3,
        QtUnVisibleList = ["挥割/爪", "暴食", "灵魂割", "祭牲",]
    };
}
