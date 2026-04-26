using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using KawaiiKiller.Weapons;

namespace KawaiiKiller.Player
{
    public readonly struct WeaponShotTelemetry
    {
        public readonly string Timestamp;
        public readonly string WeaponId;
        public readonly float BaseDamage;
        public readonly float FinalDamage;
        public readonly float CriticalChance;
        public readonly int AmmoAfterShot;
        public readonly string ActivatedEffects;

        public WeaponShotTelemetry(string timestamp, string weaponId, float baseDamage, float finalDamage,
            float criticalChance, int ammoAfterShot, string activatedEffects)
        {
            Timestamp        = timestamp;
            WeaponId         = weaponId;
            BaseDamage       = baseDamage;
            FinalDamage      = finalDamage;
            CriticalChance   = criticalChance;
            AmmoAfterShot    = ammoAfterShot;
            ActivatedEffects = activatedEffects;
        }
    }

    public class PlayerWeaponController : MonoBehaviour
    {
        public static PlayerWeaponController Instance { get; private set; }
        public enum WeaponState { Idle, Shooting, Reloading }

        // ── Inspector ──────────────────────────────────────────────────────────────
        [FormerlySerializedAs("initialWeapons")]
        [SerializeField] private List<WeaponDataSO> weaponList;

        // Arma de fallback: solo se usa en gameplay cuando el jugador no tiene
        // ninguna arma real. NUNCA ocupa un slot visible en el inventario de la UI.
        [SerializeField] private WeaponDataSO fistsWeapon;

        [SerializeField] private Transform weaponParent;
        [SerializeField] private Transform weaponFirePort;

        [FormerlySerializedAs("loadoutCapacity")]
        [SerializeField] private int baseCapacity = 4;

        [Header("Recoil References")]
        [SerializeField] private PlayerCamera playerCamera;
        [SerializeField] private CameraSpring cameraSpring;

        [Header("Recoil Recovery")]
        [SerializeField] private float recoilRecoverySpeed = 8f;

        // ── Runtime ────────────────────────────────────────────────────────────────
        private readonly List<WeaponInstance> _loadout = new List<WeaponInstance>();
        private int         _currentWeaponIndex  = -1;
        private GameObject  _currentWeaponVisual;
        private Transform   _activeFirePort;
        private WeaponState _currentState        = WeaponState.Idle;
        private Coroutine   _reloadCoroutine;
        private bool        _isAiming;
        private bool        _isInventoryOpen;
        private bool        _externalInputLocked;
        private float       _nextFireTime;
        private int         _capacityBonus       = 0;
        private readonly List<string> _activatedEffectsBuffer = new List<string>(8);
        private PlayerInputActions _inputActions;

        // ── Recoil state ──────────────────────────────────────────────────────────
        private float   _accumulatedPitch;
        private Vector3 _currentKickPos;
        private Vector3 _currentKickRot;
        private Vector3 _weaponParentOriginalPos;
        private Vector3 _weaponParentOriginalRot;
        private bool    _weaponParentOriginCaptured;

        // ── Effects state ─────────────────────────────────────────────────────────
        private AudioSource    _audioSource;
        private GameObject     _activeMuzzleFlash;
        private bool           _wasFiringLastFrame;

        // ── Eventos ────────────────────────────────────────────────────────────────
        public static event Action    OnInventoryToggled;
        public event Action<WeaponShotTelemetry> OnShotFired;
        public event Action           OnLoadoutChanged;
        public event Action<int>      OnCurrentWeaponIndexChanged;

        // ── Propiedades públicas ───────────────────────────────────────────────────
        public IReadOnlyList<WeaponInstance> Loadout            => _loadout;
        public int                           CurrentWeaponIndex => _currentWeaponIndex;
        public int                           CurrentCapacity    => baseCapacity + _capacityBonus;

        // Alias mantenido por compatibilidad con otros scripts que lo referencien
        public int LoadoutCapacity => CurrentCapacity;

        public bool IsAiming => _isAiming;

        /// <summary>El arma de fallback (puños). La UI la usa para distinguir
        /// si el jugador tiene armas reales o solo el fallback.</summary>
        public WeaponDataSO FallbackWeapon => fistsWeapon;

        /// <summary>True cuando el loadout contiene exclusivamente el arma de fallback.</summary>
        public bool IsUsingFallbackOnly =>
            fistsWeapon != null && _loadout.Count == 1 && _loadout[0].Data == fistsWeapon;

        public WeaponInstance CurrentWeapon =>
            _currentWeaponIndex >= 0 && _currentWeaponIndex < _loadout.Count
                ? _loadout[_currentWeaponIndex]
                : null;

        // ── Lifecycle ──────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _inputActions = new PlayerInputActions();
            _inputActions.Enable();

            // Asegurar que existe un AudioSource para los efectos de sonido
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f; // 2D para el jugador local
        }

        private void OnDestroy()
        {
            _inputActions?.Dispose();
        }

        private void Start()
        {
            _activeFirePort = weaponFirePort;
            InitializeLoadout();
            if (_loadout.Count > 0)
                EquipWeapon(0);
        }

        private void Update()
        {
            UpdateRecoilRecovery();

            if (_externalInputLocked) return;
            HandleInventoryInput();
            if (_isInventoryOpen || CurrentWeapon == null) return;

            HandleWeaponSwitchInput();
            HandleAimingInput();

            if (_currentState == WeaponState.Reloading) return;

            // Detectar si el jugador dejó de disparar para el efecto de humo
            bool isFiringThisFrame = _currentState == WeaponState.Shooting;
            if (_wasFiringLastFrame && !isFiringThisFrame)
                SpawnSmokeEffect();
            _wasFiringLastFrame = isFiringThisFrame;

            HandleShootingInput();
            HandleReloadInput();
        }

        // ── Inicialización ─────────────────────────────────────────────────────────
        private void InitializeLoadout()
        {
            _loadout.Clear();

            if (weaponList != null)
            {
                foreach (WeaponDataSO data in weaponList)
                {
                    if (data == null) continue;
                    if (_loadout.Count >= CurrentCapacity) break;
                    _loadout.Add(new WeaponInstance(data));
                }
            }

            EnsureFallbackWeaponExists();
            OnLoadoutChanged?.Invoke();
        }

        // ── Input handlers ─────────────────────────────────────────────────────────
        private void HandleInventoryInput()
        {
            if (!Input.GetKeyDown(KeyCode.Tab)) return;
            _isInventoryOpen = !_isInventoryOpen;
            _isAiming        = false;
            OnInventoryToggled?.Invoke();
        }

        private void HandleAimingInput()
        {
            bool aimFromActions = false;
            try 
            { 
                // Use reflection to avoid compile errors if 'Aim' is not yet defined in the generated class
                var gameplay = _inputActions.Gameplay;
                var aimProp = gameplay.GetType().GetProperty("Aim");
                if (aimProp != null)
                {
                    var action = aimProp.GetValue(gameplay) as UnityEngine.InputSystem.InputAction;
                    if (action != null)
                    {
                        aimFromActions = action.ReadValue<float>() > 0.5f;
                    }
                }
            }
            catch { /* action not defined yet in the Input Asset */ }
            
            _isAiming = aimFromActions || Input.GetMouseButton(1);
        }

        private void HandleWeaponSwitchInput()
        {
            float wheel = Input.GetAxis("Mouse ScrollWheel");
            if (wheel > 0f) EquipWeapon(WrapIndex(_currentWeaponIndex + 1));
            else if (wheel < 0f) EquipWeapon(WrapIndex(_currentWeaponIndex - 1));

            for (int i = 0; i < 9; i++)
            {
                if (!Input.GetKeyDown(KeyCode.Alpha1 + i)) continue;
                if (i < _loadout.Count) EquipWeapon(i);
                return;
            }
        }

        private int WrapIndex(int index)
        {
            if (_loadout.Count == 0) return -1;
            if (index >= _loadout.Count) return 0;
            if (index < 0) return _loadout.Count - 1;
            return index;
        }

        private void EquipWeapon(int index)
        {
            if (index < 0 || index >= _loadout.Count || index == _currentWeaponIndex) return;

            CancelReloadIfNeeded();

            if (_currentWeaponVisual != null)
                Destroy(_currentWeaponVisual);

            _currentWeaponIndex = index;
            WeaponDataSO data   = CurrentWeapon.Data;

            if (data != null && data.Prefab3D != null && weaponParent != null)
            {
                _currentWeaponVisual = Instantiate(data.Prefab3D, weaponParent);
                Transform firePoint  = _currentWeaponVisual.transform.Find("FirePoint");
                _activeFirePort      = firePoint != null ? firePoint : weaponFirePort;
            }
            else
            {
                _activeFirePort = weaponFirePort;
            }

            // Capturar posición original del weaponParent y resetear kick
            CaptureWeaponParentOrigin();
            _currentKickPos = Vector3.zero;
            _currentKickRot = Vector3.zero;

            _currentState  = WeaponState.Idle;
            _nextFireTime  = Time.time;
            OnCurrentWeaponIndexChanged?.Invoke(_currentWeaponIndex);
        }

        // ── Disparo ────────────────────────────────────────────────────────────────
        private void HandleShootingInput()
        {
            WeaponStats stats  = CurrentWeapon.FinalStats;
            bool wantsToShoot  = stats.IsAutomatic ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
            if (!wantsToShoot || Time.time < _nextFireTime) return;

            if (CurrentWeapon.CurrentAmmo <= 0) { AttemptReload(); return; }

            FireProjectile();
        }

        private void FireProjectile()
        {
            _currentState             = WeaponState.Shooting;
            CurrentWeapon.CurrentAmmo--;
            _nextFireTime             = Time.time + CurrentWeapon.FinalStats.FireDelay;
            float baseDamage          = CurrentWeapon.FinalStats.BaseDamage;

            DamagePayload payload = new DamagePayload
            {
                BaseDamage     = CurrentWeapon.FinalStats.BaseDamage,
                CriticalChance = CurrentWeapon.FinalStats.CriticalChance
            };

            _activatedEffectsBuffer.Clear();
            CurrentWeapon.OnShotFired(ref payload, _activatedEffectsBuffer);

            Projectile projectilePrefab = CurrentWeapon.Data?.ProjectilePrefab;
            Transform  spawnPort        = _activeFirePort  != null ? _activeFirePort  : weaponFirePort;
            Transform  directionPort    = weaponFirePort   != null ? weaponFirePort   : spawnPort;

            if (projectilePrefab != null && spawnPort != null)
            {
                float spreadAngle = _isAiming
                    ? CurrentWeapon.FinalStats.AimingSpreadAngle
                    : CurrentWeapon.FinalStats.BaseSpreadAngle;

                int projectileCount = Mathf.Max(1, CurrentWeapon.FinalStats.ProjectilesPerShot);
                float speed         = CurrentWeapon.FinalStats.FinalProjectileSpeed;
                Color color         = CurrentWeapon.Data.ProjectileColor;

#if UNITY_EDITOR
                // Verde: dirección a la que apunta la cámara del jugador
                Debug.DrawRay(directionPort.position, directionPort.forward * 25f, Color.green, 2f);
#endif

                for (int i = 0; i < projectileCount; i++)
                {
                    Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * Mathf.Tan(spreadAngle * Mathf.Deg2Rad);
                    Vector3 spreadDir    = directionPort.TransformDirection(
                        new Vector3(randomCircle.x, randomCircle.y, 1f)).normalized;

#if UNITY_EDITOR
                    // Azul: trayectoria real de este proyectil desde el cañón del arma
                    Debug.DrawRay(spawnPort.position, spreadDir * 25f, Color.blue, 2f);
#endif

                    Projectile p = Instantiate(projectilePrefab, spawnPort.position, Quaternion.LookRotation(spreadDir));
                    p.SetSourceWeapon(CurrentWeapon);
                    p.Initialize(payload, spreadDir, speed, 5f);
                    p.SetVisuals(color);
                }
            }

            // Aplicar recoil del arma activa
            ApplyRecoil();

            // Efectos visuales y de sonido
            PlayMuzzleFlash();
            PlayFireSound();

            _currentState = WeaponState.Idle;

            if (CurrentWeapon.CurrentAmmo <= 0)
                AttemptReload();

            string effectSummary = _activatedEffectsBuffer.Count > 0
                ? string.Join(", ", _activatedEffectsBuffer) : "Ninguno";
            string weaponId = CurrentWeapon.Data != null ? CurrentWeapon.Data.ID : "SinID";

            OnShotFired?.Invoke(new WeaponShotTelemetry(
                DateTime.UtcNow.ToString("O"), weaponId,
                baseDamage, payload.BaseDamage, payload.CriticalChance,
                CurrentWeapon.CurrentAmmo, effectSummary));
        }

        // ── Recarga ────────────────────────────────────────────────────────────────
        private void HandleReloadInput()
        {
            if (Input.GetKeyDown(KeyCode.R)) AttemptReload();
        }

        private void AttemptReload()
        {
            if (_currentState == WeaponState.Reloading) return;
            if (CurrentWeapon == null) return;
            if (CurrentWeapon.CurrentAmmo >= CurrentWeapon.FinalStats.MagazineSize) return;
            _reloadCoroutine = StartCoroutine(ReloadRoutine());
        }

        private IEnumerator ReloadRoutine()
        {
            _currentState = WeaponState.Reloading;

            // Sonido de recarga del arma activa
            PlayReloadSound();

            float time    = Mathf.Max(0f, CurrentWeapon.FinalStats.ReloadTime);
            yield return new WaitForSeconds(time);

            if (CurrentWeapon != null)
            {
                CurrentWeapon.CurrentAmmo = CurrentWeapon.FinalStats.MagazineSize;
                CurrentWeapon.OnReload();
            }

            _currentState    = WeaponState.Idle;
            _reloadCoroutine = null;
        }

        private void CancelReloadIfNeeded()
        {
            if (_reloadCoroutine == null) return;
            StopCoroutine(_reloadCoroutine);
            _reloadCoroutine = null;
            _currentState    = WeaponState.Idle;
        }

        // ── API pública ────────────────────────────────────────────────────────────
        public void SetExternalInputLocked(bool locked)
        {
            _externalInputLocked = locked;
        }

        public void ClearLoadoutForInjection()
        {
            CancelReloadIfNeeded();
        
            if (_currentWeaponVisual != null)
                Destroy(_currentWeaponVisual);
        
            _loadout.Clear();
            _currentWeaponIndex = -1;
            _currentWeaponVisual = null;
            _activeFirePort = weaponFirePort;
        }

        /// <summary>
        /// Añade un arma real al inventario del jugador.
        /// Si el jugador solo tenía el arma de fallback (puños), la remueve primero:
        /// los puños no ocupan un slot real.
        /// Retorna false si el inventario real está lleno o el arma es nula.
        /// </summary>
        public bool TryAddWeaponToInventory(WeaponDataSO weaponData)
        {
            if (weaponData == null) return false;

            // Remover el arma de fallback si era lo único en el loadout.
            // Los puños son solo un placeholder de gameplay; no consumen slot.
            if (IsUsingFallbackOnly)
            {
                _loadout.RemoveAt(0);
                _currentWeaponIndex = -1;
            }

            if (_loadout.Count >= CurrentCapacity) return false;

            _loadout.Add(new WeaponInstance(weaponData));
            OnLoadoutChanged?.Invoke();

            if (_currentWeaponIndex < 0)
                EquipWeapon(0);

            return true;
        }

        public bool TryRemoveWeaponAt(int index, out WeaponDataSO removedData)
        {
            removedData = null;
            if (index < 0 || index >= _loadout.Count) return false;

            WeaponInstance instance = _loadout[index];
            if (instance == null || instance.Data == null) return false;

            // No se pueden quitar los puños si son lo único
            if (fistsWeapon != null && instance.Data == fistsWeapon && _loadout.Count == 1)
                return false;

            removedData = instance.Data;
            _loadout.RemoveAt(index);
            EnsureFallbackWeaponExists();

            if (_loadout.Count == 0)
            {
                _currentWeaponIndex = -1;
                OnLoadoutChanged?.Invoke();
                return true;
            }

            int targetIndex = _currentWeaponIndex;
            if (targetIndex >= _loadout.Count) targetIndex = _loadout.Count - 1;
            if (targetIndex < 0)              targetIndex = 0;

            _currentWeaponIndex = -1;
            EquipWeapon(targetIndex);
            OnLoadoutChanged?.Invoke();
            return true;
        }

        public bool SelectWeaponByIndex(int index)
        {
            if (index < 0 || index >= _loadout.Count) return false;
            EquipWeapon(index);
            return true;
        }

        /// <summary>
        /// Expande la capacidad del inventario en runtime.
        /// Se invocará desde el futuro ModifierEffectSO de tipo CapacityExpand.
        /// </summary>
        public void AddCapacityBonus(int amount)
        {
            if (amount <= 0) return;
            _capacityBonus += amount;
            OnLoadoutChanged?.Invoke();
        }

        // ── Fallback ───────────────────────────────────────────────────────────────
        private void EnsureFallbackWeaponExists()
        {
            if (_loadout.Count > 0) return;
            if (fistsWeapon == null) return;
            _loadout.Add(new WeaponInstance(fistsWeapon));
        }

        // ── Recoil ────────────────────────────────────────────────────────────────
        private void CaptureWeaponParentOrigin()
        {
            if (weaponParent == null) { _weaponParentOriginCaptured = false; return; }
            _weaponParentOriginalPos     = weaponParent.localPosition;
            _weaponParentOriginalRot     = weaponParent.localEulerAngles;
            _weaponParentOriginCaptured  = true;
        }

        private void ApplyRecoil()
        {
            WeaponDataSO data = CurrentWeapon?.Data;
            if (data == null) return;

            float multiplier = _isAiming ? data.AdsRecoilMultiplier : 1f;

            // ── Cámara: pitch instantáneo ──────────────────────────────────────
            float pitch = UnityEngine.Random.Range(data.RecoilPitchMin, data.RecoilPitchMax) * multiplier;
            _accumulatedPitch += pitch;
            if (playerCamera != null)
                playerCamera.AddRecoilPitch(pitch);

            // ── Cámara: impulso al spring ──────────────────────────────────────
            if (cameraSpring != null && playerCamera != null)
            {
                Vector3 kickBack = -playerCamera.transform.forward * data.RecoilKickStrength * multiplier;
                cameraSpring.AddImpulse(kickBack);
            }

            // ── Modelo: kick aditivo ───────────────────────────────────────────
            if (weaponParent != null && _weaponParentOriginCaptured)
            {
                _currentKickPos += data.KickPositionOffset * multiplier;
                _currentKickRot += data.KickRotationOffset * multiplier;

                // Clamp para no acumular infinitamente en ráfagas
                float maxZ = Mathf.Abs(data.KickPositionOffset.z) * 2.5f;
                _currentKickPos.z = Mathf.Clamp(_currentKickPos.z, -maxZ, maxZ);
                float maxX = Mathf.Abs(data.KickRotationOffset.x) * 2.5f;
                _currentKickRot.x = Mathf.Clamp(_currentKickRot.x, -maxX, maxX);
            }
        }

        private void UpdateRecoilRecovery()
        {
            // ── Recovery de cámara ─────────────────────────────────────────────
            if (Mathf.Abs(_accumulatedPitch) > 0.001f)
            {
                float delta = _accumulatedPitch * recoilRecoverySpeed * Time.deltaTime;
                _accumulatedPitch -= delta;
                if (playerCamera != null)
                    playerCamera.AddRecoilPitch(-delta);
            }
            else
            {
                _accumulatedPitch = 0f;
            }

            // ── Recovery del modelo del arma ───────────────────────────────────
            if (weaponParent != null && _weaponParentOriginCaptured)
            {
                float returnSpeed = 10f;
                if (CurrentWeapon?.Data != null)
                    returnSpeed = CurrentWeapon.Data.KickReturnSpeed;

                _currentKickPos = Vector3.Lerp(_currentKickPos, Vector3.zero, returnSpeed * Time.deltaTime);
                _currentKickRot = Vector3.Lerp(_currentKickRot, Vector3.zero, returnSpeed * Time.deltaTime);

                weaponParent.localPosition    = _weaponParentOriginalPos + _currentKickPos;
                weaponParent.localEulerAngles = _weaponParentOriginalRot + _currentKickRot;
            }
        }

        // ── Efectos visuales y de sonido ───────────────────────────────────────────
        private void PlayMuzzleFlash()
        {
            WeaponDataSO data = CurrentWeapon?.Data;
            if (data == null || data.MuzzleFlashPrefab == null) return;

            Transform spawnPoint = _activeFirePort != null ? _activeFirePort : weaponFirePort;
            if (spawnPoint == null) return;

            // Destruir el fogonazo anterior si existe
            if (_activeMuzzleFlash != null)
                Destroy(_activeMuzzleFlash);

            _activeMuzzleFlash = Instantiate(data.MuzzleFlashPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
            Destroy(_activeMuzzleFlash, 0.15f);
        }

        private void PlayFireSound()
        {
            WeaponDataSO data = CurrentWeapon?.Data;
            if (data == null || data.FireSound == null || _audioSource == null) return;
            _audioSource.PlayOneShot(data.FireSound, data.FireSoundVolume);
        }

        private void PlayReloadSound()
        {
            WeaponDataSO data = CurrentWeapon?.Data;
            if (data == null || data.ReloadSound == null || _audioSource == null) return;
            _audioSource.PlayOneShot(data.ReloadSound, data.ReloadSoundVolume);
        }

        private void SpawnSmokeEffect()
        {
            WeaponDataSO data = CurrentWeapon?.Data;
            if (data == null || data.SmokeEffectPrefab == null) return;

            Transform spawnPoint = _activeFirePort != null ? _activeFirePort : weaponFirePort;
            if (spawnPoint == null) return;

            GameObject smoke = Instantiate(data.SmokeEffectPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
            Destroy(smoke, 2f);
        }
    }
}