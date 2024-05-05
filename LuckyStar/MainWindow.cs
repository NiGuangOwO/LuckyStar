using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ECommons.Automation;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace LuckyStar.Windows;

public unsafe class MainWindow : Window, IDisposable
{
    private string state = "";
    private bool isRunning = false;

    private bool waitingFirst = false;
    private long throttleTime = 0;

    private int dataIndex = 0;
    private bool needToTakeOff = true;
    private bool readyToTheNextpos = true;
    private List<(float X, float Y, float Z)> currentList { get; set; } = [];

    public MainWindow() : base("LuckyStar", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        Svc.Framework.Update += OnUpdate;
    }
    public void OnUpdate(IFramework framework)
    {
        if (isRunning)
        {
            if (needToTakeOff)
            {
                if (!Svc.Condition[ConditionFlag.Mounted])
                {
                    state = "上坐骑";
                    Mount();
                }
                else
                {
                    if (!Svc.Condition[ConditionFlag.InFlight])
                    {
                        state = "起飞";
                        Takeoff();
                    }
                    else
                    {
                        needToTakeOff = false;
                    }
                }
            }
            Run();
        }
        else
        {
            state = "未运行";
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Svc.Framework.Update -= OnUpdate;
    }

    public override void Draw()
    {
        if (ImGui.Button("停止"))
        {
            isRunning = false;
            VnavmeshStop();
            reset();
        }

        ImGui.SameLine();
        ImGui.Text($"状态：{state}");
        if (dataIndex == 0 && waitingFirst)
        {
            ImGui.SameLine();
            if (ImGui.Button("强制跳过"))
            {
                dataIndex++;
                waitingFirst = false;
                needToTakeOff = true;
                readyToTheNextpos = true;
            }
        }

        ImGui.Checkbox("##开启延迟", ref Plugin.Configuration.DelayEnable);
        ImGui.SameLine();
        ImGui.Text("每轮第一只刷新时延迟下坐骑");
        if (Plugin.Configuration.DelayEnable)
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100f);
            if (ImGui.InputInt("延迟时间 (s)", ref Plugin.Configuration.DelayTime))
            {
                if (Plugin.Configuration.DelayTime < 0)
                    Plugin.Configuration.DelayTime = 0;
                Plugin.Configuration.Save();
            }
        }

        using var tab = ImRaii.TabBar("mainTab");
        if (tab)
        {
            using (var tabItem = ImRaii.TabItem("农怪"))
            {
                if (tabItem)
                {
                    ImGui.Text("萨维奈岛-颇胝迦");
                    ImGui.SameLine();
                    ImGui.BeginDisabled(isRunning);
                    if (ImGui.Checkbox("阿输陀花", ref Plugin.Configuration.PoZhiJia_1_check))
                    {
                        Plugin.Configuration.Save();
                    }
                    ImGui.SameLine();
                    if (ImGui.Checkbox("毕舍遮", ref Plugin.Configuration.PoZhiJia_2_check))
                    {
                        Plugin.Configuration.Save();
                    }
                    ImGui.SameLine();
                    if (ImGui.Checkbox("金刚尾", ref Plugin.Configuration.PoZhiJia_3_check))
                    {
                        Plugin.Configuration.Save();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Go##PoZhiJia"))
                    {
                        reset();
                        if (Plugin.Configuration.PoZhiJia_1_check)
                            currentList.AddRange(MobsData.PoZhiJia.阿输陀花);
                        if (Plugin.Configuration.PoZhiJia_2_check)
                            currentList.AddRange(MobsData.PoZhiJia.毕舍遮);
                        if (Plugin.Configuration.PoZhiJia_3_check)
                            currentList.AddRange(MobsData.PoZhiJia.金刚尾);
                        currentList = GetShortestPath(currentList);
                        isRunning = true;
                    }
                    ImGui.EndDisabled();

                    ImGui.Separator();
                    ImGui.Text("叹息海-沉思之物");
                    ImGui.SameLine();
                    ImGui.BeginDisabled(isRunning);
                    if (ImGui.Checkbox("思考之物", ref Plugin.Configuration.TanXiZhiWu_1_check))
                    {
                        Plugin.Configuration.Save();
                    }
                    ImGui.SameLine();
                    if (ImGui.Checkbox("彷徨之物", ref Plugin.Configuration.TanXiZhiWu_2_check))
                    {
                        Plugin.Configuration.Save();
                    }
                    ImGui.SameLine();
                    if (ImGui.Checkbox("叹息之物", ref Plugin.Configuration.TanXiZhiWu_3_check))
                    {
                        Plugin.Configuration.Save();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Go##Tanxi"))
                    {
                        reset();
                        if (Plugin.Configuration.TanXiZhiWu_1_check)
                            currentList.AddRange(MobsData.TanXiZhiWu.叹息之物);
                        if (Plugin.Configuration.TanXiZhiWu_2_check)
                            currentList.AddRange(MobsData.TanXiZhiWu.彷徨之物);
                        if (Plugin.Configuration.TanXiZhiWu_3_check)
                            currentList.AddRange(MobsData.TanXiZhiWu.叹息之物);
                        currentList = GetShortestPath(currentList);
                        isRunning = true;
                    }
                    ImGui.EndDisabled();

                    ImGui.Separator();
                    ImGui.Text("拉凯提亚大森林-伊休妲");
                    ImGui.SameLine();
                    ImGui.BeginDisabled(isRunning);
                    if (ImGui.Checkbox("人偶", ref Plugin.Configuration.YiXiuDa_1_check))
                    {
                        Plugin.Configuration.Save();
                    }
                    ImGui.SameLine();
                    if (ImGui.Checkbox("石蒺藜", ref Plugin.Configuration.YiXiuDa_2_check))
                    {
                        Plugin.Configuration.Save();
                    }
                    ImGui.SameLine();
                    if (ImGui.Checkbox("器皿", ref Plugin.Configuration.YiXiuDa_3_check))
                    {
                        Plugin.Configuration.Save();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Go##YiXiuDa"))
                    {
                        reset();
                        if (Plugin.Configuration.YiXiuDa_1_check)
                            currentList.AddRange(MobsData.YiXiuDa.人偶);
                        if (Plugin.Configuration.YiXiuDa_2_check)
                            currentList.AddRange(MobsData.YiXiuDa.石蒺藜);
                        if (Plugin.Configuration.YiXiuDa_3_check)
                            currentList.AddRange(MobsData.YiXiuDa.器皿);
                        currentList = GetShortestPath(currentList);
                        isRunning = true;
                    }
                    ImGui.EndDisabled();

                    ImGui.Separator();
                    ImGui.Text("基拉巴尼亚边区-优昙婆罗花");
                    ImGui.SameLine();
                    ImGui.BeginDisabled(isRunning);
                    if (ImGui.Checkbox("莱西", ref Plugin.Configuration.优昙婆罗花_1))
                    {
                        Plugin.Configuration.Save();
                    }
                    ImGui.SameLine();
                    if (ImGui.Checkbox("狄亚卡", ref Plugin.Configuration.优昙婆罗花_2))
                    {
                        Plugin.Configuration.Save();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Go##优昙婆罗花"))
                    {
                        reset();
                        if (Plugin.Configuration.优昙婆罗花_1)
                            currentList.AddRange(MobsData.优昙婆罗花.莱西);
                        if (Plugin.Configuration.优昙婆罗花_2)
                            currentList.AddRange(MobsData.优昙婆罗花.狄亚卡);
                        currentList = GetShortestPath(currentList);
                        isRunning = true;
                    }
                    ImGui.EndDisabled();

                    ImGui.Separator();
                    ImGui.Text("魔大陆阿济兹拉-卢克洛塔");
                    ImGui.SameLine();
                    ImGui.BeginDisabled(isRunning);
                    if (ImGui.Checkbox("奇美拉", ref Plugin.Configuration.卢克洛塔_1))
                    {
                        Plugin.Configuration.Save();
                    }
                    ImGui.SameLine();
                    if (ImGui.Checkbox("海德拉", ref Plugin.Configuration.卢克洛塔_2))
                    {
                        Plugin.Configuration.Save();
                    }
                    ImGui.SameLine();
                    if (ImGui.Checkbox("薇薇尔飞龙", ref Plugin.Configuration.卢克洛塔_3))
                    {
                        Plugin.Configuration.Save();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Go##卢克洛塔"))
                    {
                        reset();
                        if (Plugin.Configuration.卢克洛塔_1)
                        {
                            currentList.AddRange(MobsData.卢克洛塔.奇美拉);
                        }
                        if (Plugin.Configuration.卢克洛塔_2)
                        {
                            currentList.AddRange(MobsData.卢克洛塔.海德拉);
                        }
                        if (Plugin.Configuration.卢克洛塔_3)
                        {
                            currentList.AddRange(MobsData.卢克洛塔.薇薇尔飞龙);
                        }
                        currentList = GetShortestPath(currentList);
                        isRunning = true;
                    }
                    ImGui.EndDisabled();
                }
            }

            using (var tabItem = ImRaii.TabItem("Debug"))
            {
                if (tabItem)
                {
                    ImGui.Text($"Position: {Svc.ClientState.LocalPlayer.Position}");
                    if (ImGui.Button("复制"))
                    {
                        ImGui.SetClipboardText($"({Svc.ClientState.LocalPlayer.Position.X}f,{Svc.ClientState.LocalPlayer.Position.Y}f,{Svc.ClientState.LocalPlayer.Position.Z}f),");
                    }
                    ImGui.Text($"isRunning: {isRunning}");
                    ImGui.Text($"DataIndex: {dataIndex}");
                    ImGui.Text($"waitingFirst: {waitingFirst}");
                    ImGui.Text($"throttleTime: {throttleTime}");
                    ImGui.Text($"needToTakeOff: {needToTakeOff}");
                    ImGui.Text($"readyToTheNextpos: {readyToTheNextpos}");
                    foreach (var pos in currentList)
                    {
                        if (currentList[dataIndex] == pos)
                        {
                            ImGui.TextColored(ImGuiColors.HealerGreen, $"({pos.X},{pos.Y},{pos.Z})");
                        }
                        else
                        {
                            ImGui.Text($"({pos.X},{pos.Y},{pos.Z})");
                        }
                    }
                }
            }
        }
    }

    public void Run()
    {
        if (dataIndex >= currentList.Count)
        {
            dataIndex = 0;
            waitingFirst = true;
            return;
        }

        if (Svc.Condition[ConditionFlag.InFlight] && readyToTheNextpos)
        {
            state = "寻路至下一只小怪";
            readyToTheNextpos = false;
            VnavmeshStop();
            flyto(currentList[dataIndex].X, currentList[dataIndex].Y, currentList[dataIndex].Z);
            return;
        }

        var Posdistance = Math.Sqrt(Math.Pow(currentList[dataIndex].X - Svc.ClientState.LocalPlayer.Position.X, 2) + Math.Pow(currentList[dataIndex].Z - Svc.ClientState.LocalPlayer.Position.Z, 2));
        if (!readyToTheNextpos && Posdistance < 5)
        {
            if (Svc.Objects.OfType<BattleChara>().Where(b => MobsData.Nameid.Contains(b.NameId) && !b.IsDead && Vector3.Distance(Svc.ClientState.LocalPlayer?.Position ?? Vector3.Zero, b.Position) <= 20).Any())
            {
                if (waitingFirst)
                {
                    if (Plugin.Configuration.DelayEnable)
                    {
                        if (throttleTime == 0)
                        {
                            throttleTime = Environment.TickCount64 + (Plugin.Configuration.DelayTime * 1000);
                        }

                        if (throttleTime != 0 && Environment.TickCount64 > throttleTime)
                        {
                            if (Svc.Condition[ConditionFlag.Mounted])
                            {
                                VnavmeshStop();
                                Dismount();
                            }
                            state = "等待击杀当前小怪";
                            throttleTime = 0;
                            waitingFirst = false;
                        }
                        else
                        {
                            state = $"第一只小怪已刷新，延迟剩余{throttleTime - Environment.TickCount64}ms";
                        }
                    }
                    else
                    {
                        if (Svc.Condition[ConditionFlag.Mounted])
                        {
                            VnavmeshStop();
                            Dismount();
                        }
                        state = "等待击杀当前小怪";
                        throttleTime = 0;
                        waitingFirst = false;
                    }
                }
                else
                {
                    if (Svc.Condition[ConditionFlag.Mounted])
                    {
                        VnavmeshStop();
                        Dismount();
                    }
                    state = "等待击杀当前小怪";
                    throttleTime = 0;
                    waitingFirst = false;
                }
            }
            else
            {
                if (dataIndex == 0 && waitingFirst)
                {
                    state = "等待第一只刷新";
                    return;
                }

                if (dataIndex >= currentList.Count)
                {
                    dataIndex = 0;
                    waitingFirst = true;
                }
                else
                {
                    dataIndex++;
                }
                readyToTheNextpos = true;
                needToTakeOff = true;
            }
        }
    }

    public void reset()
    {
        currentList.Clear();

        waitingFirst = false;
        throttleTime = 0;

        dataIndex = 0;
        needToTakeOff = true;
        readyToTheNextpos = true;
    }

    public static void walkto(float x, float y, float z)
    {
        Chat.Instance.ExecuteCommand($"/vnavmesh moveto {x} {y} {z}");
    }
    public void flyto(float x, float y, float z)
    {
        if (!Svc.Condition[ConditionFlag.InFlight])
        {
            needToTakeOff = true;
        }
        else
        {
            Chat.Instance.ExecuteCommand($"/vnavmesh flyto {x} {y} {z}");
        }
    }
    public void Mount()
    {
        ActionManager.Instance()->UseAction(ActionType.GeneralAction, 9); //上坐骑
    }
    public void Dismount()
    {
        ActionManager.Instance()->UseAction(ActionType.GeneralAction, 23); //下坐骑
    }
    public void Takeoff()
    {
        ActionManager.Instance()->UseAction(ActionType.GeneralAction, 2); //起飞
        needToTakeOff = false;
    }
    public static void VnavmeshStop()
    {
        Chat.Instance.ExecuteCommand($"/vnavmesh stop");
    }

    // 贪婪算法计算最短路径
    public static List<(float X, float Y, float Z)> GetShortestPath(List<(float X, float Y, float Z)> points)
    {
        if (points.Count <= 1)
            return points;

        var path = new List<(float X, float Y, float Z)>();

        // 初始点
        var currentPoint = points.First();
        path.Add(currentPoint);
        points.Remove(currentPoint);

        // 计算路径
        while (points.Count > 0)
        {
            var nearestPoint = points.OrderBy(p => Distance(currentPoint, p)).First();
            path.Add(nearestPoint);
            currentPoint = nearestPoint;
            points.Remove(currentPoint);
        }

        return path;
    }

    // 计算两点之间的距离
    public static float Distance((float X, float Y, float Z) p1, (float X, float Y, float Z) p2)
    {
        var dx = p2.X - p1.X;
        var dz = p2.Z - p1.Z;
        return (float)Math.Sqrt((dx * dx) + (dz * dz));
    }
}
