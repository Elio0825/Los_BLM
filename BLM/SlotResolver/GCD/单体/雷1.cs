using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;
using BattleData = los.BLM.SlotResolver.BattleData;

namespace Los.BLM.SlotResolver.GCD.单体;

public class 雷1 : ISlotResolver
{
    private readonly uint _skillId = Skill.雷一.GetActionChange();
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
        if (BLMHelper.双目标aoe()||BLMHelper.三目标aoe()&&Core.Me.Level>26) return -100;
        if (!Qt.Instance.GetQt("Dot")) return -3;
        if (Qt.Instance.GetQt("TTK")) return -5;
        if (BattleData.Instance.正在特殊循环中) return -4;
        if (Core.Me.IsCasting) return -30;
        if (BLMHelper.补dot() && Core.Me.HasAura(Buffs.雷云buff)) return 1;
        return -99;
    }
}
