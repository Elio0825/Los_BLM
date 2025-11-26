using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;
using BattleData = los.BLM.SlotResolver.BattleData;

namespace Los.BLM.SlotResolver.GCD.AOE
{
    
    public class 雷2 : ISlotResolver
    {
        private uint _skillId = 0;

        private Spell? GetSpell()
        {
            if (_skillId == 0)
                return null;

            
            if (_skillId.IsAoe() && BlackMageQT.GetQt("智能AOE"))
                return _skillId.GetSpellBySmartTarget();

            return _skillId.GetSpell();
        }

        public void Build(Slot slot)
        {
            var spell = GetSpell();
            if (spell != null)
                slot.Add(spell);
        }

        public int Check()
        {
            
            if (Core.Me.Level < 26)
                return -50;
            if (!BLMHelper.双目标aoe() && !BLMHelper.三目标aoe())
                return -100;
            if (!Qt.Instance.GetQt("Dot"))
                return -2;
            if (BattleData.Instance.正在特殊循环中)
                return -4;
            if (!BLMHelper.冰状态)
                return -6;
            if (Core.Me.IsCasting)
                return -30;
            if (!Core.Me.HasAura(Buffs.雷云buff))
                return -7;
            if (!BLMHelper.补dot())
                return -8;
            var aoeThunderId    = Skill.雷二.GetActionChange();
            var aoeThunderSpell = aoeThunderId.GetSpell();
            if (aoeThunderSpell == null)
                return -51;
            if (!aoeThunderSpell.IsReadyWithCanCast())
                return -52;
            _skillId = aoeThunderId;
            var minHit = BLMHelper.三目标aoe() ? 3 : 2;

            if (_skillId.IsAoe() && _skillId.GetAoeHitCount(damageRange: 5) < minHit)
                return -9;
            return 1;
        }
    }
}
