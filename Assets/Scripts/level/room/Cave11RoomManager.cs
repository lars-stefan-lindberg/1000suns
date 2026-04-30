using System.Collections;
using UnityEngine;

public class Cave11RoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _deeBlockLanded;
    [SerializeField] private GameObject _fallingBlock;
    [SerializeField] private GameObject _blockAlreadyFallen;

    void Start()
    {
        if(GameManager.obj.HasEvent(_deeBlockLanded))
            _blockAlreadyFallen.SetActive(true);
    }

    public void DropBlock() {
        if(GameManager.obj.HasEvent(_deeBlockLanded))
            return;
        _fallingBlock.SetActive(true);
        StartCoroutine(DelayedRegisterGameEvent());
    }

    private IEnumerator DelayedRegisterGameEvent() {
        yield return new WaitForSeconds(2f);
        GameManager.obj.RegisterEvent(_deeBlockLanded);
    }
}
