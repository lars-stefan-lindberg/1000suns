using UnityEngine;

public class Cave28RoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _secretRevealed;
    [SerializeField] private GameObject _secretWall;

    void Start()
    {
        if(GameManager.obj.HasEvent(_secretRevealed)) {
            _secretWall.SetActive(false);
        }
    }

    public void SetSecretRevealed() {
        GameManager.obj.RegisterEvent(_secretRevealed);
    }
}
