using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager obj;
    public bool SinglePlayerMode;
    public bool IsJoiningPlayers = false;
    public bool IsSelectingCharacters = false;

    private List<PlayerSlot> slots = new List<PlayerSlot>();


    public void AddPlayerSlot(PlayerSlot slot) {
        slots.Add(slot);
    }

    public void ClearPlayerSlots() {
        slots.Clear();
    }

    public List<PlayerSlot> GetPlayerSlots() {
        return slots;
    }

    public void SetPlayerInputs() {
        var playerSlots = GetPlayerSlots();
        PlayerSlot eliSlot = playerSlots.Where(slot => slot.character == PlayerSlot.CharacterType.Eli).First();
        PlayerMovement.obj.SetPlayerInputDevice(eliSlot);
        PlayerSlot deeSlot = playerSlots.Where(slot => slot.character == PlayerSlot.CharacterType.Dee).First();
        ShadowTwinMovement.obj.SetPlayerInputDevice(deeSlot);
    }

    void Awake()
    {
        obj = this;
    }

    void OnDestroy()
    {
        obj = null;
    }
}
