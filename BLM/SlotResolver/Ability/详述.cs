using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.SlotResolver.Data;

namespace Los.BLM.SlotResolver.Ability;

public class 详述 : ISlotResolver
{
    private readonly uint _skillId = Skill.详述;
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
        if (Core.Me.Level < 86) return -6;
        if (!_skillId.GetSpell().IsReadyWithCanCast()) return -1;
        if (Core.Me.Level >= 98)
        {
            if (BLMHelper.通晓层数 == 3) return -2;
            if (BLMHelper.通晓层数 == 2)
            {
                if (BLMHelper.通晓剩余时间 < 4000) return -3;
            }
            if (GCDHelper.GetGCDCooldown() < 500) return -4;
            return 1;
        }

        if (Core.Me.Level < 98)
        {
            if (BLMHelper.通晓层数 == 2) return -2;
            if (BLMHelper.通晓层数 == 1)
            {
                if (BLMHelper.通晓剩余时间 < 4000) return -3;
            }
            if (GCDHelper.GetGCDCooldown() < 500) return -4;
            return 1;
        }

        return -99;
    }
}
