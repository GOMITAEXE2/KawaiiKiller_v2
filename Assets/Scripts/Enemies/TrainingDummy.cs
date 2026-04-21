using System.Collections;
using UnityEngine;
using KawaiiKiller.Weapons;

namespace KawaiiKiller.Core
{
    [RequireComponent(typeof(DamageableLink))]
    public class TrainingDummy : MonoBehaviour, IDamageable, IBurnable
    {
        [SerializeField] private float maxHealth    = 500f;
        [SerializeField] private float respawnDelay = 2f;

        public float CurrentHealth { get; private set; }
        public float MaxHealth     => maxHealth;
        public bool  IsDead        { get; private set; }

        public event System.Action<float, float> OnHealthChanged;
        public event System.Action               OnDeath;
        public event System.Action               OnRespawn;

        private Coroutine _burnRoutine;
        private Coroutine _respawnRoutine;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(DamagePayload payload)
        {
            if (IsDead) return;

            float dmg = Mathf.Max(0f, payload.BaseDamage);
            CurrentHealth = Mathf.Max(0f, CurrentHealth - dmg);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth <= 0f) HandleDeath();
        }

        public void ApplyBurn(float damagePerSecond, float duration)
        {
            if (_burnRoutine != null) StopCoroutine(_burnRoutine);
            _burnRoutine = StartCoroutine(BurnRoutine(damagePerSecond, duration));
        }

        private IEnumerator BurnRoutine(float dps, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration && !IsDead)
            {
                TakeDamage(new DamagePayload { BaseDamage = dps });
                yield return new WaitForSeconds(1f);
                elapsed += 1f;
            }
            _burnRoutine = null;
        }

        private void HandleDeath()
        {
            IsDead = true;
            OnDeath?.Invoke();

            if (_respawnRoutine != null) StopCoroutine(_respawnRoutine);
            _respawnRoutine = StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnDelay);

            if (_burnRoutine != null) { StopCoroutine(_burnRoutine); _burnRoutine = null; }

            IsDead        = false;
            CurrentHealth = maxHealth;
            OnRespawn?.Invoke();
            _respawnRoutine = null;
        }
    }
}