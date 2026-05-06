using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave33RoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _floorBroken;

    public void OnFloorBroken() {
        GameManager.obj.RegisterEvent(_floorBroken);
    }
}
