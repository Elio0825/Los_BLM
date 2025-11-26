using System.Numerics;
using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.View.JobView;
using AEAssist.CombatRoutine.View.JobView.HotkeyResolver;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using los.BLM.Helper;
using los.BLM.SlotResolver.Data;

namespace los.BLM.QtUI.Hotkey
{
   
    public class AetherStepMouseHotkey : IHotkeyResolver
    {
        private const uint SpellId = Skill.以太步;

        
        private static readonly MemApiParty PartyApi = Core.Resolve<MemApiParty>();
        
        public void Draw(Vector2 size)
        {
            HotkeyHelper.DrawSpellImage(size, SpellId.AdaptiveId());
        }

        public void DrawExternal(Vector2 size, bool isActive)
        {
            Draw(size);
        }

        
        public int Check()
        {
            var mo = PartyApi.Mo();
            if (mo == null)
                return -10;
            var spell = new Spell(SpellId, mo);
            if (!spell.IsReadyWithCanCast())
                return -1;
            if (spell.CheckInHPQueue())
                return -3;

            return 0; 
        }

       
        public void Run()
        {
            var mo = PartyApi.Mo();
            if (mo == null)
                return;

            var spell = new Spell(SpellId, mo);
            if (!spell.IsReadyWithCanCast())
                return;
            
            AI.Instance.BattleData.AddSpell2NextSlot(spell);
        }
    }
}