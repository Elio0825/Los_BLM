using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM;
using los.BLM.Helper;
using los.BLM.SlotResolver;
using los.BLM.SlotResolver.Data;
using BattleData = los.BLM.SlotResolver.BattleData;

namespace Los.BLM.SlotResolver.GCD.单体
{
  
    public class 单体90_99 : ISlotResolver
    {
        public static 单体90_99? Instance { get; private set; }

        public 单体90_99()
        {
            Instance = this;
        }
        private uint _skillId = 0;
        private int _fire4Count = 0;
        private bool _afParadoxUsed = false;
        private uint _lastSeenGcd = 0;
        private const int FireParadoxMpCost = 1600;
        private static readonly FireDecisionConfig FireConfig_90 = new FireDecisionConfig
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
            _afParadoxUsed = false;
            _lastSeenGcd = 0;
        }
        public void OnManafontUsed()
        {
            
            
            _afParadoxUsed = false;
            
            if (_fire4Count < BLMHelper.耀星层数)
                _fire4Count = BLMHelper.耀星层数;
        }

        private void SanityFix()
        {
            // 脱战：整轮重置，避免带着上一场战斗的火四计数进来
            if (!Helper.是否在战斗中())
            {
                ResetLoop();
                return;
            }

            // 死亡：直接重置
            if (Core.Me.CurrentHp <= 0)
            {
                ResetLoop();
                return;
            }

            // 不在火也不在冰：比如刚进本/刚复活
            if (!BLMHelper.火状态 && !BLMHelper.冰状态)
            {
                ResetLoop();
                return;
            }

            // 已经离开火态了但内部还记着火相计数
            if (!BLMHelper.火状态 && (_fire4Count > 0 || _afParadoxUsed))
            {
                ResetLoop();
            }

            // 蓝很低且不在冰态：大概率刚复活或循环完全断了，下一轮直接从冰三起手
            if (Helper.蓝量 <= 3000 && !BLMHelper.冰状态)
            {
                ResetLoop();
            }
        }

        /// <summary>
        /// 用上一发 GCD 更新火四计数/火中悖论等状态
        /// </summary>
        private void UpdateStateFromLastGcd()
        {
            var lastGcd = BattleData.Instance.前一gcd;
            if (lastGcd == 0 || lastGcd == _lastSeenGcd)
                return;

            _lastSeenGcd = lastGcd;

            // 一轮循环的“硬重置点”：打出冰三 或 绝望
            if (lastGcd is Skill.冰三 or Skill.绝望)
            {
                _fire4Count = 0;
                _afParadoxUsed = false;
                return;
            }

            // 刚刚开始火相：火三 或 火中悖论
            if (lastGcd == Skill.火三 || (lastGcd == Skill.悖论 && BLMHelper.火状态))
            {
                _fire4Count = 0;
                _afParadoxUsed = lastGcd == Skill.悖论;
                return;
            }

            // 火相中统计火四数量
            if (lastGcd == Skill.火四 && BLMHelper.火状态)
            {
                _fire4Count++;
                return;
            }

            // 标记已经用过火中悖论
            if (lastGcd == Skill.悖论 && BLMHelper.火状态)
            {
                _afParadoxUsed = true;
                return;
            }

            // 回到冰相时，顺便清掉火相记忆
            if (BLMHelper.冰状态 && !BLMHelper.火状态)
            {
                _fire4Count = 0;
                _afParadoxUsed = false;
            }
        }

        #endregion

        #region 冰（B3 → B4 → P → 进火）

        private uint GetIcePhaseSkill()
        {
            var setting = BlackMageSetting.Instance;

            
            if (BLMHelper.冰层数 < 3)
                return Skill.冰三;

            if (BLMHelper.冰针 < 3)
                return Skill.冰澈;
            
            if (BLMHelper.悖论指示)
            {
                
                if (!setting.压缩冰悖论)
                    return Skill.悖论;
                
                if (Helper.IsMove && Helper.可瞬发())
                    return Skill.悖论;
                var lastGcd = BattleData.Instance.前一gcd.GetActionChange();
                bool lastWasIce4 = lastGcd == Skill.冰澈;
                bool readyToLeaveIce =
                    BLMHelper.冰层数 == 3 &&
                    BLMHelper.冰针 == 3 &&
                    Core.Me.CurrentMp >= Core.Me.MaxMp * 0.95;
                if (lastWasIce4 && readyToLeaveIce)
                    return Skill.悖论;
            }

            
            return Skill.火三;
        }

        #endregion

        #region 火（F3 → F4×3 → P → F4×3 → 绝望）

        private uint GetFirePhaseSkill()
        {
            UpdateStateFromLastGcd();

            var setting  = BlackMageSetting.Instance;
            bool compress = setting.压缩火悖论;

            bool moving     = Helper.IsMove && Helper.可瞬发();
            bool hasParadox = BLMHelper.悖论指示;
            int  mp         = (int)Helper.蓝量;

            const int DespairMinMp = 800;

            
            if (BLMHelper.火层数 < 3)
            {
                
                if (hasParadox &&
                    !FirePhaseHelper.HasFirestarter &&
                    mp >= FireParadoxMpCost)
                {
                    return Skill.悖论;
                }
                
                return FireConfig_90.EntryFireSpell; // 火三
            }
            
            bool lowMpForF4 = FirePhaseHelper.IsLowMpForFire(FireConfig_90);
            
            if (!_afParadoxUsed &&
                hasParadox &&
                mp >= FireParadoxMpCost &&
                _fire4Count >= 3 && _fire4Count < 6)
            {
                if (!compress || moving)
                {
                    
                    
                    return Skill.悖论;
                }
                
            }
            if (!lowMpForF4 && _fire4Count < 6)
                return FireConfig_90.FireSpamSpell; // 火四
            if (compress &&
                !_afParadoxUsed &&
                hasParadox &&
                mp >= FireParadoxMpCost &&
                !moving)
            {
                return Skill.悖论;
            }
            
            if (mp >= DespairMinMp)
                return Skill.绝望;
            
            return FireConfig_90.FallbackIceSpell; // 冰三
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
            
            if (Core.Me.Level < 90 || Core.Me.Level >= 100)
                return -90;
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