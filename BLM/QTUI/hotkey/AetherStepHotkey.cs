using System.Numerics;
using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.View.JobView;
using AEAssist.CombatRoutine.View.JobView.HotkeyResolver;
using AEAssist.Extension;
using AEAssist.Helper;
using Los.ModernJobViewFramework.HotKey;
using los.BLM.SlotResolver.Data;

namespace los.BLM.QtUI.Hotkey
{
    public class AetherStepHotkey : IHotkeyResolver
    {
        private const uint SpellId = Skill.以太步;
        private readonly int _partyIndex;
        private Spell _spell;

        public AetherStepHotkey(int partyIndex)
        {
            _partyIndex = partyIndex;
            UpdateSpell();
        }

        private void UpdateSpell()
        {
            var party = PartyHelper.Party;
            if (party.Count > _partyIndex)
                _spell = new Spell(SpellId, party[_partyIndex]);
        }

        public void Draw(Vector2 size)
        {
            HotkeyHelper.DrawSpellImage(size, SpellId);
        }

        public void DrawExternal(Vector2 size, bool isActive)
        {
            UpdateSpell();

            if (_spell != null && _spell.IsReadyWithCanCast())
            {
                if (isActive)
                    HotkeyHelper.DrawActiveState(size);
                else
                    HotkeyHelper.DrawGeneralState(size);
            }
            else
            {
                HotkeyHelper.DrawDisabledState(size);
            }

            HotkeyHelper.DrawCooldownText(_spell, size);
        }

        public int Check()
        {
            var party = PartyHelper.Party;
            if (party.Count <= _partyIndex)
                return -1;

            var member = party[_partyIndex];
            if (!member.IsTargetable || member.IsDead())
                return -2;

            UpdateSpell();
            if (_spell == null || !_spell.IsReadyWithCanCast())
                return -3;

            return 0;
        }

        public void Run()
        {
            var party = PartyHelper.Party;
            if (party.Count <= _partyIndex)
                return;

            var member = party[_partyIndex];

            var slot = new Slot();
            slot.Add(new Spell(SpellId, member));
            
            AI.Instance.BattleData.NextSlot = slot;
        }
    }
}

