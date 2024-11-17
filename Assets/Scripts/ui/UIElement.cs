using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIElement : MonoBehaviour, IMoveHandler
{
    [SerializeField] private bool _topMenuOption = false;
    [SerializeField] private bool _bottomMenuOption = false;

    public void OnMove(AxisEventData eventData)
    {
        if(eventData.moveDir == MoveDirection.Up && _topMenuOption)
            return;
        if(eventData.moveDir == MoveDirection.Down && _bottomMenuOption)
            return;
        if(eventData.moveDir == MoveDirection.Up || eventData.moveDir == MoveDirection.Down) {
            SoundFXManager.obj.PlayUIBrowse();
        }
    }
}
