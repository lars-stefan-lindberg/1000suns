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

    public void DisableAll() {
        eliInput.enabled = false;
        deeInput.enabled = false;
        blobInput.enabled = false;
    }

    void OnDestroy()
    {
        obj = null;
    }

    private PlayerInput SetActiveCharacterEnabled() {
        if(deeInput.enabled) {
            blobInput.enabled = false;
            eliInput.enabled = false;
            return deeInput;
        } else if(blobInput.enabled) {
            eliInput.enabled = false;
            deeInput.enabled = false;
            return blobInput;
        } else {
            blobInput.enabled = false;
            deeInput.enabled = false;
            return eliInput;
        }
    }

    public bool IsEliActive() {
        return eliInput.enabled;
    }

    public bool IsBlobActive() {
        return blobInput.enabled;
    }

    public bool IsDeeActive() {
        return deeInput.enabled;
    }

    private void OnGamepadCountChanged(int count)
    {
        //TODO: Disable for now. Handle device connect/disconnect
        return;
        if (count <= 0)
        {
            //The assumption of count <= 0 right now is that there are no gamepads connected, but a keyboard
            _isCoopActive = false;
            _waitingForSecondPlayerJoin = false;
            _pendingJoinGamepad = null; 

            var activeInput = SetActiveCharacterEnabled();

            //TODO: Should check if keyboard is available before setting it
            if (!string.IsNullOrEmpty(keyboardControlSchemeName))
            {
                activeInput.SwitchCurrentControlScheme(
                    keyboardControlSchemeName,
                    Keyboard.current, Mouse.current
                );
            }

            return;
        }

        if (count == 1)
        {
            //If gamepad count is 1, we assume that keyboard and gamepad connected. For now we we will set
            //gamepad control to active character.
            //TODO: Somehow support local co-op with keyboard and gamepad connected (while still also supporting single-player with keyboard and gamepad connected)
            if (string.IsNullOrEmpty(keyboardControlSchemeName) || string.IsNullOrEmpty(gamepadControlSchemeName))
            {
                return;
            }

            // 1 gamepad connected: prepare for co-op but wait for button press
            _isCoopActive = false;
            _waitingForSecondPlayerJoin = true;

            var activeInput = SetActiveCharacterEnabled();

            activeInput.SwitchCurrentControlScheme(
                gamepadControlSchemeName,
                Gamepad.all[0]
            );

            _pendingJoinGamepad = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;

            return;
        }

        // count >= 2: handled by OnSecondGamepadConnected
    }

    private void OnSecondGamepadConnected(Gamepad firstGamepad, Gamepad secondGamepad)
    {
        //TODO: Disable for now. Handle device connect/disconnect
        return;
        if (string.IsNullOrEmpty(gamepadControlSchemeName))
        {
            return;
        }

        _isCoopActive = true;
        _waitingForSecondPlayerJoin = false;
        _pendingJoinGamepad = null;

        //TODO: Need to handle if Eli is in blob form
        blobInput.enabled = false;
        eliInput.enabled = true;
        deeInput.enabled = true;

        eliInput.SwitchCurrentControlScheme(gamepadControlSchemeName, firstGamepad);
        deeInput.SwitchCurrentControlScheme(gamepadControlSchemeName, secondGamepad);
    }

    private void OnSecondGamepadDisconnected()
    {
        //TODO: Disable for now. Handle device connect/disconnect
        return;
        if (_isCoopActive)
        {
            //TODO: Should switch to "active" character
            SwitchToEli();
        }
    }

    //TODO: This needs to handle single-player as well when keyboard and one gamepad is connected
    // private void OnAnyInputPerformed(object obj, InputActionChange change)
    // {
    //     if (!_waitingForSecondPlayerJoin)
    //     {
    //         return;
    //     }

    //     if (change != InputActionChange.ActionPerformed)
    //     {
    //         return;
    //     }

    //     var action = obj as InputAction;
    //     if (action == null || action.activeControl == null)
    //     {
    //         return;
    //     }

    //     var device = action.activeControl.device as Gamepad;
    //     if (device == null || _pendingJoinGamepad == null)
    //     {
    //         return;
    //     }

    //     if (device != _pendingJoinGamepad)
    //     {
    //         return;
    //     }

    //     // This is the first gamepad pressing a button: enable Dee and enter co-op
    //     _waitingForSecondPlayerJoin = false;
    //     _isCoopActive = true;

    //     blobInput.enabled = false;
    //     eliInput.enabled = true;
    //     deeInput.enabled = true;

    //     eliInput.SwitchCurrentControlScheme(
    //         keyboardControlSchemeName,
    //         Keyboard.current, Mouse.current
    //     );

    //     deeInput.SwitchCurrentControlScheme(
    //         gamepadControlSchemeName,
    //         _pendingJoinGamepad
    //     );

    //     _pendingJoinGamepad = null;
    // }
}
