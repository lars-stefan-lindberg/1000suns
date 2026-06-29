using UnityEngine;

public class Cave431RoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _secretRevealed;
    [SerializeField] private GameObject _secretWall;
    [SerializeField] private GameObject _floatingPlatform;

    void Start()
    {
        if(GameManager.obj.HasEvent(_secretRevealed)) {
            _secretWall.SetActive(false);
            _floatingPlatform.SetActive(true);
            _floatingPlatform.GetComponentInChildren<FloatyPlatform>().SetAlpha(1f);
        }
    }

    public void SetSecretRevealed() {
        GameManager.obj.RegisterEvent(_secretRevealed);
        _floatingPlatform.SetActive(true);
        StartCoroutine(_floatingPlatform.GetComponentInChildren<FloatyPlatform>().FadeInSprite());
    }
}
