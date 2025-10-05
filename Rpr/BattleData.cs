namespace ElliotZ.Rpr;

public class BattleData {
  public static BattleData Instance = new();
  
  /// <summary>
  /// 用于记录gcd复唱时间
  /// </summary>
  public int GcdDuration = 2500;
  public int NumBurstPhases = 0;
  public int HoldCommunio = 0;
  
  public static bool IsChanged;
  
  public static void RebuildSettings() {
    if (!IsChanged) return;

    IsChanged = false;
    GlobalSetting.Build(RprRotationEntry.SettingsFolderPath, true);
    RprSettings.Build(RprRotationEntry.SettingsFolderPath);
  }
}
