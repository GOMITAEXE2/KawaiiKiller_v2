using System.Collections.Generic;
using UnityEngine;
using KawaiiKiller.Weapons;
using KawaiiKiller.Modifiers;
using KawaiiKiller.Shop;

namespace KawaiiKiller.UI.Shop
{
    [CreateAssetMenu(fileName = "ShopDatabase", menuName = "KawaiiKiller/Shop/Shop Database")]
    public class ShopDatabaseSO : ScriptableObject
    {
        public RaritySettingsSO      RaritySettings;
        public List<WeaponDataSO>    AvailableWeapons;
        public List<ModifierDataSO>  AvailableUpgrades;
        public List<ModifierDataSO>  AvailableAttachments;

        public WeaponDataSO FindWeapon(string id)
        {
            if (AvailableWeapons == null) return null;
            foreach (var w in AvailableWeapons)
                if (w != null && w.ID == id) return w;
            return null;
        }

        public ModifierDataSO FindModifier(string id)
        {
            if (AvailableUpgrades != null)
                foreach (var m in AvailableUpgrades)
                    if (m != null && m.ID == id) return m;

            if (AvailableAttachments != null)
                foreach (var m in AvailableAttachments)
                    if (m != null && m.ID == id) return m;

            return null;
        }
    }
}