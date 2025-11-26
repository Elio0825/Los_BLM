using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;
using BattleData = los.BLM.SlotResolver.BattleData;

namespace Los.BLM.SlotResolver.Ability
{
    public class 三连咏唱 : ISlotResolver
    {
        private readonly uint _skillId = Skill.三连;

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
            if (Core.Me.Level < 66) return -80;

            if (BattleData.Instance.三连走位)
                return -5;

            if (spell.Charges < 1 || !spell.IsReadyWithCanCast())
                return -1;

            if (BlackMageQT.GetQt("TTK"))
                return 999;
            if (BattleData.Instance.前一gcd is Skill.冰澈 or Skill.玄冰)
                return -10;
            

            
            if (Qt.Instance.GetQt("三连走位"))
            {
                
                var allowedWeaves = Helper.GetAllowedWeavesThisGcd();

                if (Helper.IsMove             
                    && !Helper.可瞬发()       
                    && !Core.Me.IsCasting
                    && allowedWeaves > 0)
                {
                    return 50;
                }
            }

             
            if (Helper.可瞬发())
                return -4;

            
            
            
            if (BLMHelper.火状态)
            {
                
                
                if (BLMHelper.三目标aoe() || BLMHelper.双目标aoe())
                {
                    
                    if (Core.Me.CurrentMp < 800 && BLMHelper.耀星层数 == 6)
                        return -22;
                    
                }
                
                if (Skill.墨泉.技能CD() < 500 ||
                    Skill.墨泉.AbilityCoolDownInNextXgcDsWindow(5))
                    return -8;

                
                if (Core.Me.Level == 100 &&
                    Core.Me.CurrentMp <= 4400 &&
                    Qt.Instance.GetQt("三连进冰")&&
                    BLMHelper.耀星层数 >= 5 &&
                    Skill.即刻.技能CD() > 0 &&                             // 即刻当前在 CD
                    !Skill.即刻.AbilityCoolDownInNextXgcDsWindow(3))        // 且 3 GCD 内不会转好
                {
                    return 1;
                }

                
                return -10;
            }

            if (BLMHelper.冰状态 && BLMHelper.冰层数 < 3)
            {
                
                if (Core.Me.Level < 100)
                    return -100;
                if (BLMHelper.三目标aoe() || BLMHelper.双目标aoe())
                    return -234;
                if (BLMHelper.悖论指示)
                    return -3;
                if (!Qt.Instance.GetQt("三连进冰"))
                    return -200;
                return 2;
            }
            
            return -99;
        }
    }
}
