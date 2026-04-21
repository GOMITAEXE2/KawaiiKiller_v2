using UnityEngine;
using KawaiiKiller.Player;
using KawaiiKiller.Weapons;
using KawaiiKiller.Modifiers;
using KawaiiKiller.UI.Shop;

namespace KawaiiKiller.Core
{
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        private const string SaveKey           = "KawaiiKiller_Save";
        private const string EndlessUnlockKey  = "KawaiiKiller_EndlessUnlocked";

        [SerializeField] private ShopDatabaseSO database;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool HasSave() => PlayerPrefs.HasKey(SaveKey);

        public void Save(PlayerEconomy economy, PlayerWeaponController weaponController)
        {
            if (economy == null || weaponController == null) return;

            SaveData data     = new SaveData();
            data.gameMode     = GameManager.Instance.CurrentMode;
            data.currentRound = WaveManager.Instance != null ? WaveManager.Instance.CurrentRound : 1;
            data.currentMoney = economy.CurrentMoney;

            foreach (WeaponInstance instance in weaponController.Loadout)
            {
                if (instance?.Data == null) continue;

                WeaponSaveEntry entry = new WeaponSaveEntry
                {
                    weaponID    = instance.Data.ID,
                    currentAmmo = instance.CurrentAmmo
                };

                foreach (ModifierSlot slot in instance.ModifierSlots)
                    entry.modifierSlots.Add(new ModifierSaveEntry
                    {
                        modifierID = slot.IsEmpty ? string.Empty : slot.Data.ID
                    });

                data.loadout.Add(entry);
            }

            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public SaveData Load() =>
            HasSave() ? JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey)) : null;

        public void ClearSave() => PlayerPrefs.DeleteKey(SaveKey);

        public WeaponDataSO   FindWeapon(string id)   => database?.FindWeapon(id);
        public ModifierDataSO FindModifier(string id) => database?.FindModifier(id);

        public bool IsEndlessUnlocked() => PlayerPrefs.GetInt(EndlessUnlockKey, 0) == 1;

        public void SetEndlessUnlocked()
        {
            PlayerPrefs.SetInt(EndlessUnlockKey, 1);
            PlayerPrefs.Save();
        }
    }
}