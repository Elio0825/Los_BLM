using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.SlotResolver;
using los.BLM.SlotResolver.Data;
using BattleData = los.BLM.SlotResolver.BattleData;

namespace Los.BLM.SlotResolver.GCD.单体
{
    public class 单体60_71 : ISlotResolver
    {
        private uint _skillId = 0;
        private int _fire4Count = 0;
        private uint _lastSeenGcd = 0;

        private static readonly FireDecisionConfig FireConfig_60_71 = new FireDecisionConfig
        {
            FireSpamSpell    = Skill.火四,
            EntryFireSpell   = Skill.火三,
            FallbackIceSpell = Skill.冰三,
            AllowFirestarter = true,
            ReserveMp        = 0
        };

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

        private void ResetLoop()
        {
            _fire4Count = 0;
            _lastSeenGcd = 0;
        }

        private void SanityFix()
        {
            if (Core.Me.CurrentHp <= 0)
            {
                ResetLoop();
                return;
            }

            if (!BLMHelper.火状态 && !BLMHelper.冰状态)
            {
                ResetLoop();
                return;
            }

            if (!BLMHelper.火状态 && _fire4Count > 0)
            {
                ResetLoop();
            }

            if (BLMHelper.火状态 && BLMHelper.火层数 < 3 && _fire4Count > 0)
            {
                _fire4Count = 0;
            }

            if (Core.Me.CurrentMp <= 3000 && !BLMHelper.冰状态)
            {
                ResetLoop();
            }
        }

        private void UpdateStateFromLastGcd()
        {
            var lastGcd = BattleData.Instance.前一gcd;
            if (lastGcd == 0 || lastGcd == _lastSeenGcd)
                return;

            _lastSeenGcd = lastGcd;

            if (lastGcd == Skill.冰三)
            {
                _fire4Count = 0;
                return;
            }

            if (lastGcd == Skill.火三 && BLMHelper.火状态)
            {
                _fire4Count = 0;
                return;
            }

            if (lastGcd == Skill.火四 && BLMHelper.火状态)
            {
                _fire4Count++;
                return;
            }

            if (BLMHelper.冰状态 && !BLMHelper.火状态)
            {
                _fire4Count = 0;
            }
        }

        private uint GetIcePhaseSkill()
        {
            var lastGcd = BattleData.Instance.前一gcd;

            if (lastGcd == Skill.冰三)
            {
                if (BLMHelper.冰针 < 3)
                    return Skill.冰澈;

                return Skill.火三;
            }

            if (BLMHelper.冰层数 < 3)
                return Skill.冰三;

            if (BLMHelper.冰针 < 3)
                return Skill.冰澈;

            return Skill.火三;
        }

        private uint GetFirePhaseSkill()
        {
            UpdateStateFromLastGcd();

            var mp = (int)Helper.蓝量;

            // 兜底
            if (BLMHelper.火状态 && mp < 800)
                return Skill.冰三;
            
            if (BLMHelper.火层数 < 3)
                return FireConfig_60_71.EntryFireSpell;

            int fire4Cost = FirePhaseHelper.GetFireMpCost(Skill.火四);

            if (mp >= fire4Cost)
                return FireConfig_60_71.FireSpamSpell;

            
            return FireConfig_60_71.FallbackIceSpell;
        }

        private uint GetSkillId()
        {
            SanityFix();

            if (BattleData.Instance.正在特殊循环中)
                return 0;

            if (BLMHelper.火状态)
                return GetFirePhaseSkill();

            if (BLMHelper.冰状态)
                return GetIcePhaseSkill();

            
            return Skill.冰三;
        }

        public int Check()
        {
            if (Core.Me.Level < 60 || Core.Me.Level >= 72)
                return -60;

            
            if (BLMHelper.三目标aoe() || BLMHelper.双目标aoe())
                return -234;

            if (Helper.IsMove && !Helper.可瞬发())
                return -99;

            _skillId = GetSkillId();

            if (_skillId == 0)
                return -1;

            
            var spell = _skillId.GetSpell();
            if (spell == null || !spell.IsReadyWithCanCast())
            {
                _skillId = 0;
                return -2;
            }

            return (int)_skillId;
        }
    }
}
