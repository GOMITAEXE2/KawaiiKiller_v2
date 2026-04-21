using UnityEngine;
using KawaiiKiller.Items;

namespace KawaiiKiller.Shop
{
    [CreateAssetMenu(fileName = "RaritySettings", menuName = "KawaiiKiller/Shop/Rarity Settings")]
    public class RaritySettingsSO : ScriptableObject
    {
        [Range(0f, 1f)] public float SimpleWeight     = 0.60f;
        [Range(0f, 1f)] public float RaraWeight       = 0.25f;
        [Range(0f, 1f)] public float ExtrañaWeight    = 0.12f;
        [Range(0f, 1f)] public float LegendariaWeight = 0.03f;

        public float GetWeight(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Simple      => SimpleWeight,
                ItemRarity.Rara        => RaraWeight,
                ItemRarity.Extraña     => ExtrañaWeight,
                ItemRarity.Legendaria  => LegendariaWeight,
                _                      => 0f
            };
        }
    }
}