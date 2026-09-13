using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave14RoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _secretRevealed;
    [SerializeField] private GameEventId _blockDropped;
    [SerializeField] private GameObject _secretWall;
    [SerializeField] private GameObject _fallingBlock;
    [SerializeField] private Transform _fallingBlockLowerThreshold;

    void Start()
    {
        if(GameManager.obj.HasEvent(_secretRevealed)) {
            _secretWall.SetActive(false);
        }
        if(GameManager.obj.HasEvent(_blockDropped))
            _fallingBlock.SetActive(false);
    }

    public void SetSecretRevealed() {
        GameManager.obj.RegisterEvent(_secretRevealed);
    }

    void FixedUpdate() {
        if(GameManager.obj.HasEvent(_blockDropped)) {
            return;
        }
        if(_fallingBlock.transform.position.y <= _fallingBlockLowerThreshold.position.y) {
            GameManager.obj.RegisterEvent(_blockDropped);
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        }
    }
}
