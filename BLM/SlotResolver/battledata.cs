using System.Collections.Concurrent;
using AEAssist.CombatRoutine.Module;
using Los;
using Los.BLM;

namespace los.BLM.SlotResolver;

public class BattleData
{
    public static BattleData Instance = new();

    public uint 前一能力技 { get; set; } = 0;
    public uint 前一gcd { get; set; } = 0;
    public bool 已使用瞬发 { get; set; } = false;
    public bool IsInnerOpener = false;
    public bool 正在特殊循环中 { get; set; } = false;
    public bool 正在双星灵墨泉 { get; set; } = false;

    public bool 需要即刻 { get; set; } = false;
    public int AoEFlareCount { get; set; } = 0;
    public int AoEFlareMax   { get; set; } = 2;

    public bool 需要瞬发gcd { get; set; } = false;
    public bool 三冰针进冰 { get; set; } = false;

    public List<uint> 冰状态gcd = [];
    public List<uint> 火状态gcd = [];
    public List<uint> 上一轮循环 = [];
    public bool HotkeyUseHighPrioritySlot = false; // 热键使用高优先级队列


    //循环控制
    public bool 三连走位 = false;
    public bool 即刻三连无移动判断 = false;
    public bool 起手 = false;
    public bool aoe火二 = false;
    public bool 特供循环 = false;
    public bool 压缩冰悖论 = false;
    public bool 压缩火悖论 = false;
    public bool 核爆收尾 = false;
    /// <summary>
    /// 火阶段收尾（绝望之后）、下一发预期是冰三进冰的这段时间
    /// 给即刻/三连判断是否要“进冰瞬发”
    /// </summary>
    public bool 进冰瞬发窗口 { get; set; }

    public static ConcurrentDictionary<string, long> 技能内置cd = new ConcurrentDictionary<string, long>();
    public static bool isChange;
    public static bool isBattleDataStop;

    public static void Reset()
    {
        技能内置cd = new ConcurrentDictionary<string, long>();
        if (isBattleDataStop)
        {
            PlayerOptions.Instance.Stop = false;
        }
    }

    public static bool IsChanged;

    public static void RebuildSettings()
    {
        if (!IsChanged) return;

        IsChanged = false;
        GlobalSetting.Build(BlackMageACR.settingFolderPath, true);
        BlackMageSetting.Build(BlackMageACR.settingFolderPath);
    }

    public static void Stop()
    {
        isBattleDataStop = true;
        PlayerOptions.Instance.Stop = true;
    }
}

