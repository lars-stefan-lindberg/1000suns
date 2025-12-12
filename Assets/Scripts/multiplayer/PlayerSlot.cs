using UnityEngine.InputSystem;

[System.Serializable]
public class PlayerSlot
{
    public int slotIndex;              // 0 or 1
    public InputDevice device;         // null = keyboard or AI / shared mode
    public CharacterType character;

    public enum CharacterType {
        None,
        Eli,
        Dee
    }
}
