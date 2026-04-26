using UnityEngine;

namespace KawaiiKiller.Player
{
    public class PlayerEconomy : MonoBehaviour
    {
        public static PlayerEconomy Instance { get; private set; }

        [SerializeField] private int _startingMoney = 1000;
        public int CurrentMoney { get; private set; }

        public event System.Action<int, int> OnMoneyChanged; // (current, change)

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            // Solo aplica el valor inicial si PersistentPlayer no inyectó uno
            if (PersistentPlayer.Instance == null || !PersistentPlayer.Instance.HasPendingInjection)
            {
                CurrentMoney = _startingMoney;
                OnMoneyChanged?.Invoke(CurrentMoney, 0);
            }
        }

        public bool HasEnough(int amount) => CurrentMoney >= amount;

        public bool TrySpend(int amount)
        {
            if (amount <= 0) return false;
            if (!HasEnough(amount)) return false;
            CurrentMoney -= amount;
            OnMoneyChanged?.Invoke(CurrentMoney, -amount);
            return true;
        }

        public void AddMoney(int amount)
        {
            if (amount <= 0) return;
            CurrentMoney += amount;
            OnMoneyChanged?.Invoke(CurrentMoney, amount);
        }

        public void SetMoney(int amount)
        {
            int old = CurrentMoney;
            CurrentMoney = Mathf.Max(0, amount);
            OnMoneyChanged?.Invoke(CurrentMoney, CurrentMoney - old);
        }
    }
}