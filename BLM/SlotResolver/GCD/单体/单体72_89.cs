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
    
    public class 单体72_89 : ISlotResolver
    {
        private uint _skillId = 0;

        /// 当前这一轮已经打了多少发火四
        private int _fire4Count = 0;

        /// 上一次 GCD
        private uint _lastSeenGcd = 0;

        /// <summary>
        /// 72–89 火：
        /// - 进火：火三
        /// - 回冰：冰三
        /// - 预留蓝：至少保证一发绝望
        private static readonly FireDecisionConfig FireConfig_72_89 = new FireDecisionConfig
        {
            FireSpamSpell    = Skill.火四,
            EntryFireSpell   = Skill.火三,
            FallbackIceSpell = Skill.冰三,
            AllowFirestarter = true, 
            ReserveMp        = 800
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

        #region 状态机

        private void ResetLoop()
        {
            _fire4Count = 0;
            _lastSeenGcd = 0;
        }

        
        /// 自修正，防止死亡/断轴后状态乱掉
        
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
            
            if (lastGcd is Skill.绝望 or Skill.冰三)
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

        #endregion

        #region 冰：B3 → B4 → F3

        private uint GetIcePhaseSkill()
        {
            
            if (BLMHelper.冰层数 < 3)
                return Skill.冰三;

            
            if (BLMHelper.冰针 < 3)
                return Skill.冰澈;
            
            return Skill.火三;
        }

        #endregion

        #region 火：F3 → F4×7 → 绝望

        private uint GetFirePhaseSkill()
        {
            UpdateStateFromLastGcd();

            const int DespairMinMp = 800;
            var mp = (int)Helper.蓝量;
            
            if (BLMHelper.火状态 && mp < DespairMinMp)
                return Skill.冰三;
            if (BLMHelper.火层数 < 3)
                return FireConfig_72_89.EntryFireSpell; 
            
            bool lowMpForF4 = FirePhaseHelper.IsLowMpForFire(FireConfig_72_89);

            
            if (_fire4Count < 7 && !lowMpForF4)
                return FireConfig_72_89.FireSpamSpell; // 火四

            
            if (mp >= DespairMinMp)
                return Skill.绝望;
            
            return FireConfig_72_89.FallbackIceSpell; // 冰三
        }

        #endregion

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
            
            if (Core.Me.Level < 72 || Core.Me.Level >= 90)
                return -72;
            if (BLMHelper.三目标aoe() || BLMHelper.双目标aoe())
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