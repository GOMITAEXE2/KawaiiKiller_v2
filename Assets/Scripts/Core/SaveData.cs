using System;
using System.Collections.Generic;

namespace KawaiiKiller.Core
{
    [Serializable]
    public class SaveData
    {
        public GameManager.GameMode gameMode;
        public int   currentRound;
        public int   currentMoney;
        public List<WeaponSaveEntry> loadout = new List<WeaponSaveEntry>();
    }

    [Serializable]
    public class WeaponSaveEntry
    {
        public string weaponID;
        public int    currentAmmo;
        public List<ModifierSaveEntry> modifierSlots = new List<ModifierSaveEntry>();
    }

    [Serializable]
    public class ModifierSaveEntry
    {
        public string modifierID; // vacío si el slot está libre
    }
}