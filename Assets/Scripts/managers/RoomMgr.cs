using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Room size: X: 40, Y: 22.5
public class RoomMgr : MonoBehaviour
{
    [SerializeField] private List<GameObject> roomObjects;
    public UnityEvent OnRoomEnter;
    public UnityEvent OnRoomExit;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            roomObjects.ForEach(roomObject => roomObject.SetActive(true));
            OnRoomEnter?.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            roomObjects.ForEach(roomObject => roomObject.SetActive(false));
            OnRoomExit?.Invoke();
        }
    }
}
