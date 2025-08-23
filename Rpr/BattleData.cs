using ElliotZ.Common;

namespace ElliotZ.Rpr;

public class BattleData
{
    private static bool _isChange;
    
    public int NumBurstPhases = 0;
    /// <summary>
    /// 用于记录gcd复唱时间
    /// </summary>
    public int GcdDuration = 2500;
    public static BattleData Instance = new();
    
    /// <summary>
    /// 上一个技能是否为神秘环
    /// </summary>
    public bool JustCastAC = false;

    public static void ReBuildSettings()
    {
        if (!_isChange) return;
        
        _isChange = false;
        GlobalSetting.Build(RprRotationEntry.SettingsFolderPath, "EZRpr", true);
        RprSettings.Build(RprRotationEntry.SettingsFolderPath);
    }
}
