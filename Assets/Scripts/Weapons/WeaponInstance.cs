using System.Collections.Generic;
using UnityEngine;
using KawaiiKiller.Modifiers;

namespace KawaiiKiller.Weapons
{
    public class WeaponInstance
    {
        public WeaponDataSO Data { get; private set; }
        public int CurrentAmmo { get; set; }
        public Dictionary<string, object> DynamicVariables { get; private set; }
        public List<ModifierSlot> ModifierSlots { get; private set; }
        public WeaponStats FinalStats { get; private set; }
        public List<TipoArma> FinalTipos { get; private set; }

        public WeaponInstance(WeaponDataSO weaponData)
        {
            Data = weaponData;
            DynamicVariables = new Dictionary<string, object>();
            ModifierSlots = new List<ModifierSlot>();

            int maxUpgrades = Data != null ? Data.MaxUpgrades : 0;
            int maxAttachments = Data != null ? Data.MaxAttachments : 0;

            for (int i = 0; i < maxUpgrades; i++)
            {
                ModifierSlots.Add(new ModifierSlot(SlotType.Upgrade));
            }

            for (int i = 0; i < maxAttachments; i++)
            {
                ModifierSlots.Add(new ModifierSlot(SlotType.Attachment));
            }

            if (Data != null && Data.StartingModifiers != null)
            {
                for (int i = 0; i < Data.StartingModifiers.Count; i++)
                {
                    ModifierDataSO modifier = Data.StartingModifiers[i];
                    if (modifier == null) continue;

                    for (int j = 0; j < ModifierSlots.Count; j++)
                    {
                        ModifierSlot slot = ModifierSlots[j];
                        if (slot == null || slot.EquippedModifier != null) continue;
                        if (!slot.CanEquip(modifier)) continue;

                        slot.Equip(modifier);
                        break;
                    }
                }
            }

            RecalculateStats();
            CurrentAmmo = FinalStats.MagazineSize;
        }

        public void RecalculateStats()
        {
            WeaponStats stats = Data != null ? Data.WeaponStats : default;
            List<TipoArma> tipos = Data != null && Data.Tipos != null ? new List<TipoArma>(Data.Tipos) : new List<TipoArma>();

            for (int i = 0; i < ModifierSlots.Count; i++)
            {
                ModifierSlot slot = ModifierSlots[i];
                if (slot == null || slot.EquippedModifier == null || slot.EquippedModifier.Efectos == null) continue;

                for (int j = 0; j < slot.EquippedModifier.Efectos.Count; j++)
                {
                    ModifierEffectSO efecto = slot.EquippedModifier.Efectos[j];
                    if (efecto == null) continue;
                    efecto.ProcesarTipos(this, tipos);
                    efecto.ProcesarStats(this, ref stats);
                }
            }

            FinalStats = stats;
            FinalTipos = tipos;
        }

        public bool TryEquipModifier(int slotIndex, ModifierDataSO modifier)
        {
            if (slotIndex < 0 || slotIndex >= ModifierSlots.Count) return false;
            ModifierSlot slot = ModifierSlots[slotIndex];
            if (slot == null || !slot.CanEquip(modifier)) return false;
            slot.Equip(modifier);
            RecalculateStats();
            return true;
        }

        public void UnequipModifier(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= ModifierSlots.Count) return;
            ModifierSlot slot = ModifierSlots[slotIndex];
            if (slot == null) return;
            slot.Unequip();
            RecalculateStats();
        }

        public void OnShotFired(ref DamagePayload payload)
        {
            OnShotFired(ref payload, null);
        }

        public void OnShotFired(ref DamagePayload payload, List<string> activatedEffects)
        {
            for (int i = 0; i < ModifierSlots.Count; i++)
            {
                ModifierSlot slot = ModifierSlots[i];
                if (slot == null || slot.EquippedModifier == null || slot.EquippedModifier.Efectos == null) continue;

                for (int j = 0; j < slot.EquippedModifier.Efectos.Count; j++)
                {
                    ModifierEffectSO efecto = slot.EquippedModifier.Efectos[j];
                    if (efecto == null) continue;
                    float beforeDamage = payload.BaseDamage;
                    float beforeCritical = payload.CriticalChance;
                    efecto.OnShotFired(this, ref payload);
                    if (activatedEffects != null && (beforeDamage != payload.BaseDamage || beforeCritical != payload.CriticalChance))
                    {
                        activatedEffects.Add(efecto.name);
                    }
                }
            }
        }

        public void OnProjectileHit(GameObject objetivo, ref DamagePayload payload)
        {
            for (int i = 0; i < ModifierSlots.Count; i++)
            {
                ModifierSlot slot = ModifierSlots[i];
                if (slot == null || slot.EquippedModifier == null || slot.EquippedModifier.Efectos == null) continue;

                for (int j = 0; j < slot.EquippedModifier.Efectos.Count; j++)
                {
                    ModifierEffectSO efecto = slot.EquippedModifier.Efectos[j];
                    if (efecto == null) continue;
                    efecto.OnProjectileHit(this, objetivo, ref payload);
                }
            }
        }

        public void OnReload()
        {
            for (int i = 0; i < ModifierSlots.Count; i++)
            {
                ModifierSlot slot = ModifierSlots[i];
                if (slot == null || slot.EquippedModifier == null || slot.EquippedModifier.Efectos == null) continue;

                for (int j = 0; j < slot.EquippedModifier.Efectos.Count; j++)
                {
                    ModifierEffectSO efecto = slot.EquippedModifier.Efectos[j];
                    if (efecto == null) continue;
                    efecto.OnReload(this);
                }
            }
        }

        public void NotifyEnemyKilled(GameObject objetivo)
        {
            for (int i = 0; i < ModifierSlots.Count; i++)
            {
                ModifierSlot slot = ModifierSlots[i];
                if (slot == null || slot.EquippedModifier == null || slot.EquippedModifier.Efectos == null) continue;

                for (int j = 0; j < slot.EquippedModifier.Efectos.Count; j++)
                {
                    ModifierEffectSO efecto = slot.EquippedModifier.Efectos[j];
                    if (efecto == null) continue;
                    efecto.AlMatarEnemigo(this, objetivo);
                }
            }
        }
    }
}
