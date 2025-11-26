using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;
using BattleData = los.BLM.SlotResolver.BattleData;

public class 群体100 : ISlotResolver
{
    private uint _skillId = 0;
    private bool IsTwoTarget  => BLMHelper.双目标aoe();
    private bool IsThreePlus  => BLMHelper.三目标aoe();
    private bool _wasInFireLast = false;
    private int  _flareCountInThisFire = 0;

    private Spell? GetSpell()
    {
        if (_skillId == 0)
            return null;
        if (_skillId.IsAoe())
        {
            return Qt.Instance.GetQt("智能AOE")
                ? _skillId.GetSpellBySmartTarget()
                : _skillId.GetSpell();
        }

        return _skillId.GetSpell();
    }

    public int Check()
    {
        var level = Core.Me.Level;
        
        if (level < 100)
            return -100;
        
        if (!IsTwoTarget && !IsThreePlus)
            return -101;
        
        if (BattleData.Instance.正在特殊循环中)
            return -102;
        bool inFire = BLMHelper.火状态;
        if (inFire != _wasInFireLast)
        {
            _wasInFireLast        = inFire;
            _flareCountInThisFire = 0;
        }
        
        if (Helper.IsMove && !Helper.可瞬发())
            return -99;
        if (!BLMHelper.冰状态 && !BLMHelper.火状态)
        {
            _skillId = Skill.冰冻.GetActionChange();
            var spell = _skillId.GetSpell();
            if (spell != null && spell.IsReadyWithCanCast())
                return (int)_skillId;

            return -104;
        }

        if (BLMHelper.冰状态)
            return CheckIcePhase();

        if (BLMHelper.火状态)
            return CheckFirePhase();

        

        return -103;
    }
    
    private int CheckIcePhase()
    {
        
        if (Helper.IsMove && !Helper.可瞬发())
        {
            _skillId = 0;
            return -211;
        }
        
        if (BLMHelper.冰针 < 3 )
        {
            
            uint iceSkillId = Skill.玄冰.GetActionChange();

            _skillId = iceSkillId;

            var spell = _skillId.GetSpell();
            if (spell == null || !spell.IsReadyWithCanCast())
                return -201;
            
            if (IsThreePlus && _skillId.IsAoe())
            {
                if (_skillId.GetAoeHitCount(damageRange: 5) < 3)
                    return -202;
            }

            return (int)_skillId;
        }
        
        _skillId = 0;
        return -203;
    }

    
    private int CheckFirePhase()
    {
        
        if (Helper.IsMove && !Helper.可瞬发())
        {
            _skillId = 0;
            return -311;
        }

        var mp      = Core.Me.CurrentMp;
        var minHit  = IsThreePlus ? 3 : 2;
        var soulFull = BLMHelper.耀星层数 >= 6;

        
        if (soulFull)
        {
            var flareStarId = Skill.耀星.GetActionChange();
            _skillId        = flareStarId;

            var flareStar = _skillId.GetSpell();
            if (flareStar != null &&
                flareStar.IsReadyWithCanCast() &&
                (!_skillId.IsAoe() || _skillId.GetAoeHitCount(damageRange: 5) >= minHit))
            {
                
                return (int)_skillId;
            }
            
        }

        
        if (mp < 800)
        {
            _skillId = 0;
            return -302;
        }

        _skillId = Skill.核爆.GetActionChange();

        var flareSpell = _skillId.GetSpell();
        if (flareSpell == null || !flareSpell.IsReadyWithCanCast())
        {
            _skillId = 0;
            return -301;
        }

        if (_skillId.IsAoe() && _skillId.GetAoeHitCount(damageRange: 5) < minHit)
        {
            _skillId = 0;
            return -303;
        }

        
        _flareCountInThisFire = Math.Clamp(_flareCountInThisFire + 1, 0, 3);

        return (int)_skillId;
    }


    public void Build(Slot slot)
    {
        var spell = GetSpell();
        if (spell == null)
            return;

        slot.Add(spell);
    }
}
