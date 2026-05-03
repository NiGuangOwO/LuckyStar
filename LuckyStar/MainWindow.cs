using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using System;
using System.Collections.Generic;

namespace LuckyStar;

public class MainWindow : Window, IDisposable
{
    private readonly FarmController controller;

    public MainWindow() : base("LuckyStar", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        controller = new FarmController();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        controller.Dispose();
    }

    public override void Draw()
    {
        DrawTopBar();

        using var tab = ImRaii.TabBar("mainTab");
        if (!tab) return;

        using (var tabItem = ImRaii.TabItem("农怪"))
        {
            if (tabItem) DrawFarmTab();
        }

        if (Plugin.Configuration.DevMode)
        {
            using var tabItem = ImRaii.TabItem("Debug");
            if (tabItem) DrawDebugTab();
        }
    }

    // ---- 顶部控制栏 ----

    private void DrawTopBar()
    {
        if (ImGui.Button("停止"))
        {
            controller.Stop();
        }

        ImGui.SameLine();

        if (controller.IsRunning)
            ImGui.TextColored(ImGuiColors.HealerGreen, $"状态：{controller.State}");
        else
            ImGui.Text($"状态：{controller.State}");

        if (controller.DataIndex == 0 && controller.WaitingFirst)
        {
            ImGui.SameLine();
            if (ImGui.Button("强制跳过"))
                controller.ForceSkipFirst();
        }

        if (ImGui.Checkbox("##DelayEnable", ref Plugin.Configuration.DelayEnable))
            Plugin.Configuration.Save();
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

        if (ImGui.Checkbox("##DevMode", ref Plugin.Configuration.DevMode))
            Plugin.Configuration.Save();
        ImGui.SameLine();
        ImGui.Text("开发者模式");
    }

    // ---- 农怪 Tab ----

    private void DrawFarmTab()
    {
        bool running = controller.IsRunning;

        // 颇胝迦
        ImGui.Text("萨维奈岛 - 颇胝迦");
        ImGui.SameLine();
        ImGui.BeginDisabled(running);
        if (ImGui.Checkbox("阿输陀花", ref Plugin.Configuration.PoZhiJia_1)) Plugin.Configuration.Save();
        ImGui.SameLine();
        if (ImGui.Checkbox("毕舍遮", ref Plugin.Configuration.PoZhiJia_2)) Plugin.Configuration.Save();
        ImGui.SameLine();
        if (ImGui.Checkbox("金刚尾", ref Plugin.Configuration.PoZhiJia_3)) Plugin.Configuration.Save();
        ImGui.SameLine();
        if (ImGui.Button("Go##PoZhiJia"))
            StartFarm(
                (Plugin.Configuration.PoZhiJia_1, MobsData.PoZhiJia.阿输陀花),
                (Plugin.Configuration.PoZhiJia_2, MobsData.PoZhiJia.毕舍遮),
                (Plugin.Configuration.PoZhiJia_3, MobsData.PoZhiJia.金刚尾));
        ImGui.EndDisabled();

        ImGui.Separator();

        // 叹息海
        ImGui.Text("叹息海 - 沉思之物");
        ImGui.SameLine();
        ImGui.BeginDisabled(running);
        if (ImGui.Checkbox("思考之物", ref Plugin.Configuration.TanXiZhiWu_1)) Plugin.Configuration.Save();
        ImGui.SameLine();
        if (ImGui.Checkbox("彷徨之物", ref Plugin.Configuration.TanXiZhiWu_2)) Plugin.Configuration.Save();
        ImGui.SameLine();
        if (ImGui.Checkbox("叹息之物", ref Plugin.Configuration.TanXiZhiWu_3)) Plugin.Configuration.Save();
        ImGui.SameLine();
        if (ImGui.Button("Go##TanXi"))
            StartFarm(
                (Plugin.Configuration.TanXiZhiWu_1, MobsData.TanXiZhiWu.思考之物),
                (Plugin.Configuration.TanXiZhiWu_2, MobsData.TanXiZhiWu.彷徨之物),
                (Plugin.Configuration.TanXiZhiWu_3, MobsData.TanXiZhiWu.叹息之物));
        ImGui.EndDisabled();

        ImGui.Separator();

        // 伊休妲
        ImGui.Text("拉凯提亚大森林 - 伊休妲");
        ImGui.SameLine();
        ImGui.BeginDisabled(running);
        if (ImGui.Checkbox("人偶", ref Plugin.Configuration.YiXiuDa_1)) Plugin.Configuration.Save();
        ImGui.SameLine();
        if (ImGui.Checkbox("石蒺藜", ref Plugin.Configuration.YiXiuDa_2)) Plugin.Configuration.Save();
        ImGui.SameLine();
        if (ImGui.Checkbox("器皿", ref Plugin.Configuration.YiXiuDa_3)) Plugin.Configuration.Save();
        ImGui.SameLine();
        if (ImGui.Button("Go##YiXiuDa"))
            StartFarm(
                (Plugin.Configuration.YiXiuDa_1, MobsData.YiXiuDa.人偶),
                (Plugin.Configuration.YiXiuDa_2, MobsData.YiXiuDa.石蒺藜),
                (Plugin.Configuration.YiXiuDa_3, MobsData.YiXiuDa.器皿));
        ImGui.EndDisabled();

        ImGui.Separator();

        // 优昙婆罗花
        ImGui.Text("基拉巴尼亚边区 - 优昙婆罗花");
        ImGui.SameLine();
        ImGui.BeginDisabled(running);
        if (ImGui.Checkbox("莱西", ref Plugin.Configuration.YouTan_1)) Plugin.Configuration.Save();
        ImGui.SameLine();
        if (ImGui.Checkbox("狄亚卡", ref Plugin.Configuration.YouTan_2)) Plugin.Configuration.Save();
        ImGui.SameLine();
        if (ImGui.Button("Go##YouTan"))
            StartFarm(
                (Plugin.Configuration.YouTan_1, MobsData.优昙婆罗花.莱西),
                (Plugin.Configuration.YouTan_2, MobsData.优昙婆罗花.狄亚卡));
        ImGui.EndDisabled();

        ImGui.Separator();

        // 卢克洛塔
        ImGui.Text("魔大陆阿济兹拉 - 卢克洛塔");
        ImGui.SameLine();
        ImGui.BeginDisabled(running);
        if (ImGui.Checkbox("奇美拉", ref Plugin.Configuration.LuKeLuoTa_1)) Plugin.Configuration.Save();
        ImGui.SameLine();
        if (ImGui.Checkbox("海德拉", ref Plugin.Configuration.LuKeLuoTa_2)) Plugin.Configuration.Save();
        ImGui.SameLine();
        if (ImGui.Checkbox("薇薇尔飞龙", ref Plugin.Configuration.LuKeLuoTa_3)) Plugin.Configuration.Save();
        ImGui.SameLine();
        if (ImGui.Button("Go##LuKeLuoTa"))
            StartFarm(
                (Plugin.Configuration.LuKeLuoTa_1, MobsData.卢克洛塔.奇美拉),
                (Plugin.Configuration.LuKeLuoTa_2, MobsData.卢克洛塔.海德拉),
                (Plugin.Configuration.LuKeLuoTa_3, MobsData.卢克洛塔.薇薇尔飞龙));
        ImGui.EndDisabled();

        ImGui.Separator();

        // 蚓螈巨虫（无子选项）
        ImGui.Text("北萨纳兰 - 蚓螈巨虫");
        ImGui.SameLine();
        ImGui.BeginDisabled(running);
        if (ImGui.Button("Go##Yinyuan"))
            controller.Start(new List<(float, float, float)>(MobsData.蚓螈巨虫));
        ImGui.EndDisabled();
    }

    /// <summary>
    /// 根据选中的子列表合并后启动农怪。
    /// </summary>
    private void StartFarm(params (bool Enabled, List<(float X, float Y, float Z)> Points)[] sources)
    {
        var points = new List<(float, float, float)>();
        foreach (var (enabled, list) in sources)
        {
            if (enabled) points.AddRange(list);
        }
        controller.Start(points);
    }

    // ---- Debug Tab ----

    private void DrawDebugTab()
    {
        var player = Svc.Objects.LocalPlayer;
        ImGui.Text($"Position: {player?.Position}");
        if (ImGui.Button("复制坐标"))
        {
            ImGui.SetClipboardText(
                $"({player?.Position.X}f,{player?.Position.Y}f,{player?.Position.Z}f),");
        }

        ImGui.Separator();
        ImGui.Text($"IsRunning: {controller.IsRunning}");
        ImGui.Text($"DataIndex: {controller.DataIndex} / {controller.CurrentList.Count}");
        ImGui.Text($"WaitingFirst: {controller.WaitingFirst}");
        ImGui.Text($"NeedToTakeOff: {controller.NeedToTakeOff}");
        ImGui.Text($"ReadyToNextPos: {controller.ReadyToTheNextPos}");
        ImGui.Text($"PosDistance: {controller.PosDistance:F2}");

        ImGui.Separator();
        ImGui.Text("路径点列表：");
        for (int i = 0; i < controller.CurrentList.Count; i++)
        {
            var pos = controller.CurrentList[i];
            bool isCurrent = i == controller.DataIndex;
            if (isCurrent)
                ImGui.TextColored(ImGuiColors.HealerGreen, $">> ({pos.X:F2}, {pos.Y:F2}, {pos.Z:F2})");
            else
                ImGui.Text($"   ({pos.X:F2}, {pos.Y:F2}, {pos.Z:F2})");
        }
    }
}
