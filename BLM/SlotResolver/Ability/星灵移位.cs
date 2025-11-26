using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;
using Los.BLM.SlotResolver.Special;
using BattleData = los.BLM.SlotResolver.BattleData;

namespace Los.BLM.SlotResolver.Ability;

public class 星灵移位 : ISlotResolver
{
    private readonly uint _skillId = Skill.星灵移位;

    private Spell? GetSpell()
    {
        return _skillId.GetSpell(SpellTargetType.Self);
    }

    public void Build(Slot slot)
    {
        var spell = GetSpell();
        if (spell != null)
            slot.Add(spell);
    }

    public int Check()
    {
        var spell = GetSpell();
        if (spell == null)
            return -1;
        
        // 获取冷却时间
        double cooldownMs = Core.Resolve<MemApiSpell>().GetCooldown(_skillId).TotalMilliseconds;
        
        if (!BLMHelper.冰状态 && !BLMHelper.火状态)
            return -2;

        var level = Core.Me.Level;
        
        if (level >= 58 &&
            (BLMHelper.双目标aoe() || BLMHelper.三目标aoe()))
        {
            // 先检查使用条件（不考虑冷却）
            int aoeCheck = CheckAoE_High();
            
            // ✅ 修复：在AOE场景下，如果满足使用条件但星灵移位还在冷却中，
            // 且上一个GCD是玄冰，冷却时间较短（<3000ms），返回1让系统等待冷却完成
            if (aoeCheck > 0 && cooldownMs > 0 && cooldownMs < 3000)
            {
                uint lastGcd = BattleData.Instance.前一gcd;
                uint lastActionId = lastGcd == 0 ? 0u : lastGcd.GetActionChange();
                if (lastActionId == Skill.玄冰)
                {
                    // 返回1，让系统知道应该等待冷却完成
                    // 系统会持续检查，直到冷却完成
                    return 1;
                }
            }
            
            // 如果冷却时间>0且不在等待范围内，返回-1
            if (cooldownMs > 0)
                return -1;
            
            return aoeCheck;
        }

        
        // 如果冷却时间>0，返回-1
        if (cooldownMs > 0)
            return -1;
        
        return CheckSingleTarget();
    }

    
    private int CheckAoE_High()
    {
        
        if (BLMHelper.冰状态)
        {
            // ✅ 修复：如果上一个GCD是玄冰，即使冰针 != 3，也允许使用星灵移位
            // 因为玄冰释放后，冰针应该变成3，但状态更新可能有延迟
            uint lastGcd = BattleData.Instance.前一gcd;
            uint lastActionId = lastGcd == 0 ? 0u : lastGcd.GetActionChange();
            bool lastGcdWasIce4 = lastActionId == Skill.玄冰;
            
            // 如果上一个GCD是玄冰，或者RecentlyUsed检查通过，允许使用
            bool ice4RecentlyUsed = Skill.玄冰.RecentlyUsed(2500);
            
            if (BLMHelper.冰针 != 3 && !lastGcdWasIce4 && !ice4RecentlyUsed)
                return -211;

            if (BattleData.Instance.正在特殊循环中)
                return -212;

            return 1;
        }

        
        if (BLMHelper.火状态)
        {
            
            if (BLMHelper.耀星层数 == 6 && Core.Me.Level == 100)
                return -4;

            
            if (Core.Me.CurrentMp < 800)
            {
                
                if (Skill.墨泉.AbilityCoolDownInNextXgcDsWindow(2)
                    || Skill.墨泉.GetSpell()?.IsReadyWithCanCast() == true
                    || Skill.墨泉.RecentlyUsed())
                {
                    
                    return -221;
                }

                
                return 1;
            }

            
            return -220;
        }

        
        return -299;
    }

    
    private int CheckSingleTarget()
    {
        
        if (Qt.Instance.GetQt("TTK"))
        {
            if (Core.Me.Level < 90) return -90;
            if (BLMHelper.火状态 && Core.Me.CurrentMp < 800) return 88;
            if (BLMHelper.冰状态 && !BLMHelper.悖论指示) return 99;
        }

        
        if (BLMHelper.火状态)
        {
            
            if (Skill.墨泉.AbilityCoolDownInNextXgcDsWindow(2)
                || Skill.墨泉.GetSpell()?.IsReadyWithCanCast() == true
                || Skill.墨泉.RecentlyUsed())
                return -66;

            
            if (Core.Me.CurrentMp >= 800) return -3;
            if (BLMHelper.耀星层数 == 6 && Core.Me.Level == 100) return -4;
            if (Helper.可瞬发()) return 1;

            if (Core.Me.Level < 90) return -90;
            if (Core.Me.Level < 100) return -100;

            if (BattleData.Instance.三连走位 && !Skill.即刻.AbilityCoolDownInNextXgcDsWindow(1))
                return -5;

            if (!Helper.可瞬发()
                && !Skill.即刻.AbilityCoolDownInNextXgcDsWindow(1)
                && Skill.三连.GetSpell().Charges < 1)
                return -5;

            return 4; // 2
        }

        
        if (BLMHelper.冰状态)
        {
            if (BLMHelper.悖论指示 && !BattleData.Instance.压缩冰悖论) return -3;
            if (BLMHelper.冰层数 != 3) return -4;
            if (BLMHelper.冰针 != 3) return -6;

            if (Core.Me.Level < 90)
            {
                if (Helper.有buff(Buffs.火苗buff)) return 72;
                return -90;
            }

            if (Core.Me.Level < 100
                && BattleData.Instance.特供循环
                && (new 开满转火().StartCheck() > 0 || BattleData.Instance.正在特殊循环中))
                return -8;

            return 4; //  2，
        }

        return -99;
    }

    
    private static bool IsIce4(uint id)
    {
        return id == Skill.玄冰;
    }
}
