using System;
using UnityEngine;
using KawaiiKiller.Player;

namespace KawaiiKiller.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth  = 100f;
        [SerializeField] private WinLoseHandler winLoseHandler;

        public float MaxHealth     { get; private set; }
        public float CurrentHealth { get; private set; }
        public bool  IsInvulnerable { get; private set; }
        public bool  IsDead        { get; private set; }

        public event Action<float, float> OnHealthChanged;
        public event Action               OnDeath;

        private void Start()
        {
            MaxHealth     = maxHealth;
            CurrentHealth = MaxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || IsInvulnerable || amount <= 0f) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (CurrentHealth <= 0f) Die();
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;

            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void SetInvulnerable(bool value) => IsInvulnerable = value;

        public void SetMaxHealth(float value, bool refill = false)
        {
            MaxHealth = Mathf.Max(1f, value);
            if (refill) CurrentHealth = MaxHealth;
            else CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;

            OnDeath?.Invoke();
            winLoseHandler?.HandlePlayerDeath();
        }
    }
}