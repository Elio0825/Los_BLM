using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using Los.BLM.SlotResolver;
using los.BLM.SlotResolver.Data;
using BattleData = los.BLM.SlotResolver.BattleData;

namespace Los.BLM.SlotResolver.GCD.单体
{
   
    public class 单体1_34 : ISlotResolver
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
            
            if (BattleData.Instance.正在特殊循环中)
                return 0;

            var mp    = (int)Core.Me.CurrentMp;
            var maxMp = (int)Core.Me.MaxMp;
            var fireCost = FirePhaseHelper.GetFireMpCost(Skill.火一);
            
            if (mp >= maxMp)
                return Skill.火一;
            if (BLMHelper.火状态 && mp >= fireCost)
                return Skill.火一;
            return Skill.冰一;
        }

        public int Check()
        {
            
            if (Core.Me.Level < 1 || Core.Me.Level >= 35)
                return -1;
            if (Core.Me.Level >= 12 && BLMHelper.三目标aoe())
                return -234;
            if (Helper.IsMove && !Helper.可瞬发())
                return -99;

            _skillId = GetSkillId();

            if (_skillId == 0)
                return -1;
            
            return (int)_skillId;
        }
    }
}