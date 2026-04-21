using System.Collections.Generic;
using UnityEngine;
using KawaiiKiller.UI.Shop;
using KawaiiKiller.Items;

namespace KawaiiKiller.Modifiers
{
    [CreateAssetMenu(fileName = "ModifierData", menuName = "KawaiiKiller/Modifiers/Data")]
    public class ModifierDataSO : ScriptableObject, IStorable
    {
        public string ID;
        public string Nombre;
        public Sprite Icono;
        public SlotType TipoDeSlot;
        public int Precio;
        public ItemRarity Rareza = ItemRarity.Simple;
        public List<ModifierEffectSO> Efectos;

        public Sprite Icon  => Icono;
        public int Price    => Precio;
    }
}