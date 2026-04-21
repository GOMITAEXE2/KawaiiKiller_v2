using System;
using System.Collections;
using UnityEngine;
using KawaiiKiller.Weapons;
using KawaiiKiller.Player;

namespace KawaiiKiller.Debugging
{
    public readonly struct DummyDamageEvent
    {
        public readonly string Timestamp;
        public readonly float IncomingDamage;
        public readonly float HealthBefore;
        public readonly float HealthAfter;
        public readonly bool IsDead;
        public readonly bool IsCriticalPayload;

        public DummyDamageEvent(string timestamp, float incomingDamage, float healthBefore, float healthAfter, bool isDead, bool isCriticalPayload)
        {
            Timestamp = timestamp;
            IncomingDamage = incomingDamage;
            HealthBefore = healthBefore;
            HealthAfter = healthAfter;
            IsDead = isDead;
            IsCriticalPayload = isCriticalPayload;
        }
    }

    public class TestDummyTarget : MonoBehaviour, IDamageable, IBurnable
    {
        [SerializeField] private float maxHealth = 500f;
        [SerializeField] private bool autoRespawn = true;
        [SerializeField] private float respawnDelay = 1.5f;
        [SerializeField] private PlayerWeaponController sourceWeaponController;

        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }
        public int DeathCount { get; private set; }
        public float TotalDamageReceived { get; private set; }

        public event Action<DummyDamageEvent> OnDamaged;
        public event Action<string, int> OnDied;
        public event Action<string, float> OnRespawned;

        private Coroutine _burnRoutine;
        private Coroutine _respawnRoutine;

        private void Awake()
        {
            CurrentHealth = Mathf.Max(1f, maxHealth);
            gameObject.AddComponent<DamageableLink>();
        }

        public void TakeDamage(DamagePayload payload)
        {
            if (IsDead) return;

            float damage = Mathf.Max(0f, payload.BaseDamage);
            float before = CurrentHealth;
            CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
            TotalDamageReceived += damage;
            bool deadNow = CurrentHealth <= 0f;
            string timestamp = DateTime.UtcNow.ToString("O");

            OnDamaged?.Invoke(new DummyDamageEvent(timestamp, damage, before, CurrentHealth, deadNow, payload.CriticalChance >= 1f));
            Debug.Log($"[{timestamp}] [Dummy] Damage={damage:F4} HealthBefore={before:F4} HealthAfter={CurrentHealth:F4} Dead={deadNow}");

            if (deadNow)
            {
                HandleDeath(timestamp);
            }
        }

        public void ApplyBurn(float damagePerSecond, float duration)
        {
            if (_burnRoutine != null)
            {
                StopCoroutine(_burnRoutine);
            }

            _burnRoutine = StartCoroutine(BurnRoutine(Mathf.Max(0f, damagePerSecond), Mathf.Max(0f, duration)));
        }

        private IEnumerator BurnRoutine(float dps, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration && !IsDead)
            {
                TakeDamage(new DamagePayload { BaseDamage = dps, CriticalChance = 0f });
                yield return new WaitForSeconds(1f);
                elapsed += 1f;
            }

            _burnRoutine = null;
        }

        private void HandleDeath(string timestamp)
        {
            IsDead = true;
            DeathCount++;
            OnDied?.Invoke(timestamp, DeathCount);
            Debug.Log($"[{timestamp}] [Dummy] Eliminated DeathCount={DeathCount}");

            WeaponInstance weapon = sourceWeaponController != null ? sourceWeaponController.CurrentWeapon : null;
            if (weapon != null)
            {
                weapon.NotifyEnemyKilled(gameObject);
            }

            if (autoRespawn)
            {
                if (_respawnRoutine != null)
                {
                    StopCoroutine(_respawnRoutine);
                }

                _respawnRoutine = StartCoroutine(RespawnRoutine());
            }
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(Mathf.Max(0f, respawnDelay));
            ResetDummy();
            _respawnRoutine = null;
        }

        [ContextMenu("Reset Dummy")]
        public void ResetDummy()
        {
            if (_burnRoutine != null)
            {
                StopCoroutine(_burnRoutine);
                _burnRoutine = null;
            }

            IsDead = false;
            CurrentHealth = Mathf.Max(1f, maxHealth);
            string timestamp = DateTime.UtcNow.ToString("O");
            OnRespawned?.Invoke(timestamp, CurrentHealth);
            Debug.Log($"[{timestamp}] [Dummy] Respawned Health={CurrentHealth:F4}");
        }
    }
}
