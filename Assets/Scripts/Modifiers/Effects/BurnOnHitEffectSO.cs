using UnityEngine;
using KawaiiKiller.Weapons;

namespace KawaiiKiller.Modifiers
{
    [CreateAssetMenu(fileName = "BurnOnHitEffectSO", menuName = "KawaiiKiller/Modifiers/Effects/BurnOnHit")]
    public class BurnOnHitEffectSO : ModifierEffectSO
    {
        [SerializeField] private float burnDamagePerSecond = 10f;
        [SerializeField] private float burnDuration = 5f;

        public override void OnProjectileHit(Weapons.WeaponInstance arma, GameObject objetivo, ref DamagePayload payload)
        {
            if (objetivo == null) return;
            if (!objetivo.TryGetComponent<IBurnable>(out var burnable)) return;
            burnable.ApplyBurn(burnDamagePerSecond, burnDuration);
        }
    }
}
