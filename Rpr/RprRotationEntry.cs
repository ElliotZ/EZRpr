using AEAssist.CombatRoutine;
using ElliotZ.Rpr.QtUI;
using ElliotZ.Rpr.SlotResolvers.FixedSeq;
using ElliotZ.Rpr.Triggers;

// ReSharper disable ClassNeverInstantiated.Global

namespace ElliotZ.Rpr;

public class RprRotationEntry : IRotationEntry {
  private bool _disposed;
  public string AuthorName { get; set; } = Helper.AuthorName;
  private const Jobs _targetJob = Jobs.Reaper;
  private const AcrType _acrType = AcrType.Normal;
  private const int _minLevel = 1;
  private const int _maxLevel = 100;

  private const string _description = "镰刀ACR，日随场景经过测试，高难理论可行" 
                                    + "但是未特殊适配，欢迎使用并提出改进建议。";

  public static string SettingsFolderPath = "";

  public Rotation Build(string settingFolder) {
    SettingsFolderPath = settingFolder;
    RprSettings.Build(SettingsFolderPath);
    GlobalSetting.Build(SettingsFolderPath, false);
    Qt.Build();
    var rot = new Rotation(RotationPrioSys.SlotResolvers) {
        TargetJob = _targetJob,
        AcrType = _acrType,
        MinLevel = _minLevel,
        MaxLevel = _maxLevel,
        Description = _description,
    };
    rot.AddOpener(level => level < 88 ? new OpenerCountDownOnly() : new Opener100());
    rot.SetRotationEventHandler(new EventHandler());
    rot.AddTriggerAction(new TriggerActionQt(), 
                         new TriggerActionHotkey(), 
                         new TriggerActionHoldCommunio(), 
                         new TriggerActionAcrModeSettings());
    rot.AddTriggerCondition(new TriggerCondQt(),
                            new TriggerCondSoul(),
                            new TriggerCondShroud(),
                            new TriggerCondBluOrb(),
                            new TriggerCondPurpOrb());
    rot.AddCanUseHighPrioritySlotCheck(Helper.HighPrioritySlotCheckFunc);
    rot.AddSlotSequences(new DblEnshPrep());
    return rot;
  }

  public IRotationUI GetRotationUI() {
    return Qt.Instance;
  }

  public void OnDrawSetting() { }

  public void Dispose() {
    Dispose(true);
    GC.SuppressFinalize(this);
  }
  
  protected virtual void Dispose(bool disposing) {
    if (_disposed) {
      return;
    }

    if (disposing) {
      Qt.Instance.Dispose();
    }
    
    _disposed = true;
  }
}
