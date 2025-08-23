using AEAssist.CombatRoutine;
using ElliotZ.Common;
using ElliotZ.Rpr.QtUI;
using ElliotZ.Rpr.SlotResolvers.FixedSeq;
using ElliotZ.Rpr.Triggers;

namespace ElliotZ.Rpr;

public class RprRotationEntry : IRotationEntry
{
    public string AuthorName { get; set; } = Helper.AuthorName;
    private const Jobs TargetJob = Jobs.Reaper;
    private const AcrType AcrType = AEAssist.CombatRoutine.AcrType.Normal;
    private const int MinLevel = 1;
    private const int MaxLevel = 100;
    private const string Description = "镰刀ACR，日随场景经过测试，高难理论可行" +
                                       "但是未特殊适配，欢迎使用并提出改进建议。";
    public static string SettingsFolderPath = "";
    
    public Rotation Build(string settingFolder)
    {
        SettingsFolderPath = settingFolder;
        RprSettings.Build(SettingsFolderPath);
        GlobalSetting.Build(SettingsFolderPath, "EZRpr", false);
        Qt.Build();
        var rot = new Rotation(RotationPrioSys.SlotResolvers)
        {
            TargetJob = TargetJob,
            AcrType = AcrType,
            MinLevel = MinLevel,
            MaxLevel = MaxLevel,
            Description = Description,
        };
        rot.AddOpener(level => level < 88 ? new OpenerCountDownOnly() : new Opener100());
        rot.SetRotationEventHandler(new EventHandler());
        rot.AddTriggerAction(new TriggerActionQt(), new TriggerActionHotkey());
        rot.AddTriggerCondition(new TriggerCondQt());
        rot.AddCanUseHighPrioritySlotCheck(Helper.HighPrioritySlotCheckFunc);
        rot.AddSlotSequences(new DblEnshPrep());
        return rot;
    }
    
    public IRotationUI GetRotationUI() { return Qt.Instance; }
    public void OnDrawSetting() { }
    public void Dispose() { }
}
