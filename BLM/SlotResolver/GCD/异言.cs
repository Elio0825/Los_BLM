

using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;

namespace Los.BLM.SlotResolver.GCD;

public class 异言 : ISlotResolver
{
    private readonly uint _skillId = Skill.异言;
    private Spell? GetSpell()
    {
        return  _skillId.GetSpell();
    }
    public void Build(Slot slot)
    {
        var spell = GetSpell();
        if (spell != null) 
            slot.Add(spell);
    }

    public int Check()
    {
        if (Qt.Instance.GetQt("倾泻资源") && BLMHelper.通晓层数 > 0) return 666;
        if (Core.Me.Level < 80) return -80;
        if (!Qt.Instance.GetQt("通晓")) return -2;
        if (BLMHelper.三目标aoe() || BLMHelper.双目标aoe()) return -3;
        if (Core.Me.Level >= 98)
        {
            if (BLMHelper.通晓层数 == 3 && BLMHelper.通晓剩余时间 <= 10000) return 2;
            if (BLMHelper.通晓层数 == 3 && Skill.详述.GetSpell().AbilityCoolDownInNextXgcDsWindow(1)) return 3;
            if (BLMHelper.火状态)
            {
                if (Core.Me.CurrentMp < 800 && BLMHelper.耀星层数 != 6)
                {
                    if (Skill.墨泉.技能CD() < 300 && Skill.墨泉.GetSpell().IsReadyWithCanCast()) return -3;
                    if (Skill.墨泉.GetSpell().AbilityCoolDownInNextXgcDsWindow(2)) return 4;
                }
            }
        }

        if (Core.Me.Level >= 80 && Core.Me.Level < 98)
        {
            if (BLMHelper.通晓层数 == 2 && BLMHelper.通晓剩余时间 < 8000) return 2;
            if (BLMHelper.通晓层数 == 2 && Core.Me.Level >= 86 && Skill.详述.GetSpell().AbilityCoolDownInNextXgcDsWindow(1)) return 3;
        }
        
        
        return -99;
    }
}
