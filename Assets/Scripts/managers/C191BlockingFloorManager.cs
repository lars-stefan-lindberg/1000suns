using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C191BlockingFloorManager : MonoBehaviour
{
    private SpriteRenderer _floorRenderer;
    void Awake()
    {
        if(!GameEventManager.obj.AfterPowerUpRoomsCompletedWallBreak) {
            _floorRenderer = GetComponentInChildren<SpriteRenderer>();
            _floorRenderer.color = new Color(_floorRenderer.color.r, _floorRenderer.color.g, _floorRenderer.color.b, 0);
            gameObject.SetActive(false);
        }
    }
}
