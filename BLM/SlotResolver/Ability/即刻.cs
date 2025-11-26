using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;

namespace Los.BLM.SlotResolver.Ability
{
    public class 即刻 : ISlotResolver
    {
        private readonly uint _skillId = Skill.即刻;

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
            if (!spell.IsReadyWithCanCast())
                return -1;
            if (Qt.Instance.GetQt("TTK"))
                return 999;
            if (Helper.可瞬发())
                return -3;
            if (Core.Me.Level < 50) return -80;

            
            if (BLMHelper.冰状态 && BLMHelper.冰层数 < 3)
            {
                
                if (!Qt.Instance.GetQt("即刻进冰"))
                    return -200;
                if (BLMHelper.三目标aoe() || BLMHelper.双目标aoe())
                    return -30;
                if (Skill.冰三.RecentlyUsed() || Skill.冰冻.RecentlyUsed())
                    return -4;
                return 5;//2
            }

            return -99;
        }
    }
}
