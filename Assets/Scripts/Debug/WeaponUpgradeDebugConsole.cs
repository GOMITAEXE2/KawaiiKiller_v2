using TMPro;
using UnityEngine;
using KawaiiKiller.Player;
using System.Collections.Generic; // Keep for potential future use, though not strictly needed now.

namespace KawaiiKiller.Debugging
{
    public class WeaponUpgradeDebugConsole : MonoBehaviour
    {
        [SerializeField] private PlayerWeaponController weaponController;
        [SerializeField] private TestDummyTarget dummyTarget;
        [SerializeField] private TMP_Text outputText;

        // --- State Variables for HUD ---
        
        // Weapon State
        private string _weaponId = "N/A";
        private int _ammoAfterShot = 0;
        private float _finalDamage = 0f;
        private string _activatedEffects = "Ninguno";

        // Hit State
        private float _lastIncomingDamage = 0f;
        private bool _wasLastHitCritical = false;
        private float _dummyHealthAfter = 0f;
        
        // Burn State (with a simple timer for the indicator)
        private bool _isBurning = false;
        private float _burnDamage = 0f;
        private float _lastBurnTickTime = -1f;
        private const float BURN_INDICATOR_DURATION = 1.2f; // How long "BURNING" stays visible after a tick

        private void OnEnable()
        {
            if (weaponController != null) weaponController.OnShotFired += HandleShot;
            if (dummyTarget != null)
            {
                dummyTarget.OnDamaged += HandleDummyDamaged;
                // Keeping these in case you want to add more HUD features later
                dummyTarget.OnDied += HandleDummyDied;
                dummyTarget.OnRespawned += HandleDummyRespawn;
            }
        }

        private void OnDisable()
        {
            if (weaponController != null) weaponController.OnShotFired -= HandleShot;
            if (dummyTarget != null)
            {
                dummyTarget.OnDamaged -= HandleDummyDamaged;
                dummyTarget.OnDied -= HandleDummyDied;
                dummyTarget.OnRespawned -= HandleDummyRespawn;
            }
        }

        private void Update()
        {
            if (outputText == null) return;

            // --- Update HUD Text Every Frame ---

            // Check if the burn indicator should be turned off
            if (_isBurning && Time.time - _lastBurnTickTime > BURN_INDICATOR_DURATION)
            {
                _isBurning = false;
            }
            
            // Part 1: Weapon Status
            string weaponPart = $"ARMA: {_weaponId} [{_ammoAfterShot}] | DAÑO: {_finalDamage:F1} | MEJORAS: {_activatedEffects}";

            // Part 2: Hit Status
            string hitPart = $"IMPACTO: {_lastIncomingDamage:F1} (Critico: {_wasLastHitCritical}) | VIDA DUMMY: {_dummyHealthAfter:F1}";

            // Part 3: Burn Status (optional)
            string burnPart = _isBurning ? $" | QUEMADURA (+{_burnDamage:F1})" : "";

            // Combine and set the text
            outputText.text = $"{weaponPart}\n{hitPart}{burnPart}";
        }

        private void HandleShot(WeaponShotTelemetry telemetry)
        {
            // Update weapon state from shot data
            _weaponId = telemetry.WeaponId;
            _ammoAfterShot = telemetry.AmmoAfterShot;
            _finalDamage = telemetry.FinalDamage;
            _activatedEffects = string.IsNullOrEmpty(telemetry.ActivatedEffects) || telemetry.ActivatedEffects == "Ninguno" 
                ? "Ninguna" 
                : telemetry.ActivatedEffects;
        }

        private void HandleDummyDamaged(DummyDamageEvent damageEvent)
        {
            // Update hit and dummy state from damage data
            _lastIncomingDamage = damageEvent.IncomingDamage;
            _wasLastHitCritical = damageEvent.IsCriticalPayload;
            _dummyHealthAfter = damageEvent.HealthAfter;

            // Heuristic to detect if it's a burn tick:
            // If the damage isn't a crit and is significantly smaller than the weapon's base damage,
            // we assume it's a Damage-Over-Time effect.
            // This could be made more robust by adding a 'DamageType' to the event payload.
            if (!_wasLastHitCritical && _lastIncomingDamage < _finalDamage * 0.75f)
            {
                _isBurning = true;
                _burnDamage = _lastIncomingDamage;
                _lastBurnTickTime = Time.time;
            }
        }
        
        private void HandleDummyDied(string timestamp, int deathCount)
        {
            // You could add a "Kills: X" to the HUD here if you wanted
        }

        private void HandleDummyRespawn(string timestamp, float health)
        {
            // Reset hit-related stats on respawn
            _lastIncomingDamage = 0f;
            _wasLastHitCritical = false;
            _dummyHealthAfter = health;
            _isBurning = false;
        }
    }
}
