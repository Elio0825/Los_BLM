using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Los;
using los.BLM.SlotResolver.Data;
using los.BLM.QtUI;
using BattleData = los.BLM.SlotResolver.BattleData; 

namespace los.BLM.Helper;

public static class Helper
{
    public const string AuthorName = "Los";
    /// <summary>
    /// 当前是否处于“开启减少动画锁”模式：
    /// </summary>
    public static bool 减少动画锁环境 =>
        BlackMageSetting.Instance != null && BlackMageSetting.Instance.动画锁模式 == 1;
    private static int _GcdDuration = 0;


    public static bool 是否在副本中()
    {
        return Core.Resolve<MemApiCondition>().IsBoundByDuty();
    }
    public static long 蓝量 => Core.Me.CurrentMp;
    public static bool 可读条()
    {
        return !IsMove || 可瞬发();
    }

    public static double 复唱时间() => Core.Resolve<MemApiSpell>().GetGCDDuration();
    public static bool 可瞬发() => Core.Me.HasAura(Buffs.即刻Buff) || Core.Me.HasAura(Buffs.三连Buff);
    public static bool 是否在战斗中()
    {
        return Core.Me.InCombat();
    }
    
    public static int GetAuraTimeLeft(uint buffId) => Core.Resolve<MemApiBuff>().GetAuraTimeleft(Core.Me, buffId, true);
    
    public static void SendTips(string msg, int s = 1, int time = 3000) => Core.Resolve<MemApiChatMessage>()
        .Toast2(msg, s, time);

    public static bool IsMove => MoveHelper.IsMoving();

    
    public static GeneralSettings GlobalSettings => SettingMgr.GetSetting<GeneralSettings>();

    
    public static uint GetTerritoyId => Core.Resolve<MemApiMap>().GetCurrTerrId();

    
    public static uint GetActionChange(this uint spellId) => Core.Resolve<MemApiSpell>().CheckActionChange(spellId);
    public static uint AdaptiveId(this uint spellId) => Core.Resolve<MemApiSpell>()
        .CheckActionChange(spellId);
    public static bool IsUnlockWithRoleSkills(this Spell spell) {
        // dirty fix for now; need better ways to detect if a role skill is unlocked
        return SpellsDef.RoleSkills.Contains(spell.Id)
               || spell.IsUnlock();
    }
    public static bool CheckInHPQueue(this Spell spell) {
        if (spell.IsAbility()) {
            var all = HPQueueToStrList(AI.Instance.BattleData.HighPrioritySlots_OffGCD);
            return all.Contains(spell.Name);
        } else {
            var all = HPQueueToStrList(AI.Instance.BattleData.HighPrioritySlots_GCD);
            return all.Contains(spell.Name);
        }
    }

public static int GetAllowedWeavesThisGcd()
{
    var lastGcdId = BattleData.Instance.前一gcd;
    if (lastGcdId == 0)
        return 0;
    
    var actionId = lastGcdId.GetActionChange();

    int baseAllowed;
    
    if (IsInstantWeaveWindowSpell(actionId))
    {
        baseAllowed = 2;
    }
    else if (IsPhaseChangeWeaveSpell(actionId))
    {
        baseAllowed = 1;
    }
    else
    {
        
        baseAllowed = 0;
    }

    
    if (减少动画锁环境
        && baseAllowed == 0
        && IsNormalCastGcd(actionId))
    {
       
        return 1;
    }

    return baseAllowed;
}


private static bool IsInstantWeaveWindowSpell(uint actionId)
{
    return actionId is
        Skill.异言   // Xenoglossy
        or Skill.秽浊 // Foul
        or Skill.悖论 // Paradox
        or Skill.绝望; // Despair
}


private static bool IsPhaseChangeWeaveSpell(uint actionId)
{
    return actionId is
        Skill.火三   // Fire III
        or Skill.冰三 // Blizzard III
        
        // or Skill.火二
        // or Skill.冰二
        ;
}


private static bool IsNormalCastGcd(uint actionId)
{
    var spell = actionId.GetSpell();
    if (spell == null)
        return false;

   
    if (spell.IsAbility())
        return false;

    
    if (IsInstantWeaveWindowSpell(actionId))
        return false;

    if (IsPhaseChangeWeaveSpell(actionId))
        return false;

    return true;
}

    private static List<string> HPQueueToStrList(Queue<Slot> src) {
        List<string> result = [];
        result.AddRange(collection: src.SelectMany(slot => slot.Actions)
            .Select(slotAction => slotAction.Spell.Name));
        return result;
    }
    public static bool CheckInHPQueueTop(this Spell spell) {
        if (spell.IsAbility()) {
            var all = HPQueueToStrList(AI.Instance.BattleData.HighPrioritySlots_OffGCD);
            return all.Count > 0 && all[0] == spell.Name;
        } else {
            var all = HPQueueToStrList(AI.Instance.BattleData.HighPrioritySlots_GCD);
            return all.Count > 0 && all[0] == spell.Name;
        }
    }

    
    public static bool TargetIsBoss => Core.Me.GetCurrTarget().IsBoss();

    public static bool TargetIsDummy => Core.Me.GetCurrTarget().IsDummy();
    public static bool TargetIsBossOrDummy => TargetIsBoss || TargetIsDummy;
    public static uint GetTerritoryId => Core.Resolve<MemApiMap>().GetCurrTerrId();
    public static bool InCasualDutyNonBoss =>
        Core.Resolve<MemApiDuty>().InMission
        && Core.Resolve<MemApiDuty>().DutyMembersNumber() is 4 or 24 
        && GetTerritoryId is not (1048 or 1045 or 1046 or 1047)
        && !TargetIsBossOrDummy
        && !Core.Resolve<MemApiDuty>().InBossBattle;
    public static int HighPrioritySlotCheckFunc(SlotMode mode, Slot slot)
{
    
    if (mode != SlotMode.OffGcd)
        return 1;

    
    var allowedWeaves = GetAllowedWeavesThisGcd();

    // 上一个 GCD
    var lastGcdId = BattleData.Instance.前一gcd;
    uint lastActionId = lastGcdId == 0 ? 0u : lastGcdId.GetActionChange();

    
    uint nextAbilityId = slot.Actions.Count > 0 ? slot.Actions[0].Spell.Id : 0u;

    // 小工具函数
    bool IsShiftOrLey(uint id) =>
        id is Skill.星灵移位 or Skill.墨泉;

    bool IsShiftOnly(uint id) =>
        id is Skill.星灵移位;

    
    if (!减少动画锁环境 && lastActionId == Skill.核爆)
    {
        if (!IsShiftOrLey(nextAbilityId))
        {
            
            return -15;
        }

        
        allowedWeaves = 1;
    }

    
    if (lastActionId == Skill.耀星)
    {
        if (!IsShiftOrLey(nextAbilityId))
        {
            
            return -16;
        }

        
        allowedWeaves = 1;
    }

    
    if (lastActionId == Skill.玄冰 || lastActionId == Skill.冰冻)
    {
        if (!IsShiftOnly(nextAbilityId))
        {
            
            return -17;
        }

        
        allowedWeaves = 1;
    }

    
    if (allowedWeaves <= 0)
    {
        // 这个 GCD 类型不适合织能力技
        return -10;
    }

    // 当前这个 slot 里已经排了几个能力技（本 GCD 内）
    var usedWeaves = slot.Actions.Count;
    if (usedWeaves >= allowedWeaves)
    {
       
        return -11;
    }

    
    var gcdCd = GCDHelper.GetGCDCooldown();

    
    if (gcdCd is > 750 and < 1500)
        return -12;

    
    if (slot.Actions.Count > 1 && gcdCd < 1500)
        return -13;

    return 1;
}

    public static double 连击剩余时间 => Core.Resolve<MemApiSpell>().GetComboTimeLeft().TotalMilliseconds;

    public static bool 在近战范围内 =>
        Core.Me.Distance(Core.Me.GetCurrTarget()!) <= SettingMgr.GetSetting<GeneralSettings>().AttackRange;

    public static bool 在背身位 => Core.Resolve<MemApiTarget>().IsBehind;
    public static bool 在侧身位 => Core.Resolve<MemApiTarget>().IsFlanking;

    
    public static int 充能技能冷却时间(uint skillId)
    {
        var spell = skillId.GetSpell();
        return (int)(spell.Cooldown.TotalMilliseconds -
                     (spell.RecastTime.TotalMilliseconds / spell.MaxCharges) * (spell.MaxCharges - 1));
    }
    public static bool AnyAuraTimerLessThan(List<uint> auras, int timeLeft) {
        return Core.Me.StatusList.Any(aura => (aura.StatusId != 0)
                                              && (Math.Abs(aura.RemainingTime) * 1000.0 <= timeLeft)
                                              && auras.Contains(aura.StatusId));
    }

    public static bool 有buff(uint buffId) => Core.Me.HasAura(buffId);

    /// <summary>
    /// 自身有buff且时间小于
    /// </summary>
    public static bool Buff时间小于(uint buffId, int timeLeft)
    {
        if (!Core.Me.HasAura(buffId)) return false;
        return GetAuraTimeLeft(buffId) <= timeLeft;
    }

    /// <summary>
    /// 目标有buff且时间小于，有buff参数如果为false，则当目标没有玩家的buff是也返回true
    /// </summary>
    public static bool 目标Buff时间小于(uint buffId, int timeLeft, bool 有buff = true)
    {
        var target = Core.Me.GetCurrTarget();
        if (target == null) return false;

        if (有buff)
        {
            if (!target.HasLocalPlayerAura(buffId)) return false;
        }
        else
        {
            if (!target.HasLocalPlayerAura(buffId)) return true;
        }

        var time = Core.Resolve<MemApiBuff>().GetAuraTimeleft(target, buffId, true);
        return time <= timeLeft;
    }

    /// <summary>
    /// 在list中添加一个唯一的元素
    /// </summary>
    public static bool TryAdd<T>(this List<T> list, T item)
    {
        if (list.Contains(item)) return false;
        list.Add(item);
        return true;
    }

    public static bool 目标有任意我的buff(List<uint> buffs) =>
        buffs.Any(buff => Core.Me.GetCurrTarget()!.HasLocalPlayerAura(buff));


    public static IBattleChara? 最优aoe目标(this uint spellId, int count)
    {
        return TargetHelper.GetMostCanTargetObjects(spellId, count);
    }

    public static int 目标周围可选中敌人数量(this IBattleChara? target, int range)
    {
        return TargetHelper.GetNearbyEnemyCount(target, 25, range);
    }

   
    public static IBattleChara? GetSmartAoeCenter(this uint spellId, int damageRange = 5)
    {
        var current = Core.Me.GetCurrTarget();
        if (current == null)
            return null;

        
        if (!BlackMageQT.GetQt("智能aoe目标"))
            return current;

       
        var count = current.目标周围可选中敌人数量(damageRange);
        if (count <= 1)
            return current;

        
        var best = spellId.最优aoe目标(count);
        return best ?? current;
    }

    // ========= 新增：按技能范围计算 AoE 命中数量 =========
    /// <summary>
    /// 按技能自身施法距离 + 伤害半径，统计以指定中心为圆心的可命中敌人数量
    /// </summary>
    /// <param name="spellId">技能 ID / 变化后的技能 ID</param>
    /// <param name="center">AoE 中心，不传则使用智能 AoE 中心目标</param>
    /// <param name="damageRange">技能伤害半径（默认 5）</param>
    public static int GetAoeHitCount(this uint spellId, IBattleChara? center = null, int damageRange = 5)
    {
        var memSpell = Core.Resolve<MemApiSpell>();
        var castRange = (int)memSpell.GetActionRange(spellId);

        // 防止某些技能读不到射程，做个保护
        if (castRange <= 0)
            castRange = 25;

        // 若没指定中心，则用“智能 aoe 中心”
        var target = center ?? spellId.GetSmartAoeCenter(damageRange);
        if (target == null)
            return 0;

        return TargetHelper.GetNearbyEnemyCount(target, castRange, damageRange);
    }

    /// <summary>
    /// 获取非战斗状态时开了盾姿的人
    /// </summary>
    /// <returns></returns>
    public static IBattleChara? GetMt()
    {
        PartyHelper.UpdateAllies();
        return PartyHelper.CastableTanks
            .FirstOrDefault(p => p.HasAnyAura([743, 1833, 79, 91]));
    }

    public static bool In团辅()
    {
        //检测目标团辅
        List<uint> 目标团辅 = [背刺, 连环计];
        if (目标团辅.Any(buff => 目标Buff时间小于(buff, 15000))) return true;

        //检测自身团辅
        List<uint> 自身团辅 = [灼热之光, 星空, 占卜, 义结金兰, 战斗连祷, 大舞, 战斗之声, 鼓励, 神秘环];
        return 自身团辅.Any(buff => Buff时间小于(buff, 15000));
    }

    private static uint
        背刺 = 3849,
        强化药 = 49,
        灼热之光 = 2703,
        星空 = 3685,
        占卜 = 1878,
        义结金兰 = 1185,
        战斗连祷 = 786,
        大舞 = 1822,
        战斗之声 = 141,
        鼓励 = 1239,
        神秘环 = 2599,
        连环计 = 2617;
}
