using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;
using BattleData = los.BLM.SlotResolver.BattleData;
using los.BLM.SlotResolver.GCD.单体;
using Los.BLM.SlotResolver.GCD.单体;

namespace Los.BLM.SlotResolver.Ability
{
    public class 墨泉 : ISlotResolver
    {
        
        private readonly uint _skillId = Skill.墨泉;

        private Spell? GetSpell() => _skillId.GetSpell();

        public int Check()
        {
            
            if (!BLMHelper.火状态) return -2;
            if (BLMHelper.耀星层数 == 6) return -3;
            if (BattleData.Instance.正在特殊循环中) return -4;
            if (BLMHelper.火层数 < 3) return -5;
            if (Core.Me.CurrentMp >= 800) return -6;
            if (!Qt.Instance.GetQt("魔泉")) return -7;
            var spell = GetSpell();
            if (spell == null || !spell.IsReadyWithCanCast()) return -8;
            if (BLMHelper.冰层数 >= 3) return -9;
            var allowedWeaves = Helper.GetAllowedWeavesThisGcd();
            var lastGcd = BattleData.Instance.前一gcd.GetActionChange();
            if (allowedWeaves <= 0 &&
                lastGcd is Skill.核爆 or Skill.耀星)
            {
                allowedWeaves = 1;
            }

            if (allowedWeaves <= 0)
                return -10;
            
            if (BattleData.Instance.前一gcd is Skill.冰澈 or Skill.玄冰 &&
                BattleData.Instance.前一能力技 == Skill.星灵移位)
                return -11;

            
            return 1;
        }

        public void Build(Slot slot)
        {
            var spell = GetSpell();
            if (spell == null)
                return;

            
            slot.Add(spell);

            if (Core.Me.Level >= 100)
            {
                单体100.Instance?.OnManafontUsed();
            }
            else if (Core.Me.Level >= 90)
            {
                单体90_99.Instance?.OnManafontUsed();
            }
        }
    }
}
