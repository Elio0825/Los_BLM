using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM;
using los.BLM.Helper;
using los.BLM.SlotResolver.Data;

namespace los.BLM.SlotResolver.GCD.单体;


public class 单体100 : ISlotResolver
{
    public static 单体100? Instance { get; private set; }

    public 单体100()
    {
        Instance = this;
    }
    private uint _skillId = 0;
    private int _fire4Count = 0;
    private bool _afParadoxUsed = false;
    private uint _lastSeenGcd = 0;
    private const int FireParadoxMpCost = 1600;

    
    private static readonly FireDecisionConfig FireConfig_100 = new FireDecisionConfig
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

    private void ResetLoop()
    {
        _fire4Count = 0;
        _afParadoxUsed = false;
        _lastSeenGcd = 0;
        BattleData.Instance.进冰瞬发窗口 = false;
    }
    public void OnManafontUsed()
    {
        
        _afParadoxUsed = false;
        if (_fire4Count < BLMHelper.耀星层数)
            _fire4Count = BLMHelper.耀星层数;
    }

    private void SanityFix()
    {
        
        if (!Helper.Helper.是否在战斗中())
        {
            ResetLoop();
            return;
        }
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
        if (!BLMHelper.火状态 && (_fire4Count > 0 || _afParadoxUsed))
        {
            ResetLoop();
        }
        if (_fire4Count < BLMHelper.耀星层数)
        {
            _fire4Count = BLMHelper.耀星层数;
        }
        if (Helper.Helper.蓝量 <= 3000 && !BLMHelper.冰状态)
        {
            ResetLoop();
            
        }
    }

    #region 状态更新

    private void UpdateStateFromLastGcd()
    {
        var lastGcd = BattleData.Instance.前一gcd;
        if (lastGcd == 0 || lastGcd == _lastSeenGcd)
            return;

        _lastSeenGcd = lastGcd;
        if (lastGcd == Skill.绝望 && BLMHelper.火状态)
        {
            BattleData.Instance.进冰瞬发窗口 = true;
        }
        if (lastGcd == Skill.冰三)
        {
            BattleData.Instance.进冰瞬发窗口 = false;
        }
        if (lastGcd is Skill.冰三 or Skill.绝望 or Skill.耀星)
        {
            _fire4Count = 0;
            _afParadoxUsed = false;
            return;
        }
        if (lastGcd == Skill.火三 || (lastGcd == Skill.悖论 && BLMHelper.火状态))
        {
            _fire4Count = 0;
            _afParadoxUsed = lastGcd == Skill.悖论;
            return;
        }
        if (lastGcd == Skill.火四 && BLMHelper.火状态)
        {
            _fire4Count++;
            return;
        }
        if (lastGcd == Skill.悖论 && BLMHelper.火状态)
        {
            _afParadoxUsed = true;
            return;
        }
        if (BLMHelper.冰状态 && !BLMHelper.火状态)
        {
            _fire4Count = 0;
            _afParadoxUsed = false;
        }
    }

    #endregion

    #region 冰

 
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
            if (Helper.Helper.IsMove && Helper.Helper.可瞬发())
                return Skill.悖论;
            var lastGcd = BattleData.Instance.前一gcd;
            bool lastWasIce4 = lastGcd == Skill.冰澈;
            bool readyToLeaveIce =
                BLMHelper.冰层数 == 3 &&
                BLMHelper.冰针 == 3 &&
                Core.Me.CurrentMp >= Core.Me.MaxMp * 0.95;

            if (lastWasIce4 && readyToLeaveIce)
                return Skill.悖论;
        }
        
        return 0;
    }

    #endregion

    #region 火（使用 FirePhaseHelper 做火4 判定 + 预留绝望蓝）

    private uint GetFirePhaseSkill()
    {
        UpdateStateFromLastGcd();

        var setting  = BlackMageSetting.Instance;
        bool compress = setting.压缩火悖论;

        bool moving     = Helper.Helper.IsMove && Helper.Helper.可瞬发();
        bool hasParadox = BLMHelper.悖论指示;
        int  mp         = (int)Helper.Helper.蓝量;

        const int DespairMinMp = 800;
        if (BLMHelper.火层数 < 3)
        {
            if (hasParadox && !FirePhaseHelper.HasFirestarter && mp >= FireParadoxMpCost)
                return Skill.悖论;
            return FireConfig_100.EntryFireSpell;
        }
        

        bool soulFull    = BLMHelper.耀星层数 >= 6;
        bool soulNotFull = !soulFull;
        bool lowMpForF4 = FirePhaseHelper.IsLowMpForFire(FireConfig_100);
        if (!_afParadoxUsed &&
            hasParadox &&
            mp >= FireParadoxMpCost &&
            _fire4Count >= 3 && _fire4Count < 6)
        {
            if (!compress || moving)
                return Skill.悖论;
        }
        
        if (soulNotFull && _fire4Count < 6 && !lowMpForF4)
            return FireConfig_100.FireSpamSpell; 
        if (compress &&
            !_afParadoxUsed &&
            hasParadox &&
            mp >= FireParadoxMpCost &&
            !moving)
        {
            return Skill.悖论;
        }
        if (soulFull)
            return Skill.耀星;

        if (mp >= DespairMinMp)
            return Skill.绝望;
        return FireConfig_100.FallbackIceSpell; // 冰三
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
        return Skill.火三;
    }

    public int Check()
    {
        if (Core.Me.Level != 100)
            return -100;
        if (BLMHelper.三目标aoe() || BLMHelper.双目标aoe())
            return -234;
        if (Helper.Helper.IsMove && !Helper.Helper.可瞬发())
            return -99;

        _skillId = GetSkillId();

        if (_skillId == 0)
            return -1;
        return (int)_skillId;
    }
}