using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;
using BattleData = los.BLM.SlotResolver.BattleData;

public class 群体58_99 : ISlotResolver
{
    private uint _skillId = 0;

    private bool IsTwoTarget  => BLMHelper.双目标aoe();
    private bool IsThreePlus  => BLMHelper.三目标aoe();

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

        if (level < 58 || level >= 100)
            return -100;

        if (!IsTwoTarget && !IsThreePlus)
            return -101;

        if (BattleData.Instance.正在特殊循环中)
            return -102;

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

    // ✅ 修复：冰针 >= 3 时，如果玄冰刚刚释放，等待状态更新
    // 避免在状态更新延迟期间返回 -203，导致与星灵移位冲突
    if (Skill.玄冰.RecentlyUsed(1000)) // 1秒内刚释放过玄冰
    {
        _skillId = 0;
        // 返回 -201 表示技能不可用，让系统等待状态更新
        // 这样星灵移位有机会在状态更新后使用
        return -201;
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

        
        if (Core.Me.CurrentMp < 800)
        {
            _skillId = 0;
            return -302;
        }

        _skillId = Skill.核爆.GetActionChange();

        var spell = _skillId.GetSpell();
        if (spell == null || !spell.IsReadyWithCanCast())
        {
            _skillId = 0;
            return -301;
        }

        var minHit = IsThreePlus ? 3 : 2;
        if (_skillId.GetAoeHitCount(damageRange: 5) < minHit)
        {
            _skillId = 0;
            return -303;
        }

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
