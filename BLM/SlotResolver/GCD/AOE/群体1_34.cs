using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver;
using los.BLM.SlotResolver.Data;
using BattleData = los.BLM.SlotResolver.BattleData;

namespace Los.BLM.SlotResolver.GCD.AOE
{
    
    public class 群体1_34 : ISlotResolver
    {
        private uint _skillId = 0;

        private Spell? GetSpell()
        {
            if (_skillId == 0)
                return null;

            
            if (_skillId.IsAoe() && Qt.Instance.GetQt("智能AOE"))
                return _skillId.GetSpellBySmartTarget();

            return _skillId.GetSpell();
        }

        public int Check()
        {
            var level = Core.Me.Level;
            
            if (level < 12 || level >= 35)
                return -100;
            if (!BLMHelper.三目标aoe())
                return -101;

            if (BattleData.Instance.正在特殊循环中)
                return -102;
            if (Helper.IsMove && !Helper.可瞬发())
                return -99;

            _skillId = GetSkillId();

            var spell = GetSpell();
            if (spell == null || !spell.IsReadyWithCanCast())
                return -103;
            return (int)_skillId;
        }
        
        private uint GetSkillId()
        {
            var level = Core.Me.Level;
            var mp    = (int)Core.Me.CurrentMp;
            var maxMp = (int)Core.Me.MaxMp;
            var iceId = GetIceAoESkillId(level);
            var fireId = GetFireAoESkillId(level);
            uint fireBaseId = level >= 18 ? Skill.火二 : Skill.火一;
            var  fireCost   = FirePhaseHelper.GetFireMpCost(fireBaseId);
            if (BLMHelper.冰状态)
            {
                if (mp < maxMp)
                    return iceId;
                
                return fireId;
            }
            
            if (BLMHelper.火状态)
            {
                if (mp >= fireCost)
                    return fireId;
                return iceId;
            }

            // ===== 无冰无火：先尝试火二，不够就冰冻 =====
            if (mp >= fireCost)
                return fireId;

            return iceId;
        }


        private uint GetIceAoESkillId(int level)
        {
            if (level >= 12)
                return Skill.冰冻.GetActionChange();

            
            return Skill.冰一;
        }

        
        private uint GetFireAoESkillId(int level)
        {
            if (level >= 18)
                return Skill.火二.GetActionChange();

            
            return Skill.火一;
        }

        public void Build(Slot slot)
        {
            var spell = GetSpell();
            if (spell != null)
                slot.Add(spell);
        }
    }
}