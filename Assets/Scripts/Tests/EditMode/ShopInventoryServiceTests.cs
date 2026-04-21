#if UNITY_INCLUDE_TESTS && KAWAII_ENABLE_TESTS
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using KawaiiKiller.Player;
using KawaiiKiller.Weapons;
using KawaiiKiller.UI.Shop;
using KawaiiKiller.Modifiers;

public class ShopInventoryServiceTests
{
    private class TestDamageEffectSO : ModifierEffectSO
    {
        public float ExtraDamage = 5f;
        public override void ApplyStats(WeaponInstance arma, ref WeaponStats stats)
        {
            stats.BaseDamage += ExtraDamage;
        }
    }

    [Test]
    public void BuyWeapon_SpendsMoney_AndAddsWeapon()
    {
        GameObject go = new GameObject("player-test");
        PlayerEconomy economy = go.AddComponent<PlayerEconomy>();
        PlayerWeaponController controller = go.AddComponent<PlayerWeaponController>();
        economy.AddMoney(500);

        WeaponDataSO weapon = CreateWeaponData("RifleTest", 120);
        ShopInventoryService service = new ShopInventoryService();
        ShopTransactionResult result = service.TryBuyWeapon(economy, controller, weapon, 120);

        Assert.AreEqual(ShopTransactionStatus.Success, result.Status);
        Assert.AreEqual(380, economy.CurrentMoney);
        Assert.AreEqual(1, controller.Loadout.Count);
    }

    [Test]
    public void SellWeapon_WhenLastWeaponAndFistsConfigured_KeepsFallback()
    {
        GameObject go = new GameObject("player-test-sell");
        PlayerEconomy economy = go.AddComponent<PlayerEconomy>();
        PlayerWeaponController controller = go.AddComponent<PlayerWeaponController>();
        economy.AddMoney(500);

        WeaponDataSO fists = CreateWeaponData("Fists", 0);
        FieldInfo fistsField = typeof(PlayerWeaponController).GetField("fistsWeapon", BindingFlags.NonPublic | BindingFlags.Instance);
        fistsField.SetValue(controller, fists);

        WeaponDataSO weapon = CreateWeaponData("Shotgun", 200);
        controller.TryAddWeaponToInventory(weapon);

        ShopInventoryService service = new ShopInventoryService();
        ShopTransactionResult result = service.TrySellWeapon(economy, controller, 0, 0.5f);

        Assert.AreEqual(ShopTransactionStatus.Success, result.Status);
        Assert.AreEqual(600, economy.CurrentMoney);
        Assert.AreEqual(1, controller.Loadout.Count);
        Assert.AreEqual("Fists", controller.Loadout[0].Data.Nombre);
    }

    [Test]
    public void WeaponInstance_AppliesStartingModifierToMatchingSlot()
    {
        WeaponDataSO weapon = CreateWeaponData("SMG", 100);
        weapon.MaxUpgrades = 1;
        weapon.MaxAttachments = 0;
        weapon.WeaponStats = new WeaponStats
        {
            BaseDamage = 10f,
            FireRate = 2f,
            CriticalChance = 0f,
            ProjectilesPerShot = 1,
            IsAutomatic = true,
            BaseSpreadAngle = 1f,
            AimingSpreadAngle = 0.5f,
            MagazineSize = 20,
            ReloadTime = 1f
        };

        ModifierDataSO modifier = ScriptableObject.CreateInstance<ModifierDataSO>();
        modifier.TipoDeSlot = SlotType.Upgrade;
        TestDamageEffectSO effect = ScriptableObject.CreateInstance<TestDamageEffectSO>();
        modifier.Efectos = new List<ModifierEffectSO> { effect };
        weapon.StartingModifiers = new List<ModifierDataSO> { modifier };

        WeaponInstance instance = new WeaponInstance(weapon);

        Assert.AreEqual(1, instance.ModifierSlots.Count);
        Assert.IsFalse(instance.ModifierSlots[0].IsEmpty);
        Assert.AreEqual(15f, instance.FinalStats.BaseDamage);
    }

    private WeaponDataSO CreateWeaponData(string name, int price)
    {
        WeaponDataSO weapon = ScriptableObject.CreateInstance<WeaponDataSO>();
        weapon.ID = name + "_ID";
        weapon.Nombre = name;
        weapon.Precio = price;
        weapon.Tipos = new List<TipoArma> { TipoArma.Rifle };
        weapon.WeaponStats = new WeaponStats
        {
            BaseDamage = 20f,
            FireRate = 3f,
            CriticalChance = 0f,
            ProjectilesPerShot = 1,
            IsAutomatic = true,
            BaseSpreadAngle = 1f,
            AimingSpreadAngle = 0.5f,
            MagazineSize = 30,
            ReloadTime = 1f
        };
        return weapon;
    }
}
#endif
