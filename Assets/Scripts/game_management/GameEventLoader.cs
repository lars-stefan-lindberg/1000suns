using UnityEngine;

public class GameEventLoader : MonoBehaviour
{
    [SerializeField] private GameEventId _gameEventId;
    
    void Start()
    {
        if(GameManager.obj.HasEvent(_gameEventId))
            gameObject.SetActive(false);
        else
            gameObject.SetActive(true);
    }
}
