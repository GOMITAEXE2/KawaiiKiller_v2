using System;
using UnityEngine;

namespace KawaiiKiller.Weapons
{
    [Serializable]
    public struct WeaponStats
    {
        public float BaseDamage;
        public float FireRate;
        public float CriticalChance;
        public int ProjectilesPerShot;
        public bool IsAutomatic;
        public float BaseSpreadAngle;
        public float AimingSpreadAngle;
        public int MagazineSize;
        public float ReloadTime;

        public float FireDelay => FireRate > 0f ? 1f / FireRate : float.MaxValue;
    }
}
