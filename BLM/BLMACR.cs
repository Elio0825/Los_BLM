using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Opener;
using Los;
using los.BLM;
using los.BLM.Helper;
using los.BLM.QtUI;
using Los.BLM.SlotResolver.Ability; // 你的新 BlackMageSetting 在这个命名空间里

using Los.BLM.SlotResolver.GCD;
using Los.BLM.SlotResolver.GCD.AOE;
using los.BLM.SlotResolver.GCD.单体;
using Los.BLM.SlotResolver.GCD.单体;
using Los.BLM.SlotResolver.Opener;
using Los.BLM.SlotResolver.Special;
using los.BLM.Triggers;



namespace Los.BLM;

public static class BlackMageACR
{
    private const long Version = 20251121;
    public static string settingFolderPath = "";

    
    public static readonly List<SlotResolverData> SlotResolverData =
    [
        // GCD
        new(new TTK(), SlotMode.Gcd),
        new(new 异言(), SlotMode.Gcd),
        new(new 秽浊(), SlotMode.Gcd),
        new(new 异言(), SlotMode.Gcd),
        new(new 雷1(), SlotMode.Gcd),
        new(new 雷2(), SlotMode.Gcd),
        new(new 瞬发gcd触发器(), SlotMode.Gcd),
        new(new 群体100(), SlotMode.Gcd),
        new(new 群体58_99(), SlotMode.Gcd),
        new(new 群体50_57(), SlotMode.Gcd),
        new(new 群体35_49(), SlotMode.Gcd),
        new(new 群体1_34(), SlotMode.Gcd),
        new(new 单体100(), SlotMode.Gcd),
        new(new 单体90_99(), SlotMode.Gcd),
        new(new 单体72_89(), SlotMode.Gcd),
        new(new 单体60_71(), SlotMode.Gcd),
        new(new 单体35_59(), SlotMode.Gcd),
        new(new 单体1_34(), SlotMode.Gcd),
        new(new 核爆补耀星(), SlotMode.Gcd),

       

        // Ability
        new(new 星灵移位(), SlotMode.OffGcd),
        new(new 即刻(), SlotMode.OffGcd),
        new(new 三连咏唱(), SlotMode.OffGcd),
        new(new 醒梦(), SlotMode.OffGcd),
        new(new 详述(), SlotMode.OffGcd),
        new(new 墨泉(), SlotMode.OffGcd),
        new(new 黑魔纹(), SlotMode.OffGcd),
        new(new 爆发药(), SlotMode.OffGcd),
    ];

    
    public static void Init(string settingFolder)
    {
        settingFolderPath = settingFolder;

        
        GlobalSetting.Build(settingFolder, false);

        
        BlackMageSetting.Build(settingFolder);

        
        los.BLM.QtUI.Qt.Build();
    }

    
    public static Rotation Build()
    {
        return new Rotation(SlotResolverData)
        {
            TargetJob = Jobs.BlackMage,
            AcrType   = AcrType.Both,
            MinLevel  = 1,
            MaxLevel  = 100,
            Description = "支持日随/高难 请打开悬浮窗查看设置",
        }
        .AddOpener(GetOpener)
        .SetRotationEventHandler(new BLMEvetHandle())
        .AddSlotSequences(特殊序列.Build())
        .AddTriggerAction(new TriggerActionQt(), 
            new TriggerActionHotkey())
        .AddTriggerCondition(new TriggerCondQt())
        .AddCanUseHighPrioritySlotCheck(Helper.HighPrioritySlotCheckFunc);
    }

   
    private static IOpener? GetOpener(uint level)
    {
        var setting = BlackMageSetting.Instance;

        
        if (!Qt.Instance.GetQt("起手"))
            return null;

        switch (setting.起手选择)
        {
            
            case 0:
                return null;
            case 1:
                if (level == 100)
                {
                    if (setting.标准57)   return new Opener57();
                    if (setting.核爆起手) return new Opener核爆();
                    if (setting.开挂循环) return new Opener57开挂循环();
                    return null;
                }

                if (level >= 90 && level < 100) return new Opener_lv90();
                if (level >= 80 && level < 90)  return new Opener_lv80();
                if (level >= 70 && level < 80)  return new Opener_lv70();
                return null;
            
            case 2:
                return level >= 70 ? new Opener_lv70() : null;
            
            case 3:
                return level >= 80 ? new Opener_lv80() : null;
            
            case 4:
                return level >= 90 ? new Opener_lv90() : null;
            
            case 5:
                return level >= 100 ? new Opener57() : null;
            
            case 6:
                return level >= 100 ? new Opener核爆() : null;
            
            case 7:
                return level >= 100 ? new Opener57开挂循环() : null;

            default:
                return null;
        }
    }
}