using System.Collections.Generic;
using UnityEngine;
using KawaiiKiller.UI.Shop;
using KawaiiKiller.Items;

namespace KawaiiKiller.Weapons
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "KawaiiKiller/Weapon/Data")]
    public class WeaponDataSO : ScriptableObject, IStorable
    {
        public string ID;
        public string Nombre;
        public int Precio = 100;
        public ItemRarity Rareza = ItemRarity.Simple;
        public GameObject Prefab3D;
        public Projectile ProjectilePrefab;
        public Sprite Icono;
        public WeaponStats WeaponStats;
        public List<TipoArma> Tipos;
        public int MaxUpgrades;
        public int MaxAttachments;
        public List<Modifiers.ModifierDataSO> StartingModifiers;

        [Header("Recoil — Cámara")]
        public float RecoilPitchMin = 1.5f;
        public float RecoilPitchMax = 3.0f;
        public float RecoilKickStrength = 0.08f;
        public float AdsRecoilMultiplier = 0.25f;

        [Header("Recoil — Modelo del arma")]
        public Vector3 KickPositionOffset = new Vector3(0f, 0.02f, -0.08f);
        public Vector3 KickRotationOffset = new Vector3(-8f, 0f, 0f);
        public float KickReturnSpeed = 10f;

        [Header("Effects")]
        [Tooltip("Prefab de partículas del fogonazo. Se instancia en el FirePoint al disparar.")]
        public GameObject MuzzleFlashPrefab;

        [Tooltip("Prefab de partículas de humo post-disparo. Se instancia cuando el arma deja de disparar.")]
        public GameObject SmokeEffectPrefab;

        [Tooltip("Sonido que se reproduce al disparar.")]
        public AudioClip FireSound;

        [Tooltip("Volumen del sonido de disparo (0-1).")]
        [Range(0f, 1f)]
        public float FireSoundVolume = 0.8f;

        [Tooltip("Sonido que se reproduce al recargar.")]
        public AudioClip ReloadSound;

        [Tooltip("Volumen del sonido de recarga (0-1).")]
        [Range(0f, 1f)]
        public float ReloadSoundVolume = 0.6f;
        public GameObject ImpactEffectPrefab;
        public Color ProjectileColor = Color.yellow;
        public Sprite Icon  => Icono;
        public int Price    => Precio;
    }
}