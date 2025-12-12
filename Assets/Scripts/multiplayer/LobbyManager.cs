using System.Collections.Generic;
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

    void Awake()
    {
        obj = this;
    }

    void OnDestroy()
    {
        obj = null;
    }
}
