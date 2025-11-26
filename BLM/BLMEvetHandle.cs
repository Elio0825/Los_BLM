using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using los.BLM;
using los.BLM.Helper;
using los.BLM.QtUI;
using los.BLM.SlotResolver.Data;
using BattleData = los.BLM.SlotResolver.BattleData;

namespace Los.BLM
{
   
    public class BLMEvetHandle : IRotationEventHandler
    {
        private int 释放技能时状态 = 0;

        private readonly HashSet<uint> _gcdSpellIds = new HashSet<uint>
        {
            Skill.冰一, Skill.冰三, Skill.冰冻.GetActionChange(), Skill.冰澈,
            Skill.玄冰, Skill.玄冰.GetActionChange(),
            Skill.火一, Skill.火三,
            Skill.火二, Skill.火二.GetActionChange(),
            Skill.火四, Skill.核爆, Skill.绝望, Skill.耀星,
            Skill.异言, Skill.悖论, Skill.秽浊,
            Skill.雷一.GetActionChange(), Skill.雷二.GetActionChange(),
            Skill.崩溃, Skill.灵极魂
        };

        
        private readonly HashSet<uint> _ogcdSpellIds = new HashSet<uint>
        {
            Skill.黑魔纹,Skill.三连,Skill.墨泉,Skill.即刻,Skill.星灵移位,Skill.醒梦,Skill.详述
        };

        private readonly uint[] _fireSpellIds = new[]
        {
            Skill.核爆, Skill.火三, Skill.火四, Skill.绝望,
            Skill.火二, Skill.火二.GetActionChange(),
            Skill.耀星, Skill.悖论
        };

        private readonly uint[] _bSpellIds = new[]
        {
            Skill.悖论, Skill.冰三, Skill.冰澈, Skill.冰冻.GetActionChange(),
            Skill.玄冰, Skill.玄冰.GetActionChange(),
            Skill.灵极魂
        };

        
        private DateTime _lastOutOfCombatTime = DateTime.MinValue;

       public async Task OnPreCombat()
{
    
    if (Core.Me.InCombat())
    {
        
        _lastOutOfCombatTime = DateTime.MinValue;
        return;
    }

    
    if (_lastOutOfCombatTime == DateTime.MinValue)
        _lastOutOfCombatTime = DateTime.UtcNow;

   
    if (Qt.Instance.GetQt("脱战转圈"))
    {
        if (Core.Me.CurrentHp <= 0)
            return;

        var oocDuration = DateTime.UtcNow - _lastOutOfCombatTime;
        if (oocDuration.TotalSeconds < 5)
            return;

        
        bool hasFullIceStacks = BLMHelper.冰层数 >= 3;
        bool hasFullHearts    = Core.Me.Level >= 58 ? BLMHelper.冰针 >= 3 : true;
        bool hasFullMp        = Core.Me.CurrentMp >= 10000;

        
        bool finished = BLMHelper.火状态 && hasFullHearts;

         
        if (finished)
            return;

        
        if (BLMHelper.火状态)
        {
            var transpose = Skill.星灵移位.GetSpell(SpellTargetType.Self);
            if (transpose != null && transpose.IsReadyWithCanCast())
            {
                await transpose.Cast(); // 火 -> 冰
                return;
            }
        }

        
        if (BLMHelper.冰状态)
        {
            
            if (!hasFullIceStacks || !hasFullHearts || !hasFullMp)
            {
                var soul = Skill.灵极魂.GetSpell(SpellTargetType.Self);
                if (soul != null && soul.IsReadyWithCanCast())
                {
                    await soul.Cast();
                }
            }
            else
            {
                // 冰3 + 3冰针 + 满蓝
                var transpose = Skill.星灵移位.GetSpell(SpellTargetType.Self);
                if (transpose != null && transpose.IsReadyWithCanCast())
                {
                    await transpose.Cast();
                }
            }

            return;
        }

        
        return;
    }

    
    
    if (!Qt.Instance.GetQt("Boss上天"))
        return;

    
    if (BLMHelper.火状态 && Skill.星灵移位.GetSpell().IsReadyWithCanCast())
    {
        await Skill.星灵移位.GetSpell(SpellTargetType.Self).Cast();
    }

    
    if (BLMHelper.冰状态 && (BLMHelper.冰层数 < 3 || BLMHelper.冰针 < 3 || Core.Me.CurrentMp < 10000))
    {
        var soul = Skill.灵极魂.GetSpell(SpellTargetType.Self);
        if (soul != null && soul.IsReadyWithCanCast())
        {
            await soul.Cast();
            await Task.Delay(100);
        }
    }
}


        public void OnResetBattle()
        {
           
            BattleData.Instance = new BattleData();
            BattleData.Instance.IsInnerOpener = false;
            BattleData.Instance.需要即刻 = false;
            BattleData.Instance.需要瞬发gcd = false;
            BattleData.Instance.正在特殊循环中 = false;
            BattleData.Reset();
            BattleData.RebuildSettings();

            
            _lastOutOfCombatTime = DateTime.MinValue;
        }

        private int 转圈次数 = 0;

        public async Task OnNoTarget()
        {
            
            if (!Core.Me.InCombat())
                return;
            if (AI.Instance.BattleData.CurrBattleTimeInMs < 10 * 1000)
                return;
            
            BattleData.Instance.需要即刻 = false;
            BattleData.Instance.需要瞬发gcd = false;
            BattleData.Instance.正在特殊循环中 = false;

            if (!Qt.Instance.GetQt("Boss上天"))
                return;

           
            if (BLMHelper.火状态 && Skill.星灵移位.GetSpell().IsReadyWithCanCast()&&BLMHelper.冰层数 < 3)
            {
                await Skill.星灵移位.GetSpell(SpellTargetType.Self).Cast();
            }

            
            if (BLMHelper.冰状态 &&
                (BLMHelper.冰层数 < 3 || BLMHelper.冰针 < 3 || Core.Me.CurrentMp < 10000))
            {
                if (转圈次数 < 3)
                {
                    var soul = Skill.灵极魂.GetSpell(SpellTargetType.Self);
                    if (soul != null && soul.IsReadyWithCanCast())
                    {
                        await soul.Cast();
                        转圈次数++;
                    }
                }
            }

            await Task.CompletedTask;
        }

        public void OnSpellCastSuccess(Slot slot, Spell spell)
        {
            if (_gcdSpellIds.Contains(spell.Id))
                BattleData.Instance.已使用瞬发 = GCDHelper.GetGCDCooldown() >=
                                                  (Core.Me.HasAura(Buffs.咏速Buff) ? 1500 : 1700);
        }

        public void AfterSpell(Slot slot, Spell spell)
        {
            if (释放技能时状态 == 1)
            {
                if (spell.Id == Skill.火二.GetActionChange() || spell.Id == Skill.火三 || spell.Id == Skill.星灵移位)
                {
                    转圈次数 = 0;
                    BattleData.Instance.上一轮循环 = new List<uint>(BattleData.Instance.冰状态gcd);
                    BattleData.Instance.冰状态gcd.Clear();
                    if (spell.Id == Skill.火二.GetActionChange() || spell.Id == Skill.火三)
                    {
                        BattleData.Instance.火状态gcd.Add(spell.Id);
                    }
                }
                else
                {
                    BattleData.Instance.冰状态gcd.Add(spell.Id);
                }
            }
            else if (释放技能时状态 == 2)
            {
                if (spell.Id == Skill.冰冻.GetActionChange() || spell.Id == Skill.冰三 || spell.Id == Skill.星灵移位)
                {
                    BattleData.Instance.上一轮循环 = new List<uint>(BattleData.Instance.火状态gcd);
                    BattleData.Instance.火状态gcd.Clear();
                    if (spell.Id == Skill.冰冻.GetActionChange() || spell.Id == Skill.冰三)
                    {
                        BattleData.Instance.冰状态gcd.Add(spell.Id);
                    }
                }
                else
                {
                    BattleData.Instance.火状态gcd.Add(spell.Id);
                }
            }
            else if (释放技能时状态 == 0)
            {
                BattleData.Instance.冰状态gcd.Clear();
                BattleData.Instance.火状态gcd.Clear();
                if (_fireSpellIds.Contains(spell.Id))
                {
                    BattleData.Instance.火状态gcd.Add(spell.Id);
                }
                else if (_bSpellIds.Contains(spell.Id))
                {
                    BattleData.Instance.冰状态gcd.Add(spell.Id);
                }
            }

            if (spell.Id == Skill.星灵移位)
            {
                if (BLMHelper.冰状态) 释放技能时状态 = 1;
                else if (BLMHelper.火状态) 释放技能时状态 = 2;
                else 释放技能时状态 = 0;

                if (BLMHelper.冰状态 && BLMHelper.冰针 == 3)
                {
                    BattleData.Instance.三冰针进冰 = true;
                }
            }

            
            if (_gcdSpellIds.Contains(spell.Id))
            {
                BattleData.Instance.前一gcd = spell.Id;
                AI.Instance.BattleData.CurrGcdAbilityCount = 1;
                BattleData.Instance.已使用瞬发 = GCDHelper.GetGCDCooldown() >=
                                              (Core.Me.HasAura(Buffs.咏速Buff) ? 1500 : 1700);
                if (BLMHelper.冰状态) 释放技能时状态 = 1;
                else if (BLMHelper.火状态) 释放技能时状态 = 2;
                else 释放技能时状态 = 0;
            }

            if (BattleData.Instance.三冰针进冰)
            {
                if (spell.Id == Skill.冰澈 || spell.Id == Skill.玄冰)
                {
                    BattleData.Instance.三冰针进冰 = false;
                }
            }

            
            if (_ogcdSpellIds.Contains(spell.Id))
            {
                BattleData.Instance.前一能力技 = spell.Id;
            }

            
            if (BattleData.Instance.已使用瞬发)
            {
                BattleData.Instance.需要瞬发gcd = false;
                AI.Instance.BattleData.CurrGcdAbilityCount = 2;
            }
        }

        public void OnBattleUpdate(int currTimeInMs)
        {
            
            if (Core.Me.InCombat())
            {
                _lastOutOfCombatTime = DateTime.MinValue;
            }

            
            if (Helper.可瞬发())
                BattleData.Instance.需要即刻 = false;

            
            if (BLMHelper.在发呆())
            {
                if (BLMHelper.可用瞬发() != 0)
                    BattleData.Instance.需要瞬发gcd = true;
                else
                    BattleData.Instance.需要即刻 = true;
            }

           
            if (Core.Me.IsCasting)
            {
                BattleData.Instance.需要即刻 = false;
                BattleData.Instance.已使用瞬发 = false;
                BattleData.Instance.需要瞬发gcd = false;
            }

            if (!BattleData.Instance.特供循环)
                BattleData.Instance.正在特殊循环中 = false;
        }

        public void OnEnterRotation()
        {
            LogHelper.Print("使用LosBLM");
            LogHelper.Print("建议设置提前使用gcd时间为50，使用fuckanime三插，DR能力技动画减少");

            if (Helper.GlobalSettings.NoClipGCD3)
                LogHelper.PrintError("不要勾选【全局能力技不卡GCD】选项");
            Qt.LoadQtStates();

            BattleData.Instance.IsInnerOpener = false;
            
            _lastOutOfCombatTime = DateTime.UtcNow;
        }

        public void OnExitRotation()
        {
            Qt.SaveQtStates();
            BlackMageSetting.Instance.Save();
        }

        public void OnTerritoryChanged()
        {
            
        }
    }
}
