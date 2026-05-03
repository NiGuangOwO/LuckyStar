using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ECommons.Automation;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace LuckyStar;

public unsafe class FarmController : IDisposable
{
    public string State { get; private set; } = "未运行";
    public bool IsRunning { get; private set; } = false;
    public int DataIndex { get; private set; } = 0;
    public double PosDistance { get; private set; } = 0;
    public bool WaitingFirst { get; private set; } = false;
    public bool NeedToTakeOff { get; private set; } = true;
    public bool ReadyToTheNextPos { get; private set; } = true;
    public List<(float X, float Y, float Z)> CurrentList { get; private set; } = [];

    private long throttleTime = 0;

    public FarmController()
    {
        Svc.Framework.Update += OnUpdate;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Svc.Framework.Update -= OnUpdate;
    }

    public void Start(List<(float X, float Y, float Z)> points)
    {
        Reset();
        CurrentList = GetShortestPath(points);
        TurnOnAE();
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
        VnavmeshStop();
        TurnOffAE();
        Reset();
    }

    public void ForceSkipFirst()
    {
        DataIndex++;
        WaitingFirst = false;
        NeedToTakeOff = true;
        ReadyToTheNextPos = true;
    }

    private void Reset()
    {
        CurrentList.Clear();
        WaitingFirst = false;
        throttleTime = 0;
        DataIndex = 0;
        NeedToTakeOff = true;
        ReadyToTheNextPos = true;
    }

    private void OnUpdate(IFramework framework)
    {
        if (!IsRunning)
        {
            State = "未运行";
            return;
        }

        if (NeedToTakeOff)
        {
            if (!Svc.Condition[ConditionFlag.Mounted])
            {
                State = "上坐骑";
                Mount();
            }
            else if (!Svc.Condition[ConditionFlag.InFlight])
            {
                State = "起飞";
                Takeoff();
            }
            else
            {
                NeedToTakeOff = false;
            }
            return; // 起飞流程未完成时不执行寻路
        }

        Run();
    }

    private void Run()
    {
        if (DataIndex >= CurrentList.Count)
        {
            DataIndex = 0;
            WaitingFirst = true;
            return;
        }

        var target = CurrentList[DataIndex];

        if (Svc.Condition[ConditionFlag.InFlight] && ReadyToTheNextPos)
        {
            State = $"寻路至下一只小怪 ({DataIndex + 1}/{CurrentList.Count})";
            ReadyToTheNextPos = false;
            VnavmeshStop();
            Flyto(target.X, target.Y, target.Z);
            return;
        }

        // XZ 平面距离（忽略 Y 轴高度差，用于判断水平接近程度）
        var xzDist = Math.Sqrt(
            Math.Pow(target.X - Svc.Objects.LocalPlayer!.Position.X, 2) +
            Math.Pow(target.Z - Svc.Objects.LocalPlayer!.Position.Z, 2));
        PosDistance = xzDist;

        if (!ReadyToTheNextPos && xzDist < 5)
        {
            var hasMob = Svc.Objects.OfType<IBattleChara>()
                .Any(b => MobsData.Nameid.Contains(b.NameId)
                       && !b.IsDead
                       && Vector3.Distance(Svc.Objects.LocalPlayer?.Position ?? Vector3.Zero, b.Position) <= 25);

            if (hasMob)
            {
                HandleMobFound(xzDist);
            }
            else
            {
                HandleMobNotFound();
            }
        }
    }

    private void HandleMobFound(double xzDist)
    {
        if (WaitingFirst)
        {
            if (Plugin.Configuration.DelayEnable)
            {
                if (throttleTime == 0)
                    throttleTime = Environment.TickCount64 + (Plugin.Configuration.DelayTime * 1000);

                if (Environment.TickCount64 > throttleTime)
                {
                    TryDismountIfClose(xzDist);
                    State = "等待击杀当前小怪";
                    throttleTime = 0;
                    WaitingFirst = false;
                }
                else
                {
                    State = $"第一只小怪已刷新，延迟剩余 {throttleTime - Environment.TickCount64}ms";
                }
            }
            else
            {
                TryDismountIfClose(xzDist);
                State = "等待击杀当前小怪";
                throttleTime = 0;
                WaitingFirst = false;
            }
        }
        else
        {
            TryDismountIfClose(xzDist);
            State = "等待击杀当前小怪";
            throttleTime = 0;
        }
    }

    private void HandleMobNotFound()
    {
        if (DataIndex == 0 && WaitingFirst)
        {
            State = "等待第一只刷新";
            return;
        }

        if (DataIndex >= CurrentList.Count)
        {
            DataIndex = 0;
            WaitingFirst = true;
        }
        else
        {
            DataIndex++;
        }

        ReadyToTheNextPos = true;
        NeedToTakeOff = true;
    }

    private void TryDismountIfClose(double xzDist)
    {
        if (Svc.Condition[ConditionFlag.Mounted] && xzDist < 3)
        {
            VnavmeshStop();
            Dismount();
        }
    }

    // ---- 坐骑 / 寻路 / AE 控制 ----

    private void Mount()
    {
        ActionManager.Instance()->UseAction(ActionType.GeneralAction, 9);
    }

    private void Dismount()
    {
        ActionManager.Instance()->UseAction(ActionType.GeneralAction, 23);
    }

    private void Takeoff()
    {
        ActionManager.Instance()->UseAction(ActionType.GeneralAction, 2);
        NeedToTakeOff = false;
    }

    private void Flyto(float x, float y, float z)
    {
        if (!Svc.Condition[ConditionFlag.InFlight])
        {
            NeedToTakeOff = true;
        }
        else
        {
            Chat.ExecuteCommand($"/vnav flyto {x} {y} {z}");
        }
    }

    public static void VnavmeshStop()
    {
        Chat.ExecuteCommand("/vnav stop");
    }

    public static void TurnOnAE()
    {
        Chat.ExecuteCommand("/aeTargetSelector on");
        Chat.ExecuteCommand("/aeTargetSelector mode7");
        Chat.ExecuteCommand("/aepull on");
    }

    public static void TurnOffAE()
    {
        Chat.ExecuteCommand("/aeTargetSelector off");
        Chat.ExecuteCommand("/aepull off");
    }

    // ---- 路径规划 ----

    /// <summary>
    /// 贪心最近邻算法计算路径顺序，从玩家当前位置出发。
    /// 使用三维距离排序以避免垂直距离差异导致路径跳跃（与 XZ 距离判断接近不同，
    /// 此处是为了保证整体路径连续性）。
    /// </summary>
    public static List<(float X, float Y, float Z)> GetShortestPath(List<(float X, float Y, float Z)> points)
    {
        if (points.Count <= 1)
            return [.. points];

        // 拷贝，避免修改调用方传入的原始列表
        var remaining = new List<(float X, float Y, float Z)>(points);
        var path = new List<(float X, float Y, float Z)>(remaining.Count);

        var playerPos = Svc.Objects.LocalPlayer!.Position;
        var current = remaining.OrderBy(p => Distance((playerPos.X, playerPos.Y, playerPos.Z), p)).First();
        path.Add(current);
        remaining.Remove(current);

        while (remaining.Count > 0)
        {
            var nearest = remaining.OrderBy(p => Distance(current, p)).First();
            path.Add(nearest);
            current = nearest;
            remaining.Remove(current);
        }

        return path;
    }

    /// <summary>三维欧几里得距离</summary>
    public static double Distance((float X, float Y, float Z) p1, (float X, float Y, float Z) p2)
    {
        var dx = p1.X - p2.X;
        var dy = p1.Y - p2.Y;
        var dz = p1.Z - p2.Z;
        return Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
    }
}
