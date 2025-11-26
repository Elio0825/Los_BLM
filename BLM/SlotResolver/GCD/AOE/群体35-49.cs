using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;
using BattleData = los.BLM.SlotResolver.BattleData;

namespace Los.BLM.SlotResolver.GCD.AOE
{
    public class 群体35_49 : ISlotResolver
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

            // 等级 & 目标数 过滤
            if (level < 35 || level >= 50)
                return -100;
            if (!BLMHelper.三目标aoe())
                return -101;
            if (BattleData.Instance.正在特殊循环中)
                return -102;
            if (Helper.IsMove && !Helper.可瞬发())
                return -99;

            _skillId = GetSkillId(level);
            if (_skillId == 0)
                return -1;

            var spell = GetSpell();
            if (spell == null || !spell.IsReadyWithCanCast())
                return -103;

            // 用技能 ID 当优先级
            return (int)_skillId;
        }

        private uint GetSkillId(int level)
        {
            int mp    = (int)Core.Me.CurrentMp;
            int maxMp = (int)Core.Me.MaxMp;

            uint fire2  = Skill.火二.GetActionChange();
            uint freeze = Skill.冰冻.GetActionChange();
            uint ice4   = Skill.玄冰.GetActionChange(); // 玄冰：只在 40+ 有效

            // 火二的实际蓝耗（带火档修正）
            int fire2Cost = 3000;

            // ========== 无元素态：优先用火二开火态 ==========
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

            // ========== 火态：蓝够就一直火二，不够就转冰 ==========
            if (BLMHelper.火状态)
            {
                // 蓝量足够打一发火二
                if (mp >= fire2Cost)
                {
                    var fireSpell = fire2.GetSpell();
                    if (fireSpell != null && fireSpell.IsReadyWithCanCast())
                        return fire2;
                }

                // 不够打一发火二 -> 冰冻进冰态
                var freezeSpell = freeze.GetSpell();
                if (freezeSpell != null && freezeSpell.IsReadyWithCanCast())
                    return freeze;

                return 0;
            }

            // ========== 冰态 ==========
            if (BLMHelper.冰状态)
            {
                // ---- 40+：有玄冰，按“玄冰回蓝到 9000 再火二”来 ----
                if (level >= 40)
                {
                    if (mp < 9000)
                    {
                        // 优先玄冰，比冰冻回蓝更快
                        var ice4Spell = ice4.GetSpell();
                        if (ice4Spell != null && ice4Spell.IsReadyWithCanCast())
                            return ice4;

                        var freezeSpell = freeze.GetSpell();
                        if (freezeSpell != null && freezeSpell.IsReadyWithCanCast())
                            return freeze;

                        return 0;
                    }

                    // MP ≥ 9000：尝试用火二进火态
                    var fireSpell2 = fire2.GetSpell();
                    if (fireSpell2 != null && fireSpell2.IsReadyWithCanCast())
                        return fire2;

                    // 火二因为各种原因放不出去，就继续在冰里回蓝兜底
                    var ice4Again = ice4.GetSpell();
                    if (ice4Again != null && ice4Again.IsReadyWithCanCast())
                        return ice4;

                    var freezeAgain = freeze.GetSpell();
                    if (freezeAgain != null && freezeAgain.IsReadyWithCanCast())
                        return freeze;

                    return 0;
                }

                // ---- 35–39：没有玄冰，只能靠冰冻回蓝 ----
                // 如果这里也用 mp >= 9000，会有很大概率永远达不到阈值，形成“只会冰冻”的现象。
                // 所以这段直接沿用 50–57 的思路：蓝没满就继续冰冻，蓝满了就火二。
                if (mp < maxMp)
                {
                    var freezeSpell = freeze.GetSpell();
                    if (freezeSpell != null && freezeSpell.IsReadyWithCanCast())
                        return freeze;

                    return 0;
                }

                // 蓝满了 -> 火二进火态
                var fireSpellLow = fire2.GetSpell();
                if (fireSpellLow != null && fireSpellLow.IsReadyWithCanCast())
                    return fire2;

                // 火二打不出去就继续冰冻兜底
                var freezeAgainLow = freeze.GetSpell();
                if (freezeAgainLow != null && freezeAgainLow.IsReadyWithCanCast())
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
