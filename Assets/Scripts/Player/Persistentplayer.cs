using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using KawaiiKiller.Weapons;
using KawaiiKiller.Modifiers;

namespace KawaiiKiller.Player
{
    public class PersistentPlayer : MonoBehaviour
    {
        public static PersistentPlayer Instance { get; private set; }

        public bool HasPendingInjection { get; private set; }

        private readonly List<WeaponDataSO> _savedWeapons   = new List<WeaponDataSO>();
        private readonly List<List<string>> _savedModifiers = new List<List<string>>();
        private readonly List<int>          _savedAmmo      = new List<int>();
        private int _savedMoney;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        public void CaptureStateFromSave(KawaiiKiller.Core.SaveData saveData)
        {
            if (saveData == null) return;

            _savedWeapons.Clear();
            _savedModifiers.Clear();
            _savedAmmo.Clear();

            _savedMoney = saveData.currentMoney;

            foreach (KawaiiKiller.Core.WeaponSaveEntry entry in saveData.loadout)
            {
                if (string.IsNullOrEmpty(entry.weaponID)) continue;

                WeaponDataSO weapon = KawaiiKiller.Core.SaveSystem.Instance?.FindWeapon(entry.weaponID);
                if (weapon == null) continue;

                _savedWeapons.Add(weapon);
                _savedAmmo.Add(entry.currentAmmo);

                List<string> modIDs = new List<string>();
                foreach (KawaiiKiller.Core.ModifierSaveEntry modEntry in entry.modifierSlots)
                    modIDs.Add(modEntry.modifierID ?? string.Empty);

                _savedModifiers.Add(modIDs);
            }

            HasPendingInjection = _savedWeapons.Count > 0;
        }

        public void CaptureState(PlayerWeaponController controller, PlayerEconomy economy)
        {
            _savedWeapons.Clear();
            _savedModifiers.Clear();
            _savedAmmo.Clear();

            _savedMoney = economy.CurrentMoney;

            foreach (WeaponInstance instance in controller.Loadout)
            {
                if (instance?.Data == null) continue;

                _savedWeapons.Add(instance.Data);
                _savedAmmo.Add(instance.CurrentAmmo);

                List<string> modIDs = new List<string>();
                foreach (ModifierSlot slot in instance.ModifierSlots)
                    modIDs.Add(slot.IsEmpty ? string.Empty : slot.Data.ID);

                _savedModifiers.Add(modIDs);
            }

            HasPendingInjection = true;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!HasPendingInjection) return;

            PlayerWeaponController controller = FindFirstObjectByType<PlayerWeaponController>();
            PlayerEconomy          economy    = FindFirstObjectByType<PlayerEconomy>();

            if (controller == null || economy == null) return;

            economy.SetMoney(_savedMoney);
            InjectLoadout(controller);

            HasPendingInjection = false;
        }

        private void InjectLoadout(PlayerWeaponController controller)
        {
            controller.ClearLoadoutForInjection();

            for (int i = 0; i < _savedWeapons.Count; i++)
            {
                WeaponDataSO weaponData = _savedWeapons[i];
                if (weaponData == null) continue;
                if (!controller.TryAddWeaponToInventory(weaponData)) continue;

                WeaponInstance instance = controller.Loadout[controller.Loadout.Count - 1];
                if (instance == null) continue;

                // Resuelve modificadores via SaveSystem que ya referencia ShopDatabaseSO
                if (i < _savedModifiers.Count)
                {
                    List<string> modIDs = _savedModifiers[i];
                    for (int j = 0; j < modIDs.Count && j < instance.ModifierSlots.Count; j++)
                    {
                        if (string.IsNullOrEmpty(modIDs[j])) continue;
                        ModifierDataSO mod = KawaiiKiller.Core.SaveSystem.Instance?.FindModifier(modIDs[j]);
                        if (mod != null) instance.TryEquipModifier(j, mod);
                    }
                }

                if (i < _savedAmmo.Count)
                    instance.CurrentAmmo = _savedAmmo[i];
            }
        }
    }
}