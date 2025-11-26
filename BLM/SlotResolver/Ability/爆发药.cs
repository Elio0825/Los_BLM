using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using los.BLM.QtUI;

namespace Los.BLM.SlotResolver.Ability;

public class 爆发药 : ISlotResolver
{
    
    public int Check()
    {
        if (!Qt.Instance.GetQt("爆发药")) return -2;
        if (!ItemHelper.CheckCurrJobPotion()) return -3;
        return 1;
    }

    public void Build(Slot slot)
    {
        slot.Add(Spell.CreatePotion());
    }
}