using UnityEngine;

public class DialogueCanvasSetMainCamera : MonoBehaviour
{
    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
        canvas.sortingLayerName = "UI";
    }
}
