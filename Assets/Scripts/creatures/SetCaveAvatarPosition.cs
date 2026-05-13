using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCaveAvatarPosition : MonoBehaviour
{
    [SerializeField] private Transform _targetPosition;
    [SerializeField] private bool _flipX = true;
    [SerializeField] private string _roomId;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(LevelManager.obj.IsLevelCompleted(_roomId)) {
            Destroy(gameObject);
        }
        else if(collision.CompareTag("Player")) {
            CaveAvatar.obj.SetPosition(_targetPosition.position, false);
            CaveAvatar.obj.SetFlipX(_flipX);
            CaveAvatar.obj.gameObject.SetActive(true);
        }
    }
}
