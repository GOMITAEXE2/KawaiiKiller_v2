using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace KawaiiKiller.Player
{

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [Space]
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraLean cameraLean;
    [Space]
    [SerializeField] Volume volume;
    [SerializeField] StanceVignette stanceVignette;
    [Space]
    //[SerializeField] private PlayerWeaponController weaponController;

    private PlayerInputActions _inputActions;
    private bool               _inputLocked;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _inputActions = new PlayerInputActions();
        _inputActions.Enable();
        if (playerCharacter == null || playerCamera == null || cameraSpring == null || cameraLean == null || stanceVignette == null || volume == null) return;
        playerCharacter.Initialize();
        playerCamera.Initialize(playerCharacter.GetCameraTarget());

        cameraSpring.Initialize();
        cameraLean.Initialize();
        stanceVignette.Initialize(volume.profile);

    }

    void OnDestroy()
    {
        _inputActions?.Dispose();
    }

    void Update()
    {
        if (_inputActions == null || playerCharacter == null || playerCamera == null) return;
        if (_inputLocked) return;
        var input = _inputActions.Gameplay;
        var deltaTime = Time.deltaTime;

        var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };
        playerCamera.UpdateRotation(cameraInput);

        var characterInput = new CharacterInput
        {
            Rotation    = playerCamera.transform.rotation,
            Move        = input.Move.ReadValue<Vector2>(),
            Jump        = input.Jump.WasPressedThisFrame(),
            JumpSustain = input.Jump.IsPressed(),
            //Crouch    = input.Crouch.WasPressedThisFrame()
            // ? CrouchInput.Toggle
            // : CrouchInput.None
            CrouchDown  = input.Crouch.WasPressedThisFrame(),
            CrouchHold  = input.Crouch.IsPressed(),
            SprintDown  = input.Sprint.WasPressedThisFrame(),
            SprintHold  = input.Sprint.IsPressed()
        };
        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody(deltaTime);

#if UNITY_EDITOR
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out var hit))
            {
                Teleport(hit.point);
            }
        }
        #endif
    }

    void LateUpdate()
    {
        if (_inputLocked) return;
        if (playerCharacter == null || playerCamera == null || cameraSpring == null || cameraLean == null || stanceVignette == null) return;
        var deltaTime = Time.deltaTime;
        var cameraTarget = playerCharacter.GetCameraTarget();
        var state = playerCharacter.GetState();

        playerCamera.UpdatePosition(cameraTarget);
        cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
        cameraLean.UpdateLean
        (
            deltaTime,
            state.Stance is Stance.Slide, 
            state.Acceleration,
            cameraTarget.up
        );

        stanceVignette.UpdateVignette(deltaTime, state.Stance);
    }

    public void SetUIMode(bool uiOpen)
    {
        _inputLocked = uiOpen;
        if (uiOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            _inputActions?.Gameplay.Disable();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
            _inputActions?.Gameplay.Enable();
        }
    }

    public void Teleport(Vector3 position)
    {
        playerCharacter.SetPosition(position);
    }
}

} // namespace KawaiiKiller.Player