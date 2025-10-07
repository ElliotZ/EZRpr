using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.View.JobView.HotkeyResolver;
using AEAssist.Helper;
using ElliotZ.Rpr.QtUI.Hotkey;
using JobViewWindow = ElliotZ.ModernJobViewFramework.JobViewWindow;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ElliotZ.Rpr.QtUI;

public static class Qt {
  public static JobViewWindow Instance { get; private set; }
  public static MacroManager MacroMan;
  public static MobPullManager MobMan;
  private static readonly bool _forceNextSlots = RprSettings.Instance.ForceNextSlotsOnHKs;
  private static Dictionary<string, bool> _currQtStatesDict = RprSettings.Instance.QtStatesCasual;

  private static readonly List<QtInfo> _qtKeys = [
      new("爆发药", "Pot", false, null, ""),
      new("起手药", "OpenerPot", false, null, 
          "开了之后起手序列中会喝药"),
      new("起手", "Opener", true, null, ""),
      new("单魂衣", "SingleShroud", false, null, 
       "不给爆发留魂衣，爆发只会有1个大丰收送的魂衣"),
      new("爆发", "burst", true, BurstQtAction, 
          "神秘环和夜游魂衣的总控"),
      new("神秘环", "ArcaneCircle", true, null, ""),
      new("魂衣", "Enshroud", true, null, ""),
      new("大丰收", "PlenHar", true, null, ""),
      new("印记", "DeathsDesign", true, null, 
       "死亡之影和死亡之涡"),
      new("灵魂割", "SoulSkill", true, null, 
       "灵魂切割以及AOE灵魂钐割"),
      new("挥割/爪", "Bloodstalk", true, null, 
       "隐匿挥割以及派生的缢杀爪/绞决爪，以及AOE束缚挥割"),
      new("暴食", "Gluttony", true, null, ""),
      new("完人", "Perfectio", true, null, ""),
      new("真北", "TrueNorth", true, null, ""),
      new("收获月", "HarvestMoon", true, null, ""),
      new("勾刃", "Harpe", true, null, ""),
      new("AOE", "AOE", true, null, ""),
      new("播魂种", "Soulsow", true, null, ""),
      new("祭牲", "Sacrificium", true, null, ""),
      new("倾泻资源", "Dump", false, null, 
       "会扔收获月，会尽快变身"),
      new("真北优化", "OptiNorth", true, null, 
       "够红条但是身位不对的时候会攒一攒"),
      new("智能AOE", "SmartAOE", true, null, 
       "会自动选择AOE目标，包括暴食团契这类技能"),
      new("自动突进", "AutoIngress", false, null, 
       "只会在跳了之后能打到的时候跳，能用勾刃就不会跳"),
      new("爆发准备", "PreBurst", true, null, 
          "用于控制附体前30秒的印记准备，如果你不知道自己在干嘛的话不要手动开关，建议隐藏"),
  ];

  private static readonly List<HotKeyInfo> _hkResolvers = [
      new("入境", "Ingress", new IngressHK(IngressHK.CurrDir)),
      new("出境", "Egress", new EgressHK(IngressHK.CurrDir)),
      new("入境<t>", "Ingress<t>", new IngressHK(IngressHK.FaceTarget)),
      new("出境<t>", "Egress<t>", new EgressHK(IngressHK.FaceTarget)),
      new("入境<cam>", "Ingress<cam>", new IngressHK(IngressHK.FaceCam)),
      new("出境<cam>", "Egress<cam>", new EgressHK(IngressHK.FaceCam)),
      new("神秘纹", "Crest", new HotKeyResolver(SpellsDef.ArcaneCrest, 
                                             SpellTargetType.Self, 
                                             _forceNextSlots)),
      new("LB", "LB", new HotKeyResolver_LB()),
      new("亲疏", "Armslength", new HotKeyResolver(SpellsDef.ArmsLength, 
                                                 SpellTargetType.Self, 
                                                 _forceNextSlots)),
      new("内丹", "SecondWind", new HotKeyResolver(SpellsDef.SecondWind, 
                                                 SpellTargetType.Self, 
                                                 _forceNextSlots)),
      new("浴血", "BloodBath", new HotKeyResolver(SpellsDef.Bloodbath, 
                                                SpellTargetType.Self, 
                                                _forceNextSlots)),
      new("牵制", "Feint", new HotKeyResolver(SpellsDef.Feint, 
                                            useHighPrioritySlot: _forceNextSlots)),
      new("真北", "TrueNorth", new HotKeyResolver(SpellsDef.TrueNorth, 
                                                SpellTargetType.Self, 
                                                _forceNextSlots)),
      new("播魂种", "Soulsow", new SoulSowHvstMnHK()),
      new("疾跑", "Sprint", new HotKeyResolver_疾跑()),
      new("爆发药", "Pot", new HotKeyResolver_Potion()),
  ];

  public static void SaveQtStates() {
    string[] qtArray = Instance.GetQtArray();

    foreach (string name in qtArray) {
      bool state = Instance.GetQt(name);
      _currQtStatesDict[name] = state;
    }

    RprSettings.Instance.Save();
    LogHelper.Print("QT设置已保存");
  }

  public static void LoadQtStates() {
    foreach (var qtState in _currQtStatesDict) {
      Instance.SetQt(qtState.Key, qtState.Value);
    }

    if (RprSettings.Instance.Debug) LogHelper.Print("QT设置已重载");
  }

  public static void LoadQtStatesNoPot() {
    foreach (var qtState in _currQtStatesDict.Where(qtState => 
                                                  qtState.Key is not 
                                                      ("爆发药" 
                                                    or "智能AOE" 
                                                    or "自动突进"))) {
      Instance.SetQt(qtState.Key, qtState.Value);
    }

    if (RprSettings.Instance.Debug) {
      LogHelper.Print("除爆发药和智能AOE以外QT设置已重载");
    }
  }

  public static void Build() {
    Instance = new JobViewWindow(RprSettings.Instance.JobViewSave,
                                 RprSettings.Instance.Save,
                                 "EZRpr");
    Instance.SetUpdateAction(OnUIUpdate);

    MacroMan = new MacroManager(Instance, "/EZRpr", _qtKeys, _hkResolvers, true);
    MacroMan.BuildCommandList();
    
    MobMan = new MobPullManager(Instance);
    MobMan.BurstQTs.Add("爆发");
    MobMan.BurstQTs.Add("神秘环");
    MobMan.BurstQTs.Add("魂衣");

    //其余tab窗口
    ReadmeTab.Build(Instance);
    SettingTab.Build(Instance);
    DevTab.Build(Instance);
  }

  private static void OnUIUpdate() {
    MacroMan.UseToast2 = RprSettings.Instance.ShowToast;
    Instance.IsHardCoreMode = RprSettings.Instance.IsHardCoreMode;
    _currQtStatesDict = RprSettings.Instance.IsHardCoreMode 
                            ? RprSettings.Instance.QtStatesHardCore 
                            : RprSettings.Instance.QtStatesCasual;

    if (RprSettings.Instance.CommandWindowOpen) {
      MacroMan.DrawCommandWindow(ref RprSettings.Instance.CommandWindowOpen);
    }
  }

  private static void BurstQtAction(bool isSet) {
    Instance.SetQt("神秘环", isSet); 
    Instance.SetQt("魂衣", isSet); 
  }
}
