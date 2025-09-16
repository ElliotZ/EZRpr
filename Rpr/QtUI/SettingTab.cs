using Dalamud.Interface.Colors;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using ElliotZ.ModernJobViewFramework;

namespace ElliotZ.Rpr.QtUI;

public static class SettingTab {
  private static int _8幻药;
  private static int _宝药;
  private static int _2宝药;

  public static void Build(JobViewWindow instance) {
    instance.AddTab("设置", _ => {
      if (ImGui.CollapsingHeader("模式和QT设置", ImGuiTreeNodeFlags.DefaultOpen)) {
        ImGui.Dummy(new Vector2(5, 0));
        ImGui.SameLine();
        ImGui.BeginGroup();
        
        if (ImGui.Button("日随模式", new Vector2(100f, 30f))) {
          RprHelper.CasualMode();
          RprSettings.Instance.Save();
        }

        ImGui.SameLine();
        if (ImGui.Button("高难模式", new Vector2(100f, 30f))) {
          RprHelper.HardCoreMode();
          RprSettings.Instance.Save();
        }

        ImGui.Text("高难模式会把自回设置全部关闭，如果你有需求就自己开。");

        ImGui.Separator();
        ImGui.Checkbox("自动重置QT", ref RprSettings.Instance.RestoreQtSet);
        ImGui.SameLine();
        if (ImGui.Button("记录当前QT设置")) Qt.SaveQtStates();
        ImGui.Text("会从当前记录过的QT设置重置。");
        ImGui.Text("爆发药、智能AOE以及自动突进这几个QT不会被重置。");
        ImGui.Text("日随和高难模式的默认QT值是分开保存的。\n"
                 + "记录按钮会把当前的QT状态保存到当前模式的存档中。");
        ImGui.Checkbox("无时间轴时自动设置日随模式", ref RprSettings.Instance.AutoSetCasual);
        
        ImGui.Separator();
        if (ImGui.Button("重设当前模式默认QT设置")) {
          RprSettings.Instance.ResetQtStates(RprSettings.Instance.AcrMode);
          RprSettings.Instance.Save();
        }
        
        ImGui.EndGroup();
        ImGui.Dummy(new Vector2(0, 10));
      }

      if (ImGui.CollapsingHeader("一般设定")) {
        ImGui.Dummy(new Vector2(5, 0));
        ImGui.SameLine();
        ImGui.BeginGroup();
        ImGui.Text("动画锁长度(ms)");
        ImGui.SetNextItemWidth(200f);
        ImGui.SliderInt("(10-1000)", ref RprSettings.Instance.AnimLock, 10, 1000);
        ImGui.PushStyleColor(ImGuiCol.Text,
                             ImGui.ColorConvertFloat4ToU32(
                                 new Vector4(1f, 1f, 0f, 1f)));
        ImGui.Text("↑没装FuckAnimationLock或者类似插件的建议装一个。");
        ImGui.Text("安装之后，这边应该填写FuckAnimationLock中设置的时间，");
        ImGui.Text("再加上你的ping，并酌情增加10-20ms的余量。");
        ImGui.Text("除了起手的爆发药三插选项以外本ACR不会打出三插。");
        ImGui.PopStyleColor();
        ImGui.Checkbox("读条技能施放忽略移动状态（移动中也会使用）", 
                       ref RprSettings.Instance.ForceCast);
        ImGui.Checkbox("Hotkeys使用强制队列（可能会造成卡GCD）",
                       ref RprSettings.Instance.ForceNextSlotsOnHKs);
        ImGui.Text("设置之后需要保存设置并重新加载ACR生效");
        ImGui.Separator();
        ImGui.Checkbox("真北期间不绘制身位", ref RprSettings.Instance.NoPosDrawInTN);
        string posStyle = RprSettings.Instance.PosDrawStyle switch {
            0 => "不填充",
            1 => "填充70%",
            _ => "填充满",
        };
        ImGui.Text("身位风格设置");
        ImGui.SetNextItemWidth(120f);

        if (ImGui.BeginCombo("选择身位绘制风格", posStyle)) {
          if (ImGui.Selectable("不填充", RprSettings.Instance.PosDrawStyle == 0)) {
            RprSettings.Instance.PosDrawStyle = 0;
            RprSettings.Instance.Save();
          }

          if (ImGui.Selectable("填充70%", RprSettings.Instance.PosDrawStyle == 1)) {
            RprSettings.Instance.PosDrawStyle = 1;
            RprSettings.Instance.Save();
          }

          if (ImGui.Selectable("填充满（默认）", RprSettings.Instance.PosDrawStyle == 2)) {
            RprSettings.Instance.PosDrawStyle = 2;
            RprSettings.Instance.Save();
          }

          ImGui.EndCombo();
        }
        
        ImGui.Separator();
        ImGui.Text("高级设置");
        ImGui.Checkbox("Debug", ref RprSettings.Instance.Debug);
        ImGui.SameLine();
        ImGui.Checkbox("StopHelper Debug", ref StopHelper.Debug);
        ImGui.EndGroup();
        ImGui.Dummy(new Vector2(0, 10));
      }

      if (ImGui.CollapsingHeader("日随QoL设定")) {
        ImGui.Dummy(new Vector2(5, 0));
        ImGui.SameLine();
        ImGui.BeginGroup();
        ImGui.Checkbox("小怪低血量不开爆发", ref RprSettings.Instance.NoBurst);

        if (RprSettings.Instance.NoBurst) {
          ImGui.Text("小于设定数会关闭夜游魂衣和神秘环QT。");
          ImGui.Text("如果设置了QT重载，脱战会自动开启。");
          ImGui.SetNextItemWidth(200f);
          ImGui.SliderFloat("平均血量阈值(0-0.2)",
                            ref RprSettings.Instance.MinMobHpPercent,
                            0f,
                            0.2f);
          ImGui.SetNextItemWidth(200f);
          ImGui.SliderInt("平均死亡时间（秒）(0-20)",
                          ref RprSettings.Instance.MinTTK,
                          0,
                          20);
          ImGui.Separator();
        }

        ImGui.Checkbox("坦克拉怪中留CD技能", ref RprSettings.Instance.PullingNoBurst);

        if (RprSettings.Instance.PullingNoBurst) {
          ImGui.Text("小怪集中度");
          ImGui.SetNextItemWidth(200f);
          ImGui.SliderFloat("(0-1.0)",
                            ref RprSettings.Instance.ConcentrationThreshold,
                            0,
                            1f);
          ImGui.PushStyleColor(ImGuiCol.Text,
                               ImGui.ColorConvertFloat4ToU32(
                                   ImGuiColors.ParsedGold));
          ImGui.Text("原理解析：");
          ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + 410f);
          ImGui.Text("每秒检测T移动的距离，如果小于设定好的阈值(1.5m)，");
          ImGui.Text("则视为T已拉到位。接下来比较T周围25m内与5m内的小怪数量，计算集中度，");
          ImGui.Text("再拿这个集中度与设定好的阈值对比，如果大于阈值才开启爆发。");
          ImGui.Text("影响技能：死亡之影（涡），神秘环，夜游魂衣，暴食。");
          ImGui.Text("实验性功能测试不一定稳定，如果遇到问题请手动调整集中度阈值。");
          ImGui.PopStyleColor();
          ImGui.PopTextWrapPos();
        }

        ImGui.Checkbox("加速度炸弹/热病/目标无敌/自身无法行动期间自动停手",
                       ref RprSettings.Instance.HandleStopMechs);

        if (ImGui.CollapsingHeader("自动回复/减伤设置")) {
          ImGui.Text("血量阈值是最大血量的比例。");
          ImGui.Checkbox("自动神秘纹", ref RprSettings.Instance.AutoCrest);

          if (RprSettings.Instance.AutoCrest) {
            ImGui.SetNextItemWidth(200f);
            ImGui.SliderFloat("自动神秘纹血量阈值 (0.10-1.00)",
                              ref RprSettings.Instance.CrestPercent,
                              0.1f,
                              1.0f);
            ImGui.Separator();
          }

          ImGui.Checkbox("自动内丹", ref RprSettings.Instance.AutoSecondWind);

          if (RprSettings.Instance.AutoSecondWind) {
            ImGui.SetNextItemWidth(200f);
            ImGui.SliderFloat("自动内丹血量阈值 (0.10-0.99)",
                              ref RprSettings.Instance.SecondWindPercent,
                              0.1f,
                              0.99f);
            ImGui.Separator();

            if (!RprSettings.Instance.JobViewSave.HotkeyUnVisibleList
                            .Contains("内丹")) {
              RprSettings.Instance.JobViewSave.HotkeyUnVisibleList.Add("内丹");
            }
          } else {
            RprSettings.Instance.JobViewSave.HotkeyUnVisibleList.Remove("内丹");
          }

          ImGui.Checkbox("自动浴血", ref RprSettings.Instance.AutoBloodBath);

          if (RprSettings.Instance.AutoBloodBath) {
            ImGui.SetNextItemWidth(200f);
            ImGui.SliderFloat("自动浴血血量阈值 (0.10-0.99)",
                              ref RprSettings.Instance.BloodBathPercent,
                              0.1f,
                              0.99f);
            ImGui.Separator();

            if (!RprSettings.Instance.JobViewSave.HotkeyUnVisibleList
                            .Contains("浴血")) {
              RprSettings.Instance.JobViewSave.HotkeyUnVisibleList.Add("浴血");
            }
          } else {
            RprSettings.Instance.JobViewSave.HotkeyUnVisibleList.Remove("浴血");
          }

          ImGui.Checkbox("自动牵制", ref RprSettings.Instance.AutoFeint);

          if (RprSettings.Instance.AutoFeint) {
            if (!RprSettings.Instance.JobViewSave.HotkeyUnVisibleList
                            .Contains("牵制")) {
              RprSettings.Instance.JobViewSave.HotkeyUnVisibleList.Add("牵制");
            }
          } else {
            RprSettings.Instance.JobViewSave.HotkeyUnVisibleList.Remove("牵制");
          }
        }

        ImGui.EndGroup();
        ImGui.Dummy(new Vector2(0, 10));
      }

      if (ImGui.CollapsingHeader("起手设定")) {
        ImGui.Dummy(new Vector2(5, 0));
        ImGui.SameLine();
        ImGui.BeginGroup();
        ImGui.Checkbox("起手三插爆发药", ref RprSettings.Instance.TripleWeavePot);
        ImGui.BeginGroup();
        ImGui.Checkbox("倒计时疾跑", ref RprSettings.Instance.PrepullSprint);
        ImGui.SameLine();
        ImGui.Checkbox("起手突进", ref RprSettings.Instance.PrepullIngress);
        ImGui.EndGroup();
        ImGui.Text("倒数勾刃读条时间(ms)");
        ImGui.SetNextItemWidth(200f);
        ImGui.SliderInt("(100-2000)",
                        ref RprSettings.Instance.PrepullCastTimeHarpe,
                        100,
                        2000);
        ImGui.Separator();

        if (ImGui.Button("获取爆发药情况")) {
          _8幻药 = CItemHelper.FindItem((uint)Potion._8级刚力之幻药);
          _宝药 = CItemHelper.FindItem((uint)Potion.刚力之宝药);
          _2宝药 = CItemHelper.FindItem((uint)Potion._2级刚力之宝药);
        }

        if (_8幻药 > 0) {
          ImGui.Text($"8级刚力之幻药：{_8幻药} 瓶");
          DrawPotion(Potion._8级刚力之幻药);
        }

        if (_宝药 > 0) {
          ImGui.Text($"刚力之宝药：{_宝药} 瓶");
          DrawPotion(Potion.刚力之宝药);
        }

        if (_2宝药 > 0) {
          ImGui.Text($"2级刚力之宝药：{_2宝药} 瓶");
          DrawPotion(Potion._2级刚力之宝药);
        }

        ImGui.EndGroup();
        ImGui.Dummy(new Vector2(0, 10));
      }

      if (ImGui.CollapsingHeader("宏指令操作HotKey和QT")) {
        if (ImGui.Button("查看指令")) RprSettings.Instance.CommandWindowOpen = true;
        ImGui.Checkbox("使用Toast2提示QT状态", ref RprSettings.Instance.ShowToast);
      }
    });
  }

  private static void DrawPotion(Potion potion) {
    int id = (int)potion;
    ImGui.SameLine();

    if (ImGui.Button("复制id###药水" + id)) {
      ImGui.SetClipboardText(id.ToString());
    }
  }
}
