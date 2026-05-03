using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace LuckyStar;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    // 颇胝迦
    public bool PoZhiJia_1 = true;
    public bool PoZhiJia_2 = true;
    public bool PoZhiJia_3 = true;

    // 叹息海
    public bool TanXiZhiWu_1 = true;
    public bool TanXiZhiWu_2 = true;
    public bool TanXiZhiWu_3 = true;

    // 伊休妲
    public bool YiXiuDa_1 = true;
    public bool YiXiuDa_2 = true;
    public bool YiXiuDa_3 = true;

    // 优昙婆罗花
    public bool YouTan_1 = true;
    public bool YouTan_2 = true;

    // 卢克洛塔
    public bool LuKeLuoTa_1 = true;
    public bool LuKeLuoTa_2 = true;
    public bool LuKeLuoTa_3 = true;

    public bool DelayEnable = false;
    public int DelayTime = 10;

    public bool DevMode = false;

    [NonSerialized]
    private IDalamudPluginInterface? pluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
        pluginInterface!.SavePluginConfig(this);
    }
}
