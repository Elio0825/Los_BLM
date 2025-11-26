using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.SlotResolver.Data;
using BattleData = los.BLM.SlotResolver.BattleData;

namespace Los.BLM.SlotResolver.GCD.单体
{
   
    public class 单体35_59 : ISlotResolver
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

        #region 冰：B3 → B1 星灵 → F3

        private uint GetIcePhaseSkill()
        {
            
            if (BLMHelper.冰层数 < 3)
                return Skill.冰三;
            
            if (Core.Me.CurrentMp < Core.Me.MaxMp)
                return Skill.冰一;
            
            return Skill.火三;
        }

        #endregion

        #region 火：F3 → 火一 星灵（中途火苗火三）→ 蓝不够就 B3

        private uint GetFirePhaseSkill()
        {
            if (BLMHelper.火层数 < 3)
                return Skill.火三;
            if (BLMHelper.有火苗)
                return Skill.火三;
            if (Helper.蓝量 >= FirePhaseHelper.GetFireMpCost(Skill.火一))
                return Skill.火一;
            return Skill.冰三;
        }

        #endregion

        private uint GetSkillId()
        {
            
            if (BattleData.Instance.正在特殊循环中)
                return 0;

            if (BLMHelper.火状态)
                return GetFirePhaseSkill();

            if (BLMHelper.冰状态)
                return GetIcePhaseSkill();
            return Skill.火三;
        }

        public int Check()
        {
            
            if (Core.Me.Level < 35 || Core.Me.Level >= 60)
                return -35;

            
            if (BLMHelper.三目标aoe())
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
