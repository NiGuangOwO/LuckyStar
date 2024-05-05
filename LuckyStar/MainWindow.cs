using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ECommons.Automation;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace LuckyStar.Windows;

public unsafe class MainWindow : Window, IDisposable
{
    private int dataIndex = 0;
    private bool needToTakeOff = true;
    private bool readyToTheNextpos = true;
    private bool isRunning = false;
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
                    Mount();
                }
                else
                {
                    if (!Svc.Condition[ConditionFlag.InFlight])
                    {
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
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Svc.Framework.Update -= OnUpdate;
    }

    public override void Draw()
    {
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
                    ImGui.SameLine();
                    if (ImGui.Button("Stop##PoZhiJia"))
                    {
                        isRunning = false;
                        Stop();
                        reset();
                    }

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
                    ImGui.SameLine();
                    if (ImGui.Button("Stop##Tanxi"))
                    {
                        isRunning = false;
                        Stop();
                        reset();
                    }

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
                    ImGui.SameLine();
                    if (ImGui.Button("Stop##YiXiuDa"))
                    {
                        isRunning = false;
                        Stop();
                        reset();
                    }

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
                    ImGui.SameLine();
                    if (ImGui.Button("Stop##优昙婆罗花"))
                    {
                        isRunning = false;
                        Stop();
                        reset();
                    }
                }
            }
            using (var tabItem = ImRaii.TabItem("Debug"))
            {
                if (tabItem)
                {
                    ImGui.Text($"Position: {Player.GameObject->Position}");
                    if (ImGui.Button("复制"))
                    {
                        ImGui.SetClipboardText($"({Svc.ClientState.LocalPlayer.Position.X}f,{Svc.ClientState.LocalPlayer.Position.Y}f,{Svc.ClientState.LocalPlayer.Position.Z}f),");
                    }
                    ImGui.Text($"isRunning: {isRunning}");
                    ImGui.Text($"DataIndex: {dataIndex}");
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
            return;
        }

        if (Svc.Condition[ConditionFlag.InFlight] && readyToTheNextpos)
        {
            readyToTheNextpos = false;
            Stop();
            flyto(currentList[dataIndex].X, currentList[dataIndex].Y, currentList[dataIndex].Z);
            return;
        }

        var Posdistance = Math.Sqrt(Math.Pow(currentList[dataIndex].X - Svc.ClientState.LocalPlayer.Position.X, 2) + Math.Pow(currentList[dataIndex].Z - Svc.ClientState.LocalPlayer.Position.Z, 2));
        if (!readyToTheNextpos && Posdistance < 5)
        {
            if (Svc.Objects.OfType<BattleChara>().Where(b => MobsData.Nameid.Contains(b.NameId) && !b.IsDead && Vector3.Distance(Svc.ClientState.LocalPlayer?.Position ?? Vector3.Zero, b.Position) <= 20).Any())
            {
                if (Svc.Condition[ConditionFlag.Mounted])
                {
                    Stop();
                    Dismount();
                }
            }
            else
            {
                Stop();
                if (dataIndex >= currentList.Count)
                {
                    dataIndex = 0;
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
        dataIndex = 0;
        needToTakeOff = true;
        readyToTheNextpos = true;
        currentList.Clear();
    }

    public static void walkto(float x, float y, float z)
    {
        Chat.Instance.ExecuteCommand($"/vnavmesh moveto {x} {y} {z}");
    }

    public void walktowithmount(float x, float y, float z)
    {
        if (!Svc.Condition[ConditionFlag.Mounted])
        {
            Mount();
        }
        else
        {
            Chat.Instance.ExecuteCommand($"/vnavmesh moveto {x} {y} {z}");
        }
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
    public static void Stop()
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
