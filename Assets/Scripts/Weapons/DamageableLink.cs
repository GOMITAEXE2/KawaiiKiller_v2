using UnityEngine;

namespace KawaiiKiller.Weapons
{
    public class DamageableLink : MonoBehaviour
    {
        public IDamageable Damageable { get; private set; }

        private void Awake()
        {
            Damageable = GetComponent<IDamageable>();
        }
    }
}
