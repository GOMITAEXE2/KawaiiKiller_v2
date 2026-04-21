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

        public Sprite Icon  => Icono;
        public int Price    => Precio;
    }
}