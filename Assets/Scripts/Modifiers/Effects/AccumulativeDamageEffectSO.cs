using UnityEngine;

namespace KawaiiKiller.Modifiers
{
    [CreateAssetMenu(fileName = "AccumulativeDamageEffectSO", menuName = "KawaiiKiller/Modifiers/Effects/AccumulativeDamage")]
    public class AccumulativeDamageEffectSO : ModifierEffectSO
    {
        [SerializeField] private float accumulationFactor = 0.5f;
        [SerializeField] private int triggerEveryShots = 2;

        private const string KeyShotCount = "Accumulative_ShotCount";
        private const string KeyLastDamage = "Accumulative_LastDamage";
        private const string KeyBonus = "Accumulative_Bonus";

        public override void OnShotFired(Weapons.WeaponInstance arma, ref Weapons.DamagePayload payload)
        {
            if (arma == null) return;

            float bonus = arma.DynamicVariables.TryGetValue(KeyBonus, out object value) && value is float f ? f : 0f;
            if (bonus > 0f)
            {
                payload.BaseDamage += bonus;
            }

            int shotCount = arma.DynamicVariables.TryGetValue(KeyShotCount, out object countObj) && countObj is int count ? count : 0;
            float previousDamage = arma.DynamicVariables.TryGetValue(KeyLastDamage, out object lastObj) && lastObj is float lastDamage ? lastDamage : 0f;

            shotCount++;
            if (triggerEveryShots > 0 && shotCount % triggerEveryShots == 0)
            {
                bonus += previousDamage * Mathf.Max(0f, accumulationFactor);
            }

            arma.DynamicVariables[KeyShotCount] = shotCount;
            arma.DynamicVariables[KeyLastDamage] = payload.BaseDamage;
            arma.DynamicVariables[KeyBonus] = bonus;
        }

        public override void OnReload(Weapons.WeaponInstance arma)
        {
            if (arma == null) return;
            arma.DynamicVariables[KeyShotCount] = 0;
            arma.DynamicVariables[KeyLastDamage] = 0f;
            arma.DynamicVariables[KeyBonus] = 0f;
        }
    }
}
