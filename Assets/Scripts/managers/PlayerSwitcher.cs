using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwitcher : MonoBehaviour
{
    public static PlayerSwitcher obj;
    public PlayerInput eliInput;
    public PlayerInput deeInput;
    public PlayerInput blobInput;

    public string keyboardControlSchemeName;
    public string gamepadControlSchemeName;

    private bool _isCoopActive;
    private Gamepad _pendingJoinGamepad;
    private bool _waitingForSecondPlayerJoin;

    void Awake()
    {
        obj = this;
        InputSystem.onActionChange += OnAnyInputPerformed;
    }

    void OnEnable()
    {
        InputDeviceListener.OnSecondGamepadConnected += OnSecondGamepadConnected;
        InputDeviceListener.OnSecondGamepadDisconnected += OnSecondGamepadDisconnected;
        InputDeviceListener.OnGamepadCountChanged += OnGamepadCountChanged;
    }

    void OnDisable()
    {
        InputDeviceListener.OnSecondGamepadConnected -= OnSecondGamepadConnected;
        InputDeviceListener.OnSecondGamepadDisconnected -= OnSecondGamepadDisconnected;
        InputDeviceListener.OnGamepadCountChanged -= OnGamepadCountChanged;
    }

    void Start()
    {
        SwitchToEli();
    }

    public void SwitchToEli()
    {
        _isCoopActive = false;
        deeInput.enabled = false;
        blobInput.enabled = false;
        eliInput.enabled = true;
    }

    public void SwitchToDee()
    {
        _isCoopActive = false;
        eliInput.enabled = false;
        blobInput.enabled = false;
        deeInput.enabled = true;
    }

    public void SwitchToBlob()
    {
        _isCoopActive = false;
        eliInput.enabled = false;
        deeInput.enabled = false;
        blobInput.enabled = true;
    }

    void OnDestroy()
    {
        InputSystem.onActionChange -= OnAnyInputPerformed;
        obj = null;
    }

    private void OnGamepadCountChanged(int count)
    {
        if (count <= 0)
        {
            _isCoopActive = false;
            _waitingForSecondPlayerJoin = false;
            _pendingJoinGamepad = null;

            deeInput.enabled = false;
            blobInput.enabled = false;
            eliInput.enabled = true;

            if (!string.IsNullOrEmpty(keyboardControlSchemeName))
            {
                eliInput.SwitchCurrentControlScheme(
                    keyboardControlSchemeName,
                    Keyboard.current, Mouse.current
                );
            }

            return;
        }

        if (count == 1)
        {
            if (string.IsNullOrEmpty(keyboardControlSchemeName) || string.IsNullOrEmpty(gamepadControlSchemeName))
            {
                return;
            }

            // 1 gamepad connected: prepare for co-op but wait for button press
            _isCoopActive = false;
            _waitingForSecondPlayerJoin = true;

            blobInput.enabled = false;
            deeInput.enabled = false;
            eliInput.enabled = true;

            eliInput.SwitchCurrentControlScheme(
                keyboardControlSchemeName,
                Keyboard.current, Mouse.current
            );

            _pendingJoinGamepad = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;

            return;
        }

        // count >= 2: handled by OnSecondGamepadConnected
    }

    private void OnSecondGamepadConnected(Gamepad firstGamepad, Gamepad secondGamepad)
    {
        if (string.IsNullOrEmpty(gamepadControlSchemeName))
        {
            return;
        }

        _isCoopActive = true;
        _waitingForSecondPlayerJoin = false;
        _pendingJoinGamepad = null;

        blobInput.enabled = false;
        eliInput.enabled = true;
        deeInput.enabled = true;

        eliInput.SwitchCurrentControlScheme(gamepadControlSchemeName, firstGamepad);
        deeInput.SwitchCurrentControlScheme(gamepadControlSchemeName, secondGamepad);
    }

    private void OnSecondGamepadDisconnected()
    {
        if (_isCoopActive)
        {
            SwitchToEli();
        }
    }

    private void OnAnyInputPerformed(object obj, InputActionChange change)
    {
        if (!_waitingForSecondPlayerJoin)
        {
            return;
        }

        if (change != InputActionChange.ActionPerformed)
        {
            return;
        }

        var action = obj as InputAction;
        if (action == null || action.activeControl == null)
        {
            return;
        }

        var device = action.activeControl.device as Gamepad;
        if (device == null || _pendingJoinGamepad == null)
        {
            return;
        }

        if (device != _pendingJoinGamepad)
        {
            return;
        }

        // This is the first gamepad pressing a button: enable Dee and enter co-op
        _waitingForSecondPlayerJoin = false;
        _isCoopActive = true;

        blobInput.enabled = false;
        eliInput.enabled = true;
        deeInput.enabled = true;

        eliInput.SwitchCurrentControlScheme(
            keyboardControlSchemeName,
            Keyboard.current, Mouse.current
        );

        deeInput.SwitchCurrentControlScheme(
            gamepadControlSchemeName,
            _pendingJoinGamepad
        );

        _pendingJoinGamepad = null;
    }
}
