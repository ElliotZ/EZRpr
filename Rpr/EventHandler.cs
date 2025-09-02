using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.View;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.JobApi;
using AEAssist.MemoryApi;
using ElliotZ.Common;
using ElliotZ.Rpr.QtUI;
using Task = System.Threading.Tasks.Task;

namespace ElliotZ.Rpr;

public class EventHandler : IRotationEventHandler
{
    public void OnResetBattle()
    {
        BattleData.Instance = new BattleData();

        // initialize pull record
        if (AI.Instance.BattleData.CurrBattleTimeInMs >= 0)
        {
            if (RprSettings.Instance.PullingNoBurst)
            {
                Qt.MobMan.Reset();
            }
            //MeleePosHelper.Clear();
            if (RprSettings.Instance.RestoreQtSet)
            {
                Qt.LoadQtStatesNoPot();
            }
        }
        //BattleData.Instance.IsPulling = false;
        MeleePosHelper.Clear();

        BattleData.ReBuildSettings();
    }

    public async Task OnNoTarget()
    {
        // maybe add soulsow, idk
        if (RprSettings.Instance.HandleStopMechs) StopHelper.StopActions(1000);

        if (RprSettings.Instance.Debug) LogHelper.Print("no target");

        await Task.CompletedTask;
    }

    public void OnSpellCastSuccess(Slot slot, Spell spell)
    {
    }

    public async Task OnPreCombat()
    {
        if (RprSettings.Instance.PullingNoBurst)
        {
            Qt.MobMan.Reset();
        }
        StopHelper.StopActions(1000);

        // out of combat soulsow
        if (SpellsDef.Soulsow.GetSpell().IsReadyWithCanCast() && Qt.Instance.GetQt("播魂种"))
        {
            await SpellsDef.Soulsow.GetSpell().Cast();
        }
    }

    public void AfterSpell(Slot slot, Spell spell)
    {
        //记录复唱时间
        var d = Core.Resolve<MemApiSpell>().GetGCDDuration(true);
        if (d > 0) BattleData.Instance.GcdDuration = d;

        //Single Weave Skills
        if (spell.Id is SpellsDef.VoidReaping or SpellsDef.CrossReaping)
            AI.Instance.BattleData.CurrGcdAbilityCount = 1;

        BattleData.Instance.JustCastAC = spell.Id is SpellsDef.ArcaneCircle;
    }

    public void OnBattleUpdate(int currTime)
    {
        //stop casting soulsow if just entered combat
        if (currTime < 3000 && Core.Me.CastActionId == SpellsDef.Soulsow)
        {
            Core.Resolve<MemApiSpell>().CancelCast();
        }

        if (RprSettings.Instance.PullingNoBurst)
        {
            Qt.MobMan.HoldBurstIfPulling(currTime, RprSettings.Instance.ConcentrationThreshold);
        }

        if (RprSettings.Instance.NoBurst)
        {
            Qt.MobMan.HoldBurstIfMobsDying(currTime, 
                                           RprSettings.Instance.MinMobHpPercent, 
                                           RprSettings.Instance.MinTTK * 1000);
        }

        // stop action during accel bombs, pyretics and/or when boss is invuln
        if (RprSettings.Instance.HandleStopMechs) StopHelper.StopActions(1000);

        // positional indicator
        var gcdProgPctg = (int)((GCDHelper.GetGCDCooldown() / (double)BattleData.Instance.GcdDuration) * 100);
        var inTN = Core.Me.HasAura(AurasDef.TrueNorth) &&
                   !RprSettings.Instance.NoPosDrawInTN;
        var gibGallowsReady = Core.Me.HasAura(AurasDef.SoulReaver) ||
                              Core.Me.HasAura(AurasDef.Executioner);
        var gibGallowsJustUsed =
                Helper.GetActionChange(SpellsDef.Gibbet).RecentlyUsed(500) ||
                Helper.GetActionChange(SpellsDef.Gallows).RecentlyUsed(500);
        var staticPosProg = RprSettings.Instance.PosDrawStyle switch
        {
            0 => 1,
            1 => 70,
            _ => 100,
        };

        if (!inTN &&
                (!Qt.Instance.GetQt("AOE") || 
                    TargetHelper.GetNearbyEnemyCount(8) < 3) &&
                !Core.Me.HasAura(AurasDef.Enshrouded) &&
                (Core.Me.GetCurrTarget() is not null &&
                 Core.Me.GetCurrTarget().HasPositional()))
        {
            if (gibGallowsReady && !gibGallowsJustUsed)
            {
                if (Core.Me.HasAura(AurasDef.EnhancedGallows))
                {
                    MeleePosHelper.Draw(MeleePosHelper.Pos.Behind, gcdProgPctg);
                }
                else if (Core.Me.HasAura(AurasDef.EnhancedGibbet))
                {
                    MeleePosHelper.Draw(MeleePosHelper.Pos.Flank, gcdProgPctg);
                }
                else { MeleePosHelper.Clear(); }
            }
            else if (Core.Resolve<JobApi_Reaper>().SoulGauge >= 50 ||
                        SpellsDef.SoulSlice.GetSpell().IsReadyWithCanCast())
            {
                if (Core.Me.HasAura(AurasDef.EnhancedGallows))
                {
                    MeleePosHelper.Draw(MeleePosHelper.Pos.Behind, staticPosProg);
                }
                else if (Core.Me.HasAura(AurasDef.EnhancedGibbet))
                {
                    MeleePosHelper.Draw(MeleePosHelper.Pos.Flank, staticPosProg);
                }
                else { MeleePosHelper.Clear(); }
            }
            else { MeleePosHelper.Clear(); }
        }
        else { MeleePosHelper.Clear(); }
    }

    public void OnEnterRotation() //切换到当前ACR
    {
        Helper.SendTips("欢迎使用EZRpr，使用前请把左上角悬浮窗拉大查看README。");
        LogHelper.Print("欢迎使用EZRpr，如有问题和反馈可以在DC找我。");

        //检查全局设置
        if (Helper.GlblSettings.NoClipGCD3)
            LogHelper.PrintError("建议在acr全局设置中取消勾选【全局能力技不卡GCD】选项");

        Qt.MacroMan.Init();
    }

    public void OnExitRotation() //退出ACR
    {
        Qt.MacroMan.Exit();
    }

    public void OnTerritoryChanged()
    {
        if (RprSettings.Instance.RestoreQtSet)
        {
            Qt.LoadQtStatesNoPot();
        }
    }
}