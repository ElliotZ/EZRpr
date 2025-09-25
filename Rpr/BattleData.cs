namespace ElliotZ.Rpr;

public class BattleData {
  public static BattleData Instance = new();
  
  /// <summary>
  /// 用于记录gcd复唱时间
  /// </summary>
  public int GcdDuration = 2500;
  public int NumBurstPhases = 0;
  
  private static bool _isChange;
  
  public static void ReBuildSettings() {
    if (!_isChange) return;

    _isChange = false;
    GlobalSetting.Build(RprRotationEntry.SettingsFolderPath, true);
    RprSettings.Build(RprRotationEntry.SettingsFolderPath);
  }
}
