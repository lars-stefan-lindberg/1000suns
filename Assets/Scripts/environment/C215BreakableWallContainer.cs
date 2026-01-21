using UnityEngine;

public class C215BreakableWallContainer : MonoBehaviour
{
    void Awake() {
        if(GameManager.obj.C215WallBroken) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
    }
}
