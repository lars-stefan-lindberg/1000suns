using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave8RoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _wallBroken;

    public void RegisterWallBroken() {
        GameManager.obj.RegisterEvent(_wallBroken);
    }
}
