using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;

namespace Los.BLM.SlotResolver.GCD;

public class 秽浊 : ISlotResolver
{
    private readonly uint _skillId = Skill.秽浊;

    private Spell? GetSpell()
    {
        
        return Qt.Instance.GetQt("智能AOE")
            ? _skillId.GetSpellBySmartTarget()
            : _skillId.GetSpell();
    }

    public void Build(Slot slot)
    {
        var spell = GetSpell();
        if (spell != null)
            slot.Add(spell);
    }

    public int Check()
    {
        
        if (!Qt.Instance.GetQt("通晓"))
            return -5;

        var level = Core.Me.Level;

        
        if (level < 70)
            return -99;

        // 70–79：只有秽浊，没有异言 
        if (level >= 70 && level < 80)
        {
            if (Helper.IsMove)
            {
                return -5;
            }
            if (BLMHelper.通晓层数 >= 1)
                return 2;   

            return -99;
        }

        // 80+：异言单体，秽浊只用于 2+ 目标 AoE 
        
        var isTwoTarget = BLMHelper.双目标aoe();
        var isThreePlus = BLMHelper.三目标aoe();

        if (!isTwoTarget && !isThreePlus)
            return -100; 

        
        if (BlackMageQT.GetQt("Dump") && BLMHelper.通晓层数 > 0)
            return 666;   

        
        if (level >= 98)
        {
            
            if (BLMHelper.通晓层数 >= 3)
            {
                
                if (BLMHelper.通晓剩余时间 <= 10000)
                    return 2;

                
                if (Skill.详述.GetSpell().AbilityCoolDownInNextXgcDsWindow(1))
                    return 3;

                
                return 2;
            }

            
            if (BLMHelper.通晓层数 == 2 && BLMHelper.通晓剩余时间 < 8000)
                return 2;

            
            if (BLMHelper.通晓层数 >= 1 &&
                Skill.详述.GetSpell().AbilityCoolDownInNextXgcDsWindow(1))
                return 3;

            
            if (BLMHelper.火状态 &&
                Core.Me.CurrentMp < 800 &&
                BLMHelper.耀星层数 != 6)
            {
                
                if (Skill.墨泉.技能CD() < 300 &&
                    Skill.墨泉.GetSpell().IsReadyWithCanCast())
                    return -3;

                
                if (Skill.墨泉.GetSpell().AbilityCoolDownInNextXgcDsWindow(2))
                    return 4;
            }

            return -99;
        }

        
        if (level >= 80 && level < 98)
        {
            
            if (BLMHelper.通晓层数 >= 2)
            {
                
                if (BLMHelper.通晓剩余时间 < 8000)
                    return 2;
                if (level >= 86 &&
                    Skill.详述.GetSpell().AbilityCoolDownInNextXgcDsWindow(1))
                    return 3;
                
                return 2;
            }
            
            if (BLMHelper.通晓层数 == 1 &&
                BLMHelper.通晓剩余时间 <= 6000)
                return 2;

            
        }

        return -99;
    }
}
