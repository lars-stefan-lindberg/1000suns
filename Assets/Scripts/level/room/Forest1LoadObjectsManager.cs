using System.Collections.Generic;
using UnityEngine;

public class Forest1LoadObjectsManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> _preLoad;
    [SerializeField] private List<GameObject> _loadOnRoomEnter;
    [SerializeField] private GameEventId _tentCutSceneCompleted;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            if(!GameManager.obj.HasEvent(_tentCutSceneCompleted))
                return;
            foreach (GameObject obj in _loadOnRoomEnter)
            {
                obj.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            foreach (GameObject obj in _loadOnRoomEnter)
            {
                obj.SetActive(false);
            }
        }
    }

    public void LoadRoomObjects() {
        foreach (GameObject obj in _loadOnRoomEnter)
        {
            obj.SetActive(true);
        }
    }
    
    public void LoadIntroObjects() {
        foreach (GameObject obj in _preLoad)
        {
            obj.SetActive(true);
        }
    }
}
