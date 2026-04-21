using UnityEngine;

namespace KawaiiKiller.Modifiers
{
    [CreateAssetMenu(fileName = "DanioPorBajasEffectSO_01", menuName = "KawaiiKiller/Modifiers/Effects/DanioPorBajas_01")]
    public class DanioPorBajasEffectSO_01 : ModifierEffectSO
    {
        private const string ClaveBajas = "Bajas_DanioPlus";
        public float danioExtraPorBaja;

        public override void AlMatarEnemigo(Weapons.WeaponInstance arma, GameObject objetivo)
        {
            if (arma == null) return;

            int contador = 0;
            if (arma.DynamicVariables.TryGetValue(ClaveBajas, out object valor) && valor is int bajas)
            {
                contador = bajas;
            }

            arma.DynamicVariables[ClaveBajas] = contador + 1;
        }

        public override void ProcesarStats(Weapons.WeaponInstance arma, ref Weapons.WeaponStats stats)
        {
            if (arma == null) return;
            if (!arma.DynamicVariables.TryGetValue(ClaveBajas, out object valor)) return;
            if (valor is not int contador) return;

            stats.BaseDamage += contador * danioExtraPorBaja;
        }
    }
}
