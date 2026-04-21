using System.Collections.Generic;
using UnityEngine;

namespace KawaiiKiller.Modifiers
{
    public abstract class ModifierEffectSO : ScriptableObject
    {
        public virtual void ProcesarTipos(Weapons.WeaponInstance arma, List<Weapons.TipoArma> tiposActuales) { }
        public virtual void ProcesarStats(Weapons.WeaponInstance arma, ref Weapons.WeaponStats stats) => ApplyStats(arma, ref stats);
        public virtual void AlMatarEnemigo(Weapons.WeaponInstance arma, GameObject objetivo) { }
        public virtual void ApplyStats(Weapons.WeaponInstance arma, ref Weapons.WeaponStats stats) { }
        public virtual void OnShotFired(Weapons.WeaponInstance arma, ref Weapons.DamagePayload payload) { }
        public virtual void OnProjectileHit(Weapons.WeaponInstance arma, GameObject objetivo, ref Weapons.DamagePayload payload) { }
        public virtual void OnReload(Weapons.WeaponInstance arma) { }
    }
}
