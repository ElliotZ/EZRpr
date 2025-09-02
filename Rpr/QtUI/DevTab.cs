using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.GUI;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using ElliotZ.Common;
using ElliotZ.Common.ModernJobViewFramework;
using ImGuiNET;

namespace ElliotZ.Rpr.QtUI;

public static class DevTab
{
    public static void Build(JobViewWindow instance)
    {
        instance.AddTab("Dev", _ =>
        {
            if (ImGui.CollapsingHeader("Dev信息"))
            {
                if (ECHelper.ClientState.LocalPlayer is not null)
                {
                    ImGui.Text($"周围小怪总当前血量百分比: " +
                               $"{MobPullManager.GetTotalHealthPercentageOfNearbyEnemies() * 100f:F2}%");
                    ImGui.Text($"预估周围小怪平均死亡时间: " +
                               $"{MobPullManager.GetAverageTTKOfNearbyEnemies() / 1000f:F2} 秒");
                    ImGui.Text($"上一个连击: {Core.Resolve<MemApiSpell>().GetLastComboSpellId()}");
                    ImGui.Text($"上一个GCD: {Core.Resolve<MemApiSpellCastSuccess>().LastGcd}");
                    ImGui.Text($"上一个oGCD: {Core.Resolve<MemApiSpellCastSuccess>().LastAbility}");
                    ImGui.Text("下一个GCD Slot：" + RotationPrioSys.CheckFirstAvailableSkillGCD());
                    ImGui.Text("下一个oGCD Slot：" + RotationPrioSys.CheckFirstAvailableSkillOffGCD());
                    ImGui.Text("目标距离：" + Core.Me.Distance(Core.Me.GetCurrTarget()));
                    ImGui.Text($"当前地图ID: {Core.Resolve<MemApiZoneInfo>().GetCurrTerrId()} ");
                    ImGui.Text($"角色当前坐标: {Core.Me.Position} ");
                    if (ImGui.Button("CID:" + ECHelper.ClientState.LocalContentId))
                    {
                        ImGui.SetClipboardText(ECHelper.ClientState.LocalContentId.ToString());
                        LogHelper.Print("已复制CID到剪贴板");
                    }
                    Dictionary<uint, IBattleChara> dictionary = [];
                    Core.Resolve<MemApiTarget>().GetNearbyGameObjects(20f, dictionary);
                    dictionary.Remove(Core.Me.EntityId);
                    var text2 = string.Join(", ",
                                               dictionary.Values.Select(character 
                                                   => $"{character.Name}"));
                    ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + 410f);
                    ImGui.Text("周围20m目标: " + text2);
                    var targetGOID = Core.Me.GetCurrTarget() is null ? 
                        "null" : 
                        Core.Me.GetCurrTarget().GameObjectId.ToString();
                    ImGui.Text("Target GameObjectId:" + targetGOID);
                    ImGui.Text("Self Casting Spell ID" + (Core.Me.CastActionId).ToString());
                    ImGui.PopTextWrapPos();
                    ImGui.Text("Qt.mobMan.Holding: " + Qt.MobMan.Holding);
                    ImGui.Text($"自身面向 ({Core.Me.Rotation:F2})");
                }

                ImGuiHelper.Separator();
                if (ImGui.Button("Load Settings"))
                {
                    RprSettings.Build(RprRotationEntry.SettingsFolderPath);
                }
            }

            if (ImGui.CollapsingHeader("插入技能状态"))
            {
                if (ImGui.Button("清除队列"))
                {
                    AI.Instance.BattleData.HighPrioritySlots_OffGCD.Clear();
                    AI.Instance.BattleData.HighPrioritySlots_GCD.Clear();
                }

                ImGui.SameLine();
                if (ImGui.Button("清除一个"))
                {
                    if (AI.Instance.BattleData.HighPrioritySlots_OffGCD.Count != 0)
                    {
                        AI.Instance.BattleData.HighPrioritySlots_OffGCD.Dequeue();
                    }
                    if (AI.Instance.BattleData.HighPrioritySlots_GCD.Count != 0)
                    {
                        AI.Instance.BattleData.HighPrioritySlots_GCD.Dequeue();
                    }
                }

                ImGui.Text("-------能力技-------");
                if (AI.Instance.BattleData.HighPrioritySlots_OffGCD.Count > 0)
                {
                    foreach (var item in
                             AI.Instance.BattleData.HighPrioritySlots_OffGCD.SelectMany(spell 
                                 => spell.Actions))
                    {
                        ImGui.Text(item.Spell.Name);
                    }
                }

                ImGui.Text("-------GCD-------");
                if (AI.Instance.BattleData.HighPrioritySlots_GCD.Count > 0)
                {
                    foreach (var item2 in
                             AI.Instance.BattleData.HighPrioritySlots_GCD.SelectMany(spell 
                                 => spell.Actions))
                    {
                        ImGui.Text(item2.Spell.Name);
                    }
                }
            }

            if (ImGui.CollapsingHeader("更新日志"))
            {
                ImGui.BeginChild("UpdateLog",
                                 new System.Numerics.Vector2(0f, 300f),
                                 border: true,
                                 ImGuiWindowFlags.HorizontalScrollbar);
                ImGui.TextWrapped("更新日志：\n\n" +
                                  "2025/08/03: 初版。\n" +
                                  "2025/08/10: 重构宏命令系统。\n" +
                                  "2025/08/14: 重构，以及加入爆发总控QT。\n" +
                                  "2025/08/16：加入印记QT。\n" +
                                  "2025/08/17: UI重做，感谢HSS老师。\n" +
                                  "2025/08/20: 优化技能和爆发窗口逻辑，轻度重构。\n" +
                                  "2025/08/23: 重构，迁移至新工作流。\n" +
                                  "2025/08/27: 现在在切换地图的时候也会重置QT，如果启用了相关设置。\n" +
                                  "2025/09/02: 修复4分以后的爆发药QT失效问题。");
                ImGui.EndChild();
            }
        });
    }
}
