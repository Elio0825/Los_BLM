using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Opener;
using AEAssist.MemoryApi;
using los.BLM;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;
using BattleData = los.BLM.SlotResolver.BattleData;

namespace Los.BLM.SlotResolver.Opener;

public class Opener_lv70 : IOpener
{
        public int StartCheck()
    {
        if (!BlackMageSetting.Instance.IsHardCoreMode) return -99;
        if (Qt.Instance.GetQt("起手") is false) return -98;
        if (BattleData.Instance.IsInnerOpener) return 1;
        if (Helper.是否在战斗中()) return -2;
        
        if (!BattleData.Instance.起手&&Core.Me.CurrentMp == 10000&&!(BLMHelper.火状态 || BLMHelper.冰状态)) return 2;
        return -1;
    }

    public int StopCheck(int index)
    {
        return -1;
    }

    public void InitCountDown(CountDownHandler countDownHandler)
    {
        int startTime = (int)(BlackMageSetting.Instance.起手预读时间 * 1000);
        if (BlackMageSetting.Instance.提前黑魔纹)
        {

            countDownHandler.AddAction(startTime + 600, Skill.黑魔纹, SpellTargetType.Self);
            countDownHandler.AddAction(startTime, Skill.火三, SpellTargetType.Target);
            countDownHandler.AddAction(startTime - 500, () => BattleData.Instance.IsInnerOpener = true);
            countDownHandler.AddAction(startTime - 2800, Skill.雷一.GetActionChange(), SpellTargetType.Target);
        }
        else
        {
            countDownHandler.AddAction(startTime, Skill.火三, SpellTargetType.Target);
            countDownHandler.AddAction(startTime - 500, () => BattleData.Instance.IsInnerOpener = true);
            countDownHandler.AddAction(startTime - 3000, Skill.雷一.GetActionChange(), SpellTargetType.Target);
        }
        countDownHandler.AddAction(1000, () => Core.Resolve<MemApiChatMessage>().Toast2("开始循环",1,1000));
    }
    public List<Action<Slot>> Sequence { get; } =
    [
        Step1,Step3,Step4,Step5,Step6
    ];

    private static void Step1(Slot slot)
    {
        
        slot.Add(new Spell(Skill.火四, SpellTargetType.Target));
        if (Qt.Instance.GetQt("爆发药"))
            slot.Add(Spell.CreatePotion());
        if (!BlackMageSetting.Instance.提前黑魔纹)
            slot.Add(new Spell(Skill.黑魔纹, SpellTargetType.Self));
    }

    private static void Step2(Slot slot)
    {

    }

    private static void Step3(Slot slot)
    {
        slot.Add(new Spell(Skill.火四, SpellTargetType.Target));
        if(!Qt.Instance.GetQt("爆发药"))
            slot.Add(new Spell(Skill.三连, SpellTargetType.Self));
        slot.Add(new Spell(Skill.火四, SpellTargetType.Target));
        slot.Add(new Spell(Skill.火四, SpellTargetType.Target));
    }

    private static void Step4(Slot slot)
    {
        //slot.Add(new Spell(Skill.绝望, SpellTargetType.Target));
        slot.Add(new Spell(Skill.墨泉, SpellTargetType.Self));
    }

    private static void Step5(Slot slot)
    {
        slot.Add(new Spell(Skill.火四, SpellTargetType.Target));
        slot.Add(new Spell(Skill.火四, SpellTargetType.Target));
        slot.Add(new Spell(Skill.火四, SpellTargetType.Target));
        slot.Add(new Spell(Skill.火四, SpellTargetType.Target));
        slot.Add(new Spell(Skill.火四, SpellTargetType.Target));
        slot.Add(new Spell(Skill.火四, SpellTargetType.Target));
        
    }

    private static void Step6(Slot slot)
    {
        //slot.Add(new Spell(Skill.三连, SpellTargetType.Self));
        slot.Add(new Spell(Skill.火四, SpellTargetType.Target));
        slot.Add(new Spell(Skill.雷一.GetActionChange(), SpellTargetType.Target));
        if(BattleData.Instance.IsInnerOpener)
            BattleData.Instance.IsInnerOpener = false;
    }

    private static void Step7(Slot slot)
    {
        //slot.Add(new Spell(Skill.绝望, SpellTargetType.Target));
        //slot.Add(new Spell(Skill.星灵移位, SpellTargetType.Self));
        
    }
}