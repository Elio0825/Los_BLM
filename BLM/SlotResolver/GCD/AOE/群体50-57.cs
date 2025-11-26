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
   
    public class 群体50_57 : ISlotResolver
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

            
            if (level < 50 || level >= 58)
                return -100;
            if (!BLMHelper.三目标aoe())
                return -101;
            if (BattleData.Instance.正在特殊循环中)
                return -102;
            if (Helper.IsMove && !Helper.可瞬发())
                return -99;
            _skillId = GetSkillId();

            if (_skillId == 0)
                return -1;
            var spell = GetSpell();
            if (spell == null || !spell.IsReadyWithCanCast())
                return -103;
            return (int)_skillId;
        }

        private uint GetSkillId()
        {
            int mp    = (int)Core.Me.CurrentMp;
            int maxMp = (int)Core.Me.MaxMp;

            
            uint fire2  = Skill.火二.GetActionChange();
            uint flare  = Skill.核爆;
            uint freeze = Skill.冰冻.GetActionChange();
            uint ice4   = Skill.玄冰.GetActionChange();
            
            int fire2Cost = FirePhaseHelper.GetFireMpCost(Skill.火二);
            
            if (!BLMHelper.火状态 && !BLMHelper.冰状态)
            {
                var fireSpell = fire2.GetSpell();
                if (mp >= fire2Cost && fireSpell != null && fireSpell.IsReadyWithCanCast())
                    return fire2;

                var freezeSpell = freeze.GetSpell();
                if (freezeSpell != null && freezeSpell.IsReadyWithCanCast())
                    return freeze;

                return 0;
            }
            
            if (BLMHelper.火状态)
            {
                
                if (mp >= fire2Cost)
                {
                    var fireSpell = fire2.GetSpell();
                    if (fireSpell != null && fireSpell.IsReadyWithCanCast())
                        return fire2;
                }

                
                if (mp >= 800)
                {
                    var flareSpell = flare.GetSpell();
                    if (flareSpell != null && flareSpell.IsReadyWithCanCast())
                        return flare;
                }
                
                var freezeSpell2 = freeze.GetSpell();
                if (freezeSpell2 != null && freezeSpell2.IsReadyWithCanCast())
                    return freeze;
                
                return 0;
            }
            
            if (BLMHelper.冰状态)
            {
                
                if (mp < maxMp)
                {
                    var ice4Spell = ice4.GetSpell();
                    if (ice4Spell != null && ice4Spell.IsReadyWithCanCast())
                        return ice4;

                    
                    var freezeSpell = freeze.GetSpell();
                    if (freezeSpell != null && freezeSpell.IsReadyWithCanCast())
                        return freeze;

                    return 0;
                }
                
                var fireSpell2 = fire2.GetSpell();
                if (fireSpell2 != null && fireSpell2.IsReadyWithCanCast())
                    return fire2;
                
                var ice4Again = ice4.GetSpell();
                if (ice4Again != null && ice4Again.IsReadyWithCanCast())
                    return ice4;

                var freezeAgain = freeze.GetSpell();
                if (freezeAgain != null && freezeAgain.IsReadyWithCanCast())
                    return freeze;

                return 0;
            }

            return 0;
        }

        public void Build(Slot slot)
        {
            var spell = GetSpell();
            if (spell != null)
                slot.Add(spell);
        }
    }
}
