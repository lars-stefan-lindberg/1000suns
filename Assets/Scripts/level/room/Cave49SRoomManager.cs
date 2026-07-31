using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave49SRoomManager : MonoBehaviour
{
    [SerializeField] private GameObject _tilemap1;
    [SerializeField] private GameEventId _tilemap1RevealedEventId;
    [SerializeField] private GameObject _tilemap2;
    [SerializeField] private GameEventId _tilemap2RevealedEventId;
    [SerializeField] private GameObject _tilemap3;
    [SerializeField] private GameEventId _tilemap3RevealedEventId;

    void Start() {
        if(GameManager.obj.HasEvent(_tilemap1RevealedEventId)) {
            _tilemap1.SetActive(false);
        }
        if(GameManager.obj.HasEvent(_tilemap2RevealedEventId)) {
            _tilemap2.SetActive(false);
        }
        if(GameManager.obj.HasEvent(_tilemap3RevealedEventId)) {
            _tilemap3.SetActive(false);
        }
    }

    public void SetTilemap1Revealed() {
        GameManager.obj.RegisterEvent(_tilemap1RevealedEventId);
    }
    
    public void SetTilemap2Revealed() {
        GameManager.obj.RegisterEvent(_tilemap2RevealedEventId);
    }
    
    public void SetTilemap3Revealed() {
        GameManager.obj.RegisterEvent(_tilemap3RevealedEventId);
    }
}
