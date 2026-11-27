using UnityEngine;

/// <summary>
/// Owns the OS cursor state. Gameplay locks (and hides) the cursor so the mouse can be used for aiming.
/// Menus unlock it so the player can leave the window (alt-tab), but keep it hidden while menus do not support the mouse.
/// The instance is created automatically at startup, no scene setup is needed.
/// </summary>
public class CursorManager : MonoBehaviour
{
    // Menus do not support the mouse yet. Flip this once /Plans/Mouse Support In Menus.md is implemented.
    private const bool SHOW_CURSOR_IN_MENUS = false;

    private const string INSTANCE_NAME = "CursorManager";

    private static CursorManager _instance;
    private static bool _isMenuOpen = false;
    private static bool _hasExplicitMode = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        _instance = null;
        _isMenuOpen = false;
        _hasExplicitMode = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateInstance()
    {
        if (_instance == null)
        {
            GameObject host = new GameObject(INSTANCE_NAME);
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<CursorManager>();
        }

        // Default is gameplay, unless a menu (e.g. the title screen) already asked for menu mode during scene Awake.
        if (!_hasExplicitMode)
        {
            ApplyCursorState();
        }
    }

    /// <summary>
    /// Switches between menu mode (cursor unlocked, hidden for now) and gameplay mode (cursor locked and hidden).
    /// </summary>
    /// <param name="isMenuOpen">True when a menu (title, pause, config screens) is active.</param>
    public static void SetMenuMode(bool isMenuOpen)
    {
        _hasExplicitMode = true;
        _isMenuOpen = isMenuOpen;
        ApplyCursorState();
    }

    private static void ApplyCursorState()
    {
        if (_isMenuOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = SHOW_CURSOR_IN_MENUS;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // The OS or the Editor (Escape in Game View, alt-tab) can release the lock; restore the state matching the current mode.
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            ApplyCursorState();
        }
    }
}
