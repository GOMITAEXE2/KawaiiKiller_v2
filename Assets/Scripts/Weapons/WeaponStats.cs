using System;
using UnityEngine;

namespace KawaiiKiller.Weapons
{
    [Serializable]
    public struct WeaponStats
    {
        public float BaseDamage;

        [Tooltip("Segundos entre cada disparo. Menor = más rápido. Ej: 0.1 = 10 disparos/seg, 0.5 = 2 disparos/seg")]
        public float FireRate;

        public float CriticalChance;
        public int ProjectilesPerShot;
        public bool IsAutomatic;
        public float BaseSpreadAngle;
        public float AimingSpreadAngle;
        public int MagazineSize;
        public float ReloadTime;
        public float ProjectileSpeed;

        /// <summary>
        /// Tiempo entre disparos en segundos. 
        /// FireRate ahora SE USA DIRECTAMENTE como el delay (no como tasa).
        /// Si es 0 o negativo, el arma no dispara.
        /// </summary>
        public float FireDelay => FireRate > 0f ? FireRate : float.MaxValue;

        /// <summary>Returns ProjectileSpeed, or 140f if the field was never set (default 0).</summary>
        public float FinalProjectileSpeed => ProjectileSpeed > 0f ? ProjectileSpeed : 140f;
    }
}
