using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;

namespace Los.BLM.SlotResolver.GCD;

public class TTK : ISlotResolver
{
    private uint _skillId = 0;
    private Spell? GetSpell()
    {
        return _skillId.GetSpell();
    }
    public void Build(Slot slot)
    {
        var spell = GetSpell();
        if (spell != null) 
            slot.Add(spell);
    }

    private uint GetSkillId()
    {
        if (!Qt.Instance.GetQt("TTK")) return 0;
        if (BLMHelper.通晓层数 > 0) return BLMHelper.三目标aoe() || BLMHelper.双目标aoe() ? Skill.秽浊 : Skill.异言;
        if (BLMHelper.悖论指示) return Skill.悖论;
        if (BLMHelper.耀星层数 == 6) return Skill.耀星;
        if (Core.Me.CurrentMp >= 800 && BLMHelper.火状态 && Core.Me.Level >= 100 ) return Skill.绝望;
        return 0;
    }
    public int Check()
    {
        _skillId = GetSkillId();
        if (_skillId == 0) return -1;
        return (int)_skillId;
    }
}