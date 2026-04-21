using UnityEngine;

namespace KawaiiKiller.Modifiers
{
    public class ModifierSlot
    {
        public ModifierDataSO EquippedModifier { get; private set; }
        public SlotType AcceptedType { get; private set; }

        public bool IsEmpty => EquippedModifier == null;
        public ModifierDataSO Data => EquippedModifier;

        public ModifierSlot(SlotType type)
        {
            AcceptedType = type;
        }

        public bool CanEquip(ModifierDataSO mod)
        {
            return mod != null && mod.TipoDeSlot == AcceptedType;
        }

        public void Equip(ModifierDataSO mod)
        {
            if (!CanEquip(mod)) return;
            EquippedModifier = mod;
        }

        public void Unequip()
        {
            EquippedModifier = null;
        }
    }
}
