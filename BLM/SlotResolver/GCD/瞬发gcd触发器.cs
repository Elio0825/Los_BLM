using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.QtUI;

namespace Los.BLM.SlotResolver.GCD;

public class 瞬发gcd触发器 : ISlotResolver
{
    private uint _skillId = 0;
    private Spell? GetSpell()
    {
        
        if (_skillId.IsAoe()) return BlackMageQT.GetQt("智能aoe目标")  ? _skillId.GetSpellBySmartTarget() : _skillId.GetSpell();
        return  _skillId.GetSpell();
    }
    public int Check()
    {
        if (Helper.可瞬发()) return -2;
        if (!los.BLM.SlotResolver.BattleData.Instance.需要瞬发gcd) return -3;
        _skillId = BLMHelper.可用瞬发();
        if (_skillId == 0) return -4;
        if (_skillId.RecentlyUsed()) return -5;
        return 1;
    }


    public void Build(Slot slot)
    {
        var spell = GetSpell();
        if (spell != null)
        {
            slot.Add(spell);
            los.BLM.SlotResolver.BattleData.Instance.需要瞬发gcd = false;
        }
    }
}