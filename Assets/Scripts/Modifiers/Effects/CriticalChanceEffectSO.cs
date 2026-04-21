using UnityEngine;

namespace KawaiiKiller.Modifiers
{
    [CreateAssetMenu(fileName = "CriticalChanceEffectSO", menuName = "KawaiiKiller/Modifiers/Effects/CriticalChance")]
    public class CriticalChanceEffectSO : ModifierEffectSO
    {
        [SerializeField] private float criticalChance = 1f;

        public override void ApplyStats(Weapons.WeaponInstance arma, ref Weapons.WeaponStats stats)
        {
            stats.CriticalChance = Mathf.Clamp01(Mathf.Max(stats.CriticalChance, criticalChance));
        }
    }
}
