#if UNITY_INCLUDE_TESTS && KAWAII_ENABLE_TESTS
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using KawaiiKiller.Weapons;
using KawaiiKiller.Debugging;

public class ProjectileDamageTests
{
    [UnityTest]
    public IEnumerator Projectile_Damages_Target_On_Hit()
    {
        // --- ARRANGE ---

        // 1. Create Target
        GameObject targetGO = new GameObject("Target");
        targetGO.transform.position = Vector3.forward * 10f;
        TestDummyTarget dummy = targetGO.AddComponent<TestDummyTarget>();
        BoxCollider collider = targetGO.AddComponent<BoxCollider>();
        float initialHealth = dummy.CurrentHealth;

        // 2. Create Projectile
        GameObject projectileGO = new GameObject("Projectile");
        Projectile projectile = projectileGO.AddComponent<Projectile>();

        // 3. Define projectile properties
        DamagePayload payload = new DamagePayload { BaseDamage = 10f, CriticalChance = 0f };
        Vector3 direction = Vector3.forward;
        float speed = 50f;
        float lifetime = 2f;

        // --- ACT ---

        // Initialize and "fire" the projectile
        projectile.Initialize(payload, direction, speed, lifetime);

        // Wait for a physics frame to allow collision to register
        yield return new WaitForFixedUpdate();
        // Wait another frame to be sure
        yield return new WaitForFixedUpdate();


        // --- ASSERT ---
        Assert.Less(dummy.CurrentHealth, initialHealth, "Target health should decrease after being hit.");
        Assert.AreEqual(initialHealth - payload.BaseDamage, dummy.CurrentHealth, "Target health should be reduced by the projectile's base damage.");

        // --- CLEANUP ---
        Object.Destroy(targetGO);
        Object.Destroy(projectileGO);
    }
}
#endif
